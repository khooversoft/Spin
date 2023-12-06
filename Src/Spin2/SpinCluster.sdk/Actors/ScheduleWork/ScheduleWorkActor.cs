using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using SpinCluster.abstraction;
using SpinCluster.sdk.Actors.Scheduler;
using SpinCluster.sdk.Application;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.ScheduleWork;

public interface IScheduleWorkActor : IGrainWithStringKey
{
    Task<Option> AddRunResult(RunResultModel runResult, string traceId);
    Task<Option<WorkAssignedModel>> Assign(string agentId, string traceId);
    Task<Option> CompletedWork(AssignedCompleted completed, string traceId);
    Task<Option> Create(ScheduleCreateModel work, string traceId);
    Task<Option> Delete(string traceId);
    Task<Option> Exist(string traceId);
    Task<Option<ScheduleWorkModel>> Get(string traceId);
    Task<Option> ReleaseAssign(bool force, string traceId);
}


public class ScheduleWorkActor : Grain, IScheduleWorkActor
{
    private readonly IPersistentState<ScheduleWorkModel> _state;
    private readonly IClusterClient _clusterClient;
    private readonly ILogger<ScheduleWorkActor> _logger;

    public ScheduleWorkActor(
        [PersistentState(stateName: SpinConstants.Ext.Json, storageName: SpinConstants.SpinStateStore)] IPersistentState<ScheduleWorkModel> state,
        IClusterClient clusterClient,
        ILogger<ScheduleWorkActor> logger
        )
    {
        _state = state.NotNull();
        _clusterClient = clusterClient.NotNull();
        _logger = logger.NotNull();
    }

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        this.VerifySchema(SpinConstants.Schema.ScheduleWork, new ScopeContext(_logger));

        ValidateActorKey();
        ValidateModel();

