using System.Collections;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Data;

public class GraphNodeIndex : IEnumerable<GraphNode>
{
    private readonly Dictionary<string, GraphNode> _index;
    private readonly Action<GraphNode> _removeEvent;
    private readonly object _lock;

    public GraphNodeIndex(object syncLock, Action<GraphNode> removeEvent)
    {
        _lock = syncLock.NotNull();
        _removeEvent = removeEvent.NotNull();
        _index = new Dictionary<string, GraphNode>(StringComparer.OrdinalIgnoreCase);
    }

    public GraphNode this[string key]
    {
        get => _index[key];

        set
        {
            lock (_lock)
            {
                Remove(key);
                Add(value);
            }
        }
    }

    public int Count => _index.Count;

    public Option Add(GraphNode node, bool upsert = false)
    {
        if (!node.Validate(out var v)) return v;

        lock (_lock)
        {
            Option option = _index.TryAdd(node.Key, node) switch
            {
                true => StatusCode.OK,
                false => (StatusCode.Conflict, $"Node key={node.Key} already exist"),
            };

            if (option.IsOk() || !upsert) return option;

            var readNode = _index[node.Key];
            readNode = readNode with
            {
                Tags = readNode.Tags.Clone().Set(node.Tags),
            };

            _index[node.Key] = readNode;

            return StatusCode.OK;
        }
    }

    public bool ContainsKey(string key) => _index.ContainsKey(key);

    public void Clear()
    {
        lock (_lock)
        {
            _index.Clear();
        }
    }

    public Option<GraphNode> Get(string nodeKey) => _index.TryGetValue(nodeKey, out var value) switch
    {
        true => value,
        false => StatusCode.NotFound
    };

    public bool Remove(string key) => Remove(key, out var _);

    public bool Remove(string key, out GraphNode? value)
    {
        lock (_lock)
        {
            bool state = _index.Remove(key, out value);
            if (state) _removeEvent(value.NotNull());
            return state;
        }
    }

    public Option Update(IReadOnlyList<GraphNode> query, Func<GraphNode, GraphNode> update)
    {
        query.NotNull();
        update.NotNull();
        if (query.Count == 0) return StatusCode.NoContent;

        lock (_lock)
        {
            query.ForEach(x =>
            {
                var n = update(x);
                x.Key.Equals(n.Key).Assert(x => x == true, "Cannot change the primary key");
                _index[x.Key] = n;
            });

            return StatusCode.OK;
        }
    }

    public bool TryGetValue(string key, out GraphNode? value) => _index.TryGetValue(key, out value);

    public IEnumerator<GraphNode> GetEnumerator() => _index.Values.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
