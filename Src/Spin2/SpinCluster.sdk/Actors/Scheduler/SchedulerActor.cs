﻿using Microsoft.Extensions.Logging;
using Orleans.Concurrency;
using Orleans.Runtime;
using SpinCluster.sdk.Actors.Agent;
using SpinCluster.sdk.Actors.Contract;
using SpinCluster.sdk.Application;
using Toolbox.Block;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Smartc;

public interface ISchedulerActor : IGrainWithStringKey
{
    Task<Option<ScheduleWorkModel>> AssignWork(string agentId, string traceId);
    Task<Option> CompletedWork(string workId, RunResultModel runResult, string traceId);
    Task<Option> EnqueueSchedule(ScheduleWorkModel work, string traceId);
    Task<Option<ScheduleWorkModel>> GetDetail(string workId, string traceId);
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
        this.VerifySchema(SpinConstants.Schema.Scheduler, new ScopeContext(_logger));
        if (_state.RecordExists) Validate();

        return base.OnActivateAsync(cancellationToken);
    }

    public async Task<Option<ScheduleWorkModel>> AssignWork(string agentId, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Get work, actorKey={actorKey}", this.GetPrimaryKeyString());
        if (!_state.RecordExists) return StatusCode.NotFound;

        await CleanQueue(context);

        // Verify agent is registered and active
        Option agentLookup = await _clusterClient.GetResourceGrain<IAgentActor>(agentId).IsActive(context.TraceId);
        if (agentLookup.IsError()) return agentLookup.ToOptionStatus<ScheduleWorkModel>();

        // Is there any work?
        Option<(ScheduleWorkModel Item, int Index)> work = _state.State
            .WorkItems
            .WithIndex()
            .FirstOrDefaultOption(x => x.Item.Assigned == null);

        if (work.IsNoContent()) return StatusCode.NotFound;

        // Assign
        var mod = work.Return().Item with
        {
            Assigned = new AssignedModel
            {
                AgentId = agentId,
            },
        };

        await ValidateAndWrite();
        return StatusCode.OK;
    }

    public async Task<Option> CompletedWork(string workId, RunResultModel runResult, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Complete scheduled work, actorKey={actorKey}, workId={workId}, runResult={runResult}",
            this.GetPrimaryKeyString(), workId, runResult);

        var test = new OptionTest()
            .Test(() => _state.RecordExists ? StatusCode.OK : new Option(StatusCode.NotFound, "No schedules exist"))
            .Test(() => workId.IsNotEmpty() ? StatusCode.OK : new Option(StatusCode.BadRequest, "workId is empty"))
            .Test(runResult.Validate);
        if (test.IsError()) return test.Option.LogResult(context.Location());

        int index = _state.State.WorkItems
            .WithIndex()
            .Where(x => x.Item.WorkId == workId)
            .Select(x => x.Index)
            .FirstOrDefault(-1);

        if (index == -1) return StatusCode.NotFound;

        ScheduleWorkModel removed = _state.State.WorkItems[index];
        _state.State.WorkItems.RemoveAt(index);

        removed = removed with
        {
            RunResult = runResult,
        };

        _state.State.CompletedItems.Add(removed);
        await ValidateAndWrite();

        return StatusCode.OK;
    }

    public async Task<Option> EnqueueSchedule(ScheduleWorkModel work, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Schedule work, actorKey={actorKey}, work={work}", this.GetPrimaryKeyString(), work);

        var v = work.Validate().LogResult(context.Location());
        if (v.IsError()) return v;

        _state.State = _state.RecordExists ? _state.State : new SchedulesModel();
        _state.State.WorkItems.Add(work);

        await ValidateAndWrite();
        return StatusCode.OK;
    }

    public Task<Option<ScheduleWorkModel>> GetDetail(string workId, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        if (!_state.RecordExists) return new Option<ScheduleWorkModel>(StatusCode.NotFound).ToTaskResult();

        ScheduleWorkModel? scheduleWork = _state.State.WorkItems.FirstOrDefault(x => x.WorkId == workId);
        if (scheduleWork == null) return new Option<ScheduleWorkModel>(StatusCode.NotFound).ToTaskResult();

        return new Option<ScheduleWorkModel>(scheduleWork).ToTaskResult();
    }

    public Task<Option<SchedulesModel>> GetSchedules(string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        if (!_state.RecordExists) return new Option<SchedulesModel>(StatusCode.NotFound).ToTaskResult();

        return _state.State.ToOption().ToTaskResult();
    }

    private async Task CleanQueue(ScopeContext context)
    {
        _state.State.Assert(x => x != null, "State is not set");

        var retireList = _state.State.WorkItems
            .Where(x => x.Assigned != null && !x.Assigned.IsValid())
            .ToArray();

        if (retireList.Length == 0) return;

        context.Location().LogInformation("Retiring {number} of work items, workIds={workIds}", retireList.Length, retireList.Select(x => x.WorkId).Join(';'));

        _state.State = _state.State with
        {
            WorkItems = _state.State.WorkItems
                .Where(x => !retireList.Any(y => x.WorkId == y.WorkId))
                .ToList(),
            CompletedItems = _state.State.CompletedItems.Concat(retireList).ToList(),
        };

        await ValidateAndWrite();
    }

    private async Task ValidateAndWrite()
    {
        _state.State.Assert(x => x != null, "State is not set");
        Validate();

        await _state.WriteStateAsync();
    }

    private void Validate()
    {
        var v = _state.State.Validate();
        if (v.IsError()) throw new InvalidOperationException($"ScheduleModel is not valid on read, error={v.Error}");
    }
}

