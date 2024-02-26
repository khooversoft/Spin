using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Data;

public class GraphCommand
{
    private readonly GraphMap _map;
    private readonly object _syncLock;

    public GraphCommand(GraphMap map, object syncLock)
    {
        _map = map.NotNull();
        _syncLock = syncLock.NotNull();
    }

    public Option<GraphQueryResults> Execute(string graphQuery)
    {
        Option<IReadOnlyList<IGraphQL>> result = GraphLang.Parse(graphQuery);
        if (result.IsError()) return result.ToOptionStatus<GraphQueryResults>();

        IReadOnlyList<IGraphQL> commands = result.Return();
        var results = new Sequence<GraphQueryResult>();

        lock (_syncLock)
        {
            foreach (var cmd in commands)
            {
                switch (cmd)
                {
                    case GraphNodeAdd addNode: results += AddNode(addNode); break;
                    case GraphEdgeAdd addEdge: results += AddEdge(addEdge); break;
                    case GraphEdgeUpdate updateEdge: results += UpdateEdge(updateEdge); break;
                    case GraphNodeUpdate updateNode: results += UpdateNode(updateNode); break;
                    case GraphEdgeDelete deleteEdge: results += DeleteEdge(deleteEdge); break;
                    case GraphNodeDelete deleteNode: results += DeleteNode(deleteNode); break;
                    case GraphSelect select: results += Select(select); break;
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

    private GraphQueryResult AddNode(GraphNodeAdd addNode)
    {
        var graphNode = new GraphNode
        {
            Key = addNode.Key,
            Tags = addNode.Tags,
        };

        var result = _map.Nodes.Add(graphNode);
        return new GraphQueryResult(CommandType.AddNode, result.StatusCode, result.Error);
    }

    private GraphQueryResult AddEdge(GraphEdgeAdd addEdge)
    {
        var graphEdge = new GraphEdge
        {
            FromKey = addEdge.FromKey,
            ToKey = addEdge.ToKey,
            EdgeType = addEdge.EdgeType ?? "default",
            Tags = new Tags(addEdge.Tags),
        };

        var result = _map.Edges.Add(graphEdge);
        return new GraphQueryResult(CommandType.AddEdge, result.StatusCode, result.Error);
    }

    private GraphQueryResult UpdateEdge(GraphEdgeUpdate updateEdge)
    {
        GraphQueryResult searchResult = GraphQuery.Process(_map, updateEdge.Search);

        IReadOnlyList<GraphEdge> edges = searchResult.Edges();
        if (edges.Count == 0) return new GraphQueryResult(CommandType.UpdateEdge, StatusCode.NoContent);

        _map.Edges.Update(edges, x => x with
        {
            EdgeType = updateEdge.EdgeType ?? x.EdgeType,
            Tags = x.Tags.Set(updateEdge.Tags),
        });

        return searchResult with { CommandType = CommandType.UpdateEdge };
    }

    private GraphQueryResult UpdateNode(GraphNodeUpdate updateNode)
    {
        var searchResult = GraphQuery.Process(_map, updateNode.Search);

        IReadOnlyList<GraphNode> nodes = searchResult.Nodes();
        if (nodes.Count == 0) return new GraphQueryResult(CommandType.UpdateNode, StatusCode.NoContent);

        _map.Nodes.Update(nodes, x => x with
        {
            Tags = x.Tags.Set(updateNode.Tags),
        });

        return searchResult with { CommandType = CommandType.UpdateNode };
    }

    private GraphQueryResult DeleteEdge(GraphEdgeDelete deleteEdge)
    {
        var searchResult = GraphQuery.Process(_map, deleteEdge.Search);

        IReadOnlyList<GraphEdge> edges = searchResult.Edges();
        if (edges.Count == 0) return new GraphQueryResult(CommandType.DeleteEdge, StatusCode.NoContent);

        edges.ForEach(x => _map.Edges.Remove(x.Key));
        return searchResult with { CommandType = CommandType.DeleteEdge };
    }

    private GraphQueryResult DeleteNode(GraphNodeDelete deleteNode)
    {
        var searchResult = GraphQuery.Process(_map, deleteNode.Search);

        IReadOnlyList<GraphNode> nodes = searchResult.Nodes();
        if (nodes.Count == 0) return new GraphQueryResult(CommandType.DeleteNode, StatusCode.NoContent);

        nodes.ForEach(x => _map.Nodes.Remove(x.Key));
        return searchResult with { CommandType = CommandType.DeleteNode };
    }

    private GraphQueryResult Select(GraphSelect select)
    {
        GraphQueryResult searchResult = GraphQuery.Process(_map, select.Search);
        return searchResult with { CommandType = CommandType.Select };
    }
}
