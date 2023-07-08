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
}

internal class PrincipalKeyActor : Grain, IPrincipalKeyActor
{
    private readonly IPersistentState<PrincipalKeyModel> _state;
    private readonly IValidator<PrincipalKeyRequest> _validator;
    private readonly ILogger<PrincipalKeyActor> _logger;

    public PrincipalKeyActor(
        [PersistentState(stateName: SpinConstants.Extension.PrincipalKey, storageName: SpinConstants.SpinStateStore)] IPersistentState<PrincipalKeyModel> state,
        IValidator<PrincipalKeyRequest> validator,
        ILogger<PrincipalKeyActor> logger
        )
    {
        _state = state;
        _validator = validator;
        _logger = logger;
    }

    public async Task<SpinResponse> Create(PrincipalKeyRequest request, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        var validation = _validator.Validate(request, context.Location());
        if (!validation.IsValid) return validation.ToSpinResponse();

        context.Location().LogInformation("Creating principal key, primarykey={primaryKey}, request={request}", this.GetPrimaryKeyString(), request);
        await _state.ReadStateAsync();
        if (_state.RecordExists)
        {
            const string msg = "Cannot create new Key, public key already exist";
            context.Location().LogError(msg);
            return new SpinResponse(StatusCode.BadRequest, msg);
        }

        ObjectId privateObjectId = this.GetPrimaryKeyString()
            .ToObjectId()
            .WithSchema(SpinConstants.Schema.PrincipalPrivateKey)
            .WithExtension(SpinConstants.Extension.PrincipalKey);

        var rsaKey = new RsaKeyPair(request.KeyId);

        SpinResponse searchResult = await GrainFactory.GetGrain<ISearchActor>(SpinConstants.SchemaSearch).Exist(privateObjectId.ToString(), context.TraceId);
        if( searchResult.StatusCode.IsOk())
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
            PublicKey = rsaKey.PublicKey
        };

        var privateKey = new PrincipalPrivateKeyModel
        {
            KeyId = rsaKey.KeyId,
            OwnerId = request.OwnerId,
            Name = request.Name,
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
            context.Location().LogInformation("Setting private key for keyId={keyId}", rsaKey.KeyId);
            await GrainFactory.GetGrain<IPrincipalPrivateKeyActor>(privateObjectId.ToString()).Set(privateKey, traceId);
        }
        catch (Exception ex)
        {
            context.Location().LogCritical(ex, "Failed to set private actor, trying to reverting, keyId={keyId}", rsaKey.KeyId);
            await _state.ClearStateAsync();
        }

        return new SpinResponse(StatusCode.OK);
    }

    public async Task<SpinResponse> Delete(string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        string keyId = this.GetPrimaryKeyString().NotEmpty();

        var response = await _state.Delete(keyId, context.Location());
        if (response.StatusCode.IsError()) return response;

        var privateKeyResponse = await GrainFactory.GetGrain<IPrincipalPrivateKeyActor>(keyId).Delete(traceId);
        if (privateKeyResponse.StatusCode.IsError())
        {
            context.Location().LogWarning("Public key keyId={keyId} does not exist to delete", keyId);
        }

        return response;
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
}
