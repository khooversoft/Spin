using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Data;

public class NodeIndex : IEnumerable<Node>
{
    private readonly ConcurrentDictionary<string, Node> _nodes = new(StringComparer.OrdinalIgnoreCase);
    private readonly ReaderWriterLockSlim _lock;
    private readonly DgInternalCalls _calls;

    public NodeIndex(IEnumerable<Node> nodes, DgInternalCalls calls, ReaderWriterLockSlim lockSlim)
    {
        nodes.NotNull();
        _calls = calls.NotNull();
        _lock = lockSlim.NotNull();

        _calls.IsNodeExist = key => _nodes.ContainsKey(key);
        _calls.ClearNodes = () => _nodes.Clear();

        var n = nodes.ToArray();
        foreach (var node in n) InternalAddOrUpdate(node, isAdd: true).ThrowOnError();
    }

    public int Count
    {
        get
        {
            _lock.EnterReadLock();
            try { return _nodes.Count; }
            finally { _lock.ExitReadLock(); }
        }
    }

    public Option AddOrUpdate(Node updatedNode)
    {
        _lock.EnterWriteLock();
        try { return InternalAddOrUpdate(updatedNode, isAdd: false); }
        finally { _lock.ExitWriteLock(); }
    }

    public bool ContainsKey(string nodeKey)
    {
        nodeKey.NotEmpty();
        _lock.EnterReadLock();
        try { return _nodes.ContainsKey(nodeKey); }
        finally { _lock.ExitReadLock(); }
    }

    public IReadOnlyList<Node> GetNodes(params IEnumerable<string> nodeKeys)
    {
        var keys = nodeKeys.NotNull().ToArray();

        _lock.EnterReadLock();
        try
        {
            var list = new Sequence<Node>();
            foreach (var key in keys)
            {
                if (_nodes.TryGetValue(key, out var node)) list += node;
            }

            return list;
        }
        finally { _lock.ExitReadLock(); }
    }

    public Option Remove(string nodeKey)
    {
        _lock.EnterWriteLock();
        try
        {
            nodeKey.NotNull();

            if (_nodes.TryRemove(nodeKey, out var existing))
            {
                var nodeDelete = _calls.NodeDeleted(nodeKey);
                if (nodeDelete.IsError()) return nodeDelete;

                _calls.GetRecorder()?.Delete(existing.NodeKey, existing);
                return StatusCode.OK;
            }

            return StatusCode.NotFound;
        }
        finally { _lock.ExitWriteLock(); }
    }

    public Option TryAdd(Node node)
    {
        _lock.EnterWriteLock();
        try
        {
            if (_nodes.ContainsKey(node.NodeKey)) return StatusCode.Conflict;
            return InternalAddOrUpdate(node, isAdd: true);
        }
        finally { _lock.ExitWriteLock(); }
    }

    public bool TryGetValue(string nodeKey, [NotNullWhen(true)] out Node? value)
    {
        _lock.EnterReadLock();
        try { return _nodes.TryGetValue(nodeKey.NotEmpty(), out value); }
        finally { _lock.ExitReadLock(); }
    }

    public IEnumerator<Node> GetEnumerator()
    {
        _lock.EnterReadLock();
        try { return _nodes.Values.ToList().GetEnumerator(); }
        finally { _lock.ExitReadLock(); }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private Option InternalAddOrUpdate(Node node, bool isAdd)
    {
        Option result = default;

        _ = _nodes.AddOrUpdate(node.NodeKey, add, update);
        return result;


        Node add(string key)
        {
            _calls.GetRecorder()?.Add(node.NodeKey, node);
            result = StatusCode.OK;
            return node;
        }

        Node update(string key, Node existing)
        {
            if (isAdd)
            {
                result = StatusCode.Conflict;
                return existing;
            }

            var updated = existing with { Payload = node.Payload };
            _calls.GetRecorder()?.Update(existing.NodeKey, existing, updated);

            result = StatusCode.OK;
            return updated;
        }
    }
}

public static class NodeIndexTool
{
    public static Option Add(this NodeIndex nodeIndex, string key, DataETag? payload = null)
    {
        nodeIndex.NotNull();
        var node = new Node(key, payload);
        return nodeIndex.TryAdd(node);
    }
}
