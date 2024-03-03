using Toolbox.Tools;

namespace Toolbox.Data;

public record SearchContext
{
    public enum LastSearchType { Node, Edge }

    public LastSearchType LastSearch { get; init; }
    public GraphMap Map { get; init; } = null!;
    public IReadOnlyList<GraphNode> Nodes { get; init; } = Array.Empty<GraphNode>();
    public IReadOnlyList<GraphEdge> Edges { get; init; } = Array.Empty<GraphEdge>();
}


public static class GraphSearch
{
    public static SearchContext Search(this GraphMap subject) => new SearchContext { Map = subject.NotNull() };

    public static SearchContext Nodes(this SearchContext subject, Func<GraphNode, bool>? predicate = null)
    {
        subject.NotNull();

        var result = subject with
        {
            LastSearch = SearchContext.LastSearchType.Node,
            Nodes = subject.Map.Nodes.Where(x => predicate?.Invoke(x) ?? true).ToArray(),
            Edges = Array.Empty<GraphEdge>(),
        };

        return result;
    }

    public static SearchContext Edges(this SearchContext subject, Func<GraphEdge, bool>? predicate = null)
    {
        subject.NotNull();

        var result = subject with
        {
            LastSearch = SearchContext.LastSearchType.Edge,
            Nodes = Array.Empty<GraphNode>(),
            Edges = subject.Map.Edges.Where(x => predicate?.Invoke(x) ?? true).ToArray(),
        };

        return result;
    }

    public static SearchContext HasNode(this SearchContext subject, Func<GraphNode, bool> predicate)
    {
        subject.NotNull();
        predicate.NotNull();

        var selectedNodes = subject.Edges
            .Select(x => x.ToKey)
            .Distinct()
            .Select(x => subject.Map.Nodes[x])
            .Where(x => predicate?.Invoke(x) ?? true)
            .ToArray();

        var selectedEdges = selectedNodes
            .SelectMany(x => subject.Map.Edges.Get(x.Key))
            .ToArray();

        subject = subject with
        {
            LastSearch = SearchContext.LastSearchType.Node,
            Nodes = selectedNodes,
            Edges = selectedEdges,
        };

        return subject;
    }

    public static SearchContext HasEdge(this SearchContext subject, Func<GraphEdge, bool> predicate, EdgeDirection edgeDirection = EdgeDirection.Directed)
    {
        subject.NotNull();
        predicate.NotNull();

        var selectedEdges = subject.Nodes
            .SelectMany(x => subject.Map.Edges.Get(x.Key, edgeDirection))
            .Where(x => predicate(x))
            .ToArray();

        var selectedNodes = selectedEdges
            .Select(x => x.FromKey)
            .Distinct()
            .Select(x => subject.Map.Nodes[x])
            .ToArray();

        subject = subject with
        {
            LastSearch = SearchContext.LastSearchType.Edge,
            Nodes = selectedNodes,
            Edges = selectedEdges,
        };

        return subject;
    }
}
