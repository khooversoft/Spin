using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public static class GraphCommand
{
    public static Option<GraphQueryResults> Execute(GraphMap map, string graphQuery, ScopeContext context)
    {
        map.NotNull();

        Option<IReadOnlyList<IGraphQL>> result = GraphLang.Parse(graphQuery);
        if (result.IsError()) return result.ToOptionStatus<GraphQueryResults>();

        IReadOnlyList<IGraphQL> commands = result.Return();
        var results = new Sequence<GraphQueryResult>();

        var changeContext = new GraphChangeContext(map, new ChangeLog(), context);

        lock (map.SyncLock)
        {
            foreach (var cmd in commands)
            {
                switch (cmd)
                {
                    case GraphNodeAdd addNode: results += AddNode(addNode, changeContext); break;
                    case GraphEdgeAdd addEdge: results += AddEdge(addEdge, changeContext); break;
                    case GraphEdgeUpdate updateEdge: results += UpdateEdge(updateEdge, changeContext); break;
                    case GraphNodeUpdate updateNode: results += UpdateNode(updateNode, changeContext); break;
                    case GraphEdgeDelete deleteEdge: results += DeleteEdge(deleteEdge, changeContext); break;
                    case GraphNodeDelete deleteNode: results += DeleteNode(deleteNode, changeContext); break;
                    case GraphSelect select: results += Select(select, changeContext); break;
                }
            }
        }

        Option option = results switch
        {
            var v when !v.All(x => x.StatusCode.IsOk()) => (StatusCode.BadRequest, "One or more results has errors"),
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
        var graphNode = new GraphNode
        {
            Key = addNode.Key,
            Tags = addNode.Tags,
        };

        var result = graphContext.Map.Nodes.Add(graphNode, addNode.Upsert);
        return new GraphQueryResult(CommandType.AddNode, result.StatusCode, result.Error);
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

        var result = graphContext.Map.Edges.Add(graphEdge, upsert: addEdge.Upsert, unique: addEdge.Unique);
        return new GraphQueryResult(CommandType.AddEdge, result.StatusCode, result.Error);
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
        });

        return searchResult with { CommandType = CommandType.UpdateEdge };
    }

    private static GraphQueryResult UpdateNode(GraphNodeUpdate updateNode, GraphChangeContext graphContext)
    {
        var searchResult = GraphQuery.Process(graphContext.Map, updateNode.Search);

        IReadOnlyList<GraphNode> nodes = searchResult.Nodes();
        if (nodes.Count == 0) return new GraphQueryResult(CommandType.UpdateNode, StatusCode.NoContent);

        graphContext.Map.Nodes.Update(nodes, x => x with
        {
            Tags = x.Tags.Set(updateNode.Tags.ToString()),
        });

        return searchResult with { CommandType = CommandType.UpdateNode };
    }

    private static GraphQueryResult DeleteEdge(GraphEdgeDelete deleteEdge, GraphChangeContext graphContext)
    {
        var searchResult = GraphQuery.Process(graphContext.Map, deleteEdge.Search);

        IReadOnlyList<GraphEdge> edges = searchResult.Edges();
        if (edges.Count == 0) return new GraphQueryResult(CommandType.DeleteEdge, StatusCode.NoContent);

        edges.ForEach(x => graphContext.Map.Edges.Remove(x.Key));
        return searchResult with { CommandType = CommandType.DeleteEdge };
    }

    private static GraphQueryResult DeleteNode(GraphNodeDelete deleteNode, GraphChangeContext graphContext)
    {
        var searchResult = GraphQuery.Process(graphContext.Map, deleteNode.Search);

        IReadOnlyList<GraphNode> nodes = searchResult.Nodes();
        if (nodes.Count == 0) return new GraphQueryResult(CommandType.DeleteNode, StatusCode.NoContent);

        nodes.ForEach(x => graphContext.Map.Nodes.Remove(x.Key));
        return searchResult with { CommandType = CommandType.DeleteNode };
    }

    private static GraphQueryResult Select(GraphSelect select, GraphChangeContext graphContext)
    {
        GraphQueryResult searchResult = GraphQuery.Process(graphContext.Map, select.Search);
        return searchResult with { CommandType = CommandType.Select };
    }
}
