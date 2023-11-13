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

    public Option<GraphCommandExceuteResults> Execute(string graphQuery)
    {
        var map = _map.Copy();

        Option<IReadOnlyList<IGraphQL>> result = GraphLang.Parse(graphQuery);
        if (result.IsError()) return result.ToOptionStatus<GraphCommandExceuteResults>();

        IReadOnlyList<IGraphQL> commands = result.Return();
        var results = new Sequence<GraphCommandResult>();

        lock (_syncLock)
        {
            foreach (var cmd in commands)
            {
                switch (cmd)
                {
                    case GraphNodeAdd addNode: results += AddNode(addNode, map); break;
                    case GraphEdgeAdd addEdge: results += AddEdge(addEdge, map); break;
                    case GraphEdgeUpdate updateEdge: results += UpdateEdge(updateEdge, map); break;
                    case GraphNodeUpdate updateNode: results += UpdateNode(updateNode, map); break;
                    case GraphEdgeDelete deleteEdge: results += DeleteEdge(deleteEdge, map); break;
                    case GraphNodeDelete deleteNode: results += DeleteNode(deleteNode, map); break;
                    case GraphSelect select: results += Select(select, map); break;
                }
            }
        }

        return new GraphCommandExceuteResults
        {
            GraphMap = map,
            Items = results,
        };
    }
    private GraphCommandResult AddNode(GraphNodeAdd addNode, GraphMap map)
    {
        var graphNode = new GraphNode
        {
            Key = addNode.Key,
            Tags = addNode.Tags,
        };

        var result = map.Nodes.Add(graphNode);
        return new GraphCommandResult(CommandType.AddNode, result);
    }

    private GraphCommandResult AddEdge(GraphEdgeAdd addEdge, GraphMap map)
    {
        var graphEdge = new GraphEdge
        {
            FromKey = addEdge.FromKey,
            ToKey = addEdge.ToKey,
            EdgeType = addEdge.EdgeType ?? "default",
            Tags = new Tags(addEdge.Tags),
        };

        var result = map.Edges.Add(graphEdge);
        return new GraphCommandResult(CommandType.AddEdge, result);
    }

    private GraphCommandResult UpdateEdge(GraphEdgeUpdate updateEdge, GraphMap map)
    {
        var searchResult = map.Query().Process(updateEdge.Search);

        IReadOnlyList<GraphEdge> edges = searchResult.Edges();
        if (edges.Count == 0) return new GraphCommandResult(CommandType.UpdateEdge, StatusCode.NoContent);

        map.Edges.Update(edges, x => x with
        {
            EdgeType = updateEdge.EdgeType ?? x.EdgeType,
            Tags = x.Tags.Set(updateEdge.Tags),
        });

        return new GraphCommandResult(CommandType.UpdateEdge, searchResult);
    }

    private GraphCommandResult UpdateNode(GraphNodeUpdate updateNode, GraphMap map)
    {
        var searchResult = map.Query().Process(updateNode.Search);

        IReadOnlyList<GraphNode> nodes = searchResult.Nodes();
        if (nodes.Count == 0) return new GraphCommandResult(CommandType.UpdateNode, StatusCode.NoContent);

        map.Nodes.Update(nodes, x => x with
        {
            Tags = x.Tags.Set(updateNode.Tags),
        });

        return new GraphCommandResult(CommandType.UpdateNode, searchResult);
    }

    private GraphCommandResult DeleteEdge(GraphEdgeDelete deleteEdge, GraphMap map)
    {
        var searchResult = map.Query().Process(deleteEdge.Search);

        IReadOnlyList<GraphEdge> edges = searchResult.Edges();
        if (edges.Count == 0) return new GraphCommandResult(CommandType.DeleteEdge, StatusCode.NoContent);

        edges.ForEach(x => map.Edges.Remove(x.Key));
        return new GraphCommandResult(CommandType.DeleteEdge, searchResult);
    }

    private GraphCommandResult DeleteNode(GraphNodeDelete deleteNode, GraphMap map)
    {
        var searchResult = map.Query().Process(deleteNode.Search);

        IReadOnlyList<GraphNode> nodes = searchResult.Nodes();
        if (nodes.Count == 0) return new GraphCommandResult(CommandType.DeleteNode, StatusCode.NoContent);

        nodes.ForEach(x => map.Nodes.Remove(x.Key));
        return new GraphCommandResult(CommandType.DeleteNode, searchResult);
    }

    private GraphCommandResult Select(GraphSelect select, GraphMap map)
    {
        GraphQueryResult searchResult = map.Query().Process(select.Search);

        IReadOnlyList<IGraphCommon> nodes = searchResult.Items;
        if (nodes.Count == 0) return new GraphCommandResult(CommandType.Select, StatusCode.NoContent);
        return new GraphCommandResult(CommandType.Select, searchResult);
    }
}