        return base.OnActivateAsync(cancellationToken);
    }

    public async Task<Option> AddRunResult(RunResultModel runResult, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Add run result, actorKey={actorKey}, runResult={runResult}", this.GetPrimaryKeyString(), runResult);

        if (!_state.RecordExists) return (StatusCode.NotFound, "No schedules exist");
        if (_state.State.WorkId.ToLower() != this.GetPrimaryKeyString())
        {
            return (StatusCode.Conflict, $"WorkId={_state.State.WorkId} does not match actorKey={this.GetPrimaryKeyString()}");
        }

        if (!runResult.Validate(out var v)) return v;

        _state.State = _state.State with
        {
            RunResults = (_state.State.RunResults ?? Array.Empty<RunResultModel>())
                .Append(runResult)
                .ToArray(),
        };

        await ValidateAndWrite();
        return StatusCode.OK;
    }

    // Not REST published, all calls should be from ScheduleActor
    public async Task<Option<WorkAssignedModel>> Assign(string agentId, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Assigned to agent, actorKey={actorKey}, agentId={agentId}", this.GetPrimaryKeyString(), agentId);

        if (!_state.RecordExists) return (StatusCode.NotFound, "No schedules exist");

        Option stateOption = _state.State.GetState() switch
        {
            ScheduleWorkState.Assigned => (StatusCode.Conflict, $"actorKey={this.GetPrimaryKeyString()} already assigned"),
            ScheduleWorkState.Completed => (StatusCode.ServiceUnavailable, $"actorKey={this.GetPrimaryKeyString()} already completed"),
            _ => StatusCode.OK,
        };

        if (stateOption.IsError()) return stateOption.ToOptionStatus<WorkAssignedModel>();

        _state.State = _state.State with
        {
            Assigned = new AssignedModel { AgentId = agentId },
        };

        await ValidateAndWrite();

        return _state.State.ConvertTo();
    }

    public async Task<Option> CompletedWork(AssignedCompleted completed, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Complete scheduled work, actorKey={actorKey}, runResult={runResult}", this.GetPrimaryKeyString(), completed);

        if (!_state.RecordExists) return (StatusCode.NotFound, "No schedules exist");
        if (!completed.Validate(out var v)) return v;
        if (_state.State.Assigned == null) return (StatusCode.Conflict, "Not assigned");

        RunResultModel runResult = new RunResultModel
        {
            WorkId = this.GetPrimaryKeyString(),
            StatusCode = completed.StatusCode,
            AgentId = completed.AgentId,
            Message = completed.Message,
        };

        _state.State = _state.State with
        {
            Assigned = _state.State.Assigned.NotNull() with
            {
                AssignedCompleted = completed,
            },

            RunResults = (_state.State.RunResults ?? Array.Empty<RunResultModel>())
                .Append(runResult)
                .ToArray(),
        };

        await ValidateAndWrite();

        var moveOption = await _clusterClient
            .GetScheduleActor(_state.State.SchedulerId)
            .ChangeScheduleState(this.GetPrimaryKeyString(), ScheduleEdgeType.Completed, context.TraceId);

        if (moveOption.IsError()) context.Location().LogStatus(moveOption, "Failed to change directory's state");
        return StatusCode.OK;
    }

    public async Task<Option> Create(ScheduleCreateModel model, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Create schedule work, actorKey={actorKey}, work={work}", this.GetPrimaryKeyString(), model);

        if (_state.RecordExists) return (StatusCode.Conflict, $"Work workId={model.WorkId} already exist");
        if (!this.VerifyIdentity(model.WorkId, out var v)) return v;
        if (!model.Validate(out var v2)) return v2;

        _state.State = model.ConvertTo();
        await _state.WriteStateAsync();

        return StatusCode.OK;
    }

    public async Task<Option> Delete(string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Deleting schedule work, actorKey={actorKey}", this.GetPrimaryKeyString());

        if (!_state.RecordExists) return StatusCode.NotFound;

        ScheduleWorkModel save = _state.State;
        await _state.ClearStateAsync();

        var dirOption = await _clusterClient
            .GetScheduleActor(save.SchedulerId)
            .RemoveSchedule(save.WorkId, context.TraceId);

        if (dirOption.IsError()) return dirOption;

        return StatusCode.OK;
    }

    public Task<Option> Exist(string traceId) => new Option(_state.RecordExists ? StatusCode.OK : StatusCode.NotFound).ToTaskResult();

    public Task<Option<ScheduleWorkModel>> Get(string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Get schedule work, actorKey={actorKey}", this.GetPrimaryKeyString());

        var option = _state.RecordExists switch
        {
            true => _state.State,
            false => new Option<ScheduleWorkModel>(StatusCode.NotFound),
        };

        return option.ToTaskResult();
    }

    public async Task<Option> ReleaseAssign(bool force, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Release assingment to agent, actorKey={actorKey}", this.GetPrimaryKeyString());

        if (!_state.RecordExists) return (StatusCode.NotFound, "No schedules exist");
        if (!force && _state.State.GetState() == ScheduleWorkState.Completed) return (StatusCode.Conflict, $"actorKey={this.GetPrimaryKeyString()} already completed");

        _state.State = _state.State with { Assigned = null };
        await _state.WriteStateAsync();

        var dirOption = await _clusterClient
            .GetScheduleActor(_state.State.SchedulerId)
            .ChangeScheduleState(_state.State.WorkId, ScheduleEdgeType.Active, context.TraceId);

        return dirOption;
    }

    private async Task ValidateAndWrite()
    {
        _state.State.Assert(x => x != null, "State is not set");
        ValidateModel();
        await _state.WriteStateAsync();
    }

    private void ValidateActorKey()
    {
        string actorKey = this.GetPrimaryKeyString();
        var result = ResourceId.IsValid(actorKey, ResourceType.System, SpinConstants.Schema.ScheduleWork);
        result.Assert(x => x == true, x => $"Invalid type and/or schema, {x} does not match schedulework:{{workId}}");
    }

    private void ValidateModel()
    {
        if (!_state.RecordExists) return;
        if (_state.State.Validate(out var v)) return;

        var context = new ScopeContext(_logger);
        context.Location().LogStatus(v, "ScheduleModel is not valid");
        throw new InvalidOperationException($"ScheduleModel is not valid, statusCode={v.StatusCode}, error={v.Error}");
    }
}
