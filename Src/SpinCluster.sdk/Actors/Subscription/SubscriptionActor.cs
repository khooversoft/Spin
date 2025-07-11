using Microsoft.Extensions.Logging;
using SpinCluster.abstraction;
using SpinCluster.sdk.Application;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Subscription;

public interface ISubscriptionActor : IGrainWithStringKey
{
    Task<Option> Delete(string traceId);
    Task<Option> Exist(string traceId);
    Task<Option<SubscriptionModel>> Get(string traceId);
    Task<Option> Set(SubscriptionModel model, string traceId);
}


public class SubscriptionActor : Grain, ISubscriptionActor
{
    private readonly IPersistentState<SubscriptionModel> _state;
    private readonly ILogger<SubscriptionActor> _logger;

    public SubscriptionActor(
        [PersistentState(stateName: SpinConstants.Ext.Json, storageName: SpinConstants.SpinStateStore)] IPersistentState<SubscriptionModel> state,
        IClusterClient clusterClient,
        ILogger<SubscriptionActor> logger
        )
    {
        _state = state.NotNull();
        _logger = logger.NotNull();
    }

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        this.VerifySchema(SpinConstants.Schema.Subscription, new ScopeContext(_logger));
        return base.OnActivateAsync(cancellationToken);
    }

    public async Task<Option> Delete(string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Deleting subscription, actorKey={actorKey}", this.GetPrimaryKeyString());

        if (!_state.RecordExists) return StatusCode.NotFound;

        await _state.ClearStateAsync();
        return StatusCode.OK;
    }

    public Task<Option> Exist(string traceId)
    {
        return new Option(_state.RecordExists ? StatusCode.OK : StatusCode.NotFound).ToTaskResult();
    }

    public Task<Option<SubscriptionModel>> Get(string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Get subscription, actorKey={actorKey}", this.GetPrimaryKeyString());

        var option = _state.RecordExists switch
        {
            true => _state.State,
            false => new Option<SubscriptionModel>(StatusCode.NotFound),
        };

        return option.ToTaskResult();
    }

    public async Task<Option> Set(SubscriptionModel model, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Set subscription, actorKey={actorKey}", this.GetPrimaryKeyString());

        if (!this.VerifyIdentity(model.SubscriptionId, out var v)) return v;
        if (!model.Validate(out var v2)) return v2;

        _state.State = model;
        await _state.WriteStateAsync();

        return StatusCode.OK;
    }
}
