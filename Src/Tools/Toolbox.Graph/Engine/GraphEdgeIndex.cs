using System.Collections;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public class GraphEdgeIndex : IEnumerable<GraphEdge>
{
    private readonly Dictionary<Guid, GraphEdge> _index;
    private readonly SecondaryIndex<string, Guid> _edgesFrom;
    private readonly SecondaryIndex<string, Guid> _edgesTo;
    private readonly HashSet<GraphEdge> _masterList = new HashSet<GraphEdge>(new GraphEdgeComparer());
    private readonly object _lock;
    private readonly GraphRI _graphRI;

    internal GraphEdgeIndex(object syncLock, GraphRI graphRI, IEqualityComparer<string>? keyComparer = null)
    {
        _lock = syncLock.NotNull();
        _graphRI = graphRI.NotNull();

        _index = new Dictionary<Guid, GraphEdge>();
        _edgesFrom = new SecondaryIndex<string, Guid>(keyComparer);
        _edgesTo = new SecondaryIndex<string, Guid>(keyComparer);
    }

    public GraphEdge this[Guid key]
    {
        get => _index[key];
        internal set
        {
            lock (_lock)
            {
                if (!ValidateNodes(value, null, out var option)) option.ThrowOnError("Invalid edge");

                Remove(value.Key);
                Add(value);
            }
        }
    }

    public int Count => _index.Count;

    internal Option Add(GraphEdge edge, GsEdgeAdd? addEdge = null, IGraphTrxContext? graphContext = null)
    {
        if (!edge.Validate(out var v1)) return v1;

        lock (_lock)
        {
            if (!ValidateNodes(edge, addEdge, out var v2)) return v2;

            if (!_index.TryAdd(edge.Key, edge)) return (StatusCode.Conflict, $"key={edge.Key} already exist");
            if (_masterList.Contains(edge)) return (StatusCode.Conflict, $"Edge {edge} already exist (from key + to key + direction + tags)");

            _masterList.Add(edge).Assert(x => x == true, "Failed to add edge to master list");

            _edgesFrom.Set(edge.FromKey, edge.Key);
            _edgesTo.Set(edge.ToKey, edge.Key);

            graphContext?.ChangeLog.Push(new CmEdgeAdd(edge));
            return StatusCode.OK;
        }
    }

    internal Option Set(GraphEdge edge, GsEdgeAdd? addEdge = null, IGraphTrxContext? graphContext = null)
    {
        if (!edge.Validate(out var v1)) return v1;

        lock (_lock)
        {
            if (!ValidateNodes(edge, addEdge, out var v2)) return v2;

            if (_masterList.TryGetValue(edge, out var readEdge))
            {
                readEdge = readEdge.With(edge);
                _index[readEdge.Key] = readEdge;
                graphContext?.ChangeLog.Push(new CmEdgeChange(readEdge, edge));
                return StatusCode.OK;
            }

            _index[edge.Key] = edge;
            _masterList.Add(edge).Assert<bool, InvalidOperationException>(x => x == true, _ => "Failed to update edge on upsert");

            graphContext?.ChangeLog.Push(new CmEdgeAdd(edge));
        }

        _edgesFrom.Set(edge.FromKey, edge.Key);
        _edgesTo.Set(edge.ToKey, edge.Key);
        return StatusCode.OK;
    }

    internal void Clear()
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

    internal bool Remove(Guid edgeKey, IGraphTrxContext? graphContext = null)
    {
        lock (_lock)
        {
            if (!_index.Remove(edgeKey, out var nodeValue)) return false;

            _masterList.Remove(nodeValue);
            _edgesFrom.RemovePrimaryKey(nodeValue.Key);
            _edgesTo.RemovePrimaryKey(nodeValue.Key);

            graphContext?.ChangeLog.Push(new CmEdgeDelete(nodeValue));
            return true;
        }
    }

    internal IReadOnlyList<string> Remove(string nodeKey, IGraphTrxContext? graphContext)
    {
        lock (_lock)
        {
            var query = new GraphEdgeSearch { NodeKey = nodeKey };
            IReadOnlyList<GraphEdge> keys = Query(query);
            if (keys.Count == 0) return Array.Empty<string>();

            keys.ForEach(x => Remove(x.Key, graphContext).Assert(x => x == true, $"{x.Key} failed to remove"));

            var set = keys.Select(x => x.FromKey != nodeKey ? x.FromKey : x.ToKey).ToArray();
            return set;
        }
    }

    internal IReadOnlyList<GraphEdge> Query(GraphEdgeSearch query)
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
                .Where(x => query.EdgeType == null || x.EdgeType.Like(query.EdgeType))
                .Where(x => query.Tags.Count == 0 || query.Tags.Has(query.Tags))
                .ToArray();

            return edges;
        }
    }

    public bool TryGetValue(Guid key, out GraphEdge? value) => _index.TryGetValue(key, out value);

    internal Option Update(IReadOnlyList<GraphEdge> edges, Func<GraphEdge, GraphEdge> update, IGraphTrxContext? graphContext = null)
    {
        edges.NotNull();
        update.NotNull();
        if (edges.Count == 0) return StatusCode.OK;

        lock (_lock)
        {
            edges.ForEach(currentValue =>
            {
                _index.ContainsKey(currentValue.Key).Assert(x => x == true, $"Key={currentValue.Key} does not exist");

                GraphEdge newValue = update(currentValue);
                (newValue.Key == currentValue.Key).Assert(x => x == true, "Cannot change the primary key");
                newValue.FromKey.EqualsIgnoreCase(currentValue.FromKey).Assert(x => x == true, "Cannot change the From key key");
                newValue.ToKey.EqualsIgnoreCase(currentValue.ToKey).Assert(x => x == true, "Cannot change the To key key");
                _index[currentValue.Key] = newValue;

                graphContext?.ChangeLog.Push(new CmEdgeChange(currentValue, newValue));
            });

            return StatusCode.OK;
        }
    }

    public IEnumerator<GraphEdge> GetEnumerator() => _index.Values.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private bool ValidateNodes(GraphEdge edge, GsEdgeAdd? addEdge, out Option result)
    {
        if (!_graphRI.IsNodeExist(edge.FromKey))
        {
            result = (StatusCode.NotFound, $"Cannot add edge, FromNodeKey={edge.FromKey} does not exist");
            return false;
        }

        if (!_graphRI.IsNodeExist(edge.ToKey))
        {
            result = (StatusCode.NotFound, $"Cannot add edge, ToNodeKey={edge.ToKey} does not exist");
            return false;
        }

        if (addEdge?.Unique == true && DoesExist(edge.FromKey, edge.ToKey, addEdge.EdgeType))
        {
            result = (StatusCode.Conflict, $"Edge already exist between FromKey={edge.FromKey} and ToKey={edge.ToKey} and EdgeType={addEdge.EdgeType}");
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

    public bool DoesExist(string fromKey, string toKey, string? edgeType)
    {
        var intersect = GetIntersect(fromKey, toKey, EdgeDirection.Both);
        if (edgeType.IsEmpty() && intersect.Count > 0) return true;

        var match = intersect
            .Select(x => _index[x])
            .Where(x => x.EdgeType.EqualsIgnoreCase(edgeType!))
            .Any();

        return match;
    }
}
