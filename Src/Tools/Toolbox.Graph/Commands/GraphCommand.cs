using System.Diagnostics;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public static class GraphCommand
{
    public static Option<GraphQueryResults> Execute(GraphMap map, string graphQuery, ScopeContext context)
    {
        var graphContext = new GraphContext(map, context);
        return Execute(graphContext, graphQuery).Result;
    }

    public static async Task<Option<GraphQueryResults>> Execute(GraphContext graphContext, string graphQuery)
    {
        graphContext.NotNull();

        Option<IReadOnlyList<IGraphQL>> result = GraphLang.Parse(graphQuery);
        if (result.IsError()) return result.ToOptionStatus<GraphQueryResults>();
        IReadOnlyList<IGraphQL> commands = result.Return();

        var results = new Sequence<GraphQueryResult>();

        bool write = commands.Any(x => x is GraphNodeAdd || x is GraphEdgeAdd || x is GraphEdgeUpdate || x is GraphNodeUpdate || x is GraphEdgeDelete || x is GraphNodeDelete);

        using (var release = write ? (await graphContext.Map.ReadWriterLock.WriterLockAsync()) : (await graphContext.Map.ReadWriterLock.ReaderLockAsync()))
        {
            foreach (var cmd in commands)
            {
                GraphQueryResult qResult = cmd switch
                {
                    GraphNodeAdd addNode => AddNode(addNode, graphContext),
                    GraphEdgeAdd addEdge => AddEdge(addEdge, graphContext),
                    GraphEdgeUpdate updateEdge => UpdateEdge(updateEdge, graphContext),
                    GraphNodeUpdate updateNode => UpdateNode(updateNode, graphContext),
                    GraphEdgeDelete deleteEdge => DeleteEdge(deleteEdge, graphContext),
                    GraphNodeDelete deleteNode => await DeleteNode(deleteNode, graphContext),
                    GraphSelect select => Select(select, graphContext),

                    _ => throw new UnreachableException(),
                };

                results += qResult;

                if (qResult.Status.IsError())
                {
                    graphContext.Context.Location().LogError("Graph batch failed - rolling back: query={graphQuery}, error={error}", graphQuery, qResult.ToString());
                    graphContext.ChangeLog.Rollback();
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

    private static GraphQueryResult AddNode(GraphNodeAdd addNode, GraphContext graphContext)
    {
        var tags = addNode.Upsert ? addNode.Tags : addNode.Tags.RemoveCommands();

        var graphNode = new GraphNode(addNode.Key, tags, addNode.Links);

        Option result = addNode.Upsert switch
        {
            false => graphContext.Map.Nodes.Add(graphNode, graphContext),
            true => graphContext.Map.Nodes.Set(graphNode, graphContext),
        };

        return new GraphQueryResult(CommandType.AddNode, result);
    }

    private static GraphQueryResult AddEdge(GraphEdgeAdd addEdge, GraphContext graphContext)
    {
        var tags = addEdge.Upsert ? addEdge.Tags : addEdge.Tags.RemoveCommands();

        var graphEdge = new GraphEdge(
            key: Guid.NewGuid(),
            fromKey: addEdge.FromKey,
            toKey: addEdge.ToKey,
            edgeType: addEdge.EdgeType ?? "default",
            tags: tags,
            createdDate: DateTime.UtcNow
            );

        var result = addEdge.Upsert switch
        {
            false => graphContext.Map.Edges.Add(graphEdge, unique: addEdge.Unique, graphContext),
            true => graphContext.Map.Edges.Set(graphEdge, unique: addEdge.Unique, graphContext),
        };

        return new GraphQueryResult(CommandType.AddEdge, result);
    }

    private static GraphQueryResult UpdateEdge(GraphEdgeUpdate updateEdge, GraphContext graphContext)
    {
        GraphQueryResult searchResult = GraphQuery.Process(graphContext.Map, updateEdge.Search);

        IReadOnlyList<GraphEdge> edges = searchResult.Edges();
        if (edges.Count == 0) return new GraphQueryResult(CommandType.UpdateEdge, StatusCode.NoContent);

        graphContext.Map.Edges.Update(edges, x => x.With(updateEdge.EdgeType, updateEdge.Tags), graphContext);

        return searchResult with { CommandType = CommandType.UpdateEdge };
    }

    private static GraphQueryResult UpdateNode(GraphNodeUpdate updateNode, GraphContext graphContext)
    {
        var searchResult = GraphQuery.Process(graphContext.Map, updateNode.Search);

        IReadOnlyList<GraphNode> nodes = searchResult.Nodes();
        if (nodes.Count == 0) return new GraphQueryResult(CommandType.UpdateNode, StatusCode.NoContent);

        graphContext.Map.Nodes.Update(nodes, x => x.With(updateNode.Tags, updateNode.Links), graphContext);

        return searchResult with { CommandType = CommandType.UpdateNode };
    }

    private static GraphQueryResult DeleteEdge(GraphEdgeDelete deleteEdge, GraphContext graphContext)
    {
        var searchResult = GraphQuery.Process(graphContext.Map, deleteEdge.Search);

        IReadOnlyList<GraphEdge> edges = searchResult.Edges();
        if (edges.Count == 0) return new GraphQueryResult(CommandType.DeleteEdge, StatusCode.NoContent);

        edges.ForEach(x => graphContext.Map.Edges.Remove(x.Key, graphContext));
        return searchResult with { CommandType = CommandType.DeleteEdge };
    }

    private static async Task<GraphQueryResult> DeleteNode(GraphNodeDelete deleteNode, GraphContext graphContext)
    {
        var searchResult = GraphQuery.Process(graphContext.Map, deleteNode.Search);

        IReadOnlyList<GraphNode> nodes = searchResult.Nodes();
        if (nodes.Count == 0) return new GraphQueryResult(CommandType.DeleteNode, StatusCode.NoContent);

        if (!deleteNode.Force)
        {
            if (await CustomLinkCount(nodes, graphContext) > 0) return new GraphQueryResult(CommandType.DeleteNode, (StatusCode.Conflict, "NodeKey has attached file(s)"));
        }

        await DeleteLinks(nodes, graphContext);

        nodes.ForEach(x => graphContext.Map.Nodes.Remove(x.Key, graphContext));
        var result = searchResult with { CommandType = CommandType.DeleteNode };
        return result;
    }

    private static GraphQueryResult Select(GraphSelect select, GraphContext graphContext)
    {
        GraphQueryResult searchResult = GraphQuery.Process(graphContext.Map, select.Search);
        return searchResult with { CommandType = CommandType.Select };
    }

    private static async Task<int> CustomLinkCount(IReadOnlyList<GraphNode> nodes, GraphContext graphContext)
    {
        if (graphContext.Store == null) return 0;

        var links = nodes
            .SelectMany(x => x.Links)
            .Where(x => GraphTool.GetFileIdName(x).Func(x => x.IsOk() && x.Return() != GraphConstants.EntityName));

        int count = 0;
        foreach (var fileId in links)
        {
            var existOption = await graphContext.Store.Exist(fileId, graphContext.Context);
            if (existOption.IsOk()) count++;
        }

        return count;
    }

    private static async Task DeleteLinks(IReadOnlyList<GraphNode> nodes, GraphContext graphContext)
    {
        if (graphContext.Store == null) return;

        var linksToDelete = nodes.SelectMany(x => x.Links);
        foreach (var fileId in linksToDelete)
        {
            var existOption = await graphContext.Store.Delete(fileId, graphContext.Context);
            existOption.LogStatus(graphContext.Context.Location(), "Deleted link={fileId}", fileId);
        }
    }
}
