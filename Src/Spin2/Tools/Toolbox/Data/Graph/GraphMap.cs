using System.Collections;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Data;

public interface IGraphCommon { }

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
        _edges = new GraphEdgeIndex<TKey, TEdge>(_lock);
        _nodes = new GraphNodeIndex<TKey, TNode>(_lock, x => _edges.Remove(x.Key), keyComparer);

        KeyCompare = keyComparer.ComparerFor();
    }

    public IEqualityComparer<TKey> KeyCompare { get; }
    public GraphNodeIndex<TKey, TNode> Nodes => _nodes;
    public GraphEdgeIndex<TKey, TEdge> Edges => _edges;

    public GraphMap<TKey, TNode, TEdge> Add(IGraphCommon element)
    {
        switch (element)
        {
            case TNode node: _nodes.Add(node); break;
            case TEdge edge: _edges.Add(edge); break;
            default: throw new ArgumentException("Unknown element");
        }

        return this;
    }

    public IEnumerator<IGraphCommon> GetEnumerator()
    {
        foreach (var item in Nodes.OfType<IGraphCommon>()) yield return item;
        foreach (var item in Edges.OfType<IGraphCommon>()) yield return item;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}