using System.Collections;
using System.Collections.Immutable;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;


public class GraphEdgeIndex : IEnumerable<GraphEdge>
{
    private readonly Dictionary<GraphEdgePrimaryKey, GraphEdge> _index;
    private readonly SecondaryIndex<string, GraphEdgePrimaryKey> _edgesFrom;
    private readonly SecondaryIndex<string, GraphEdgePrimaryKey> _edgesTo;
    private readonly Dictionary<string, GraphEdgePrimaryKey> _graphEdges;
    private readonly TagIndex<GraphEdgePrimaryKey> _tagIndex;
    private readonly object _lock;
    private readonly GraphRI _graphRI;

    internal GraphEdgeIndex(object syncLock, GraphRI graphRI, IEqualityComparer<string>? keyComparer = null)
    {
        _lock = syncLock.NotNull();
        _graphRI = graphRI.NotNull();

        _index = new Dictionary<GraphEdgePrimaryKey, GraphEdge>(GraphEdgePrimaryKeyComparer.Default);
        _edgesFrom = new SecondaryIndex<string, GraphEdgePrimaryKey>(keyComparer);
        _edgesTo = new SecondaryIndex<string, GraphEdgePrimaryKey>(keyComparer);
        _graphEdges = new Dictionary<string, GraphEdgePrimaryKey>(StringComparer.OrdinalIgnoreCase);
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
            _graphEdges[edge.EdgeType] = pk;

            graphContext?.ChangeLog.Push(new CmEdgeAdd(edge));
            return StatusCode.OK;
        }
    }

    public IReadOnlyList<GraphEdgePrimaryKey> LookupByNodeKey(IEnumerable<string> nodeKeys) => nodeKeys
        .SelectMany(x => _edgesFrom.Lookup(x))
        .Concat(nodeKeys.SelectMany(x => _edgesTo.Lookup(x)))
        .Distinct(GraphEdgePrimaryKeyComparer.Default)
        .ToImmutableArray();

    public IReadOnlyList<GraphEdgePrimaryKey> LookupByFromKey(IEnumerable<string> fromNodesKeys) => fromNodesKeys
        .SelectMany(x => _edgesFrom.Lookup(x))
        .Distinct(GraphEdgePrimaryKeyComparer.Default)
        .ToImmutableArray();

    public IReadOnlyList<GraphEdgePrimaryKey> LookupByToKey(IEnumerable<string> toNodesKeys) => toNodesKeys
        .SelectMany(x => _edgesTo.Lookup(x))
        .Distinct(GraphEdgePrimaryKeyComparer.Default)
        .ToImmutableArray();

    public Option Remove(GraphEdgePrimaryKey edgeKey, IGraphTrxContext? graphContext = null)
    {
        lock (_lock)
        {
            if (!_index.Remove(edgeKey, out var edgeValue)) return (StatusCode.NotFound, $"Key={edgeKey} does not exist");

            _edgesFrom.Remove(edgeValue.FromKey, edgeKey);
            _edgesTo.Remove(edgeValue.ToKey, edgeKey);
            _graphEdges.Remove(edgeKey.EdgeType);

            graphContext?.ChangeLog.Push(new CmEdgeDelete(edgeValue));
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
            graphContext?.ChangeLog.Push(new CmEdgeAdd(edge));

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
                    _graphEdges.Remove(readEdge.EdgeType);
                    _graphEdges[edge.EdgeType] = pk;
                }

                graphContext?.ChangeLog.Push(new CmEdgeChange(readEdge, edge));
                return StatusCode.OK;
            }
        }
    }

    public bool TryGetValue(GraphEdgePrimaryKey key, out GraphEdge? edge) => _index.TryGetValue(key, out edge);

    public IEnumerator<GraphEdge> GetEnumerator() => _index.Values.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


    private bool ValidateNodes(GraphEdge edge, out Option result)
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

        result = StatusCode.OK;
        return true;
    }
}

public static class GraphEdgeIndexExtensions
{
    public static IReadOnlyList<GraphEdge> Lookup(this GraphEdgeIndex subject, IEnumerable<GraphEdgePrimaryKey> keys) => keys.NotNull()
        .Select(x => subject.TryGetValue(x, out var data) ? data : null)
        .OfType<GraphEdge>()
        .ToImmutableArray();
}
