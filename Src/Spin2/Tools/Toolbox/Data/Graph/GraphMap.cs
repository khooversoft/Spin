using System.Collections;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Data;

public interface IGraphCommon { }


public class GraphMap : GraphMap<string>
{
    public GraphMap() { }
    public GraphMap(IEnumerable<GraphNode<string>> nodes, IEnumerable<GraphEdge<string>> edges, IEqualityComparer<string>? keyComparer = null)
        : base(nodes, edges, keyComparer)
    {
    }

    public static GraphMap FromJson(string json) =>
        json.ToObject<GraphSerialization>()
        .NotNull()
        .FromSerialization();

    public static GraphMap<T> FromJson<T>(string json) where T : notnull =>
        json.ToObject<GraphSerialization<T>>()
        .NotNull()
        .FromSerialization();
}


public class GraphMap<T> : GraphMap<T, GraphNode<T>, GraphEdge<T>>
    where T : notnull
{
    public GraphMap() { }
    public GraphMap(IEnumerable<GraphNode<T>> nodes, IEnumerable<GraphEdge<T>> edges, IEqualityComparer<T>? keyComparer = null)
        : base(nodes, edges, keyComparer)
    {
    }
}


public class GraphMap<TKey, TNode, TEdge> : IEnumerable<IGraphCommon>
    where TKey : notnull
    where TNode : IGraphNode<TKey>
    where TEdge : IGraphEdge<TKey>
{
    private readonly GraphNodeIndex<TKey, TNode> _nodes;
    private readonly GraphEdgeIndex<TKey, TEdge> _edges;
    private readonly object _lock = new object();

    public GraphMap(IEqualityComparer<TKey>? keyComparer = null)
    {
        _nodes = new GraphNodeIndex<TKey, TNode>(_lock, x => _edges.NotNull().Remove(x.Key), keyComparer);
        _edges = new GraphEdgeIndex<TKey, TEdge>(_lock, x => _nodes.NotNull().ContainsKey(x));

        KeyCompare = keyComparer.ComparerFor();
    }

    public GraphMap(IEnumerable<TNode> nodes, IEnumerable<TEdge> edges, IEqualityComparer<TKey>? keyComparer = null)
        : this(keyComparer)
    {
        nodes.NotNull().ForEach(x => Nodes.Add(x).ThrowOnError("Node add failed"));
        edges.NotNull().ForEach(x => Edges.Add(x).ThrowOnError("Edge add failed"));
    }

    public IEqualityComparer<TKey> KeyCompare { get; }
    public GraphNodeIndex<TKey, TNode> Nodes => _nodes;
    public GraphEdgeIndex<TKey, TEdge> Edges => _edges;

    public GraphMap<TKey, TNode, TEdge> Add(IGraphCommon element)
    {
        switch (element)
        {
            case TNode node: _nodes.Add(node).ThrowOnError("Node add failed"); break;
            case TEdge edge: _edges.Add(edge).ThrowOnError("Edge add failed"); break;
            default: throw new ArgumentException("Unknown element");
        }

        return this;
    }

    public void Clear()
    {
        lock (_lock)
        {
            _nodes.Clear();
            _edges.Clear();
        }
    }

    public IEnumerator<IGraphCommon> GetEnumerator()
    {
        foreach (var item in Nodes.OfType<IGraphCommon>()) yield return item;
        foreach (var item in Edges.OfType<IGraphCommon>()) yield return item;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}