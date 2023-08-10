using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using SpinCluster.sdk.Actors.ActorBase;
using SpinCluster.sdk.Application;
using Toolbox.Extensions;
using Toolbox.Orleans.Types;
using Toolbox.Tools.Validation;
using Toolbox.Types;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

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
    private readonly IValidator<SubscriptionModel> _validator;
    private readonly ILogger<SubscriptionModel> _logger;

    public SubscriptionActor(
        [PersistentState(stateName: SpinConstants.Extension.Json, storageName: SpinConstants.SpinStateStore)] IPersistentState<SubscriptionModel> state,
        IValidator<SubscriptionModel> validator,
        ILogger<SubscriptionModel> logger
        )
    {
        _state = state;
        _validator = validator;
        _logger = logger;
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

        await _state.ClearStateAsync();
        return StatusCode.OK;
    }

    public Task<Option> Exist(string _) => new Option(_state.RecordExists && _state.State.IsActive ? StatusCode.OK : StatusCode.NotFound).ToTaskResult();

    public Task<Option<SubscriptionModel>> Get(string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Get subscription, actorKey={actorKey}", this.GetPrimaryKeyString());

        var option = _state.RecordExists switch
        {
            true => _state.State.ToOption<SubscriptionModel>(),
            false => new Option<SubscriptionModel>(StatusCode.NotFound),
        };

        return option.ToTaskResult();
    }

    public async Task<Option> Set(SubscriptionModel model, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Set subscription, actorKey={actorKey}", this.GetPrimaryKeyString());

        ValidatorResult validatorResult = _validator.Validate(model).LogResult(context.Location());
        if (!validatorResult.IsValid) return new Option(StatusCode.BadRequest, validatorResult.FormatErrors());

        _state.State = model;
        await _state.WriteStateAsync();

        return new Option(StatusCode.OK);
    }
}