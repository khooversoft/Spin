using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using SpinCluster.sdk.Actors.Contract;
using SpinCluster.sdk.Actors.Subscription;
using SpinCluster.sdk.Application;
using SpinCluster.sdk.Models;
using Toolbox.Block;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Smartc;

public interface ISmartcActor : IGrainWithStringKey
{
    Task<Option> CompletedWork(SmartcRunResultModel runResult, string traceId);
    Task<Option> Delete(string traceId);
    Task<Option> Exist(string traceId);
    Task<Option<SmartcModel>> Get(string traceId);
    Task<Option<AgentAssignmentModel>> GetAssignment(string traceId);
    Task<Option> Set(SmartcModel model, string traceId);
    Task<Option> SetAssignment(AgentAssignmentModel model, string traceId);

}

public class SmartcActor : Grain, ISmartcActor
{
    private readonly IPersistentState<SmartcModel> _state;
    private readonly ILogger<ContractActor> _logger;
    private readonly IClusterClient _clusterClient;

    public SmartcActor(
        [PersistentState(stateName: "default", storageName: SpinConstants.SpinStateStore)] IPersistentState<SmartcModel> state,
        IClusterClient clusterClient,
        ILogger<ContractActor> logger
        )
    {
        _state = state.NotNull();
        _clusterClient = clusterClient.NotNull();
        _logger = logger.NotNull();
    }

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        this.VerifySchema(SpinConstants.Schema.Smartc, new ScopeContext(_logger));
        return base.OnActivateAsync(cancellationToken);
    }

    public async Task<Option> CompletedWork(SmartcRunResultModel model, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Set assignment, actorKey={actorKey}, model={model}", this.GetPrimaryKeyString(), model);

        var test = new OptionTest()
            .Test(() => _state.RecordExists)
            .Test(() => _state.State.Assignment != null ? StatusCode.OK : new Option(StatusCode.BadRequest, "No assignment"))
            .Test(() => model.Validate().LogResult(context.Location()));
        if (test.IsError()) return test;

        AgentAssignmentModel assignment = _state.State.Assignment.NotNull();
        _state.State = _state.State with { Assignment = null };
        await _state.WriteStateAsync();

        var runResult = new RunResultModel
        {
            AgentId = assignment.AgentId,
            StatusCode = StatusCode.OK,
            Message = "completed",
        };

        var scheduleUpdateOption = await _clusterClient
            .GetResourceGrain<ISchedulerActor>(SpinConstants.Scheduler)
            .CompletedWork(assignment.WorkId, runResult, context.TraceId);

        if (scheduleUpdateOption.IsError()) return scheduleUpdateOption;

        return StatusCode.OK;
    }

    public async Task<Option> Delete(string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Deleting Smartc, actorKey={actorKey}", this.GetPrimaryKeyString());

        if (!_state.RecordExists)
        {
            await _state.ClearStateAsync();
            return StatusCode.NotFound;
        }

        context.Location().LogInformation("Deleted Smartc, actorKey={actorKey}", this.GetPrimaryKeyString());
        await _state.ClearStateAsync();

        return StatusCode.OK;
    }

    public async Task<Option> Exist(string traceId)
    {
        await _state.ReadStateAsync();
        return _state.RecordExists ? StatusCode.OK : StatusCode.NotFound;
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

    public Task<Option<AgentAssignmentModel>> GetAssignment(string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Get Smartc, actorKey={actorKey}", this.GetPrimaryKeyString());

        var option = (_state.RecordExists && _state.State.Assignment != null) switch
        {
            true => _state.State.Assignment,
            false => new Option<AgentAssignmentModel>(StatusCode.NotFound),
        };

        return option.ToTaskResult();
    }

    public async Task<Option> Set(SmartcModel model, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Set Smartc, actorKey={actorKey}", this.GetPrimaryKeyString());

        var test = new OptionTest()
            .Test(() => this.VerifyIdentity(model.SmartcId).LogResult(context.Location()))
            .Test(() => model.Validate().LogResult(context.Location()));
        if (test.IsError()) return test;

        _state.State = model;
        await _state.WriteStateAsync();

        return StatusCode.OK;
    }

    public async Task<Option> SetAssignment(AgentAssignmentModel model, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Set assignment, actorKey={actorKey}, model={model}", this.GetPrimaryKeyString(), model);

        var test = new OptionTest()
            .Test(() => _state.RecordExists)
            .Test(() => model.Validate().LogResult(context.Location()));
        if (test.IsError()) return test;

        _state.State = _state.State with
        {
            Assignment = model
        };

        await _state.WriteStateAsync();
        return StatusCode.OK;
    }
}

