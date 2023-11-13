using SpinCluster.sdk.Actors.Directory;
using SpinCluster.sdk.Application;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.ScheduleWork;


public static class ScheduleWorkExtension
{
    private const string _edgeTypePrefix = "scheduleWorkType:";

    public static async Task<Option> AddSchedule(this IDirectoryActor directoryActor, string workId, string? tags, string traceId)
    {
        string command = new Sequence<string>()
            .Add($"add node key={SpinConstants.Dir.ScheduleWork}")
            .Add($"add node key={workId}{(tags != null ? $",tags={tags}" : string.Empty)}")
            .Add($"add edge fromKey={SpinConstants.Dir.ScheduleWork},toKey={workId},edgeType={ScheduleEdgeType.Active.GetEdgeType()}")
            .Join(';') + ';';

        //string command2 = new GraphCmds()
        //    .Add($"add node key={SpinConstants.Dir.ScheduleWork}")
        //    .Add($"add node key={workId}{(tags != null ? $",tags={tags}" : string.Empty)}")
        //    .Add($"add edge fromKey={SpinConstants.Dir.ScheduleWork},toKey={workId},edgeType={ScheduleEdgeType.Active.GetEdgeType()}")
        //    .Build();

        var addResult = await directoryActor.Execute(command, traceId);
        return addResult.ToOptionStatus();


        //cmds += GraphCmd.Builder()
        //    .AddNode(
        //cmds += $"add node key={workId}{(tags != null ? $", tags={tags}" : null)}";
        //var nodeRequest = new GraphNode
        //{
        //    Key = workId,
        //    Tags = new Tags(tags),
        //};

        //// make sure the schedule root node is there
        //await directoryActor.AddNode(new GraphNode { Key = SpinConstants.Dir.ScheduleWork }, traceId);

        //var addNodeOption = await directoryActor.AddNode(nodeRequest, traceId);
        //if (addNodeOption.IsError()) return addNodeOption;

        //var edgeRequest = new GraphEdge
        //{
        //    FromKey = SpinConstants.Dir.ScheduleWork,
        //    ToKey = workId,
        //    EdgeType = ScheduleEdgeType.Active.GetEdgeType(),
        //};

        //var addResult = await directoryActor.AddEdge(edgeRequest, traceId);
        //return addResult;
    }

    public static async Task<Option> ChangeScheduleState(this IDirectoryActor directoryActor, string workId, ScheduleEdgeType state, string traceId)
    {
        string search = $"[fromKey={SpinConstants.Dir.ScheduleWork};toKey={workId};edgeType={_edgeTypePrefix}*]";
        string command = $"update {search} set edgeType={state.GetEdgeType()};";

        var updateResult = await directoryActor.Execute(command, traceId);
        return updateResult.ToOptionStatus();

        //var request = new DirectoryEdgeUpdate
        //{
        //    FromKey = SpinConstants.Dir.ScheduleWork,
        //    ToKey = workId,
        //    MatchEdgeType = _edgeTypePrefix + "*",
        //    UpdateEdgeType = state.GetEdgeType(),
        //};

        //var result = await directoryActor.Update(request, traceId);
        //return result;
    }

    public static async Task<Option<IReadOnlyList<GraphEdge>>> GetSchedules(this IDirectoryActor directoryActor, string traceId)
    {
        string command = $"select [fromKey={SpinConstants.Dir.ScheduleWork};edgeType={_edgeTypePrefix}*];";

        Option<GraphCommandResults> updateResult = await directoryActor.Execute(command, traceId);
        if (updateResult.IsError()) return updateResult.ToOptionStatus<IReadOnlyList<GraphEdge>>();

        var graphCmdResult = updateResult.Return();
        if (graphCmdResult.Items.Count != 1) return (StatusCode.BadRequest, $"Returned items count={graphCmdResult.Items.Count}");

        if (graphCmdResult.Items[0].StatusCode == StatusCode.NoContent) return Array.Empty<GraphEdge>();
        return updateResult.Return().Items[0].NotNull().SearchResult.NotNull().Edges().ToOption();

        //var request = new DirectoryCommand
        //{
        //    Command = $"(key={SpinConstants.Dir.ScheduleWork})->[edgeType={_edgeTypePrefix + "*"}]",
        //};

        //Option<GraphQueryResult> result = await directoryActor.Query(request, traceId);
        //if (result.IsError()) return result;

        //return result;
    }

    public static async Task<Option<IReadOnlyList<GraphEdge>>> GetActiveWorkSchedules(this IDirectoryActor directoryActor, string traceId)
    {
        string command = $"select [fromKey={SpinConstants.Dir.ScheduleWork};edgeType={_edgeTypePrefix}*];";

        Option<GraphCommandResults> updateResult = await directoryActor.Execute(command, traceId);
        if (updateResult.IsError()) return updateResult.ToOptionStatus<IReadOnlyList<GraphEdge>>();

        GraphCommandResults result = updateResult.Return();
        if (result.Items.Count != 1) return (StatusCode.BadRequest, "No result");

        return result.Items[0].SearchResult.NotNull().Edges().ToOption();


        //var request = new DirectoryCommand
        //{
        //    Command = $"(key={SpinConstants.Dir.ScheduleWork})->[edgeType={_edgeTypePrefix + "*"}]",
        //};

        //Option<GraphQueryResult> result = await directoryActor.Query(request, traceId);
        //if (result.IsError()) return result.ToOptionStatus<IReadOnlyList<GraphEdge>>();

        //IReadOnlyList<GraphEdge> data = result.Return().Items.OfType<GraphEdge>().ToArray();

        //return data.ToOption();
    }

    public static async Task<Option> RemoveSchedule(this IDirectoryActor directoryActor, string workId, string traceId)
    {
        string command = $"delete (key={workId});";

        Option<GraphCommandResults> updateResult = await directoryActor.Execute(command, traceId);
        return updateResult.ToOptionStatus();

        //var query = new DirectoryCommand { Command = $"(key={workId})" };

        //Option<GraphQueryResult> option = await directoryActor.Remove(query, traceId);
        //return option.ToOptionStatus();
    }
}
