using System.Diagnostics.CodeAnalysis;
using Toolbox.Tools;

namespace Toolbox.Data;

public readonly record struct QueryContext<T> where T : notnull
{
    [SetsRequiredMembers]
    public QueryContext() { }

    public GraphMap<T> Map { get; init; } = null!;
    public IReadOnlyList<GraphNode<T>> Nodes { get; init; } = Array.Empty<GraphNode<T>>();
    public IReadOnlyList<GraphEdge<T>> Edges { get; init; } = Array.Empty<GraphEdge<T>>();
}


// var v = g.Query.Nodes(x => x.{node}).HasEdge(x => x.{edge}).Nodes()
// var v = g.Query.Nodes(x => x.{node}).HasEdge(x => x.{edge}).Edges()
// var v = g.Query.Edges(x => x.{edge}).HasNode(x => x.{node}).Edges()
public static class GraphMapQuery
{
    public static QueryContext<T> Query<T>(this GraphMap<T> subject) where T : notnull => new QueryContext<T> { Map = subject.NotNull() };

    public static QueryContext<T> Nodes<T>(this QueryContext<T> subject, Func<GraphNode<T>, bool>? predicate = null) where T : notnull
    {
        subject.NotNull();

        var result = subject with
        {
            Nodes = subject.Map.Nodes.Where(x => predicate?.Invoke(x) ?? true).ToArray(),
            Edges = Array.Empty<GraphEdge<T>>(),
        };

        return result;
    }

    public static QueryContext<T> Edges<T>(this QueryContext<T> subject, Func<GraphEdge<T>, bool>? predicate = null) where T : notnull
    {
        subject.NotNull();

        var result = subject with
        {
            Nodes = Array.Empty<GraphNode<T>>(),
            Edges = subject.Map.Edges.Where(x => predicate?.Invoke(x) ?? true).ToArray(),
        };

        return result;
    }

    public static QueryContext<T> HasNode<T>(this QueryContext<T> subject, Func<GraphNode<T>, bool> predicate) where T : notnull
    {
        subject.NotNull();
        predicate.NotNull();

        var selectedNodes = subject.Edges
            .SelectMany(x => new T[] { x.FromKey, x.ToKey })
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

    public static QueryContext<T> HasEdge<T>(this QueryContext<T> subject, Func<GraphEdge<T>, bool> predicate) where T : notnull
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
