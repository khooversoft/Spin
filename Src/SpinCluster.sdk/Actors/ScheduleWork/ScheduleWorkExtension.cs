//using SpinCluster.sdk.Actors.Directory;
//using SpinCluster.sdk.Application;
//using Toolbox.Data;
//using Toolbox.Extensions;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace SpinCluster.sdk.Actors.ScheduleWork;


//public static class ScheduleWorkExtension
//{
//    private const string _edgeTypePrefix = "scheduleWorkType:";

//    public static async Task<Option> AddSchedule(this IDirectoryActor directoryActor, string workId, string? tags, string traceId)
//    {
//        string command = new Sequence<string>()
//            .Add($"add node key={SpinConstants.Dir.ScheduleWorkQueue}")
//            .Add($"add node key={workId}{TagsFmt(tags)}")
//            .Add($"add edge fromKey={SpinConstants.Dir.ScheduleWorkQueue},toKey={workId},edgeType={ScheduleEdgeType.Active.GetEdgeType()}")
//            .Join(';') + ';';

//        var addResult = await directoryActor.Execute(command, traceId);
//        return addResult.ToOptionStatus();
//    }

//    public static async Task<Option> ChangeScheduleState(this IDirectoryActor directoryActor, string workId, ScheduleEdgeType state, string traceId)
//    {
//        string search = $"[fromKey={SpinConstants.Dir.ScheduleWorkQueue};toKey={workId};edgeType={_edgeTypePrefix}*]";
//        string command = $"update {search} set edgeType={state.GetEdgeType()};";

//        var updateResult = await directoryActor.Execute(command, traceId);
//        return updateResult.ToOptionStatus();
//    }

//    public static async Task<Option<IReadOnlyList<GraphEdge>>> GetSchedules(this IDirectoryActor directoryActor, string traceId)
//    {
//        string command = $"select [fromKey={SpinConstants.Dir.ScheduleWorkQueue};edgeType={_edgeTypePrefix}*];";

//        Option<GraphCommandResults> updateResult = await directoryActor.Execute(command, traceId);
//        if (updateResult.IsError()) return updateResult.ToOptionStatus<IReadOnlyList<GraphEdge>>();

//        var graphCmdResult = updateResult.Return();
//        if (graphCmdResult.Items.Count != 1) return (StatusCode.BadRequest, $"Returned items count={graphCmdResult.Items.Count}");

//        if (graphCmdResult.Items[0].StatusCode == StatusCode.NoContent) return Array.Empty<GraphEdge>();
//        return updateResult.Return().Items[0].NotNull().SearchResult.NotNull().Edges().ToOption();
//    }

//    public static async Task<Option<IReadOnlyList<GraphEdge>>> GetActiveWorkSchedules(this IDirectoryActor directoryActor, string traceId)
//    {
//        string command = $"select [fromKey={SpinConstants.Dir.ScheduleWorkQueue};edgeType={_edgeTypePrefix}*];";

//        Option<GraphCommandResults> updateResult = await directoryActor.Execute(command, traceId);
//        if (updateResult.IsError()) return updateResult.ToOptionStatus<IReadOnlyList<GraphEdge>>();

//        GraphCommandResults result = updateResult.Return();
//        if (result.Items.Count != 1) return (StatusCode.BadRequest, "No result");

//        return result.Items[0].SearchResult.NotNull().Edges().ToOption();
//    }

//    public static async Task<Option> RemoveSchedule(this IDirectoryActor directoryActor, string workId, string traceId)
//    {
//        string command = $"delete (key={workId});";

//        Option<GraphCommandResults> updateResult = await directoryActor.Execute(command, traceId);
//        return updateResult.ToOptionStatus();
//    }

//    private static string TagsFmt(string? tags) => tags != null ? $",tags={tags}" : string.Empty;
//}
