﻿using Microsoft.Extensions.Logging;
using Orleans.Runtime;
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
        [PersistentState(stateName: SpinConstants.Extension.Json, storageName: SpinConstants.SpinStateStore)] IPersistentState<SubscriptionModel> state,
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

        if (!_state.RecordExists)
        {
            await _state.ClearStateAsync();
            return StatusCode.NotFound;
        }

        await _state.ClearStateAsync();
        return StatusCode.OK;
    }

    public async Task<Option> Exist(string traceId)
    {
        await _state.ReadStateAsync();
        return _state.RecordExists ? StatusCode.OK : StatusCode.NotFound;
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

        var test = new OptionTest()
            .Test(() => this.VerifyIdentity(model.SubscriptionId).LogResult(context.Location()))
            .Test(() => model.Validate().LogResult(context.Location()));
        if (test.IsError()) return test;

        _state.State = model;
        await _state.WriteStateAsync();

        return StatusCode.OK;
    }
}
