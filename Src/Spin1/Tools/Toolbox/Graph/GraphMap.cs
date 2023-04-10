using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Graph;

public class GraphMap<TKey, TNode, TEdge> : IEnumerable<IGraphCommon>
    where TKey : notnull
    where TNode : IGraphNode<TKey>
    where TEdge : IGraphEdge<TKey>
{
    private readonly Dictionary<TKey, TNode> _nodes;
    private readonly Dictionary<Guid, TEdge> _edges;

    public GraphMap(IEqualityComparer<TKey>? equalityComparer = null)
    {
        KeyCompare = equalityComparer ??
            (typeof(TKey) == typeof(string) ? (IEqualityComparer<TKey>)StringComparer.OrdinalIgnoreCase : EqualityComparer<TKey>.Default);

        _nodes = new Dictionary<TKey, TNode>(KeyCompare);
        _edges = new Dictionary<Guid, TEdge>();
    }

    public GraphMap(GraphMap<TKey, TNode, TEdge> graphMap) : this()
    {
        graphMap.NotNull();

        graphMap.Nodes.Values.ForEach(x => Add(x));
        graphMap.Edges.Values.ForEach(x => Add(x));
    }

    public IEqualityComparer<TKey> KeyCompare { get; }

    public IReadOnlyDictionary<TKey, TNode> Nodes => _nodes;

    public IReadOnlyDictionary<Guid, TEdge> Edges => _edges;

    public GraphMap<TKey, TNode, TEdge> Add(TNode node)
    {
        node.NotNull();

        _nodes[node.Key] = node;
        return this;
    }

    public GraphMap<TKey, TNode, TEdge> Add(TEdge edge)
    {
        edge
            .NotNull()
            .Assert(x => !KeyCompare.Equals(edge.FromNodeKey, edge.ToNodeKey), "From and to keys cannot be the same");

        _edges.Add(edge.Key, edge);
        return this;
    }

    public GraphMap<TKey, TNode, TEdge> RemoveNode(TKey nodeKey)
    {
        _edges.Values
            .Where(x => KeyCompare.Equals(x.FromNodeKey, nodeKey) || KeyCompare.Equals(x.ToNodeKey, nodeKey))
            .Select(x => x.Key)
            .ToList()
            .ForEach(x => _edges.Remove(x));

        _nodes.Remove(nodeKey);

        return this;
    }

    public GraphMap<TKey, TNode, TEdge> RemoveEdge(TKey fromNodeKey, TKey toNodeKey)
    {
        _edges.Values
            .Where(x => KeyCompare.Equals(x.FromNodeKey, fromNodeKey) == true && KeyCompare.Equals(x.ToNodeKey, toNodeKey))
            .Select(x => x.Key)
            .ForEach(x => _edges.Remove(x));

        return this;
    }

    public IEnumerator<IGraphCommon> GetEnumerator() => Nodes.Values.OfType<IGraphCommon>()
        .Concat(Edges.Values.OfType<IGraphCommon>())
        .ToList()
        .GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}