using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Actors.Directory;
using SpinCluster.sdk.Actors.ScheduleWork;
using SpinCluster.sdk.Application;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Queue;

public interface IQueueActor
{
}

/// <summary>
/// Handles queued operations, store is the directory graph
/// 
/// ActoryKey= "queue:{queueName}
/// </summary>
public class QueueActor : Grain, IQueueActor
{
    private readonly IClusterClient _clusterClient;
    private readonly ILogger<QueueActor> _logger;
    private const string _edgeTypePrefix = "scheduleWorkType:";

    public QueueActor(
        IClusterClient clusterClient,
        ILogger<QueueActor> logger
        )
    {
        _clusterClient = clusterClient.NotNull();
        _logger = logger.NotNull();
    }

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        this.VerifySchema(ResourceType.System, SpinConstants.Schema.Queue, new ScopeContext(_logger).Location());
        return base.OnActivateAsync(cancellationToken);
    }

    public async Task<Option> AddSchedule(string workId, string? tags, string traceId)
    {
        string command = new Sequence<string>()
            .Add($"add node key={this.GetPrimaryKeyString()}")
            .Add($"add node key={workId}{TagsFmt(tags)}")
            .Add($"add edge fromKey={this.GetPrimaryKeyString()},toKey={workId},edgeType={ScheduleEdgeType.Active.GetEdgeType()}")
            .Join(';') + ';';

        var addResult = await _clusterClient.GetDirectoryActor().Execute(command, traceId);
        return addResult.ToOptionStatus();
    }

    public async Task<Option> Clear(string principalId, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        if (!ResourceId.IsValid(principalId, ResourceType.Principal)) return StatusCode.BadRequest;

        context.Location().LogInformation("Clear queue, actorKey={actorKey}", this.GetPrimaryKeyString());

        Option<IReadOnlyList<GraphEdge>> dirResponse = await GetSchedules(traceId);
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

        return StatusCode.OK;
    }

    public async Task<Option> ChangeScheduleState(string workId, ScheduleEdgeType state, string traceId)
    {
        string search = $"[fromKey={this.GetPrimaryKeyString()};toKey={workId};edgeType={_edgeTypePrefix}*]";
        string command = $"update {search} set edgeType={state.GetEdgeType()};";

        var updateResult = await _clusterClient.GetDirectoryActor().Execute(command, traceId);
        return updateResult.ToOptionStatus();
    }

    public async Task<Option<IReadOnlyList<GraphEdge>>> GetSchedules(string traceId)
    {
        string command = $"select [fromKey={this.GetPrimaryKeyString()};edgeType={_edgeTypePrefix}*];";

        Option<GraphCommandResults> updateResult = await _clusterClient.GetDirectoryActor().Execute(command, traceId);
        if (updateResult.IsError()) return updateResult.ToOptionStatus<IReadOnlyList<GraphEdge>>();

        var graphCmdResult = updateResult.Return();
        if (graphCmdResult.Items.Count != 1) return (StatusCode.BadRequest, $"Returned items count={graphCmdResult.Items.Count}");

        if (graphCmdResult.Items[0].StatusCode == StatusCode.NoContent) return Array.Empty<GraphEdge>();
        return updateResult.Return().Items[0].NotNull().SearchResult.NotNull().Edges().ToOption();
    }

    public async Task<Option<IReadOnlyList<GraphEdge>>> GetActiveWorkSchedules(string traceId)
    {
        string command = $"select [fromKey={this.GetPrimaryKeyString()};edgeType={_edgeTypePrefix}*];";

        Option<GraphCommandResults> updateResult = await _clusterClient.GetDirectoryActor().Execute(command, traceId);
        if (updateResult.IsError()) return updateResult.ToOptionStatus<IReadOnlyList<GraphEdge>>();

        GraphCommandResults result = updateResult.Return();
        if (result.Items.Count != 1) return (StatusCode.BadRequest, "No result");

        return result.Items[0].SearchResult.NotNull().Edges().ToOption();
    }

    public async Task<Option> RemoveSchedule(string workId, string traceId)
    {
        string command = $"delete (key={workId});";

        Option<GraphCommandResults> updateResult = await _clusterClient.GetDirectoryActor().Execute(command, traceId);
        return updateResult.ToOptionStatus();
    }

    private static string TagsFmt(string? tags) => tags != null ? $",tags={tags}" : string.Empty;
}
