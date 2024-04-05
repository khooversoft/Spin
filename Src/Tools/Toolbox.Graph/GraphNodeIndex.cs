using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

[assembly: InternalsVisibleTo("Toolbox.Graph.test")]

namespace Toolbox.Graph;

public class GraphNodeIndex : IEnumerable<GraphNode>
{
    private readonly Dictionary<string, GraphNode> _index;
    private readonly object _lock;
    private readonly GraphRI _graphRI;

    internal GraphNodeIndex(object syncLock, GraphRI graphRI)
    {
        _lock = syncLock.NotNull();
        _graphRI = graphRI.NotNull();
        _index = new Dictionary<string, GraphNode>(StringComparer.OrdinalIgnoreCase);
    }

    public GraphNode this[string key]
    {
        get => _index[key];
        internal set => Set(value).ThrowOnError();
    }

    public int Count => _index.Count;

    internal Option Add(GraphNode node, GraphChangeContext? graphContext = null)
    {
        if (!node.Validate(out var v)) return v;

        lock (_lock)
        {
            Option option = _index.TryAdd(node.Key, node) switch
            {
                true => StatusCode.OK,
                false => (StatusCode.Conflict, $"Node key={node.Key} already exist"),
            };

            if (option.IsOk())
            {
                graphContext?.ChangeLog.Push(new NodeChange(node.Key, null), graphContext);
            }

            return option;
        }
    }

    internal Option Set(GraphNode node, GraphChangeContext? graphContext = null)
    {
        if (!node.Validate(out var v)) return v;

        lock (_lock)
        {
            GraphNode? current;
            if (_index.TryGetValue(node.Key, out current))
            {
                node = current.WithMerged(node);
            }

            _index[node.Key] = node;
            graphContext?.ChangeLog.Push(new NodeChange(node.Key, current), graphContext);

            return StatusCode.OK;
        }
    }

    public bool ContainsKey(string key) => _index.ContainsKey(key);

    internal void Clear()
    {
        lock (_lock)
        {
            _index.Clear();
        }
    }

    internal bool Remove(string key, GraphChangeContext? graphContext = null)
    {
        lock (_lock)
        {
            bool removed = _index.Remove(key, out var oldValue);
            if (removed)
            {
                _graphRI.RemovedNodeFromEdges(oldValue!, graphContext);
                graphContext?.ChangeLog.Push(new NodeDelete(oldValue!), graphContext);
            }

            return removed;
        }
    }

    public bool TryGetValue(string key, [NotNullWhen(true)] out GraphNode? value) => _index.TryGetValue(key, out value);

    internal bool TryUpdate(string key, Func<GraphNode, GraphNode> update, GraphChangeContext? graphContext = null)
    {
        lock (_lock)
        {
            if (!_index.TryGetValue(key, out var current)) return false;

            GraphNode newValue = update(current);
            current.Key.Equals(newValue.Key).Assert(x => x == true, "Cannot change the primary key");
            _index[key] = newValue;

            graphContext?.ChangeLog.Push(new NodeChange(key, current), graphContext);
            return true;
        }
    }

    internal Option Update(IReadOnlyList<GraphNode> query, Func<GraphNode, GraphNode> update, GraphChangeContext? graphContext = null)
    {
        query.NotNull();
        update.NotNull();
        if (query.Count == 0) return StatusCode.NoContent;

        lock (_lock)
        {
            query.ForEach(x =>
            {
                _index.ContainsKey(x.Key).Assert(x => x == true, $"Node key={x.Key} does not exist");

                var newValue = update(x);
                x.Key.Equals(newValue.Key).Assert(x => x == true, "Cannot change the primary key");
                _index[x.Key] = newValue;

                graphContext?.ChangeLog.Push(new NodeChange(x.Key, x), graphContext);
            });

            return StatusCode.OK;
        }
    }


    public IEnumerator<GraphNode> GetEnumerator() => _index.Values.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
