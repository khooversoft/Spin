using System.Collections.Concurrent;

namespace Toolbox.Extensions;

public static class ConcurrentDictionaryExtensions
{
    public static ConcurrentDictionary<TKey, TValue> ToConcurrentDictionary<TKey, TValue>(
        this IEnumerable<KeyValuePair<TKey, TValue>> source,
        IEqualityComparer<TKey>? comparer = null) where TKey : notnull
    {
        var dict = new ConcurrentDictionary<TKey, TValue>(source, comparer);
        return dict;
    }

    public static ConcurrentDictionary<TKey, TValue> ToConcurrentDictionary<TKey, TValue>(
        this IEnumerable<TValue> source,
        Func<TValue, TKey> keySelector,
        IEqualityComparer<TKey>? comparer = null) where TKey : notnull
    {
        var values = source.Select(x => new KeyValuePair<TKey, TValue>(keySelector(x), x));
        var dict = new ConcurrentDictionary<TKey, TValue>(values, comparer);
        return dict;
    }

    public static ConcurrentDictionary<TKey, TValue> ToConcurrentDictionary<TKey, TValue>(
        this IEnumerable<TValue> source,
        Func<TValue, TKey> keySelector,
        Func<TValue, TValue> valueSelector,
        IEqualityComparer<TKey>? comparer = null) where TKey : notnull
    {
        var values = source.Select(x => new KeyValuePair<TKey, TValue>(keySelector(x), valueSelector(x)));
        var dict = new ConcurrentDictionary<TKey, TValue>(values, comparer);
        return dict;
    }
}
