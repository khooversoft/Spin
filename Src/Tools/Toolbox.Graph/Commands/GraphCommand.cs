using System.Diagnostics;
using Toolbox.Extensions;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public static class GraphCommand
{
    public static Option<GraphQueryResults> Execute(GraphMap map, string graphQuery, ScopeContext context)
    {
        return Execute(map, graphQuery, null, context).Result;
    }

    public static async Task<Option<GraphQueryResults>> Execute(GraphMap map, string graphQuery, IFileStore? graphStore, ScopeContext context)
    {
        map.NotNull();

        Option<IReadOnlyList<IGraphQL>> result = GraphLang.Parse(graphQuery);
        if (result.IsError()) return result.ToOptionStatus<GraphQueryResults>();
        IReadOnlyList<IGraphQL> commands = result.Return();

        var results = new Sequence<GraphQueryResult>();
        var changeContext = new GraphChangeContext(map, graphStore, context);

        bool write = commands.Any(x => x is GraphNodeAdd || x is GraphEdgeAdd || x is GraphEdgeUpdate || x is GraphNodeUpdate || x is GraphEdgeDelete || x is GraphNodeDelete);

        using (var release = write ? (await map.ReadWriterLock.WriterLockAsync()) : (await map.ReadWriterLock.ReaderLockAsync()))
        {
            foreach (var cmd in commands)
            {
                GraphQueryResult qResult = cmd switch
                {
                    GraphNodeAdd addNode => AddNode(addNode, changeContext),
                    GraphEdgeAdd addEdge => AddEdge(addEdge, changeContext),
                    GraphEdgeUpdate updateEdge => UpdateEdge(updateEdge, changeContext),
                    GraphNodeUpdate updateNode => UpdateNode(updateNode, changeContext),
                    GraphEdgeDelete deleteEdge => DeleteEdge(deleteEdge, changeContext),
                    GraphNodeDelete deleteNode => await DeleteNode(deleteNode, changeContext),
                    GraphSelect select => Select(select, changeContext),

                    _ => throw new UnreachableException(),
                };

                results += qResult;

                if (qResult.Status.IsError())
                {
                    context.Location().LogError("Graph batch failed - rolling back: query={graphQuery}, error={error}", graphQuery, qResult.ToString());
                    changeContext.ChangeLog.Rollback();
                    break;
                }
            }
        }

        Option option = results switch
        {
            { Count: 0 } => StatusCode.OK,
            var v when v.Last().Status.IsError() => v.Last().Status,
            _ => StatusCode.OK,
        };

        var mapResult = new GraphQueryResults
        {
            Items = results,
        };

        return new Option<GraphQueryResults>(mapResult, option.StatusCode, option.Error);
    }

    private static GraphQueryResult AddNode(GraphNodeAdd addNode, GraphChangeContext graphContext)
    {
        var graphNode = new GraphNode(addNode.Key, addNode.Tags);

        Option result = addNode.Upsert switch
        {
            false => graphContext.Map.Nodes.Add(graphNode, graphContext),
            true => graphContext.Map.Nodes.Set(graphNode, graphContext),
        };

        return new GraphQueryResult(CommandType.AddNode, result);
    }

    private static GraphQueryResult AddEdge(GraphEdgeAdd addEdge, GraphChangeContext graphContext)
    {
        var graphEdge = new GraphEdge
        {
            FromKey = addEdge.FromKey,
            ToKey = addEdge.ToKey,
            EdgeType = addEdge.EdgeType ?? "default",
            Tags = new Tags(addEdge.Tags),
        };

        var result = addEdge.Upsert switch
        {
            false => graphContext.Map.Edges.Add(graphEdge, unique: addEdge.Unique, graphContext),
            true => graphContext.Map.Edges.Set(graphEdge, unique: addEdge.Unique, graphContext),
        };

        return new GraphQueryResult(CommandType.AddEdge, result);
    }

    private static GraphQueryResult UpdateEdge(GraphEdgeUpdate updateEdge, GraphChangeContext graphContext)
    {
        GraphQueryResult searchResult = GraphQuery.Process(graphContext.Map, updateEdge.Search);

        IReadOnlyList<GraphEdge> edges = searchResult.Edges();
        if (edges.Count == 0) return new GraphQueryResult(CommandType.UpdateEdge, StatusCode.NoContent);

        graphContext.Map.Edges.Update(edges, x => x with
        {
            EdgeType = updateEdge.EdgeType ?? x.EdgeType,
            Tags = x.Tags.Set(updateEdge.Tags.ToString()),
        }, graphContext);

        return searchResult with { CommandType = CommandType.UpdateEdge };
    }

    private static GraphQueryResult UpdateNode(GraphNodeUpdate updateNode, GraphChangeContext graphContext)
    {
        var searchResult = GraphQuery.Process(graphContext.Map, updateNode.Search);

        IReadOnlyList<GraphNode> nodes = searchResult.Nodes();
        if (nodes.Count == 0) return new GraphQueryResult(CommandType.UpdateNode, StatusCode.NoContent);

        graphContext.Map.Nodes.Update(nodes, x => x.WithTags(updateNode.Tags.ToString()), graphContext);

        return searchResult with { CommandType = CommandType.UpdateNode };
    }

    private static GraphQueryResult DeleteEdge(GraphEdgeDelete deleteEdge, GraphChangeContext graphContext)
    {
        var searchResult = GraphQuery.Process(graphContext.Map, deleteEdge.Search);

        IReadOnlyList<GraphEdge> edges = searchResult.Edges();
        if (edges.Count == 0) return new GraphQueryResult(CommandType.DeleteEdge, StatusCode.NoContent);

        edges.ForEach(x => graphContext.Map.Edges.Remove(x.Key, graphContext));
        return searchResult with { CommandType = CommandType.DeleteEdge };
    }

    private static Task<GraphQueryResult> DeleteNode(GraphNodeDelete deleteNode, GraphChangeContext graphContext)
    {
        var searchResult = GraphQuery.Process(graphContext.Map, deleteNode.Search);

        IReadOnlyList<GraphNode> nodes = searchResult.Nodes();
        if (nodes.Count == 0) return new GraphQueryResult(CommandType.DeleteNode, StatusCode.NoContent).ToTaskResult();

        //if (graphContext.Store != null)
        //{
        //    var allFileIds = nodes.SelectMany(x => x.FileIds);
        //    foreach (var fileId in allFileIds)
        //    {
        //        var existOption = (await graphContext.Store.Exist(fileId, graphContext.Context)).NotNull();
        //        if (existOption.IsOk()) return new GraphQueryResult(CommandType.DeleteNode, (StatusCode.Conflict, $"NodeKey has attached file {fileId}"));
        //    }
        //}

        nodes.ForEach(x => graphContext.Map.Nodes.Remove(x.Key, graphContext));
        var result = searchResult with { CommandType = CommandType.DeleteNode };
        return result.ToTaskResult<GraphQueryResult>();
    }

    private static GraphQueryResult Select(GraphSelect select, GraphChangeContext graphContext)
    {
        GraphQueryResult searchResult = GraphQuery.Process(graphContext.Map, select.Search);
        return searchResult with { CommandType = CommandType.Select };
    }
}
