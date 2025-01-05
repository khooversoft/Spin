using System.Collections.Concurrent;

namespace Toolbox.Extensions;

public static class ConcurrentDictionaryExtensions
{
    public static ConcurrentDictionary<TKey, TValue> ToConcurrentDictionary<TKey, TValue>(
            this IEnumerable<KeyValuePair<TKey, TValue>> values,
            IEqualityComparer<TKey>? comparer = null
        )
    where TKey : notnull
    {
        return new ConcurrentDictionary<TKey, TValue>(values, comparer);
    }

    public static ConcurrentDictionary<TKey, TValue> ToConcurrentDictionary<TKey, TValue>(
        this IEnumerable<TValue> source,
        Func<TValue, TKey> keySelector,
        IEqualityComparer<TKey>? comparer = null) where TKey : notnull
    {
        var values = source.Select(x => x.ToKeyValuePair(keySelector(x)));
        var dict = new ConcurrentDictionary<TKey, TValue>(values, comparer);
        return dict;
    }

    public static ConcurrentDictionary<TKey, TValue> Clone<TKey, TValue>(
            this ConcurrentDictionary<TKey, TValue> subject,
            Func<TValue, KeyValuePair<TKey, TValue>> clone,
            IEqualityComparer<TKey>? comparer = null
        ) where TKey : notnull
    {
        return new ConcurrentDictionary<TKey, TValue>(subject.Values.Select(x => clone(x)), comparer);
    }
}
