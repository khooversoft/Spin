using System.Diagnostics.CodeAnalysis;
using Toolbox.Tools;

namespace Toolbox.Data;

public readonly record struct QueryContext
{
    [SetsRequiredMembers]
    public QueryContext() { }

    public GraphMap Map { get; init; } = null!;
    public IReadOnlyList<GraphNode> Nodes { get; init; } = Array.Empty<GraphNode>();
    public IReadOnlyList<GraphEdge> Edges { get; init; } = Array.Empty<GraphEdge>();
}


// var v = g.Query.Nodes(x => x.{node}).HasEdge(x => x.{edge}).Nodes()
// var v = g.Query.Nodes(x => x.{node}).HasEdge(x => x.{edge}).Edges()
// var v = g.Query.Edges(x => x.{edge}).HasNode(x => x.{node}).Edges()
public static class GraphMapQuery
{
    public static QueryContext Query(this GraphMap subject) => new QueryContext { Map = subject.NotNull() };

    public static QueryContext Nodes(this QueryContext subject, Func<GraphNode, bool>? predicate = null)
    {
        subject.NotNull();

        var result = subject with
        {
            Nodes = subject.Map.Nodes.Where(x => predicate?.Invoke(x) ?? true).ToArray(),
            Edges = Array.Empty<GraphEdge>(),
        };

        return result;
    }

    public static QueryContext Edges(this QueryContext subject, Func<GraphEdge, bool>? predicate = null)
    {
        subject.NotNull();

        var result = subject with
        {
            Nodes = Array.Empty<GraphNode>(),
            Edges = subject.Map.Edges.Where(x => predicate?.Invoke(x) ?? true).ToArray(),
        };

        return result;
    }

    public static QueryContext HasNode(this QueryContext subject, Func<GraphNode, bool> predicate)
    {
        subject.NotNull();
        predicate.NotNull();

        var selectedNodes = subject.Edges
            .SelectMany(x => new string[] { x.FromKey, x.ToKey })
            .Distinct()
            .Select(x => subject.Map.Nodes[x])
            .Where(x => predicate?.Invoke(x) ?? true)
            .ToArray();

        var selectedEdges = selectedNodes
            .SelectMany(x => subject.Map.Edges.Get(x.Key))
            .ToArray();

        subject = subject with
        {
            Nodes = selectedNodes,
            Edges = selectedEdges,
        };

        return subject;
    }

    public static QueryContext HasEdge(this QueryContext subject, Func<GraphEdge, bool> predicate)
    {
        subject.NotNull();
        predicate.NotNull();

        var selectedEdges = subject.Nodes
            .SelectMany(x => subject.Map.Edges.Get(x.Key, EdgeDirection.Directed))
            .Where(x => predicate(x))
            .ToArray();

        var selectedNodes = selectedEdges
            .Select(x => x.FromKey)
            .Distinct()
            .Select(x => subject.Map.Nodes[x])
            .ToArray();

        subject = subject with
        {
            Nodes = selectedNodes,
            Edges = selectedEdges,
        };

        return subject;
    }
}
