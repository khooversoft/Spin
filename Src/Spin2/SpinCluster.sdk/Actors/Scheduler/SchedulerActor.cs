using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Actors.Agent;
using SpinCluster.sdk.Actors.Contract;
using SpinCluster.sdk.Actors.Directory;
using SpinCluster.sdk.Actors.Scheduler;
using SpinCluster.sdk.Actors.ScheduleWork;
using SpinCluster.sdk.Actors.Smartc;
using SpinCluster.sdk.Application;
using Toolbox.Block;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Scheduler;

// Actor key = "scheduler:{schedulerName}
public class SchedulerActor : Grain, ISchedulerActor
{
    private readonly IClusterClient _clusterClient;
    private readonly ILogger<SchedulerActor> _logger;

    public SchedulerActor(IClusterClient clusterClient, ILogger<SchedulerActor> logger)
    {
        _clusterClient = clusterClient.NotNull();
        _logger = logger.NotNull();
    }

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        this.VerifySchema(ResourceType.System, SpinConstants.Schema.Scheduler, new ScopeContext(_logger).Location());
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
        var searchOption = await GetActiveSchedules(context);
        if (searchOption.IsError()) return searchOption.ToOptionStatus<WorkAssignedModel>();

        IReadOnlyList<GraphEdge> activeWork = searchOption.Return();
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

                case { StatusCode: StatusCode.NotFound }:
                    context.Location().LogError("Removing index that not found, workId={workId}", workId);
                    await RemoveSchedule(workId, context.TraceId);
                    continue;

                default:
                    context.Location().LogError("Failed to get work schedule, workId={workId}", workId);
                    continue;
            }
        }

        return StatusCode.NotFound;
    }

    public async Task<Option> ChangeScheduleState(string workId, ScheduleEdgeType state, string traceId)
    {
        string search = $"[fromKey={this.GetPrimaryKeyString()};toKey={workId};edgeType=scheduleWorkType:*]";
        string command = $"update {search} set edgeType={state.GetEdgeType()};";

        var updateResult = await _clusterClient.GetDirectoryActor().Execute(command, traceId);
        return updateResult.ToOptionStatus();
    }

    public async Task<Option> Clear(string principalId, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        if (!ResourceId.IsValid(principalId, ResourceType.Principal)) return (StatusCode.BadRequest, "Invalid principalId");
        context.Location().LogInformation("Clear queue, actorKey={actorKey}", this.GetPrimaryKeyString());

        Option<IReadOnlyList<GraphEdge>> dirResponse = await GetSchedules(context);
        if (dirResponse.IsError()) return dirResponse.ToOptionStatus();

        IReadOnlyList<GraphEdge> items = dirResponse.Return();
        foreach (var item in items)
        {
            var result = await _clusterClient.GetResourceGrain<IScheduleWorkActor>(item.ToKey).Delete(context.TraceId);
            if (result.IsError())
            {
                context.Location().LogStatus(result, "Deleting work schedule");
            }
        }

        string command = $"delete [fromKey={this.GetPrimaryKeyString()}];";
        var deleteNode = await _clusterClient.GetDirectoryActor().Execute(command, traceId);
        if (deleteNode.IsError())
        {
            context.Location().LogStatus(deleteNode.ToOptionStatus(), "Deleting edge");
        }

        return StatusCode.OK;
    }

    public async Task<Option> CreateSchedule(ScheduleCreateModel work, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        if (!work.Validate(out var v)) return v;
        context.Location().LogInformation("Schedule work, actorKey={actorKey}, work={work}", this.GetPrimaryKeyString(), work);

        var addResult = await AddSchedule(work.WorkId, context);
        if (addResult.IsError()) return addResult;

        var createOption = await _clusterClient
            .GetResourceGrain<IScheduleWorkActor>(work.WorkId)
            .Create(work, context.TraceId);

        return createOption;
    }

    public async Task<Option<SchedulesResponseModel>> GetSchedules(string traceId)
    {
        var context = new ScopeContext(traceId, _logger);

        Option<IReadOnlyList<GraphEdge>> dirResponse = await GetSchedules(context);
        if (dirResponse.IsError()) return dirResponse.ToOptionStatus<SchedulesResponseModel>();

        IReadOnlyList<GraphEdge> items = dirResponse.Return();

        var workScheduleList = new List<(string edgeType, ScheduleWorkModel model)>();
        foreach (var item in items.Where(x => isEdgeType(x.EdgeType)))
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

        static bool isEdgeType(string edgeType) => edgeType switch
        {
            string v when v == ScheduleEdgeType.Active.GetEdgeType() => true,
            string v when v == ScheduleEdgeType.Completed.GetEdgeType() => true,

            _ => false,
        };
    }

    public async Task<Option> RemoveSchedule(string workId, string traceId)
    {
        new ScopeContext(traceId, _logger).Location().LogInformation("Delete workId={workId}", workId);

        string command = $"delete (key={workId});";
        Option<GraphCommandResults> updateResult = await _clusterClient.GetDirectoryActor().Execute(command, traceId);

        return updateResult.ToOptionStatus();
    }

    private async Task<Option> AddSchedule(string workId, ScopeContext context)
    {
        context.Location().LogInformation("Add schedule, workId={workId}", workId);

        string command = new Sequence<string>()
            .Add($"add node key={this.GetPrimaryKeyString()}")
            .Add($"add node key={workId}")
            .Add($"add edge fromKey={this.GetPrimaryKeyString()},toKey={workId},edgeType={ScheduleEdgeType.Active.GetEdgeType()}")
            .Join(';') + ';';

        var addResult = await _clusterClient.GetDirectoryActor().Execute(command, context.TraceId);
        return addResult.ToOptionStatus();
    }

    private async Task<Option<IReadOnlyList<GraphEdge>>> GetSchedules(ScopeContext context)
    {
        string command = $"select [fromKey={this.GetPrimaryKeyString()};edgeType={ScheduleEdgeTypeTool.EdgeTypeSearch}];";

        Option<GraphCommandResults> updateResult = await _clusterClient.GetDirectoryActor().Execute(command, context.TraceId);
        if (updateResult.IsError()) return updateResult.ToOptionStatus<IReadOnlyList<GraphEdge>>();

        var graphCmdResult = updateResult.Return();
        graphCmdResult.Items.Count.Assert(x => x == 1, $"Returned items count={graphCmdResult.Items.Count}");

        if (graphCmdResult.Items[0].StatusCode == StatusCode.NoContent) return Array.Empty<GraphEdge>();
        return updateResult.Return().Items[0].Edges().ToOption();
    }

    private async Task<Option<IReadOnlyList<GraphEdge>>> GetActiveSchedules(ScopeContext context)
    {
        context.Location().LogInformation("Getting active schedules");
        string command = $"select [fromKey={this.GetPrimaryKeyString()};edgeType={ScheduleEdgeType.Active.GetEdgeType()}];";

        Option<GraphCommandResults> updateResult = await _clusterClient.GetDirectoryActor().Execute(command, context.TraceId);
        if (updateResult.IsError()) return updateResult.ToOptionStatus<IReadOnlyList<GraphEdge>>();

        GraphCommandResults result = updateResult.Return();
        result.Items.Count.Assert(x => x == 1, "Multiple data sets was returned, expected only 1");

        return result.Items[0].Edges().OrderBy(x => x.CreatedDate).ToArray();
    }
}

