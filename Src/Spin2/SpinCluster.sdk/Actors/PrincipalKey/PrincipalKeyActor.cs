using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using SpinCluster.sdk.Actors.ActorBase;
using SpinCluster.sdk.Actors.Subscription;
using SpinCluster.sdk.Actors.Tenant;
using SpinCluster.sdk.Application;
using Toolbox.Extensions;
using Toolbox.Orleans.Types;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.PrincipalKey;

public interface IPrincipalKeyActor : IGrainWithStringKey
{
    Task<Option> Delete(string traceId);
    Task<Option> Exist(string traceId);
    Task<Option<PrincipalKeyModel>> Get(string traceId);
    Task<Option> Set(PrincipalKeyModel model, string traceId);
}

public class PrincipalKeyActor : Grain, IPrincipalKeyActor
{
    private readonly IPersistentState<PrincipalKeyModel> _state;
    private readonly IValidator<PrincipalKeyModel> _validator;
    private readonly ILogger<PrincipalKeyActor> _logger;

    public PrincipalKeyActor(
        [PersistentState(stateName: SpinConstants.Extension.Json, storageName: SpinConstants.SpinStateStore)] IPersistentState<PrincipalKeyModel> state,
        IValidator<PrincipalKeyModel> validator,
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

    public async Task<Option> Delete(string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Deleting PrincipalKey, actorKey={actorKey}", this.GetPrimaryKeyString());

        await _state.ClearStateAsync();
        return StatusCode.OK;
    }
    public Task<Option> Exist(string _) => new Option(_state.RecordExists && _state.State.IsActive ? StatusCode.OK : StatusCode.NotFound).ToTaskResult();

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

    public async Task<Option> Set(PrincipalKeyModel model, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Set PrincipalKey, actorKey={actorKey}", this.GetPrimaryKeyString());

        ValidatorResult validatorResult = _validator.Validate(model).LogResult(context.Location());
        if (!validatorResult.IsValid) return new Option(StatusCode.BadRequest, validatorResult.FormatErrors());

        _state.State = model;
        await _state.WriteStateAsync();

        return new Option(StatusCode.OK);
    }
}