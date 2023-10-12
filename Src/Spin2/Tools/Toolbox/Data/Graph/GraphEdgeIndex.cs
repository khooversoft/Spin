using System.Collections;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Data;

public class GraphEdgeIndex<TKey, TEdge> : IEnumerable<TEdge>
    where TKey : notnull
    where TEdge : IGraphEdge<TKey>
{
    private readonly Dictionary<Guid, TEdge> _index;
    private readonly SecondaryIndex<TKey, Guid> _edgesFrom;
    private readonly SecondaryIndex<TKey, Guid> _edgesTo;
    private readonly object _lock;
    private readonly Func<TKey, bool> _isNodeExist;

    public GraphEdgeIndex(object syncLock, Func<TKey, bool> isNodeExist, IEqualityComparer<TKey>? keyComparer = null)
    {
        _lock = syncLock.NotNull();
        _isNodeExist = isNodeExist.NotNull();

        _index = new Dictionary<Guid, TEdge>();
        _edgesFrom = new SecondaryIndex<TKey, Guid>(keyComparer);
        _edgesTo = new SecondaryIndex<TKey, Guid>(keyComparer);
    }

    public TEdge this[Guid key]
    {
        get => _index[key];
        set
        {
            lock (_lock)
            {
                if (!ValidateNodes(value, out var option)) option.ThrowOnError("Invalid edge");

                Remove(value.Key);
                Add(value);
            }
        }
    }

    public int Count => _index.Count;

    public Option Add(TEdge edge)
    {
        if (!edge.IsValid(out var v1)) return v1;

        lock (_lock)
        {
            if (!ValidateNodes(edge, out var v2)) return v2;
            if (!_index.TryAdd(edge.Key, edge)) return (StatusCode.Conflict, $"key={edge.Key} already exist");

            _edgesFrom.Set(edge.FromNodeKey, edge.Key);
            _edgesTo.Set(edge.ToNodeKey, edge.Key);

            return StatusCode.OK;
        }
    }

    public bool ContainsKey(Guid edgeKey) => _index.ContainsKey(edgeKey);

    public IReadOnlyList<TEdge> Get(TKey nodeKey, EdgeDirection direction = EdgeDirection.Both)
    {
        lock (_lock)
        {
            (IReadOnlyList<Guid> from, IReadOnlyList<Guid> to) = GetEdges(direction, nodeKey, nodeKey);
            var result = from.Concat(to).Distinct().Select(x => _index[x]).ToArray();
            return result;
        }
    }

    public IReadOnlyList<TEdge> Get(TKey fromKey, TKey toKey, EdgeDirection direction = EdgeDirection.Both)
    {
        lock (_lock)
        {
            (IReadOnlyList<Guid> from, IReadOnlyList<Guid> to) = GetEdges(direction, fromKey, toKey);

            var keys = from.Intersect(to).Distinct().ToArray();
            var result = keys.Select(x => _index[x]).ToArray();
            return result;
        }
    }

    public bool Remove(Guid edgeKey)
    {
        lock (_lock)
        {
            if (!_index.Remove(edgeKey, out var nodeValue)) return false;

            _edgesFrom.RemovePrimaryKey(nodeValue.Key);
            _edgesTo.RemovePrimaryKey(nodeValue.Key);

            return true;
        }
    }

    public bool Remove(TKey nodeKey)
    {
        lock (_lock)
        {
            var from = _edgesFrom.Lookup(nodeKey);
            var to = _edgesTo.Lookup(nodeKey);

            var keys = from.Concat(to).Distinct().ToArray();
            if (keys.Length == 0) return false;

            keys.ForEach(x => Remove(x).Assert(x => x == true, $"{x} failed to remove"));
            return true;
        }
    }

    public IReadOnlyList<TEdge> Remove(TKey fromKey, TKey toKey)
    {
        lock (_lock)
        {
            var from = _edgesFrom.Lookup(fromKey);
            var to = _edgesTo.Lookup(toKey);

            var keys = from.Intersect(to).ToArray();
            var result = keys.Select(x => _index[x]).ToArray();

            result.ForEach(x => Remove(x.Key).Assert(x => x == true, $"{x} failed to remove"));

            return result;
        }
    }

    public bool TryGetValue(Guid key, out TEdge? value) => _index.TryGetValue(key, out value);

    public IEnumerator<TEdge> GetEnumerator() => _index.Values.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private bool ValidateNodes(TEdge edge, out Option result)
    {
        if (!_isNodeExist(edge.FromNodeKey))
        {
            result = (StatusCode.NotFound, $"Cannot add edge, FromNodeKey={edge.FromNodeKey} does not exist");
            return false;
        }

        if (!_isNodeExist(edge.ToNodeKey))
        {
            result = (StatusCode.NotFound, $"Cannot add edge, ToNodeKey={edge.ToNodeKey} does not exist");
            return false;
        }

        result = StatusCode.OK;
        return true;
    }

    private (IReadOnlyList<Guid> from, IReadOnlyList<Guid> to) GetEdges(EdgeDirection direction, TKey fromNode, TKey toNode)
    {
        return direction switch
        {
            EdgeDirection.Both => (_edgesFrom.Lookup(fromNode), _edgesTo.Lookup(toNode)),
            EdgeDirection.Directed => (_edgesFrom.Lookup(fromNode), Array.Empty<Guid>()),
            _ => throw new InvalidOperationException($"Invalid direction, {direction}")
        };
    }
}
