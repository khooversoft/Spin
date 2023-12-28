using System.Collections;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Data;

public class GraphEdgeIndex : IEnumerable<GraphEdge>
{
    private readonly Dictionary<Guid, GraphEdge> _index;
    private readonly SecondaryIndex<string, Guid> _edgesFrom;
    private readonly SecondaryIndex<string, Guid> _edgesTo;
    private readonly HashSet<GraphEdge> _masterList = new HashSet<GraphEdge>(new GraphEdgeComparer());
    private readonly object _lock;
    private readonly Func<string, bool> _isNodeExist;

    public GraphEdgeIndex(object syncLock, Func<string, bool> isNodeExist, IEqualityComparer<string>? keyComparer = null)
    {
        _lock = syncLock.NotNull();
        _isNodeExist = isNodeExist.NotNull();

        _index = new Dictionary<Guid, GraphEdge>();
        _edgesFrom = new SecondaryIndex<string, Guid>(keyComparer);
        _edgesTo = new SecondaryIndex<string, Guid>(keyComparer);
    }

    public GraphEdge this[Guid key]
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

    public Option Add(GraphEdge edge, bool upsert = false)
    {
        if (!edge.Validate(out var v1)) return v1;

        lock (_lock)
        {
            if (!ValidateNodes(edge, out var v2)) return v2;
            if (!_index.TryAdd(edge.Key, edge)) return (StatusCode.Conflict, $"key={edge.Key} already exist");

            if (!_masterList.Add(edge))
            {
                if (!upsert) return (StatusCode.Conflict, $"Edge {edge} already exist (from key + to key + direction + tags)");

                if (!_masterList.TryGetValue(edge, out var readEdge)) throw new InvalidOperationException("Master list lookup failed");
                _masterList.Remove(readEdge);

                readEdge = readEdge with
                {
                    Tags = readEdge.Tags.Copy().SetValues(edge.Tags),
                };

                _masterList.Add(readEdge).Assert<bool, InvalidOperationException>(x => x == true, _ => "Failed to update edge on upsert");
            }

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

    public IReadOnlyList<GraphEdge> Get(string nodeKey, EdgeDirection direction = EdgeDirection.Both, string? matchEdgeType = null)
    {
        return Query(new GraphEdgeSearch { NodeKey = nodeKey, Direction = direction, EdgeType = matchEdgeType });
    }

    public IReadOnlyList<GraphEdge> Get(string fromKey, string toKey, EdgeDirection direction = EdgeDirection.Both, string? matchEdgeType = null)
    {
        return Query(new GraphEdgeSearch { FromKey = fromKey, ToKey = toKey, Direction = direction, EdgeType = matchEdgeType });
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

    public bool Remove(string nodeKey)
    {
        lock (_lock)
        {
            var query = new GraphEdgeSearch { NodeKey = nodeKey };
            var keys = Query(query);
            if (keys.Count == 0) return false;

            keys.ForEach(x => Remove(x.Key).Assert(x => x == true, $"{x.Key} failed to remove"));
            return true;
        }
    }

    public IReadOnlyList<GraphEdge> Query(GraphEdgeSearch query)
    {
        lock (_lock)
        {
            query.NotNull();

            IReadOnlyList<Guid> result = (query.NodeKey, query.FromKey, query.ToKey) switch
            {
                (string nodeKey, null, null) => GetInclusive(nodeKey, query.Direction),
                (null, string fromKey, null) => _edgesFrom.Lookup(fromKey),
                (null, null, string toKey) => _edgesTo.Lookup(toKey),
                (null, string fromKey, string toKey) => GetIntersect(fromKey, toKey, query.Direction),
                (null, null, null) => _index.Values.Select(x => x.Key).Distinct().ToArray(),

                _ => Array.Empty<Guid>()
            };

            var edges = result
                .Select(x => _index[x])
                .Where(x => query.EdgeType == null || x.EdgeType.IsMatch(query.EdgeType))
                .Where(x => query.Tags == null || new Tags(x.Tags).Has(query.Tags))
                .ToArray();

            return edges;
        }
    }

    public bool TryGetValue(Guid key, out GraphEdge? value) => _index.TryGetValue(key, out value);

    public Option Update(IReadOnlyList<GraphEdge> edges, Func<GraphEdge, GraphEdge> update)
    {
        edges.NotNull();
        update.NotNull();
        if (edges.Count == 0) return StatusCode.OK;

        lock (_lock)
        {
            edges.ForEach(x =>
            {
                var n = update(x);
                (n.Key == x.Key).Assert(x => x == true, "Cannot change the primary key");
                n.FromKey.EqualsIgnoreCase(x.FromKey).Assert(x => x == true, "Cannot change the From key key");
                n.ToKey.EqualsIgnoreCase(x.ToKey).Assert(x => x == true, "Cannot change the To key key");
                _index[x.Key] = n;
            });

            return StatusCode.OK;
        }
    }

    public IEnumerator<GraphEdge> GetEnumerator() => _index.Values.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private bool ValidateNodes(GraphEdge edge, out Option result)
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

    private IReadOnlyList<Guid> GetInclusive(string key, EdgeDirection direction) => direction switch
    {
        EdgeDirection.Both => _edgesFrom.Lookup(key).Concat(_edgesTo.Lookup(key)).Distinct().ToArray(),
        EdgeDirection.Directed => _edgesFrom.Lookup(key).Distinct().ToArray(),
        _ => throw new InvalidOperationException($"Invalid direction, {direction}")
    };

    private IReadOnlyList<Guid> GetIntersect(string fromKey, string toKey, EdgeDirection direction) => direction switch
    {
        EdgeDirection.Both => _edgesFrom.Lookup(fromKey).Intersect(_edgesTo.Lookup(toKey)).Distinct().ToArray(),
        EdgeDirection.Directed => _edgesFrom.Lookup(fromKey).Distinct().ToArray(),
        _ => throw new InvalidOperationException($"Invalid direction, {direction}")
    };
}
