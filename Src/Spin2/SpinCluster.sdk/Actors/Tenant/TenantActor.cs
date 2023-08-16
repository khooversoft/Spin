﻿using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using SpinCluster.sdk.Actors.ActorBase;
using SpinCluster.sdk.Actors.Subscription;
using SpinCluster.sdk.Application;
using Toolbox.Extensions;
using Toolbox.Orleans.Types;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Tenant;

public interface ITenantActor : IGrainWithStringKey
{
    Task<Option> Delete(string traceId);
    Task<Option> Exist(string traceId);
    Task<Option<TenantModel>> Get(string traceId);
    Task<Option> Set(TenantModel model, string traceId);
}


public class TenantActor : Grain, ITenantActor
{
    private readonly IPersistentState<TenantModel> _state;
    private readonly IValidator<TenantModel> _validator;
    private readonly IClusterClient _clusterClient;
    private readonly ILogger<TenantActor> _logger;

    public TenantActor(
        [PersistentState(stateName: SpinConstants.Extension.Json, storageName: SpinConstants.SpinStateStore)] IPersistentState<TenantModel> state,
        IValidator<TenantModel> validator,
        IClusterClient clusterClient,
        ILogger<TenantActor> logger
        )
    {
        _state = state;
        _validator = validator;
        _clusterClient = clusterClient;
        _logger = logger;
    }

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        this.VerifySchema(SpinConstants.Schema.Tenant, new ScopeContext(_logger));
        return base.OnActivateAsync(cancellationToken);
    }

    public async Task<Option> Delete(string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Deleting subscription, actorKey={actorKey}", this.GetPrimaryKeyString());

        await _state.ClearStateAsync();
        return StatusCode.OK;
    }

    public Task<Option> Exist(string _) => new Option(_state.RecordExists && _state.State.IsActive ? StatusCode.OK : StatusCode.NotFound).ToTaskResult();

    public Task<Option<TenantModel>> Get(string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Get subscription, actorKey={actorKey}", this.GetPrimaryKeyString());

        var option = _state.RecordExists switch
        {
            true => _state.State.ToOption<TenantModel>(),
            false => new Option<TenantModel>(StatusCode.NotFound),
        };

        return option.ToTaskResult();
    }

    public async Task<Option> Set(TenantModel model, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Set subscription, actorKey={actorKey}", this.GetPrimaryKeyString());

        ValidatorResult validatorResult = _validator.Validate(model).LogResult(context.Location());
        if (!validatorResult.IsValid) return new Option(StatusCode.BadRequest, validatorResult.FormatErrors());

        Option isSubscriptionActive = await _clusterClient
            .GetObjectGrain<ISubscriptionActor>(IdTool.CreateSubscriptionId(model.SubscriptionName))
            .Exist(context);

        if (isSubscriptionActive.StatusCode.IsError())
        {
            context.Location().LogError("SubscriptionName={subscriptionName} does not exist, error={error}", model.SubscriptionName, isSubscriptionActive.Error);
            return new Option(StatusCode.Conflict, $"SubscriptionName={model.SubscriptionName} does not exist");
        }

        _state.State = model;
        await _state.WriteStateAsync();

        return new Option(StatusCode.OK);
    }
}