using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;


public class GraphEdgeIndex : IEnumerable<GraphEdge>
{
    private readonly ConcurrentDictionary<GraphEdgePrimaryKey, GraphEdge> _index;
    private readonly SecondaryIndex<string, GraphEdgePrimaryKey> _edgesFrom;
    private readonly SecondaryIndex<string, GraphEdgePrimaryKey> _edgesTo;
    private readonly SecondaryIndex<string, GraphEdgePrimaryKey> _edgesEdges;
    private readonly TagIndex<GraphEdgePrimaryKey> _tagIndex;
    private readonly object _lock;
    private readonly GraphMap _map;

    internal GraphEdgeIndex(GraphMap map, object syncLock, IEqualityComparer<string>? keyComparer = null)
    {
        _map = map.NotNull();
        _lock = syncLock.NotNull();

        _index = new ConcurrentDictionary<GraphEdgePrimaryKey, GraphEdge>(GraphEdgePrimaryKeyComparer.Default);
        _edgesFrom = new SecondaryIndex<string, GraphEdgePrimaryKey>(keyComparer);
        _edgesTo = new SecondaryIndex<string, GraphEdgePrimaryKey>(keyComparer);
        _edgesEdges = new SecondaryIndex<string, GraphEdgePrimaryKey>(keyComparer);
        _tagIndex = new TagIndex<GraphEdgePrimaryKey>(GraphEdgePrimaryKeyComparer.Default);
    }

    public int Count => _index.Count;

    public GraphEdge this[GraphEdgePrimaryKey key]
    {
        get => _index[key];
        set => Set(value).ThrowOnError();
    }

    public Option Add(GraphEdge edge, IGraphTrxContext? graphContext = null)
    {
        if (!edge.Validate(out var v1)) return v1;

        lock (_lock)
        {
            if (!ValidateNodes(edge, out var v2)) return v2;

            var pk = edge.GetPrimaryKey();
            if (!_index.TryAdd(pk, edge)) return (StatusCode.Conflict, $"key={pk} already exist");

            _edgesFrom.Set(edge.FromKey, pk);
            _edgesTo.Set(edge.ToKey, pk);
            _edgesEdges.Set(edge.EdgeType, pk);
            _tagIndex.Set(pk, edge.Tags);

            graphContext?.ChangeLog.Push(new CmEdgeAdd(edge));
            _map.Meter.Edge.Added();
            return StatusCode.OK;
        }
    }

    public bool ContainsKey(GraphEdgePrimaryKey key) => _index.ContainsKey(key);

    public IReadOnlyList<GraphEdge> Get(IEnumerable<GraphEdgePrimaryKey> keys) => keys
        .Distinct(GraphEdgePrimaryKeyComparer.Default)
        .Select(x => TryGetValue(x, out var data) ? data : null)
        .OfType<GraphEdge>()
        .ToImmutableArray();

    public IReadOnlyList<GraphEdgePrimaryKey> LookupTag(string tag) => _tagIndex.Lookup(tag)
        .Action(x => _map.Meter.Edge.Index(x.Count > 0));

    public IReadOnlyList<GraphEdgePrimaryKey> LookupByNodeKey(IEnumerable<string> nodeKeys) => nodeKeys
        .SelectMany(x => _edgesFrom.Lookup(x))
        .Concat(nodeKeys.SelectMany(x => _edgesTo.Lookup(x)))
        .ToImmutableArray()
        .Action(x => _map.Meter.Edge.Index(x.Length > 0));

    public IReadOnlyList<GraphEdgePrimaryKey> LookupByFromKey(IEnumerable<string> fromNodesKeys) => fromNodesKeys
        .SelectMany(x => _edgesFrom.Lookup(x))
        .ToImmutableArray()
        .Action(x => _map.Meter.Edge.Index(x.Length > 0));

    public IReadOnlyList<GraphEdgePrimaryKey> LookupByToKey(IEnumerable<string> toNodesKeys) => toNodesKeys
        .SelectMany(x => _edgesTo.Lookup(x))
        .ToImmutableArray()
        .Action(x => _map.Meter.Edge.Index(x.Length > 0));

    public IReadOnlyList<GraphEdgePrimaryKey> LookupByEdgeType(IEnumerable<string> edgeTypes) => edgeTypes
        .SelectMany(x => _edgesEdges.Lookup(x))
        .ToImmutableArray()
        .Action(x => _map.Meter.Edge.Index(x.Length > 0));

    public Option Remove(GraphEdgePrimaryKey edgeKey, IGraphTrxContext? graphContext = null)
    {
        lock (_lock)
        {
            if (!_index.Remove(edgeKey, out var edgeValue)) return (StatusCode.NotFound, $"Key={edgeKey} does not exist");

            _edgesFrom.Remove(edgeValue.FromKey, edgeKey);
            _edgesTo.Remove(edgeValue.ToKey, edgeKey);
            _edgesEdges.Remove(edgeKey.EdgeType, edgeKey);
            _tagIndex.Remove(edgeKey);

            graphContext?.ChangeLog.Push(new CmEdgeDelete(edgeValue));
            _map.Meter.Edge.Deleted();
            return StatusCode.OK;
        }
    }

