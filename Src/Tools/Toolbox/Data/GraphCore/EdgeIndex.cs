using System.Collections;
using System.Collections.Concurrent;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Data;

public partial class EdgeIndex : IEnumerable<Edge>
{
    private readonly ConcurrentDictionary<string, Edge> _edges = new(StringComparer.OrdinalIgnoreCase);
    private readonly SecondaryIndex<string, string> _fromEdges = new(StringComparer.OrdinalIgnoreCase);
    private readonly SecondaryIndex<string, string> _toEdges = new(StringComparer.OrdinalIgnoreCase);
    private readonly SecondaryIndex<string, string> _typeEdges = new(StringComparer.OrdinalIgnoreCase);
    private readonly ReaderWriterLockSlim _lock;
    private readonly DgInternalCalls _calls;

    public EdgeIndex(IEnumerable<Edge> edges, DgInternalCalls calls, ReaderWriterLockSlim lockSlim)
    {
        edges.NotNull();
        _calls = calls.NotNull();
        _lock = lockSlim.NotNull();

        _calls.NodeDeleted = InternalNodeRemoved;
        _calls.ClearEdges = InternalClear;

        var n = edges.ToArray();
        foreach (var edge in n) AddOrUpdate(edge, true).ThrowOnError();
        _calls = calls;
    }

    public int Count
    {
        get
        {
            _lock.EnterReadLock();
            try { return _edges.Count; }
            finally { _lock.ExitReadLock(); }
        }
    }

    public Option AddOrUpdate(Edge updatedEdge)
    {
        _lock.EnterWriteLock();
        try { return AddOrUpdate(updatedEdge, false); }
        finally { _lock.ExitWriteLock(); }
    }

    public bool Contains(string edgeKey)
    {
        _lock.EnterReadLock();
        try { return _edges.ContainsKey(edgeKey.NotEmpty()); }
        finally { _lock.ExitReadLock(); }
    }

    public Option Remove(string edgeKey)
    {
        _lock.EnterWriteLock();
        try { return InternalRemove(edgeKey); }
        finally { _lock.ExitWriteLock(); }
    }

    public Option TryAdd(Edge edge)
    {
        _lock.EnterWriteLock();
        try { return AddOrUpdate(edge, true); }
        finally { _lock.ExitWriteLock(); }
    }

    public IEnumerator<Edge> GetEnumerator()
    {
        _lock.EnterReadLock();
        try
        {
            var snapshot = _edges.Values.ToList();
            return snapshot.GetEnumerator();
        }
        finally { _lock.ExitReadLock(); }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private Option AddOrUpdate(Edge edge, bool isAdd)
    {
        edge.NotNull();

        if (!_calls.IsNodeExist(edge.FromKey)) return (StatusCode.NotFound, $"FromKey {edge.FromKey} does not exist.");
        if (!_calls.IsNodeExist(edge.ToKey)) return (StatusCode.NotFound, $"ToKey {edge.ToKey} does not exist.");

        Option option = default;

        var result = _edges.AddOrUpdate(edge.EdgeKey, add, update);
        return option;


        Edge add(string key)
        {
            _fromEdges.Set(edge.FromKey, edge.EdgeKey);
            _toEdges.Set(edge.ToKey, edge.EdgeKey);
            _typeEdges.Set(edge.EdgeType, edge.EdgeKey);

            _calls.GetRecorder()?.Add(edge.EdgeKey, edge);
            option = StatusCode.OK;
            return edge;
        }

        Edge update(string key, Edge current)
        {
            if (isAdd)
            {
                option = (StatusCode.Conflict, $"EdgeKey {edge.EdgeKey} already exists.");
                return current;
            }

            if (current.FromKey != edge.FromKey)
            {
                _fromEdges.Remove(current.FromKey, current.EdgeKey);
                _fromEdges.Set(edge.FromKey, edge.EdgeKey);
            }
            if (current.ToKey != edge.ToKey)
            {
                _toEdges.Remove(current.ToKey, current.EdgeKey);
                _toEdges.Set(edge.ToKey, edge.EdgeKey);
            }
            if (current.EdgeType != edge.EdgeType)
            {
                _typeEdges.Remove(current.EdgeType, current.EdgeKey);
                _typeEdges.Set(edge.EdgeType, edge.EdgeKey);
            }

            _calls.GetRecorder()?.Update(edge.EdgeKey, current, edge);
            option = StatusCode.OK;
            return edge;
        }
    }

    private void InternalClear()
    {
        _edges.Clear();
        _fromEdges.Clear();
        _toEdges.Clear();
        _typeEdges.Clear();
    }

    private Option InternalNodeRemoved(string nodeKey)
    {
        var fromEdges = _fromEdges.Lookup(nodeKey);
        var toEdges = _toEdges.Lookup(nodeKey);
        var edgeKeys = fromEdges.Concat(toEdges).Distinct().ToArray();

        foreach (var edgeKey in edgeKeys)
        {
            var result = InternalRemove(edgeKey);
            if (result.IsError()) return result;
        }

        return StatusCode.OK;
    }

    private Option InternalRemove(string edgeKey)
    {
        edgeKey.NotNull();

        if (_edges.TryRemove(edgeKey, out var existing))
        {
            _calls.GetRecorder()?.Delete(existing.EdgeKey, existing);

            _fromEdges.Remove(existing.FromKey, existing.EdgeKey);
            _toEdges.Remove(existing.ToKey, existing.EdgeKey);
            _typeEdges.Remove(existing.EdgeType, existing.EdgeKey);

            return StatusCode.OK;
        }

        return StatusCode.NotFound;
    }

    private IReadOnlyList<Edge> SelectEdges(Func<IReadOnlyList<Edge>> getList)
    {
        _lock.EnterReadLock();
        try { return getList(); }
        finally { _lock.ExitReadLock(); }
    }
}

public static class EdgeIndexTool
{
    public static Option Add(this EdgeIndex edgeIndex, string fromKey, string toKey, string edgeType, DataETag? payload = null)
    {
        var edge = new Edge(fromKey, toKey, edgeType, payload);
        return edgeIndex.TryAdd(edge);
    }
}
