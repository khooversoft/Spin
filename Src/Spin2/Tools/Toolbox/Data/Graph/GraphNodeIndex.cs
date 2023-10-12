using System.Collections;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Data;

public class GraphNodeIndex<TKey, TNode> : IEnumerable<TNode>
    where TKey : notnull
    where TNode : IGraphNode<TKey>
{
    private readonly Dictionary<TKey, TNode> _index;
    private readonly Action<TNode> _removeEvent;
    private readonly object _lock;

    public GraphNodeIndex(object syncLock, Action<TNode> removeEvent, IEqualityComparer<TKey>? equalityComparer = null)
    {
        _lock = syncLock.NotNull();
        _removeEvent = removeEvent.NotNull();
        _index = new Dictionary<TKey, TNode>(equalityComparer.ComparerFor());
    }

    public TNode this[TKey key]
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

    public Option Add(TNode node)
    {
        lock (_lock)
        {
            return _index.TryAdd(node.Key, node) switch
            {
                true => StatusCode.OK,
                false => (StatusCode.Conflict, $"Node key={node.Key} already exist"),
            };
        }
    }

    public bool ContainsKey(TKey key) => _index.ContainsKey(key);

    public Option<TNode> Get(TKey nodeKey) => _index.TryGetValue(nodeKey, out var value) switch
    {
        true => value,
        false => StatusCode.NotFound
    };

    public bool Remove(TKey key) => Remove(key, out var _);

    public bool Remove(TKey key, out TNode? value)
    {
        lock (_lock)
        {
            bool state = _index.Remove(key, out value);
            if (state) _removeEvent(value.NotNull());
            return state;
        }
    }

    public bool TryGetValue(TKey key, out TNode? value) => _index.TryGetValue(key, out value);

    public IEnumerator<TNode> GetEnumerator() => _index.Values.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
