//using System.Collections;
//using System.Collections.Concurrent;
//using Toolbox.Types;

//namespace Toolbox.Data;

//public class SecondaryIndexCollection<TKey, TValue> : IEnumerable<KeyValuePair<string, IDictionary2<TValue>>>
//    where TKey : notnull
//    where TValue : notnull
//{
//    private readonly ConcurrentDictionary<string, IDictionary2<TValue>> _secondaryIndexes = new(StringComparer.OrdinalIgnoreCase);

//    public int Count => _secondaryIndexes.Count;
//    public IEnumerable<IDictionary2<TValue>> Providers => _secondaryIndexes.Values;

//    public Option<IUniqueIndexAccess<TIndexKey, TValue>> CreateUniqueIndex<TIndexKey>(string indexName, Func<TValue, TIndexKey> keySelector, IEqualityComparer<TIndexKey>? comparer = null)
//        where TIndexKey : notnull
//    {
//        var index = new UniqueIndexProvider<TIndexKey, TValue>(keySelector, comparer);
//        if (!_secondaryIndexes.TryAdd(indexName, index)) return StatusCode.Conflict;

//        return ((IUniqueIndexAccess<TIndexKey, TValue>)index).ToOption();
//    }

//    public Option<INonUniqueIndexAccess<TIndexKey, TValue>> CreateNonUniqueIndex<TIndexKey>(string indexName, Func<TValue, TIndexKey> keySelector, IEqualityComparer<TIndexKey>? comparer = null)
//        where TIndexKey : notnull
//    {
//        var index = new NonUniqueIndexProvider<TIndexKey, TValue>(keySelector, comparer);
//        var result = _secondaryIndexes.TryAdd(indexName, index);
//        if (!result) return StatusCode.Conflict;

//        return ((INonUniqueIndexAccess<TIndexKey, TValue>)index).ToOption();
//    }

//    public Option<IDictionary2<TValue>> GetIndex(string indexName)
//    {
//        if (_secondaryIndexes.TryGetValue(indexName, out var index)) return index.ToOption();
//        return StatusCode.NotFound;
//    }

//    public Option RemoveIndex(string indexName)
//    {
//        var result = _secondaryIndexes.TryRemove(indexName, out var _);
//        return result ? StatusCode.OK : StatusCode.NotFound;
//    }

//    public IEnumerator<KeyValuePair<string, IDictionary2<TValue>>> GetEnumerator() => _secondaryIndexes.GetEnumerator();
//    IEnumerator IEnumerable.GetEnumerator() => _secondaryIndexes.GetEnumerator();
//}
