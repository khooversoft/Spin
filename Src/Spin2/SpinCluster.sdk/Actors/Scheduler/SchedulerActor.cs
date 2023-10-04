using Microsoft.Extensions.Logging;
using Orleans.Concurrency;
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
    Task<Option> Completed(AssignedCompleted model, string traceId);
    Task<Option<SchedulesModel>> GetSchedules(string traceId);
}

// Actor key = "system:schedule

[StatelessWorker]
[Reentrant]
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
        if (_state.RecordExists) Validate();

        return base.OnActivateAsync(cancellationToken);
    }

    public async Task<Option> CreateSchedule(ScheduleCreateModel work, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Schedule work, actorKey={actorKey}, work={work}", this.GetPrimaryKeyString(), work);

        if (!work.Validate(out var v)) return v;

        _state.State = _state.RecordExists ? _state.State : new SchedulesModel();

        _state.State.Active.Add(new ScheduleEntry { WorkId = work.WorkId });
        await ValidateAndWrite();

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
            int index = _state.State
                .Active
                .WithIndex()
                .Where(x => x.Item.IsAssignable())
                .Select(x => x.Index)
                .FirstOrDefault(-1);

            if (index == -1) return StatusCode.NotFound;

            var assignedOption = await _clusterClient
                .GetResourceGrain<IScheduleWorkActor>(_state.State.Active[index].WorkId)
                .Assign(agentId, context.TraceId);

            switch (assignedOption)
            {
                case { StatusCode: StatusCode.OK }:
                    break;

                case { StatusCode: StatusCode.Conflict }:
                    _state.State.Active[index] = _state.State.Active[index] with { AssignedDate = DateTime.UtcNow };
                    context.Location().LogInformation("Updating index with active work item, workId={workId}", _state.State.Active[index].WorkId);
                    await ValidateAndWrite();
                    continue;

                default:
                    context.Location().LogError("Removing index that was completed or not found, workId={workId}", _state.State.Active[index].WorkId);
                    _state.State.Active.RemoveAt(index);
                    await ValidateAndWrite();
                    continue;
            }

            context.Location().LogInformation("Asigning agentId={agentId} to workId={workId}", agentId, _state.State.Active[index].WorkId);

            _state.State.Active[index] = _state.State.Active[index] with { AssignedDate = DateTime.UtcNow };

            await ValidateAndWrite();
            return assignedOption;
        }
    }

    public async Task<Option> Clear(string principalId, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Clear queue, actorKey={actorKey}", this.GetPrimaryKeyString());
        if (!ResourceId.IsValid(principalId, ResourceType.Principal)) return StatusCode.BadRequest;

        if (!_state.RecordExists) return StatusCode.OK;

        var save = _state.State;

        _state.State = new SchedulesModel();
        await _state.WriteStateAsync();

        foreach (var item in save.Active)
        {
            var removeOption = await _clusterClient
                .GetResourceGrain<IScheduleWorkActor>(item.WorkId)
                .Delete(context.TraceId);

            if (removeOption.IsOk()) context.Location().LogInformation("Cleared worked item workId={workId}", item.WorkId);
        }

        return StatusCode.OK;
    }

    public async Task<Option> Completed(AssignedCompleted model, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        if (!model.Validate(out var v)) return v;
        if (!_state.RecordExists) return StatusCode.NotFound;
        context.Location().LogInformation("Marking work completed, model={model}", model);

        _state.State.CompletedItems.Add(model);

        int index = _state.State.Active
            .WithIndex()
            .Where(x => x.Item.WorkId == model.WorkId)
            .Select(x => x.Index)
            .FirstOrDefault(-1);

        if (index >= 0)
        {
            _state.State.Active.RemoveAt(index);
        }

        await _state.WriteStateAsync();
        return StatusCode.OK;
    }

    public Task<Option<SchedulesModel>> GetSchedules(string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        if (!_state.RecordExists) return new Option<SchedulesModel>(StatusCode.NotFound).ToTaskResult();

        return _state.State.ToOption().ToTaskResult();
    }

    private async Task ValidateAndWrite()
    {
        _state.State.Assert(x => x != null, "State is not set");
        Validate();

        await _state.WriteStateAsync();
    }

    private void Validate()
    {
        if (!_state.RecordExists) return;

        var v = _state.State.Validate();
        if (v.IsError()) throw new InvalidOperationException($"ScheduleModel is not valid on read, error={v.Error}");
    }
}

