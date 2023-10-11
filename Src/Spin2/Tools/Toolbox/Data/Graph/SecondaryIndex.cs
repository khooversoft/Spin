using System.Collections;
using Toolbox.Tools;

namespace Toolbox.Data;

public class SecondaryIndex<TKey, TPrimaryKey> : IEnumerable<KeyValuePair<TKey, TPrimaryKey>>
    where TKey : notnull
    where TPrimaryKey : notnull
{
    private readonly object _lock = new object();
    private readonly DictionaryHashSet<TKey, TPrimaryKey> _index;
    private readonly DictionaryHashSet<TPrimaryKey, TKey> _reverseLookup;

    public SecondaryIndex(IEqualityComparer<TKey>? keyComparer = null, IEqualityComparer<TPrimaryKey>? primaryKeyComparer = null)
    {
        _index = new DictionaryHashSet<TKey, TPrimaryKey>(keyComparer);
        _reverseLookup = new DictionaryHashSet<TPrimaryKey, TKey>(primaryKeyComparer);
    }

    public int Count => _index.Count;
    public IReadOnlyList<TPrimaryKey> this[TKey key] => _index.Get(key);

    public SecondaryIndex<TKey, TPrimaryKey> Set(TKey key, TPrimaryKey primaryKey)
    {
        lock (_lock)
        {
            _index.Set(key, primaryKey);
            _reverseLookup.Set(primaryKey, key);
        }

        return this;
    }

    public bool Remove(TKey key)
    {
        lock (_lock)
        {
            IReadOnlyList<TPrimaryKey> referenceKeys = _index.Remove(key);

            foreach (var refKey in referenceKeys)
            {
                _reverseLookup.Remove(refKey, key);
            }
        }

        return true;
    }

    public bool RemovePrimaryKey(TPrimaryKey primaryKey)
    {
        lock (_lock)
        {
            IReadOnlyList<TKey> keys = _reverseLookup.Remove(primaryKey);
            if (keys.Count == 0) return false;

            foreach (var key in keys)
            {
                _index.Remove(key, primaryKey);
            }
        }

        return true;
    }

    public bool Remove(TKey key, TPrimaryKey primaryKey)
    {
        lock (_lock)
        {
            bool removed1 = _index.Remove(key, primaryKey);
            bool removed2 = _reverseLookup.Remove(primaryKey, key);

            return removed1 && removed2;
        }
    }

    public IReadOnlyList<TPrimaryKey> Lookup(TKey key) => _index.Get(key);
    public IReadOnlyList<TKey> LookupPrimaryKey(TPrimaryKey pkey) => _reverseLookup.Get(pkey);

    public IEnumerator<KeyValuePair<TKey, TPrimaryKey>> GetEnumerator() => _index.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
