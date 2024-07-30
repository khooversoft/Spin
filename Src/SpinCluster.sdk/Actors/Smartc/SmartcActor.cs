using Microsoft.Extensions.Logging;
using SpinCluster.abstraction;
using SpinCluster.sdk.Actors.Contract;
using SpinCluster.sdk.Application;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Smartc;

public interface ISmartcActor : IGrainWithStringKey
{
    Task<Option> Delete(string traceId);
    Task<Option> Exist(string traceId);
    Task<Option<SmartcModel>> Get(string traceId);
    Task<Option> Set(SmartcModel model, string traceId);
}

public class SmartcActor : Grain, ISmartcActor
{
    private readonly IPersistentState<SmartcModel> _state;
    private readonly ILogger<ContractActor> _logger;

    public SmartcActor(
        [PersistentState(stateName: "default", storageName: SpinConstants.SpinStateStore)] IPersistentState<SmartcModel> state,
        ILogger<ContractActor> logger
        )
    {
        _state = state.NotNull();
        _logger = logger.NotNull();
    }

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        this.VerifySchema(SpinConstants.Schema.Smartc, new ScopeContext(_logger));
        return base.OnActivateAsync(cancellationToken);
    }

    public async Task<Option> Delete(string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Deleting Smartc, actorKey={actorKey}", this.GetPrimaryKeyString());

        if (!_state.RecordExists) return StatusCode.NotFound;

        context.Location().LogInformation("Deleted Smartc, actorKey={actorKey}", this.GetPrimaryKeyString());
        await _state.ClearStateAsync();

        return StatusCode.OK;
    }

    public Task<Option> Exist(string traceId)
    {
        return new Option(_state.RecordExists ? StatusCode.OK : StatusCode.NotFound).ToTaskResult();
    }

    public Task<Option<SmartcModel>> Get(string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Get Smartc, actorKey={actorKey}", this.GetPrimaryKeyString());

        var option = _state.RecordExists switch
        {
            true => _state.State,
            false => new Option<SmartcModel>(StatusCode.NotFound),
        };

        return option.ToTaskResult();
    }

    public async Task<Option> Set(SmartcModel model, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Set Smartc, actorKey={actorKey}", this.GetPrimaryKeyString());

        if (!this.VerifyIdentity(model.SmartcId, out var v)) return v;
        if (!model.Validate(out var v2)) return v2;

        _state.State = model;
        await _state.WriteStateAsync();

        return StatusCode.OK;
    }

    //public Task<Option> SetPayload(DataObject payload, string traceId)
    //{
    //    var context = new ScopeContext(traceId, _logger);
    //    if (!payload.Validate(out var v2)) return v2;
    //    if (!_state.RecordExists) return new Option(StatusCode.NotFound).ToTaskResult();
    //    context.Location().LogInformation("Set payload, actorKey={actorKey}", this.GetPrimaryKeyString());

    //    _state.State.Payloads.Set(payload);
    //    return new Option(StatusCode.OK).ToTaskResult();
    //}
}

