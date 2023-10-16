using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
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

    public static async Task<Option> AddSchedule(this IDirectoryActor client, string workId, string? tags, string traceId)
    {
        var nodeRequest = new DirectoryNode
        {
            Key = workId,
            Tags = tags,
        };

        var addNodeOption = await client.AddNode(nodeRequest, traceId);
        if (addNodeOption.IsError()) return addNodeOption;

        var edgeRequest = new DirectoryEdge
        {
            FromKey = SpinConstants.Dir.ScheduleWork,
            ToKey = workId,
            EdgeType = ScheduleEdgeType.Active.GetEdgeType(),
        };

        var addResult = await client.AddEdge(edgeRequest, traceId);
        return addResult;
    }

    public static async Task<Option> ChangeScheduleState(this IDirectoryActor client, string workId, ScheduleEdgeType state, string traceId)
    {
        var request = new DirectoryEdgeUpdate
        {
            FromKey = SpinConstants.Dir.ScheduleWork,
            ToKey = workId,
            MatchEdgeType = _edgeTypePrefix + "*",
            UpdateEdgeType = state.GetEdgeType(),
        };

        var result = await client.Update(request, traceId);
        return result;
    }

    public static string GetEdgeType(this ScheduleEdgeType subject) => subject switch
    {
        ScheduleEdgeType.None => throw new ArgumentException("ScheduleEdgeNodeType.None is not valid"),
        var v => $"{_edgeTypePrefix}{v}",
    };

    public static async Task<Option<DirectoryResponse>> GetSchedules(this IDirectoryActor client, string traceId)
    {
        var request = new DirectoryQuery
        {
            FromKey = SpinConstants.Dir.ScheduleWork,
            MatchEdgeType = _edgeTypePrefix + "*",
        };

        Option<DirectoryResponse> result = await client.Query(request, traceId);
        if (result.IsError()) return result;

        return result;
    }

    public static async Task<Option<DirectoryResponse>> GetSchedules(this IDirectoryActor client, ScheduleEdgeType state, string traceId)
    {
        var request = new DirectoryQuery
        {
            FromKey = SpinConstants.Dir.ScheduleWork,
            MatchEdgeType = state.GetEdgeType(),
        };

        Option<DirectoryResponse> result = await client.Query(request, traceId);
        if (result.IsError()) return result;

        return result;
    }
    public static async Task<Option<IReadOnlyList<DirectoryEdge>>> GetActiveWorkSchedules(this IDirectoryActor client, string traceId)
    {
        var request = new DirectoryQuery
        {
            FromKey = SpinConstants.Dir.ScheduleWork,
            MatchEdgeType = _edgeTypePrefix + "*",
        };

        Option<DirectoryResponse> result = await client.Query(request, traceId);
        if (result.IsError()) return result.ToOptionStatus<IReadOnlyList<DirectoryEdge>>();

        return result.Return().Edges.ToOption();
    }

    public static async Task<Option> RemoveSchedule(this IDirectoryActor client, string workId, string traceId)
    {
        var query = new DirectoryQuery { NodeKey = workId };

        Option<DirectoryResponse> option = await client.Remove(query, traceId);
        return option.ToOptionStatus();
    }
}
