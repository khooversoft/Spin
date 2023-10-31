using SpinCluster.sdk.Actors.Directory;
using SpinCluster.sdk.Application;
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
        var nodeRequest = new DirectoryNode
        {
            Key = workId,
            Tags = tags,
        };

        // make sure the schedule root node is there
        await directoryActor.AddNode(new DirectoryNode { Key = SpinConstants.Dir.ScheduleWork }, traceId);

        var addNodeOption = await directoryActor.AddNode(nodeRequest, traceId);
        if (addNodeOption.IsError()) return addNodeOption;

        var edgeRequest = new DirectoryEdge
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

    public static async Task<Option<DirectoryResponse>> GetSchedules(this IDirectoryActor directoryActor, string traceId)
    {
        var request = new DirectoryQuery
        {
            FromKey = SpinConstants.Dir.ScheduleWork,
            EdgeType = _edgeTypePrefix + "*",
        };

        Option<DirectoryResponse> result = await directoryActor.Query(request, traceId);
        if (result.IsError()) return result;

        return result;
    }

    public static async Task<Option<DirectoryResponse>> GetSchedules(this IDirectoryActor directoryActor, ScheduleEdgeType state, string traceId)
    {
        var request = new DirectoryQuery
        {
            FromKey = SpinConstants.Dir.ScheduleWork,
            EdgeType = state.GetEdgeType(),
        };

        Option<DirectoryResponse> result = await directoryActor.Query(request, traceId);
        if (result.IsError()) return result;

        return result;
    }

    public static async Task<Option<IReadOnlyList<DirectoryEdge>>> GetActiveWorkSchedules(this IDirectoryActor directoryActor, string traceId)
    {
        var request = new DirectoryQuery
        {
            FromKey = SpinConstants.Dir.ScheduleWork,
            EdgeType = _edgeTypePrefix + "*",
        };

        Option<DirectoryResponse> result = await directoryActor.Query(request, traceId);
        if (result.IsError()) return result.ToOptionStatus<IReadOnlyList<DirectoryEdge>>();

        return result.Return().Edges.ToOption();
    }

    public static async Task<Option> RemoveSchedule(this IDirectoryActor directoryActor, string workId, string traceId)
    {
        var query = new DirectoryQuery { NodeKey = workId };

        Option<DirectoryResponse> option = await directoryActor.Remove(query, traceId);
        return option.ToOptionStatus();
    }
}
