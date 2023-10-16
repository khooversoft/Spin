using System.Collections;
using Toolbox.Extensions;
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

    public void Clear()
    {
        lock (_lock)
        {
            _index.Clear();
        }
    }

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

    public IReadOnlyList<TNode> Query(GraphNodeQuery<TKey> query)
    {
        query.NotNull();

        lock (_lock)
        {
            IEnumerable<TNode> result = (query.Key, query.MatchTags) switch
            {
                (null, null) => _index.Values.Select(x => x),
                (TKey nodeKey, null) => _index.TryGetValue(nodeKey, out var v) ? v.ToEnumerable() : Array.Empty<TNode>(),
                (null, string tags) => _index.Values.Where(x => x.Tags.Has(tags)),

                (TKey nodeKey, string tags) => _index.TryGetValue(nodeKey, out var v) switch
                {
                    false => Array.Empty<TNode>(),
                    true => v.Tags.Has(tags) ? v.ToEnumerable() : Array.Empty<TNode>(),
                },
            };

            return result.ToArray();
        }
    }

    public Option Update(GraphNodeQuery<TKey> query, Func<TNode, TNode> update)
    {
        update.NotNull();

        lock (_lock)
        {
            var result = Query(query);
            if (result.Count == 0) return StatusCode.NotFound;

            result.ForEach(x =>
            {
                var n = update(x);
                x.Key.Equals(n.Key).Assert(x => x == true, "Cannot change the primary key");
                _index[x.Key] = n;
            });

            return StatusCode.OK;
        }
    }

    public bool TryGetValue(TKey key, out TNode? value) => _index.TryGetValue(key, out value);

    public IEnumerator<TNode> GetEnumerator() => _index.Values.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