    public Option RemoveNodes(IEnumerable<string> nodeKeys, IGraphTrxContext? graphContext = null)
    {
        lock (_lock)
        {
            var edgePrimaryKeys = LookupByNodeKey(nodeKeys.ToArray());

            edgePrimaryKeys
                .ForEach(x => Remove(x, graphContext)
                .Assert(x => x.IsError(), $"{x} failed to remove"));

            return StatusCode.OK;
        }
    }

    public Option Set(GraphEdge edge, IGraphTrxContext? graphContext = null)
    {
        lock (_lock)
        {
            if (!ValidateNodes(edge, out var v2)) return v2;

            var pk = edge.GetPrimaryKey();
            var addResult = tryAdd(pk, edge);
            if (addResult.IsOk()) return addResult;

            var updateResult = update(edge, graphContext);
            return updateResult;
        }

        StatusCode tryAdd(GraphEdgePrimaryKey pk, GraphEdge edge)
        {
            if (!_index.TryAdd(pk, edge)) return StatusCode.NotFound;

            _edgesFrom.Set(edge.FromKey, pk);
            _edgesTo.Set(edge.ToKey, pk);
            _edgesEdges.Set(edge.EdgeType, pk);
            _tagIndex.Set(pk, edge.Tags);

            graphContext?.ChangeLog.Push(new CmEdgeAdd(edge));
            _map.Meter.Edge.Added();
            return StatusCode.OK;
        }

        Option update(GraphEdge edge, IGraphTrxContext? graphContext = null)
        {
            lock (_lock)
            {
                if (!ValidateNodes(edge, out var v2)) return v2;

                var pk = edge.GetPrimaryKey();
                if (!_index.ContainsKey(pk)) return (StatusCode.NotFound, $"Key={pk} does not exist");

                var readEdge = _index[pk];

                _index[pk] = edge;
                _tagIndex.Set(pk, edge.Tags);

                if (edge.FromKey != readEdge.FromKey)
                {
                    _edgesFrom.Remove(readEdge.FromKey, pk);
                    _edgesFrom.Set(edge.FromKey, pk);
                }

                if (edge.ToKey != readEdge.ToKey)
                {
                    _edgesTo.Remove(readEdge.ToKey, pk);
                    _edgesTo.Set(edge.ToKey, pk);
                }

                if (edge.EdgeType != readEdge.EdgeType)
                {
                    _edgesTo.Remove(readEdge.EdgeType, pk);
                    _edgesTo.Set(edge.EdgeType, pk);
                }

                graphContext?.ChangeLog.Push(new CmEdgeChange(readEdge, edge));
                _map.Meter.Edge.Updated();
                return StatusCode.OK;
            }
        }
    }

    public bool TryGetValue(GraphEdgePrimaryKey key, out GraphEdge? edge) => _index.TryGetValue(key, out edge).Action(x => _map.Meter.Edge.Index(x));

    public IEnumerator<GraphEdge> GetEnumerator() => _index.Values.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private bool ValidateNodes(GraphEdge edge, out Option result)
    {
        if (!_map.Nodes.ContainsKey(edge.FromKey))
        {
            result = (StatusCode.NotFound, $"Cannot add edge, FromNodeKey={edge.FromKey} does not exist");
            return false;
        }

        if (!_map.Nodes.ContainsKey(edge.ToKey))
        {
            result = (StatusCode.NotFound, $"Cannot add edge, ToNodeKey={edge.ToKey} does not exist");
            return false;
        }

        result = StatusCode.OK;
        return true;
    }
}

public static class GraphEdgeIndexExtensions
{
    public static IReadOnlyList<GraphEdge> LookupTagExpand(this GraphEdgeIndex index, string tag) => index.NotNull()
        .LookupTag(tag)
        .Func(x => index.Get(x));

    public static IReadOnlyList<GraphEdge> LookupByNodeKeyExpand(this GraphEdgeIndex index, IEnumerable<string> nodeKeys) => index.NotNull()
        .LookupByNodeKey(nodeKeys)
        .Func(x => index.Get(x));

    public static IReadOnlyList<GraphEdge> LookupByFromKeyExpand(this GraphEdgeIndex index, IEnumerable<string> fromNodesKeys) => index.NotNull()
        .LookupByFromKey(fromNodesKeys)
        .Func(x => index.Get(x));

    public static IReadOnlyList<GraphEdge> LookupByToKeyExpand(this GraphEdgeIndex index, IEnumerable<string> toNodesKeys) => index.NotNull()
        .LookupByToKey(toNodesKeys)
        .Func(x => index.Get(x));

    public static IReadOnlyList<GraphEdge> LookupByEdgeTypeExpand(this GraphEdgeIndex index, IEnumerable<string> fromNodesKeys) => index.NotNull()
        .LookupByEdgeType(fromNodesKeys)
        .Func(x => index.Get(x));
}
