using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using SpinCluster.sdk.Actors.ActorBase;
using SpinCluster.sdk.Actors.PrincipalPrivateKey;
using SpinCluster.sdk.Actors.Subscription;
using SpinCluster.sdk.Actors.Tenant;
using SpinCluster.sdk.Application;
using Toolbox.Extensions;
using Toolbox.Orleans.Types;
using Toolbox.Tools;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.PrincipalKey;

public interface IPrincipalKeyActor : IGrainWithStringKey
{
    Task<Option> Delete(string traceId);
    Task<Option> Exist(string traceId);
    Task<Option<PrincipalKeyModel>> Get(string traceId);
    Task<Option> Create(PrincipalKeyCreateModel model, string traceId);
    Task<Option> Update(PrincipalKeyModel model, string traceId);
    Task<Option> ValidateJwtSignature(string jwtSignature, string digest, string traceId);
}

public class PrincipalKeyActor : Grain, IPrincipalKeyActor
{
    private readonly IPersistentState<PrincipalKeyModel> _state;
    private readonly IClusterClient _clusterClient;
    private readonly IValidator<PrincipalKeyCreateModel> _createValidator;
    private readonly IValidator<PrincipalKeyModel> _updateValidator;
    private readonly ILogger<PrincipalKeyActor> _logger;

    public PrincipalKeyActor(
        [PersistentState(stateName: SpinConstants.Extension.Json, storageName: SpinConstants.SpinStateStore)] IPersistentState<PrincipalKeyModel> state,
        IClusterClient clusterClient,
        IValidator<PrincipalKeyCreateModel> createValidator,
        IValidator<PrincipalKeyModel> updateValidator,
        ILogger<PrincipalKeyActor> logger
        )
    {
        _state = state.NotNull();
        _clusterClient = clusterClient.NotNull();
        _createValidator = createValidator.NotNull();
        _updateValidator = updateValidator.NotNull();
        _logger = logger.NotNull();
    }

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        this.VerifySchema(SpinConstants.Schema.PrincipalKey, new ScopeContext(_logger));
        return base.OnActivateAsync(cancellationToken);
    }

    public async Task<Option> Delete(string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Deleting PrincipalKey, actorKey={actorKey}", this.GetPrimaryKeyString());

        if (!_state.RecordExists)
        {
            await _state.ClearStateAsync();
            return StatusCode.OK;
        }

        ObjectId privateKey = IdTool.CreatePrivateKeyId(_state.State.PrincipalId);
        await _clusterClient.GetObjectGrain<IPrincipalPrivateKeyActor>(privateKey).Delete(context.TraceId);

        await _state.ClearStateAsync();
        return StatusCode.OK;
    }
    public async Task<Option> Exist(string traceId)
    {
        var context = new ScopeContext(traceId, _logger);

        if (!_state.RecordExists || !_state.State.IsActive) return StatusCode.NotFound;

        var privateKeyExist = await _clusterClient.GetObjectGrain<IPrincipalPrivateKeyActor>(_state.State.KeyId).Exist(context.TraceId);
        return privateKeyExist;
    }

    public Task<Option<PrincipalKeyModel>> Get(string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Get PrincipalKey, actorKey={actorKey}", this.GetPrimaryKeyString());

        var option = _state.RecordExists switch
        {
            true => _state.State.ToOption<PrincipalKeyModel>(),
            false => new Option<PrincipalKeyModel>(StatusCode.NotFound),
        };

        return option.ToTaskResult();
    }

    public async Task<Option> Create(PrincipalKeyCreateModel model, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Creating public/private key for keyId={keyId}", model.KeyId);

        ValidatorResult validatorResult = _createValidator.Validate(model).LogResult(context.Location());
        if (!validatorResult.IsValid) return new Option(StatusCode.BadRequest, validatorResult.FormatErrors());


        var rsaKey = new RsaKeyPair(model.KeyId);

        var principal = new PrincipalKeyModel
        {
            KeyId = model.KeyId,
            PrincipalId = model.PrincipalId,
            Name = model.Name,
            Audience = "spin",
            AccountEnabled = model.AccountEnabled,
            PrivateKeyExist = true,
            PublicKey = rsaKey.PublicKey.ToArray(),
        };

        _state.State = principal;
        await _state.WriteStateAsync();

        var privateKey = await CreatePrivateKey(model, rsaKey, context);
        return privateKey;
    }

    public async Task<Option> Update(PrincipalKeyModel model, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Update PrincipalKey, actorKey={actorKey}", this.GetPrimaryKeyString());

        ValidatorResult validatorResult = _updateValidator.Validate(model).LogResult(context.Location());
        if (!validatorResult.IsValid) return new Option(StatusCode.BadRequest, validatorResult.FormatErrors());

        if (!_state.RecordExists) return new Option(StatusCode.Conflict, "Key must be created");

        if (!this.GetPrimaryKeyString().EqualsIgnoreCase(model.KeyId))
        {
            return new Option(StatusCode.BadRequest, $"KeyId {model.KeyId} does not match actor id={this.GetPrimaryKeyString()}");
        }

        _state.State = model;
        await _state.WriteStateAsync();

        return new Option(StatusCode.OK);
    }

    public async Task<Option> ValidateJwtSignature(string jwtSignature, string digest, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Validating JWT signature");

        await _state.ReadStateAsync();
        if (!_state.RecordExists) return StatusCode.BadRequest;

        var signature = _state.State.ToPrincipalSignature(context);
        if (signature.IsError()) return signature.ToOptionStatus();

        Option validationResult = await signature.Return().ValidateDigest(jwtSignature, digest, context.TraceId);
        return validationResult;
    }

    private async Task<Option> CreatePrivateKey(PrincipalKeyCreateModel model, RsaKeyPair rsaKey, ScopeContext context)
    {
        ObjectId privateKeyId = IdTool.CreatePrivateKeyId((PrincipalId)model.PrincipalId);

        var privatePrincipal = new PrincipalPrivateKeyModel
        {
            KeyId = model.KeyId,
            PrincipalId = model.PrincipalId,
            Name = model.Name,
            AccountEnabled = model.AccountEnabled,
            Audience = "spin",
            PrivateKey = rsaKey.PrivateKey.ToArray(),
        };

        var privateKeyActor = _clusterClient.GetObjectGrain<IPrincipalPrivateKeyActor>(privateKeyId);

        var existPrivagteKey = await privateKeyActor.Exist(context.TraceId);
        if (existPrivagteKey.StatusCode.IsOk())
        {
            context.Location().LogError("Cannot create because private key exist, keyId={keyId}", privatePrincipal.KeyId);
            return new Option(StatusCode.Conflict, $"Cannot create because private key exist, keyId={privatePrincipal.KeyId}");
        }

        var writePrivateKey = await privateKeyActor.Set(privatePrincipal, context.TraceId);
        return writePrivateKey;
    }
}