//using System.Collections;
//using System.Collections.Concurrent;

//namespace Toolbox.Data;

//public interface IUniqueIndexAccess<TKey, TValue>
//{
//    bool TryGetValue(TKey key, out TValue? value);
//}

//public class UniqueIndexProvider<TKey, TValue> : IDictionary2<TValue>, IUniqueIndexAccess<TKey, TValue>, IEnumerable<KeyValuePair<TKey, TValue>>
//    where TKey : notnull
//{
//    private Func<TValue, TKey> _keySelector;
//    private ConcurrentDictionary<TKey, TValue> _index;

//    public UniqueIndexProvider(Func<TValue, TKey> keySelector, IEqualityComparer<TKey>? comparer = null)
//    {
//        _keySelector = keySelector;
//        _index = new ConcurrentDictionary<TKey, TValue>(comparer);
//    }

//    public void Clear() => _index.Clear();
//    public void Set(TValue item) => _index[_keySelector(item)] = item;
//    public void Remove(TValue item) => _index.TryRemove(_keySelector(item), out _);

//    public bool TryGetValue(TKey key, out TValue? value) => _index.TryGetValue(key, out value);

//    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _index.GetEnumerator();
//    IEnumerator IEnumerable.GetEnumerator() => _index.GetEnumerator();
//}
