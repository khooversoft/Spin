using System.Collections;

namespace Toolbox.Data;

public class SecondaryIndex<TKey, TPrimaryKey> : IEnumerable<KeyValuePair<TKey, TPrimaryKey>>
    where TKey : notnull
    where TPrimaryKey : notnull
{
    private readonly ReaderWriterLockSlim _rwLock = new(LockRecursionPolicy.NoRecursion);
    private readonly OneToManyIndex<TKey, TPrimaryKey> _index;
    private readonly OneToManyIndex<TPrimaryKey, TKey> _reverseLookup;

    public SecondaryIndex(IEqualityComparer<TKey>? keyComparer = null, IEqualityComparer<TPrimaryKey>? primaryKeyComparer = null)
    {
        _index = new OneToManyIndex<TKey, TPrimaryKey>(keyComparer);
        _reverseLookup = new OneToManyIndex<TPrimaryKey, TKey>(primaryKeyComparer);
    }

    public int Count
    {
        get
        {
            _rwLock.EnterReadLock();
            try
            {
                return _index.Count;
            }
            finally { _rwLock.ExitReadLock(); }
        }
    }

    public IReadOnlyList<TPrimaryKey> this[TKey key]
    {
        get
        {
            _rwLock.EnterReadLock();
            try
            {
                return _index.Get(key);
            }
            finally { _rwLock.ExitReadLock(); }
        }
    }

    public void Clear()
    {
        _rwLock.EnterWriteLock();
        try
        {
            _index.Clear();
            _reverseLookup.Clear();
        }
        finally { _rwLock.ExitWriteLock(); }
    }

    public bool Remove(TKey key)
    {
        _rwLock.EnterWriteLock();
        try
        {
            IReadOnlyList<TPrimaryKey> referenceKeys = _index.Remove(key);
            if (referenceKeys.Count == 0) return false;

            foreach (var item in referenceKeys)
            {
                if (!_reverseLookup.Remove(item, key)) throw new InvalidOperationException("Unbalanced indexes");
            }
            return true;
        }
        finally { _rwLock.ExitWriteLock(); }
    }

    public bool RemovePrimaryKey(TPrimaryKey primaryKey)
    {
        _rwLock.EnterWriteLock();
        try
        {
            IReadOnlyList<TKey> keys = _reverseLookup.Remove(primaryKey);
            if (keys.Count == 0) return false;

            foreach (var item in keys)
            {
                if (!_index.Remove(item, primaryKey)) throw new InvalidOperationException("Unbalanced indexes");
            }
            return true;
        }
        finally { _rwLock.ExitWriteLock(); }
    }

    public bool Remove(TKey key, TPrimaryKey primaryKey)
    {
        _rwLock.EnterWriteLock();
        try
        {
            bool removed1 = _index.Remove(key, primaryKey);
            bool removed2 = _reverseLookup.Remove(primaryKey, key);

            return (removed1, removed2) switch
            {
                (false, false) => false,
                (true, true) => true,
                _ => throw new InvalidOperationException($"Unbalanced indexes, index={removed1}, reverse={removed2}"),
            };
        }
        finally { _rwLock.ExitWriteLock(); }
    }

    public SecondaryIndex<TKey, TPrimaryKey> Set(TKey key, TPrimaryKey primaryKey)
    {
        _rwLock.EnterWriteLock();
        try
        {
            _index.Set(key, primaryKey);
            _reverseLookup.Set(primaryKey, key);
            return this;
        }
        finally { _rwLock.ExitWriteLock(); }
    }

    public IReadOnlyList<TPrimaryKey> Lookup(TKey key)
    {
        _rwLock.EnterReadLock();
        try
        {
            return _index.Get(key);
        }
        finally { _rwLock.ExitReadLock(); }
    }

    public IReadOnlyList<TKey> LookupPrimaryKey(TPrimaryKey pkey)
    {
        _rwLock.EnterReadLock();
        try
        {
            return _reverseLookup.Get(pkey);
        }
        finally { _rwLock.ExitReadLock(); }
    }

    public IEnumerator<KeyValuePair<TKey, TPrimaryKey>> GetEnumerator()
    {
        // Snapshot to avoid holding read lock during potentially long enumerations
        List<KeyValuePair<TKey, TPrimaryKey>> snapshot;
        _rwLock.EnterReadLock();
        try
        {
            snapshot = _index.ToList();
        }
        finally { _rwLock.ExitReadLock(); }

        return snapshot.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
