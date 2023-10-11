using System.Collections;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Data;

public class GraphEdgeIndex<TKey, TEdge> : IEnumerable<TEdge>
    where TKey : notnull
    where TEdge : IGraphEdge<TKey>
{
    private readonly Dictionary<Guid, TEdge> _index;
    private readonly SecondaryIndex<TKey, Guid> _edgesFrom;
    private readonly SecondaryIndex<TKey, Guid> _edgesTo;
    private readonly object _lock;

    public GraphEdgeIndex(object syncLock, IEqualityComparer<TKey>? keyComparer = null)
    {
        _lock = syncLock.NotNull();
        _index = new Dictionary<Guid, TEdge>();
        _edgesFrom = new SecondaryIndex<TKey, Guid>(keyComparer);
        _edgesTo = new SecondaryIndex<TKey, Guid>(keyComparer);
    }

    public TEdge this[Guid key]
    {
        get => _index[key];
        set
        {
            lock (_lock)
            {
                Remove(value.Key);
                Add(value);
            }
        }
    }

    public int Count => _index.Count;

    public void Add(TEdge edge)
    {
        edge.Verify();

        lock (_lock)
        {
            _index.Add(edge.Key, edge);
            _edgesFrom.Set(edge.FromNodeKey, edge.Key);
            _edgesTo.Set(edge.ToNodeKey, edge.Key);
        }
    }

    public bool ContainsKey(Guid edgeKey) => _index.ContainsKey(edgeKey);

    public IReadOnlyList<TEdge> Get(TKey nodeKey)
    {
        lock (_lock)
        {
            var from = _edgesFrom.Lookup(nodeKey);
            var to = _edgesTo.Lookup(nodeKey);

            var result = from.Concat(to).Select(x => _index[x]).ToArray();
            return result;
        }
    }

    public IReadOnlyList<TEdge> Get(TKey fromKey, TKey toKey)
    {
        lock (_lock)
        {
            var from = _edgesFrom.Lookup(fromKey);
            var to = _edgesTo.Lookup(toKey);

            var keys = from.Intersect(to).ToArray();

            var result = keys.Select(x => _index[x]).ToArray();
            return result;
        }
    }

    public bool Remove(Guid edgeKey)
    {
        lock (_lock)
        {
            if (!_index.Remove(edgeKey, out var nodeValue)) return false;

            _edgesFrom.RemovePrimaryKey(nodeValue.Key);
            _edgesTo.RemovePrimaryKey(nodeValue.Key);

            return true;
        }
    }

    public bool Remove(TKey nodeKey)
    {
        lock (_lock)
        {
            var from = _edgesFrom.Lookup(nodeKey);
            var to = _edgesTo.Lookup(nodeKey);

            var keys = from.Concat(to).ToArray();
            if (keys.Length == 0) return false;

            keys.ForEach(x => Remove(x).Assert(x => x == true, $"{x} failed to remove"));
            return true;
        }
    }

    public IReadOnlyList<TEdge> Remove(TKey fromKey, TKey toKey)
    {
        lock (_lock)
        {
            var from = _edgesFrom.Lookup(fromKey);
            var to = _edgesTo.Lookup(toKey);

            var keys = from.Intersect(to).ToArray();
            var result = keys.Select(x => _index[x]).ToArray();

            result.ForEach(x => Remove(x.Key).Assert(x => x == true, $"{x} failed to remove"));

            return result;
        }
    }

    public bool TryGetValue(Guid key, out TEdge? value) => _index.TryGetValue(key, out value);


    public IEnumerator<TEdge> GetEnumerator() => _index.Values.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
