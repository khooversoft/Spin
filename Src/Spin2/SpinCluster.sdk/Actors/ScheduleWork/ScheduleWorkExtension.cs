using SpinCluster.sdk.Actors.Directory;
using SpinCluster.sdk.Application;
using Toolbox.Data;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.ScheduleWork;

public enum ScheduleEdgeType
{
    None,
    Active,
    Completed,
    Failed
}


public static class ScheduleWorkExtension
{
    private const string _edgeTypePrefix = "scheduleWorkType:";

    public static async Task<Option> AddSchedule(this IDirectoryActor directoryActor, string workId, string? tags, string traceId)
    {
        var nodeRequest = new GraphNode
        {
            Key = workId,
            Tags = new Tags(tags),
        };

        // make sure the schedule root node is there
        await directoryActor.AddNode(new GraphNode { Key = SpinConstants.Dir.ScheduleWork }, traceId);

        var addNodeOption = await directoryActor.AddNode(nodeRequest, traceId);
        if (addNodeOption.IsError()) return addNodeOption;

        var edgeRequest = new GraphEdge
        {
            FromKey = SpinConstants.Dir.ScheduleWork,
            ToKey = workId,
            EdgeType = ScheduleEdgeType.Active.GetEdgeType(),
        };

        var addResult = await directoryActor.AddEdge(edgeRequest, traceId);
        return addResult;
    }

    public static async Task<Option> ChangeScheduleState(this IDirectoryActor directoryActor, string workId, ScheduleEdgeType state, string traceId)
    {
        var request = new DirectoryEdgeUpdate
        {
            FromKey = SpinConstants.Dir.ScheduleWork,
            ToKey = workId,
            MatchEdgeType = _edgeTypePrefix + "*",
            UpdateEdgeType = state.GetEdgeType(),
        };

        var result = await directoryActor.Update(request, traceId);
        return result;
    }

    public static string GetEdgeType(this ScheduleEdgeType subject) => subject switch
    {
        ScheduleEdgeType.None => throw new ArgumentException("ScheduleEdgeNodeType.None is not valid"),
        var v => $"{_edgeTypePrefix}{v}",
    };

    public static async Task<Option<GraphQueryResult>> GetSchedules(this IDirectoryActor directoryActor, string traceId)
    {
        var request = new DirectoryQuery
        {
            GraphQuery = $"(key={SpinConstants.Dir.ScheduleWork})->[edgeType={_edgeTypePrefix + "*"}]",
        };

        Option<GraphQueryResult> result = await directoryActor.Query(request, traceId);
        if (result.IsError()) return result;

        return result;
    }

    public static async Task<Option<GraphQueryResult>> GetSchedules(this IDirectoryActor directoryActor, ScheduleEdgeType state, string traceId)
    {
        var request = new DirectoryQuery
        {
            GraphQuery = $"(key={SpinConstants.Dir.ScheduleWork})->[edgeType={state.GetEdgeType()}]",
        };

        Option<GraphQueryResult> result = await directoryActor.Query(request, traceId);
        if (result.IsError()) return result;

        return result;
    }

    public static async Task<Option<IReadOnlyList<GraphEdge>>> GetActiveWorkSchedules(this IDirectoryActor directoryActor, string traceId)
    {
        var request = new DirectoryQuery
        {
            GraphQuery = $"(key={SpinConstants.Dir.ScheduleWork})->[edgeType={_edgeTypePrefix + "*"}]",
        };

        Option<GraphQueryResult> result = await directoryActor.Query(request, traceId);
        if (result.IsError()) return result.ToOptionStatus<IReadOnlyList<GraphEdge>>();

        IReadOnlyList<GraphEdge> data = result.Return().Items.OfType<GraphEdge>().ToArray();

        return data.ToOption();
    }

    public static async Task<Option> RemoveSchedule(this IDirectoryActor directoryActor, string workId, string traceId)
    {
        var query = new DirectoryQuery { GraphQuery = $"(key={workId})" };

        Option<GraphQueryResult> option = await directoryActor.Remove(query, traceId);
        return option.ToOptionStatus();
    }
}
