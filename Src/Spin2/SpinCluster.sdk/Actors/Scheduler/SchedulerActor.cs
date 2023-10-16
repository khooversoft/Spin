using Microsoft.Extensions.Logging;
using Orleans.Concurrency;
using SpinCluster.sdk.Actors.Agent;
using SpinCluster.sdk.Actors.Contract;
using SpinCluster.sdk.Actors.Directory;
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
    Task<Option<WorkAssignedModel>> AssignWork(string agentId, string traceId);
    Task<Option> Clear(string principalId, string traceId);
    Task<Option> CreateSchedule(ScheduleCreateModel work, string traceId);
    Task<Option<SchedulesResponseModel>> GetSchedules(string traceId);
}

// Actor key = "system:schedule
[StatelessWorker]
public class SchedulerActor : Grain, ISchedulerActor
{
    private readonly IClusterClient _clusterClient;
    private readonly ILogger<SchedulerActor> _logger;

    public SchedulerActor(
        IClusterClient clusterClient,
        ILogger<SchedulerActor> logger
        )
    {
        _clusterClient = clusterClient.NotNull();
        _logger = logger.NotNull();
    }

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        this.GetPrimaryKeyString().Assert(x => x == SpinConstants.SchedulerActoryKey, x => $"Actor key {x} is invalid, must match {SpinConstants.SchedulerActoryKey}");

        return base.OnActivateAsync(cancellationToken);
    }

    public async Task<Option<WorkAssignedModel>> AssignWork(string agentId, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Get work, actorKey={actorKey}", this.GetPrimaryKeyString());

        // Verify agent is registered and active
        Option agentLookup = await _clusterClient.GetResourceGrain<IAgentActor>(agentId).IsActive(context.TraceId);
        if (agentLookup.IsError()) return new Option<WorkAssignedModel>(agentLookup.StatusCode, $"agentId={agentId}, " + agentLookup.Error);

        // Query directory for active schedules
        var searchOption = await _clusterClient.GetDirectoryActor().GetActiveWorkSchedules(context.TraceId);
        if (searchOption.IsError()) return searchOption.ToOptionStatus<WorkAssignedModel>();

        IReadOnlyList<DirectoryEdge> activeWork = searchOption.Return();
        if (activeWork.Count == 0) return StatusCode.NotFound;

        var stack = activeWork
            .OrderByDescending(x => x.CreatedDate)
            .Select(x => x.ToKey)
            .ToStack();

        while (stack.TryPop(out var workId))
        {
            var assignedOption = await _clusterClient
                .GetResourceGrain<IScheduleWorkActor>(workId)
                .Assign(agentId, context.TraceId);

            switch (assignedOption)
            {
                case { StatusCode: StatusCode.OK }:
                    context.Location().LogInformation("Asigning agentId={agentId} to workId={workId}", agentId, workId);
                    return assignedOption;

                case { StatusCode: StatusCode.Conflict }:
                    continue;

                case { StatusCode: StatusCode.NotFound}:
                    context.Location().LogError("Removing index that not found, workId={workId}", workId);
                    await _clusterClient.GetDirectoryActor().RemoveSchedule(workId, context.TraceId);
                    continue;

                default:
                    context.Location().LogError("Failed to get work schedule, workId={workId}", workId);
                    continue;
            }
        }

        return StatusCode.NotFound;
    }

    public async Task<Option> Clear(string principalId, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        if (!ResourceId.IsValid(principalId, ResourceType.Principal)) return StatusCode.BadRequest;

        context.Location().LogInformation("Clear queue, actorKey={actorKey}", this.GetPrimaryKeyString());

        Option<DirectoryResponse> dirResponse = await _clusterClient.GetDirectoryActor().GetSchedules(traceId);
        if (dirResponse.IsError()) return dirResponse.ToOptionStatus();

        DirectoryResponse response = dirResponse.Return();

        foreach (var item in response.Edges)
        {
            var result = await _clusterClient.GetResourceGrain<IScheduleWorkActor>(item.ToKey).Delete(context.TraceId);
            if (result.IsError())
            {
                context.Location().LogStatus(result, "Deleting work schedule");
            }
        }

        return StatusCode.OK;
    }

    public async Task<Option> CreateSchedule(ScheduleCreateModel work, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        if (!work.Validate(out var v)) return v;
        context.Location().LogInformation("Schedule work, actorKey={actorKey}, work={work}", this.GetPrimaryKeyString(), work);

        var createOption = await _clusterClient
            .GetResourceGrain<IScheduleWorkActor>(work.WorkId)
            .Create(work, context.TraceId);

        return createOption;
    }

    public async Task<Option<SchedulesResponseModel>> GetSchedules(string traceId)
    {
        var context = new ScopeContext(traceId, _logger);

        Option<DirectoryResponse> dirResponse = await _clusterClient.GetDirectoryActor().GetSchedules(traceId);
        if (dirResponse.IsError()) return dirResponse.ToOptionStatus<SchedulesResponseModel>();

        DirectoryResponse response = dirResponse.Return();

        var workScheduleList = new List<(string edgeType, ScheduleWorkModel model)>();
        foreach (var item in response.Edges)
        {
            var getOption = await _clusterClient.GetResourceGrain<IScheduleWorkActor>(item.ToKey).Get(traceId);
            if (getOption.IsError())
            {
                context.Location().LogError("Cannot find workId={workId} in directory", item.ToKey);
                continue;
            }

            workScheduleList.Add((item.EdgeType, getOption.Return()));
        }

        var result = new SchedulesResponseModel
        {
            ActiveItems = workScheduleList
                .Where(x => x.edgeType == ScheduleEdgeType.Active.GetEdgeType())
                .ToDictionary(x => x.model.WorkId, x => x.model.ConvertTo(x.edgeType)),

            CompletedItems = workScheduleList
                .Where(x => x.edgeType == ScheduleEdgeType.Completed.GetEdgeType())
                .ToDictionary(x => x.model.WorkId, x => x.model.ConvertTo(x.edgeType)),
        };

        return result;
    }
}

