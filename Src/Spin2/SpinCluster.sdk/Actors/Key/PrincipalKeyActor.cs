using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using SpinCluster.sdk.Actors.ActorBase;
using SpinCluster.sdk.Actors.Key.Private;
using SpinCluster.sdk.Actors.Search;
using SpinCluster.sdk.Application;
using SpinCluster.sdk.Types;
using Toolbox.Extensions;
using Toolbox.Security.Jwt;
using Toolbox.Security.Principal;
using Toolbox.Tools;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Key;

public interface IPrincipalKeyActor : IGrainWithStringKey
{
    Task<SpinResponse> Create(PrincipalKeyRequest request, string traceId);
    Task<SpinResponse> Delete(string traceId);
    Task<SpinResponse> Exist(string traceId);
    Task<SpinResponse<PrincipalKeyModel>> Get(string traceId);
    Task<SpinResponse> Update(PrincipalKeyRequest request, string traceId);
    Task<SpinResponse> ValidateJwtSignature(string jwtSignature, string digest, string traceId);
}

internal class PrincipalKeyActor : Grain, IPrincipalKeyActor
{
    private readonly IPersistentState<PrincipalKeyModel> _state;
    private readonly IValidator<PrincipalKeyRequest> _validator;
    private readonly ILogger<PrincipalKeyActor> _logger;
    private readonly string? _privateKeyObjectId = null;

    public PrincipalKeyActor(
        [PersistentState(stateName: SpinConstants.Extension.Json, storageName: SpinConstants.SpinStateStore)] IPersistentState<PrincipalKeyModel> state,
        IValidator<PrincipalKeyRequest> validator,
        ILogger<PrincipalKeyActor> logger
        )
    {
        _state = state;
        _validator = validator;
        _logger = logger;
    }

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        this.VerifySchema(SpinConstants.Schema.PrincipalKey, new ScopeContext(_logger));
        return base.OnActivateAsync(cancellationToken);
    }

    public async Task<SpinResponse> Create(PrincipalKeyRequest request, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);

        var validation = _validator.Validate(request, context.Location());
        if (!validation.IsValid) return validation.ToSpinResponse();

        if (request.KeyId != this.GetPrimaryKeyString())
        {
            return new SpinResponse(StatusCode.BadRequest, $"requst.KeyId={request.KeyId} does not match actor key={this.GetPrimaryKeyString()}");
        }

        context.Location().LogInformation("Creating principal key, primarykey={primaryKey}, request={request}", this.GetPrimaryKeyString(), request);
        await _state.ReadStateAsync();
        if (_state.RecordExists)
        {
            const string msg = "Cannot create new Key, public key already exist";
            context.Location().LogError(msg);
            return new SpinResponse(StatusCode.BadRequest, msg);
        }

        var rsaKey = new RsaKeyPair(request.KeyId);

        SpinResponse searchResult = await GrainFactory.GetGrain<ISearchActor>(SpinConstants.SchemaSearch).Exist(this.GetPrimaryKeyString(), context.TraceId);
        if (searchResult.StatusCode.IsOk())
        {
            const string msg = "Cannot create new key, private key already exist";
            context.Location().LogError(msg);
            return new SpinResponse(StatusCode.BadRequest, msg);
        }

        var publicKey = new PrincipalKeyModel
        {
            KeyId = rsaKey.KeyId,
            OwnerId = request.OwnerId,
            Name = request.Name,
            Audience = request.Audience,
            PublicKey = rsaKey.PublicKey,
            PrivateKeyExist = true,
        };

        var privateKey = new PrincipalPrivateKeyModel
        {
            KeyId = rsaKey.KeyId,
            OwnerId = request.OwnerId,
            Name = request.Name,
            Audience = request.Audience,
            PrivateKey = rsaKey.PrivateKey
        };

        try
        {
            context.Location().LogInformation("Setting public key for keyId={keyId}", rsaKey.KeyId);
            _state.State = publicKey;
            await _state.WriteStateAsync();
        }
        catch (Exception ex)
        {
            context.Location().LogCritical(ex, "Failed to set public actor, actorId={actorId} keyId={keyId}", this.GetPrimaryKeyString(), rsaKey.KeyId);
            return new SpinResponse(StatusCode.InternalServerError, $"Failed to set public key actor, keyId={this.GetPrimaryKeyString()}");
        }

        try
        {
            string privateKeyActorId = GetPrivateKeyObjectId();

            context.Location().LogInformation("Setting private key for keyId={keyId}", rsaKey.KeyId);
            await GrainFactory.GetGrain<IPrincipalPrivateKeyActor>(privateKeyActorId).Set(privateKey, traceId);
        }
        catch (Exception ex)
        {
            context.Location().LogCritical(ex, "Failed to set private actor, trying to reverting, keyId={keyId}, privateKeyActorId={privateKeyActorId}", rsaKey.KeyId);
            await _state.ClearStateAsync();
        }

        return new SpinResponse(StatusCode.OK);
    }

    public async Task<SpinResponse> Delete(string traceId)
    {
        var context = new ScopeContext(traceId, _logger);

        await _state.ClearStateAsync();

        string privateKeyActorId = GetPrivateKeyObjectId();
        await GrainFactory.GetGrain<IPrincipalPrivateKeyActor>(privateKeyActorId).Delete(traceId);

        return new SpinResponse(StatusCode.OK);
    }

    public Task<SpinResponse> Exist(string traceId) => Task.FromResult(new SpinResponse(_state.RecordExists ? StatusCode.OK : StatusCode.NoContent));
    public Task<SpinResponse<PrincipalKeyModel>> Get(string traceId) => _state.Get(this.GetPrimaryKeyString(), new ScopeContext(traceId, _logger).Location());

    public async Task<SpinResponse> Update(PrincipalKeyRequest request, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Updating id={id}, model={model}", this.GetPrimaryKeyString(), request.ToJsonPascalSafe(context));

        ValidatorResult validatorResult = _validator.Validate(request);
        if (!validatorResult.IsValid)
        {
            context.Location().LogError(validatorResult.FormatErrors());
            return new SpinResponse(StatusCode.BadRequest, validatorResult.FormatErrors());
        }

        if (!_state.RecordExists)
        {
            context.Location().LogInformation("Updating record for id={id}, does not exist, please use Create", this.GetPrimaryKeyString());
            return new SpinResponse(StatusCode.NotFound);
        }

        PrincipalKeyModel model = _state.State;

        model = model with
        {
            OwnerId = model.OwnerId,
            Name = model.Name,
        };

        _state.State = model;
        await _state.WriteStateAsync();

        return new SpinResponse(StatusCode.OK);
    }

    public async Task<SpinResponse> ValidateJwtSignature(string jwtSignature, string digest, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);

        await _state.ReadStateAsync();
        if (!_state.RecordExists)
        {
            context.Location().LogError("Public key does not exist to validate digest signature");
            return new SpinResponse(StatusCode.NotFound);
        }

        var signature = _state.State.ToPrincipalSignature(context);
        if (signature.IsError()) return signature.ToSpinResponse();

        Option<JwtTokenDetails> validationResult = await signature.Return().ValidateDigest(jwtSignature, digest, context);
        return validationResult.ToSpinResponse();
    }

    private string GetPrivateKeyObjectId() => _privateKeyObjectId ?? this.GetPrimaryKeyString()
        .ToObjectId()
        .WithSchema(SpinConstants.Schema.PrincipalPrivateKey)
        .ToString();
}
