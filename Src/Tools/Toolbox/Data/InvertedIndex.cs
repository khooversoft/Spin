using System.Collections;
using Toolbox.Tools;

namespace Toolbox.Data;

public class InvertedIndex<TKey, TReferenceKey> : IEnumerable<KeyValuePair<TKey, TReferenceKey>>
    where TKey : notnull
    where TReferenceKey : notnull
{
    private readonly object _lock = new object();
    private readonly Dictionary<TKey, HashSet<TReferenceKey>> _index;

    public InvertedIndex(IEqualityComparer<TKey>? keyComparer = null, IEqualityComparer<TReferenceKey>? referenceComparer = null)
    {
        KeyComparer = keyComparer.EqualityComparerFor();
        ReferenceComparer = referenceComparer.EqualityComparerFor();

        _index = new Dictionary<TKey, HashSet<TReferenceKey>>(KeyComparer);
    }

    public int Count => _index.Count;
    public IReadOnlyList<TReferenceKey> this[TKey key] => Get(key);
    public IEqualityComparer<TKey>? KeyComparer { get; }
    public IEqualityComparer<TReferenceKey>? ReferenceComparer { get; }

    public void Clear()
    {
        lock (_lock)
        {
            _index.Clear();
        }
    }

    public IReadOnlyList<TReferenceKey> Get(TKey key)
    {
        lock (_lock)
        {
            return _index.TryGetValue(key, out HashSet<TReferenceKey>? pkeys) switch
            {
                true => pkeys.ToArray(),
                false => Array.Empty<TReferenceKey>(),
            };
        }
    }

    public bool TryGetValue(TKey key, out IReadOnlyList<TReferenceKey>? value)
    {
        value = null;

        lock (_lock)
        {
            if (_index.TryGetValue(key, out HashSet<TReferenceKey>? pkeys))
            {
                value = pkeys.ToArray();
                return true;
            }

            return false;
        }
    }

    public InvertedIndex<TKey, TReferenceKey> Set(TKey key, TReferenceKey referenceKey)
    {
        lock (_lock)
        {
            if (_index.TryGetValue(key, out HashSet<TReferenceKey>? pkeys))
            {
                pkeys.Add(referenceKey);
                return this;
            }

            _index.Add(key, new HashSet<TReferenceKey>(new[] { referenceKey }, ReferenceComparer));
            return this;
        }
    }

    public IReadOnlyList<TReferenceKey> Remove(TKey key)
    {
        lock (_lock)
        {
            if (!_index.TryGetValue(key, out HashSet<TReferenceKey>? pkeys)) return Array.Empty<TReferenceKey>();

            _index.Remove(key);
            return pkeys.ToArray();
        }
    }

    public bool Remove(TKey key, TReferenceKey referenceKey)
    {
        lock (_lock)
        {
            if (!_index.TryGetValue(key, out HashSet<TReferenceKey>? pkeys)) return false;

            bool deleted = pkeys.Remove(referenceKey);
            if (pkeys.Count == 0) _index.Remove(key);

            return deleted;
        }
    }

    public IEnumerator<KeyValuePair<TKey, TReferenceKey>> GetEnumerator()
    {
        foreach (var item in _index)
        {
            foreach (var refItem in item.Value)
            {
                yield return new KeyValuePair<TKey, TReferenceKey>(item.Key, refItem);
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
