//using System.Collections;
//using Toolbox.Extensions;
//using Toolbox.Tools;

//namespace Toolbox.Data;

//public class DictionaryList<TKey, T> : IEnumerable<T> where TKey : notnull
//{
//    private readonly List<T> _list = new List<T>();
//    private readonly Dictionary<TKey, T> _dictionary;
//    private readonly object _lock = new object();
//    private readonly Func<T, TKey> _getKey;
//    private readonly IEqualityComparer<TKey>? _keyComparer;

//    public DictionaryList(Func<T, TKey> getKey, IEqualityComparer<TKey>? keyComparer = null)
//    {
//        _getKey = getKey.NotNull();
//        _keyComparer = keyComparer.EqualityComparerFor();
//        _dictionary = new Dictionary<TKey, T>(_keyComparer);
//    }

//    public DictionaryList(Func<T, TKey> getKey, IEnumerable<T> data, IEqualityComparer<TKey>? keyComparer = null)
//        : this(getKey, keyComparer)
//    {
//        data.NotNull().ForEach(x => Add(x));
//    }

//    public int Count => _list.Count;
//    public T this[TKey key] => _dictionary[key];
//    public T this[int index] => _list[index];
//    public IList<T> Values => _list;

//    public void Clear()
//    {
//        lock (_lock)
//        {
//            _list.Clear();
//            _dictionary.Clear();
//        }
//    }

//    public void Add(T value)
//    {
//        lock (_lock)
//        {
//            _dictionary.TryAdd(_getKey(value), value).Assert(x => x == true, "Key already exist");
//            _list.Add(value);
//        }
//    }

//    public bool Remove(TKey key)
//    {
//        lock (_lock)
//        {
//            if (!_dictionary.Remove(key)) return false;

//            _list.RemoveAll(x => _getKey(x).Equals(key));
//            return true;
//        }
//    }

//    public bool Remove(T value)
//    {
//        lock (_lock)
//        {
//            TKey key = _getKey(value);
//            return Remove(key);
//        }
//    }

//    public bool Remove(int index)
//    {
//        lock (_lock)
//        {
//            TKey key = _getKey(_list[index]);
//            return Remove(key);
//        }
//    }

//    public bool TryGetValue(TKey key, out T? value) => _dictionary.TryGetValue(key, out value);

//    public bool TryGetValue(int index, out T? value)
//    {
//        if (index < 0 || index >= _list.Count)
//        {
//            value = default;
//            return false;
//        }

//        value = _list[index];
//        return true;
//    }

//    public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();
//    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
//}
