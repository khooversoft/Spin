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
    private readonly HashSet<TEdge> _masterList = new HashSet<TEdge>(new GraphEdgeComparer<TKey, TEdge>());
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
        if (!edge.Validate(out var v1)) return v1;

        lock (_lock)
        {
            if (!ValidateNodes(edge, out var v2)) return v2;
            if (!_index.TryAdd(edge.Key, edge)) return (StatusCode.Conflict, $"key={edge.Key} already exist");
            if (!_masterList.Add(edge)) return (StatusCode.Conflict, $"Edge {edge} already exist (from key + to key + direction + tags)");

            _edgesFrom.Set(edge.FromKey, edge.Key);
            _edgesTo.Set(edge.ToKey, edge.Key);

            return StatusCode.OK;
        }
    }

    public void Clear()
    {
        lock (_lock)
        {
            _index.Clear();
            _edgesFrom.Clear();
            _edgesTo.Clear();
            _masterList.Clear();
        }
    }
    public bool ContainsKey(Guid edgeKey) => _index.ContainsKey(edgeKey);

    public IReadOnlyList<TEdge> Get(TKey nodeKey, EdgeDirection direction = EdgeDirection.Both, string? matchEdgeType = null)
    {
        return Query(new GraphEdgeQuery<TKey> { NodeKey = nodeKey, Direction = direction, EdgeType = matchEdgeType });
    }

    public IReadOnlyList<TEdge> Get(TKey fromKey, TKey toKey, EdgeDirection direction = EdgeDirection.Both, string? matchEdgeType = null)
    {
        return Query(new GraphEdgeQuery<TKey> { FromKey = fromKey, ToKey = toKey, Direction = direction, EdgeType = matchEdgeType });
    }

    public bool Remove(Guid edgeKey)
    {
        lock (_lock)
        {
            if (!_index.Remove(edgeKey, out var nodeValue)) return false;

            _masterList.Remove(nodeValue);
            _edgesFrom.RemovePrimaryKey(nodeValue.Key);
            _edgesTo.RemovePrimaryKey(nodeValue.Key);

            return true;
        }
    }

    public bool Remove(TKey nodeKey)
    {
        lock (_lock)
        {
            var query = new GraphEdgeQuery<TKey> { NodeKey = nodeKey };
            var keys = Query(query);
            if (keys.Count == 0) return false;

            keys.ForEach(x => Remove(x.Key).Assert(x => x == true, $"{x.Key} failed to remove"));
            return true;
        }
    }

    public IReadOnlyList<TEdge> Query(GraphEdgeQuery<TKey> query)
    {
        lock (_lock)
        {
            query.NotNull();

            IReadOnlyList<Guid> result = (query.NodeKey, query.FromKey, query.ToKey) switch
            {
                (TKey nodeKey, null, null) => GetInclusive(nodeKey, query.Direction),
                (null, TKey fromKey, null) => _edgesFrom.Lookup(fromKey),
                (null, null, TKey toKey) => _edgesTo.Lookup(toKey),
                (null, TKey fromKey, TKey toKey) => GetIntersect(fromKey, toKey, query.Direction),
                (null, null, null) => _index.Values.Select(x => x.Key).Distinct().ToArray(),

                _ => Array.Empty<Guid>()
            };

            var edges = result
                .Select(x => _index[x])
                .Where(x => query.EdgeType == null || x.EdgeType.Match(query.EdgeType))
                .Where(x => query.Tags == null || new Tags(x.Tags).Has(query.Tags))
                .ToArray();

            return edges;
        }
    }

    public bool TryGetValue(Guid key, out TEdge? value) => _index.TryGetValue(key, out value);

    public Option Update(GraphEdgeQuery<TKey> query, Func<TEdge, TEdge> update)
    {
        update.NotNull();

        lock (_lock)
        {
            IReadOnlyList<TEdge> list = Query(query);
            if (list.Count == 0) return StatusCode.OK;

            list = list.Select(x =>
            {
                var n = update(x);
                (n.Key == x.Key).Assert(x => x == true, "Cannot change the primary key");
                GraphEdgeTool.IsKeysEqual(n.FromKey, x.FromKey).Assert(x => x == true, "Cannot change the From key key");
                GraphEdgeTool.IsKeysEqual(n.ToKey, x.ToKey).Assert(x => x == true, "Cannot change the To key key");
                return n;
            })
            .ToArray();

            list.ForEach(x => _index[x.Key] = x);

            return list.Count > 0 ? StatusCode.OK : StatusCode.NotFound;
        }
    }

    public IEnumerator<TEdge> GetEnumerator() => _index.Values.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private bool ValidateNodes(TEdge edge, out Option result)
    {
        if (!_isNodeExist(edge.FromKey))
        {
            result = (StatusCode.NotFound, $"Cannot add edge, FromNodeKey={edge.FromKey} does not exist");
            return false;
        }

        if (!_isNodeExist(edge.ToKey))
        {
            result = (StatusCode.NotFound, $"Cannot add edge, ToNodeKey={edge.ToKey} does not exist");
            return false;
        }

        result = StatusCode.OK;
        return true;
    }

    private IReadOnlyList<Guid> GetInclusive(TKey key, EdgeDirection direction) => direction switch
    {
        EdgeDirection.Both => _edgesFrom.Lookup(key).Concat(_edgesTo.Lookup(key)).Distinct().ToArray(),
        EdgeDirection.Directed => _edgesFrom.Lookup(key).Distinct().ToArray(),
        _ => throw new InvalidOperationException($"Invalid direction, {direction}")
    };

    private IReadOnlyList<Guid> GetIntersect(TKey fromKey, TKey toKey, EdgeDirection direction) => direction switch
    {
        EdgeDirection.Both => _edgesFrom.Lookup(fromKey).Intersect(_edgesTo.Lookup(toKey)).Distinct().ToArray(),
        EdgeDirection.Directed => _edgesFrom.Lookup(fromKey).Distinct().ToArray(),
        _ => throw new InvalidOperationException($"Invalid direction, {direction}")
    };
}
