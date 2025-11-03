using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Toolbox.Tools;

namespace Toolbox.Data;

/// <summary>
/// Thread-safe inverted index mapping TKey -> set of TReferenceKey using
/// ConcurrentDictionary for both outer and per-key inner maps (set semantics via inner keys).
/// Optimized for high concurrency without global locks.
/// 
/// Per-key synchronization avoids races between Set / Remove(key) / Remove(key, ref),
/// and a global RW gate ensures Clear does not interleave with mutating operations.
/// Lock ordering: always take the global gate first, then the per-key lock.
/// </summary>
public class OneToManyIndex<TKey, TReferenceKey> : IEnumerable<KeyValuePair<TKey, TReferenceKey>>
    where TKey : notnull
    where TReferenceKey : notnull
{
    private readonly ConcurrentDictionary<TKey, ConcurrentHashSet<TReferenceKey>> _index;
    private readonly ConcurrentDictionary<TKey, object> _locks;
    public IEqualityComparer<TKey>? _keyComparer { get; }
    public IEqualityComparer<TReferenceKey>? _referenceComparer { get; }

    // Global gate: Clear() takes write; Set/Remove take read. Reads (Get/TryGetValue/Enumerator) stay lock-free.
    private readonly ReaderWriterLockSlim _gate = new(LockRecursionPolicy.NoRecursion);

    public OneToManyIndex(IEqualityComparer<TKey>? keyComparer = null, IEqualityComparer<TReferenceKey>? referenceComparer = null)
    {
        _keyComparer = keyComparer.EqualityComparerFor();
        _referenceComparer = referenceComparer.EqualityComparerFor();

        _index = new ConcurrentDictionary<TKey, ConcurrentHashSet<TReferenceKey>>(_keyComparer);
        _locks = new ConcurrentDictionary<TKey, object>(_keyComparer);
    }

    public int Count => _index.Count;
    public IReadOnlyList<TReferenceKey> this[TKey key] => Get(key);

    public void Clear()
    {
        // Exclusively block all Set/Remove while clearing.
        _gate.EnterWriteLock();
        try
        {
            // No need to loop per-key: with the write lock held there are no concurrent mutators,
            // and no per-key locks can be held (since mutators must first acquire the read lock).
            _index.Clear();
            _locks.Clear();
        }
        finally
        {
            _gate.ExitWriteLock();
        }
    }

    public bool ContainsKey(TKey item) => _index.ContainsKey(item);

    public IReadOnlyList<TReferenceKey> Get(TKey key) => _index.TryGetValue(key, out var inner)
        ? inner.ToImmutableArray()
        : Array.Empty<TReferenceKey>();

    public bool TryGetValue(TKey key, [NotNullWhen(true)] out IReadOnlyList<TReferenceKey>? value)
    {
        if (_index.TryGetValue(key, out var inner))
        {
            value = inner.ToImmutableArray();
            return true;
        }

        value = null;
        return false;
    }

    public OneToManyIndex<TKey, TReferenceKey> Set(TKey key, TReferenceKey referenceKey)
    {
        // Gate read side: allows concurrency among mutators but blocks when Clear holds write.
        _gate.EnterReadLock();
        try
        {
            var sync = _locks.GetOrAdd(key, static _ => new object());
            lock (sync)
            {
                var inner = _index.GetOrAdd(key, _ => new ConcurrentHashSet<TReferenceKey>(_referenceComparer));
                inner.TryAdd(referenceKey);
            }
            return this;
        }
        finally
        {
            _gate.ExitReadLock();
        }
    }

    public IReadOnlyList<TReferenceKey> Remove(TKey key)
    {
        _gate.EnterReadLock();
        try
        {
            var sync = _locks.GetOrAdd(key, static _ => new object());
            lock (sync)
            {
                if (!_index.TryRemove(key, out var inner))
                {
                    // If already removed, also clear any stale lock object if present.
                    _locks.TryRemove(key, out _);
                    return Array.Empty<TReferenceKey>();
                }

                var removed = inner.ToImmutableArray();
                _locks.TryRemove(key, out _);
                return removed;
            }
        }
        finally
        {
            _gate.ExitReadLock();
        }
    }

    public bool Remove(TKey key, TReferenceKey referenceKey)
    {
        _gate.EnterReadLock();
        try
        {
            var sync = _locks.GetOrAdd(key, static _ => new object());
            lock (sync)
            {
                if (!_index.TryGetValue(key, out var inner)) return false;

                var removed = inner.TryRemove(referenceKey);
                if (removed && inner.IsEmpty)
                {
                    _index.TryRemove(key, out _);
                    _locks.TryRemove(key, out _);
                }

                return removed;
            }
        }
        finally
        {
            _gate.ExitReadLock();
        }
    }

    public IEnumerator<KeyValuePair<TKey, TReferenceKey>> GetEnumerator()
    {
        // Snapshot-style enumeration over concurrent structures.
        foreach (var kv in _index)
        {
            var key = kv.Key;
            var inner = kv.Value;

            foreach (var refKey in inner)
            {
                yield return new KeyValuePair<TKey, TReferenceKey>(key, refKey);
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
