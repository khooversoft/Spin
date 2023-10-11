using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using SpinCluster.sdk.Actors.Agent;
using SpinCluster.sdk.Actors.Contract;
using SpinCluster.sdk.Actors.Scheduler;
using SpinCluster.sdk.Actors.ScheduleWork;
using SpinCluster.sdk.Actors.Smartc;
using SpinCluster.sdk.Application;
using Toolbox.Block;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Scheduler;

public interface ISchedulerActor : IGrainWithStringKey
{
    Task<Option> CreateSchedule(ScheduleCreateModel work, string traceId);
    Task<Option<WorkAssignedModel>> AssignWork(string agentId, string traceId);
    Task<Option> Clear(string principalId, string traceId);
    Task<Option> InternalCompleted(AssignedCompleted model, string traceId);
    Task<Option<SchedulesModel>> GetSchedules(string traceId);
    Task<Option> ResetWork(string workId, string traceId);
}

// Actor key = "system:schedule
public class SchedulerActor : Grain, ISchedulerActor
{
    private readonly IPersistentState<SchedulesModel> _state;
    private readonly IClusterClient _clusterClient;
    private readonly ILogger<SchedulerActor> _logger;

    public SchedulerActor(
        [PersistentState(stateName: SpinConstants.Extension.Json, storageName: SpinConstants.SpinStateStore)] IPersistentState<SchedulesModel> state,
        IClusterClient clusterClient,
        ILogger<SchedulerActor> logger
        )
    {
        _state = state.NotNull();
        _clusterClient = clusterClient.NotNull();
        _logger = logger.NotNull();
    }

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        this.GetPrimaryKeyString().Assert(x => x == SpinConstants.Scheduler, x => $"Actor key {x} is invalid, must match {SpinConstants.Scheduler}");

        if (_state.RecordExists)
        {
            var v = _state.State.Validate();
            if (v.IsError()) throw new InvalidOperationException($"ScheduleModel is not valid on read, error={v.Error}");
        }

        return base.OnActivateAsync(cancellationToken);
    }

    public async Task<Option> CreateSchedule(ScheduleCreateModel work, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Schedule work, actorKey={actorKey}, work={work}", this.GetPrimaryKeyString(), work);

        if (!work.Validate(out var v)) return v;

        _state.State = _state.RecordExists ? _state.State : new SchedulesModel();
        _state.State.AddToActive(new ScheduleEntry { WorkId = work.WorkId });
        await _state.WriteStateAsync();

        var createOption = await _clusterClient
            .GetResourceGrain<IScheduleWorkActor>(work.WorkId)
            .Create(work, context.TraceId);

        return createOption;
    }

    public async Task<Option<WorkAssignedModel>> AssignWork(string agentId, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Get work, actorKey={actorKey}", this.GetPrimaryKeyString());
        if (!_state.RecordExists) return StatusCode.NotFound;

        // Verify agent is registered and active
        Option agentLookup = await _clusterClient.GetResourceGrain<IAgentActor>(agentId).IsActive(context.TraceId);
        if (agentLookup.IsError()) return new Option<WorkAssignedModel>(agentLookup.StatusCode, $"agentId={agentId}, " + agentLookup.Error);

        while (true)
        {
            // Is there any work?
            string? workId = _state.State
                .ActiveItems
                .Where(x => x.Value.IsAssignable())
                .OrderBy(x => x.Value.CreatedDate)
                .Select(x => x.Key)
                .FirstOrDefault();

            if (workId == null) return StatusCode.NotFound;

            var assignedOption = await _clusterClient
                .GetResourceGrain<IScheduleWorkActor>(workId)
                .Assign(agentId, context.TraceId);

            switch (assignedOption)
            {
                case { StatusCode: StatusCode.OK }:
                    context.Location().LogInformation("Asigning agentId={agentId} to workId={workId}", agentId, workId);

                    _state.State.MoveToAssigned(workId);
                    await _state.WriteStateAsync();
                    return assignedOption;

                case { StatusCode: StatusCode.Conflict }:
                    context.Location().LogInformation("Updating index with active work item, workId={workId}", workId);

                    _state.State.MoveToAssigned(workId);
                    await _state.WriteStateAsync();
                    continue;

                default:
                    context.Location().LogError("Removing index that was completed or not found, workId={workId}", workId);

                    _state.State.RemoveAllReferences(workId);
                    await _state.WriteStateAsync();
                    continue;
            }
        }
    }

    public async Task<Option> Clear(string principalId, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        if (!ResourceId.IsValid(principalId, ResourceType.Principal)) return StatusCode.BadRequest;

        context.Location().LogInformation("Clear queue, actorKey={actorKey}", this.GetPrimaryKeyString());

        if (!_state.RecordExists) return StatusCode.OK;

        var save = _state.State;

        _state.State = new SchedulesModel();
        await _state.WriteStateAsync();

        var items = save.ActiveItems.Values.Select(x => x.WorkId)
            .Concat(save.AssignedItems.Values.Select(x => x.WorkId))
            .Concat(save.CompletedItems.Values.Select(x => x.WorkId))
            .ToArray();

        foreach (var workId in items)
        {
            var removeOption = await _clusterClient
                .GetResourceGrain<IScheduleWorkActor>(workId)
                .Delete(context.TraceId);

            if (removeOption.IsOk()) context.Location().LogInformation("Cleared worked item workId={workId}", workId);
        }

        return StatusCode.OK;
    }

    public async Task<Option> InternalCompleted(AssignedCompleted model, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        if (!model.Validate(out var v)) return v;
        if (!_state.RecordExists) return StatusCode.NotFound;

        context.Location().LogInformation("Marking work completed, model={model}", model);
        _state.State.MoveToCompleted(model);

        await _state.WriteStateAsync();
        return StatusCode.OK;
    }

    public Task<Option<SchedulesModel>> GetSchedules(string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        if (!_state.RecordExists) return new SchedulesModel().ToOption().ToTaskResult();

        return _state.State.ToOption().ToTaskResult();
    }

    public async Task<Option> ResetWork(string workId, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        if (!ResourceId.IsValid(workId, ResourceType.System, SpinConstants.Schema.ScheduleWork)) return (StatusCode.BadRequest, "Invalid workId");
        if (!_state.RecordExists) return StatusCode.NotFound;

        context.Location().LogInformation("Reset work assignment, workId={workId}", workId);
        if (_state.State.ResetToActive(workId)) await _state.WriteStateAsync();

        return StatusCode.OK;
    }
}

