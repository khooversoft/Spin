using System.Collections;
using System.Collections.Concurrent;
using Toolbox.Tools;

namespace Toolbox.Data;

/// <summary>
/// Data collection with primary index with 0-n number of secondary index
/// </summary>
public class IndexedCollection<TKey, TValue> : IEnumerable<TValue>, IDisposable
    where TKey : notnull
    where TValue : notnull
{
    private readonly ConcurrentDictionary<TKey, TValue> _primaryIndex;
    private readonly Func<TValue, TKey> _keySelector;
    private readonly ReaderWriterLockSlim _rwLock = new(LockRecursionPolicy.NoRecursion);
    private readonly SecondaryIndexCollection<TKey, TValue> _secondaryIndexCollection;
    private readonly string _storeName = "IndexedCollection-" + Guid.NewGuid().ToString();

    public IndexedCollection(Func<TValue, TKey> keySelector, IEqualityComparer<TKey>? primaryKeyComparer = null)
    {
        _keySelector = keySelector.NotNull();

        _primaryIndex = new(primaryKeyComparer);
        _secondaryIndexCollection = new SecondaryIndexCollection<TKey, TValue>();
    }

    public DataChangeRecorder DataChangeLog { get; } = new();

    //public ITransactionProvider GetProvider() => new IndexedCollectionTrxProvider<TKey, TValue>(_storeName, this);

    public TValue this[TKey key]
    {
        set
        {
            // Enforce key consistency: the provided key must match the item's primary key.
            var itemKey = _keySelector(value);
            if (!EqualityComparer<TKey>.Default.Equals(key, itemKey))
            {
                throw new ArgumentException("The provided key does not match the item's primary key.", nameof(key));
            }

            Set(value);
        }
        get => _primaryIndex[key];
    }

    public int Count => _primaryIndex.Count;
    public SecondaryIndexCollection<TKey, TValue> SecondaryIndexes => _secondaryIndexCollection;
    public IEnumerable<TKey> Keys => _primaryIndex.Keys;
    public IEnumerable<TValue> Values => _primaryIndex.Values;
    public bool ContainsKey(TKey key) => _primaryIndex.ContainsKey(key);

    public bool Remove(TValue value) => TryRemove(_keySelector(value), out _);

    public void Clear()
    {
        _rwLock.EnterWriteLock();
        try
        {
            if (DataChangeLog.GetRecorder() != null)
            {
                //foreach (var item in _primaryIndex.Values) DataChangeLog.GetRecorder()?.Delete(_keySelector(item), item);
            }

            _primaryIndex.Clear();
            foreach (var index in _secondaryIndexCollection.Providers) index.Clear();
        }
        finally
        {
            _rwLock.ExitWriteLock();
        }
    }

    public bool TryAdd(TValue item)
    {
        _rwLock.EnterWriteLock();
        try
        {
            if (!_primaryIndex.TryAdd(_keySelector(item), item)) return false;
            //DataChangeLog.GetRecorder()?.Add(_keySelector(item), item);

            foreach (var index in _secondaryIndexCollection.Providers) index.Set(item);
            return true;
        }
        finally { _rwLock.ExitWriteLock(); }
    }

    public TValue GetOrAdd(TValue item)
    {
        _rwLock.EnterWriteLock();
        try
        {
            var key = _keySelector(item);
            if (_primaryIndex.TryGetValue(key, out var existing)) return existing;

            //DataChangeLog.GetRecorder()?.Add(_keySelector(item), item);
            _primaryIndex[key] = item;
            foreach (var index in _secondaryIndexCollection.Providers) index.Set(item);
            return item;
        }
        finally { _rwLock.ExitWriteLock(); }
    }

    public bool TryGetValue(TKey key, out TValue? value)
    {
        _rwLock.EnterReadLock();
        try
        {
            return _primaryIndex.TryGetValue(key, out value);
        }
        finally { _rwLock.ExitReadLock(); }
    }

    public bool TryRemove(TKey key, out TValue? value)
    {
        _rwLock.EnterWriteLock();
        try
        {
            var result = _primaryIndex.TryRemove(key, out value);
            if (result && value != null)
            {
                //DataChangeLog.GetRecorder()?.Delete(key, value);
                foreach (var index in _secondaryIndexCollection.Providers) index.Remove(value);
            }
            return result;
        }
        finally { _rwLock.ExitWriteLock(); }
    }

    public bool TryRemove(TValue item, out TValue? value) => TryRemove(_keySelector(item), out value);

    public bool TryUpdate(TValue newValue, TValue currentValue)
    {
        var k1 = _keySelector(newValue);
        var k2 = _keySelector(currentValue);
        if (k1.Equals(k2) == false)
        {
            throw new ArgumentException("The primary key of the new value must match the primary key of the current value for an update operation.");
        }

        _rwLock.EnterWriteLock();
        try
        {
            var exist = _primaryIndex.TryGetValue(_keySelector(currentValue), out var existing);
            if (!exist || existing == null) return exist;

            var result = _primaryIndex.TryUpdate(_keySelector(newValue), newValue, existing);
            if (result)
            {
                //DataChangeLog.GetRecorder()?.Update(_keySelector(newValue), currentValue, newValue);
                foreach (var index in _secondaryIndexCollection.Providers) index.Set(newValue);
            }

            return result;
        }
        finally { _rwLock.ExitWriteLock(); }
    }

    public void Set(TValue item)
    {
        _rwLock.EnterWriteLock();
        try
        {
            var exist = _primaryIndex.TryGetValue(_keySelector(item), out var existing);

            _primaryIndex[_keySelector(item)] = item;

            if (DataChangeLog.GetRecorder() != null)
            {
                //if (exist && existing != null)
                //    DataChangeLog.GetRecorder()?.Update(_keySelector(item), existing, item);
                //else
                //    DataChangeLog.GetRecorder()?.Add(_keySelector(item), item);
            }

            foreach (var index in _secondaryIndexCollection.Providers) index.Set(item);
        }
        finally { _rwLock.ExitWriteLock(); }
    }

    public IEnumerator<TValue> GetEnumerator() => _primaryIndex.Values.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => _primaryIndex.Values.GetEnumerator();
    public void Dispose() => _rwLock.Dispose();
}
