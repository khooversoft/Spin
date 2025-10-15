using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using Toolbox.Tools;

namespace Toolbox.Data;

/// <summary>
/// Thread-safe inverted index mapping TKey -> set of TReferenceKey using
/// ConcurrentDictionary for both outer and per-key inner maps (set semantics via inner keys).
/// Optimized for high concurrency without global locks.
/// </summary>
public class InvertedIndex<TKey, TReferenceKey> : IEnumerable<KeyValuePair<TKey, TReferenceKey>>
    where TKey : notnull
    where TReferenceKey : notnull
{
    private readonly ConcurrentDictionary<TKey, ConcurrentDictionary<TReferenceKey, byte>> _index;

    public InvertedIndex(IEqualityComparer<TKey>? keyComparer = null, IEqualityComparer<TReferenceKey>? referenceComparer = null)
    {
        KeyComparer = keyComparer.EqualityComparerFor();
        ReferenceComparer = referenceComparer.EqualityComparerFor();

        _index = new ConcurrentDictionary<TKey, ConcurrentDictionary<TReferenceKey, byte>>(KeyComparer);
    }

    public int Count => _index.Count;
    public IReadOnlyList<TReferenceKey> this[TKey key] => Get(key);
    public IEqualityComparer<TKey>? KeyComparer { get; }
    public IEqualityComparer<TReferenceKey>? ReferenceComparer { get; }

    public void Clear() => _index.Clear();

    public IReadOnlyList<TReferenceKey> Get(TKey key) => _index.TryGetValue(key, out var inner)
        ? inner.Keys.ToImmutableArray()
        : Array.Empty<TReferenceKey>();

    public bool TryGetValue(TKey key, out IReadOnlyList<TReferenceKey>? value)
    {
        if (_index.TryGetValue(key, out var inner))
        {
            value = inner.Keys.ToImmutableArray();
            return true;
        }

        value = null;
        return false;
    }

    public InvertedIndex<TKey, TReferenceKey> Set(TKey key, TReferenceKey referenceKey)
    {
        var inner = _index.GetOrAdd(key, k => new ConcurrentDictionary<TReferenceKey, byte>());
        inner.TryAdd(referenceKey, 0);
        return this;
    }

    public IReadOnlyList<TReferenceKey> Remove(TKey key)
    {
        if (_index.TryRemove(key, out var inner))
        {
            return inner.Keys.ToImmutableArray();
        }

        return Array.Empty<TReferenceKey>();
    }

    public bool Remove(TKey key, TReferenceKey referenceKey)
    {
        if (!_index.TryGetValue(key, out var inner)) return false;

        var removed = inner.TryRemove(referenceKey, out _);

        // Cleanup empty inner map; remove only if the same instance (avoid races).
        if (removed && inner.IsEmpty)
        {
            _index.TryRemove(new KeyValuePair<TKey, ConcurrentDictionary<TReferenceKey, byte>>(key, inner));
        }

        return removed;
    }

    public IEnumerator<KeyValuePair<TKey, TReferenceKey>> GetEnumerator()
    {
        // Enumerations over ConcurrentDictionary are thread-safe and represent a moment-in-time snapshot.
        foreach (var kv in _index)
        {
            var key = kv.Key;
            var inner = kv.Value;

            foreach (var refKey in inner.Keys)
            {
                yield return new KeyValuePair<TKey, TReferenceKey>(key, refKey);
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
