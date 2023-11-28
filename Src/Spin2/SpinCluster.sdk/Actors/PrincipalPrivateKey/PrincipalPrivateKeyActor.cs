using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using SpinCluster.abstraction;
using SpinCluster.sdk.Application;
using Toolbox.Extensions;
using Toolbox.Security.Jwt;
using Toolbox.Security.Principal;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.PrincipalPrivateKey;

public interface IPrincipalPrivateKeyActor : IGrainWithStringKey
{
    Task<Option> Delete(string traceId);
    Task<Option> Exist(string traceId);
    Task<Option<PrincipalPrivateKeyModel>> Get(string traceId);
    Task<Option> Set(PrincipalPrivateKeyModel model, string traceId);
    Task<Option<string>> Sign(string messageDigest, string traceId);
}

public class PrincipalPrivateKeyActor : Grain, IPrincipalPrivateKeyActor
{
    private readonly IPersistentState<PrincipalPrivateKeyModel> _state;
    private readonly ILogger<PrincipalPrivateKeyActor> _logger;

    public PrincipalPrivateKeyActor(
        [PersistentState(stateName: SpinConstants.Ext.Json, storageName: SpinConstants.SpinStateStore)] IPersistentState<PrincipalPrivateKeyModel> state,
        ILogger<PrincipalPrivateKeyActor> logger
        )
    {
        _state = state;
        _logger = logger;
    }

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        this.VerifySchema(SpinConstants.Schema.PrincipalPrivateKey, new ScopeContext(_logger));
        return base.OnActivateAsync(cancellationToken);
    }

    public async Task<Option> Delete(string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Deleting PrivatePrincipalKey, actorKey={actorKey}", this.GetPrimaryKeyString());

        if (!_state.RecordExists) return StatusCode.NotFound;

        await _state.ClearStateAsync();
        return StatusCode.OK;
    }

    public Task<Option> Exist(string _) => new Option(_state.RecordExists && _state.State.IsActive ? StatusCode.OK : StatusCode.NotFound).ToTaskResult();

    public Task<Option<PrincipalPrivateKeyModel>> Get(string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Get PrivatePrincipalKey, actorKey={actorKey}", this.GetPrimaryKeyString());

        var option = _state.RecordExists switch
        {
            true => _state.State,
            false => new Option<PrincipalPrivateKeyModel>(StatusCode.NotFound),
        };

        return option.ToTaskResult();
    }

    public async Task<Option> Set(PrincipalPrivateKeyModel model, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Set PrivatePrincipalKey, actorKey={actorKey}", this.GetPrimaryKeyString());
        if (!this.VerifyIdentity(model.PrincipalPrivateKeyId, out var v)) return v;
        if (!model.Validate(out var v1)) return v1;

        if (_state.RecordExists)
        {
            model = model with { PrivateKey = _state.State.PrivateKey };
        }

        _state.State = model;
        await _state.WriteStateAsync();

        return new Option(StatusCode.OK);
    }

    public Task<Option<string>> Sign(string messageDigest, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Signing with private key, actorKey={actorKey}", this.GetPrimaryKeyString());

        if (!_state.RecordExists) return new Option<string>(StatusCode.NotFound).ToTaskResult();

        var signature = PrincipalSignature.CreateFromPrivateKeyOnly(
            _state.State.PrivateKey,
            _state.State.KeyId,
            _state.State.PrincipalId,
            _state.State.Audience
            );

        string jwtSignature = new JwtTokenBuilder()
            .SetDigest(messageDigest)
            .SetExpires(DateTime.Now.AddDays(10))
            .SetIssuedAt(DateTime.Now)
            .SetPrincipleSignature(signature)
            .Build();

        if (jwtSignature.IsEmpty())
        {
            context.Location().LogError("Failed to build JWT");
            return new Option<string>(StatusCode.Conflict, "JWT builder failed").ToTaskResult();
        }

        return jwtSignature.ToOption().ToTaskResult();
    }
}