using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using SpinCluster.sdk.Actors.PrincipalPrivateKey;
using SpinCluster.sdk.Application;
using Toolbox.Extensions;
using Toolbox.Tools;
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
    private readonly ILogger<PrincipalKeyActor> _logger;

    public PrincipalKeyActor(
        [PersistentState(stateName: SpinConstants.Extension.Json, storageName: SpinConstants.SpinStateStore)] IPersistentState<PrincipalKeyModel> state,
        IClusterClient clusterClient,
        ILogger<PrincipalKeyActor> logger
        )
    {
        _state = state.NotNull();
        _clusterClient = clusterClient.NotNull();
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

        if (!_state.RecordExists) return StatusCode.NotFound;

        string principalPrivateKeyId = _state.State.PrincipalPrivateKeyId;
        await _state.ClearStateAsync();

        Option<ResourceId> privateKey = ResourceId.Create(principalPrivateKeyId);
        if (privateKey.IsError()) return privateKey.ToOptionStatus();

        await _clusterClient.GetResourceGrain<IPrincipalPrivateKeyActor>(privateKey.Return()).Delete(context.TraceId);

        return StatusCode.OK;
    }

    public async Task<Option> Exist(string traceId)
    {
        var context = new ScopeContext(traceId, _logger);

        if (!_state.RecordExists || !_state.State.IsActive) return StatusCode.NotFound;

        var privateKeyExist = await _clusterClient.GetResourceGrain<IPrincipalPrivateKeyActor>(_state.State.PrincipalPrivateKeyId).Exist(context.TraceId);
        return privateKeyExist;
    }

    public Task<Option<PrincipalKeyModel>> Get(string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Get PrincipalKey, actorKey={actorKey}", this.GetPrimaryKeyString());

        var option = _state.RecordExists switch
        {
            true => _state.State,
            false => new Option<PrincipalKeyModel>(StatusCode.NotFound),
        };

        return option.ToTaskResult();
    }

    public async Task<Option> Create(PrincipalKeyCreateModel model, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Creating public/private key for keyId={keyId}", model.KeyId);

        if (_state.RecordExists) return StatusCode.NotFound;
        if (!this.VerifyIdentity(model.PrincipalKeyId, out var v)) return v;
        if (!model.Validate(out var v2)) return v2;

        var rsaKey = new RsaKeyPair(model.KeyId);

        var principal = new PrincipalKeyModel
        {
            PrincipalKeyId = model.PrincipalKeyId,
            KeyId = model.KeyId,
            PrincipalId = model.PrincipalId,
            Name = model.Name,
            Audience = "spin",
            Enabled = true,
            PrincipalPrivateKeyId = model.PrincipalPrivateKeyId,
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

        if (!_state.RecordExists) return StatusCode.NotFound;
        if (!this.VerifyIdentity(model.PrincipalKeyId, out var v)) return v;
        if (!model.Validate(out var v2)) return v2;

        _state.State = model;
        await _state.WriteStateAsync();

        return new Option(StatusCode.OK);
    }

    public async Task<Option> ValidateJwtSignature(string jwtSignature, string digest, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Validating JWT signature");

        if (!_state.RecordExists) return (StatusCode.NotFound, "Public key does not exit");

        var signature = _state.State.ToPrincipalSignature(context);
        if (signature.IsError()) return signature.ToOptionStatus();

        Option validationResult = await signature.Return().ValidateDigest(jwtSignature, digest, context.TraceId);
        return validationResult;
    }

    private async Task<Option> CreatePrivateKey(PrincipalKeyCreateModel model, RsaKeyPair rsaKey, ScopeContext context)
    {
        var privatePrincipal = new PrincipalPrivateKeyModel
        {
            PrincipalPrivateKeyId = model.PrincipalPrivateKeyId,
            KeyId = model.KeyId,
            PrincipalId = model.PrincipalId,
            Name = model.Name,
            Enabled = true,
            Audience = "spin",
            PrivateKey = rsaKey.PrivateKey.ToArray(),
        };

        var privateKeyActor = _clusterClient.GetResourceGrain<IPrincipalPrivateKeyActor>(model.PrincipalPrivateKeyId);

        var existPrivagteKey = await privateKeyActor.Exist(context.TraceId);
        if (existPrivagteKey.IsOk())
        {
            context.Location().LogError("Cannot create because private key exist, keyId={keyId}", privatePrincipal.KeyId);
            return new Option(StatusCode.Conflict, $"Cannot create because private key exist, keyId={privatePrincipal.KeyId}");
        }

        var writePrivateKey = await privateKeyActor.Set(privatePrincipal, context.TraceId);
        return writePrivateKey;
    }
}