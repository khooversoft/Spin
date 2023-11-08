using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Data.Graph.Execute;

public class GraphCommand
{
    private readonly GraphMap _map;
    public GraphCommand(GraphMap map) => _map = map.NotNull();

    public Option<GraphMap> Execute(string rawData)
    {
        var map = _map.Clone();

        Option<IReadOnlyList<IGraphQL>> result = GraphLang.Parse(rawData);
        if (result.IsError()) return result.ToOptionStatus<GraphMap>();

        IReadOnlyList<IGraphQL> commands = result.Return();
        var results = new Sequence<CommandResult>();

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

        return map;
    }
    private CommandResult AddNode(GraphNodeAdd addNode, GraphMap map)
    {
        var graphNode = new GraphNode
        {
            Key = addNode.Key,
            Tags = addNode.Tags,
        };

        var result = map.Nodes.Add(graphNode);
        return new CommandResult(CommandType.AddNode, result);
    }

    private CommandResult AddEdge(GraphEdgeAdd addEdge, GraphMap map)
    {
        var graphEdge = new GraphEdge
        {
            FromKey = addEdge.FromKey,
            ToKey = addEdge.ToKey,
            EdgeType = addEdge.EdgeType ?? "default",
            Tags = new Tags(addEdge.Tags),
        };

        var result = map.Edges.Add(graphEdge);
        return new CommandResult(CommandType.AddEdge, result);
    }

    private CommandResult UpdateEdge(GraphEdgeUpdate updateEdge, GraphMap map)
    {
        var searchResult = map.Query().Process(updateEdge.Search);

        IReadOnlyList<GraphEdge> edges = searchResult.Edges();
        if (edges.Count == 0) return new CommandResult(CommandType.UpdateEdge, StatusCode.NoContent);

        _map.Edges.Update(edges, x => x with
        {
            EdgeType = updateEdge.EdgeType ?? x.EdgeType,
            Tags = x.Tags.Set(updateEdge.Tags),
        });

        return new CommandResult(CommandType.UpdateEdge, edges);
    }

    private CommandResult UpdateNode(GraphNodeUpdate updateNode, GraphMap map)
    {
        var searchResult = map.Query().Process(updateNode.Search);

        IReadOnlyList<GraphNode> nodes = searchResult.Nodes();
        if (nodes.Count == 0) return new CommandResult(CommandType.UpdateNode, StatusCode.NoContent);

        _map.Nodes.Update(nodes, x => x with
        {
            Tags = x.Tags.Set(updateNode.Tags),
        });

        return new CommandResult(CommandType.UpdateNode, nodes);
    }

    private CommandResult DeleteEdge(GraphEdgeDelete deleteEdge, GraphMap map)
    {
        var searchResult = map.Query().Process(deleteEdge.Search);

        IReadOnlyList<GraphEdge> edges = searchResult.Edges();
        if (edges.Count == 0) return new CommandResult(CommandType.DeleteEdge, StatusCode.NoContent);

        edges.ForEach(x => map.Edges.Remove(x.Key));
        return new CommandResult(CommandType.DeleteEdge, edges);
    }

    private CommandResult DeleteNode(GraphNodeDelete deleteNode, GraphMap map)
    {
        var searchResult = map.Query().Process(deleteNode.Search);

        IReadOnlyList<GraphNode> nodes = searchResult.Nodes();
        if (nodes.Count == 0) return new CommandResult(CommandType.DeleteNode, StatusCode.NoContent);

        nodes.ForEach(x => map.Nodes.Remove(x.Key));
        return new CommandResult(CommandType.DeleteNode, nodes);
    }

    private CommandResult Select(GraphSelect select, GraphMap map)
    {
        var searchResult = map.Query().Process(select.Search);

        IReadOnlyList<IGraphCommon> nodes = searchResult.Items;
        if (nodes.Count == 0) return new CommandResult(CommandType.Select, StatusCode.NoContent);
        return new CommandResult(CommandType.Select, searchResult.Items);
    }
}
