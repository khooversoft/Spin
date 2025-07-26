using System.Collections;
using System.Collections.Immutable;
using Toolbox.Tools;

namespace Toolbox.Data;

/// <summary>
/// The InvertedIndex<TKey, TReferenceKey> class is a thread-safe, generic data structure designed
/// to map keys (TKey) to a set of reference keys (TReferenceKey). It is commonly used in scenarios 
/// where you need to efficiently look up relationships between entities, such as indexing or graph-based applications.
/// </summary>
/// <typeparam name="TKey">Lookup key</typeparam>
/// <typeparam name="TReferenceKey">reference key</typeparam>
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
                true => pkeys.ToImmutableArray(),
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
                value = pkeys.ToImmutableArray();
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
            return pkeys.ToImmutableArray();
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
