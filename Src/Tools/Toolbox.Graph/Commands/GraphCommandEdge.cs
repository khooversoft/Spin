//using Toolbox.Extensions;
//using Toolbox.Types;

//namespace Toolbox.Graph;

//internal static class GraphCommandEdge
//{
//    public static GraphQueryResult Add(GsEdgeAdd addEdge, IGraphTrxContext graphContext)
//    {
//        var tags = addEdge.Upsert ? addEdge.Tags : addEdge.Tags.RemoveCommands();

//        var graphEdge = new GraphEdge(
//            key: Guid.NewGuid(),
//            fromKey: addEdge.FromKey,
//            toKey: addEdge.ToKey,
//            edgeType: addEdge.EdgeType ?? "default",
//            tags: tags,
//            createdDate: DateTime.UtcNow
//            );

//        var result = addEdge.Upsert switch
//        {
//            false => graphContext.Map.Edges.Add(graphEdge, addEdge.Unique, graphContext),
//            true => graphContext.Map.Edges.Set(graphEdge, addEdge.Unique, graphContext),
//        };

//        return new GraphQueryResult(CommandType.AddEdge, result);
//    }

//    public static GraphQueryResult Update(GsEdgeUpdate updateEdge, IGraphTrxContext graphContext)
//    {
//        GraphQueryResult searchResult = GraphQuery.Process(graphContext.Map, updateEdge.Search);

//        IReadOnlyList<GraphEdge> edges = searchResult.Edges();
//        if (edges.Count == 0) return new GraphQueryResult(CommandType.UpdateEdge, StatusCode.NoContent);

//        graphContext.Map.Edges.Update(edges, x => x with { Tags = TagsTool.ProcessTags(x.Tags, updateEdge.Tags) }, graphContext);

//        return searchResult with { CommandType = CommandType.UpdateEdge };
//    }

//    public static GraphQueryResult Delete(GsEdgeDelete deleteEdge, IGraphTrxContext graphContext)
//    {
//        var searchResult = GraphQuery.Process(graphContext.Map, deleteEdge.Search);

//        IReadOnlyList<GraphEdge> edges = searchResult.Edges();
//        if (edges.Count == 0) return new GraphQueryResult(CommandType.DeleteEdge, StatusCode.NoContent);

//        edges.ForEach(x => graphContext.Map.Edges.Remove(x.Key, graphContext));
//        return searchResult with { CommandType = CommandType.DeleteEdge };
//    }

//}
