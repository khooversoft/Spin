using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Data;

internal class GraphData<TKey, TNode, TEdge> : IEnumerable<IGraphCommon>
    where TKey : notnull
    where TNode : IGraphNode<TKey>
    where TEdge : IGraphEdge<TKey>
{
    private readonly GraphNodeIndex<TKey, TNode> _nodes;
    private readonly Dictionary<Guid, TEdge> _edges;
    private readonly SecondaryIndex<TKey, Guid> _edgesFrom;
    private readonly SecondaryIndex<TKey, Guid> _edgesTo;
    private readonly object _lock = new object();

    GraphData(IEqualityComparer<TKey>? equalityComparer = null)
    {
        KeyCompare = equalityComparer.ComparerFor();

        _nodes = new GraphNodeIndex<TKey, TNode>();
        _edges = new Dictionary<Guid, TEdge>();
        _edgesFrom = new SecondaryIndex<TKey, Guid>();
        _edgesTo = new SecondaryIndex<TKey, Guid>();
    }

    public IEqualityComparer<TKey> KeyCompare { get; }
    public IReadOnlyDictionary<TKey, TNode> Nodes => _nodes;
    public IReadOnlyDictionary<Guid, TEdge> Edges => _edges;

    public void Add(TNode node)
    {
        lock (_lock)
        {
            _nodes[node.Key] = node.NotNull();
        }
    }

    public void Add(TEdge edge)
    {
        edge.NotNull()
            .Assert(x => !KeyCompare.Equals(edge.FromNodeKey, edge.ToNodeKey), "From and to keys cannot be the same");

        lock (_lock)
        {
            _edges.Add(edge.Key, edge);

            //if (!_edgesFrom.TryAdd(edge.FromNodeKey, single(edge.ToNodeKey))) _edgesFrom[edge.FromNodeKey].Add(edge.Key);
            //if (!_edgesTo.TryAdd(edge.ToNodeKey, single(edge.FromNodeKey))) _edgesTo[edge.ToNodeKey].Add(edge.Key);
        }

        //HashSet<Guid> single(TKey key) => new HashSet<Guid>(edge.Key.ToEnumerable());
    }

    public void RemoveNode(TKey nodeKey)
    {
        lock (_lock)
        {
            //if (_edgesFrom.TryGetValue(nodeKey, out Dictionary<TKey, TEdge>? fromDict))
            //{
            //}
        }
        _edges.Values
            .Where(x => KeyCompare.Equals(x.FromNodeKey, nodeKey) || KeyCompare.Equals(x.ToNodeKey, nodeKey))
            .Select(x => x.Key)
            .ToList()
            .ForEach(x => _edges.Remove(x));

        _nodes.Remove(nodeKey);
    }

    public void RemoveEdge(TKey fromNodeKey, TKey toNodeKey)
    {
        _edges.Values
            .Where(x => KeyCompare.Equals(x.FromNodeKey, fromNodeKey) == true && KeyCompare.Equals(x.ToNodeKey, toNodeKey))
            .Select(x => x.Key)
            .ForEach(x => _edges.Remove(x));
    }

    public IEnumerator<IGraphCommon> GetEnumerator() => Nodes.Values.OfType<IGraphCommon>()
         .Concat(Edges.Values.OfType<IGraphCommon>())
         .ToList()
         .GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
