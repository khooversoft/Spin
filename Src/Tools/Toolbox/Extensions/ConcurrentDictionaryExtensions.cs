using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    public static ConcurrentDictionary<TKey, TValue> Clone<TKey, TValue>(
            this ConcurrentDictionary<TKey, TValue> subject,
            Func<TValue, KeyValuePair<TKey, TValue>> clone,
            IEqualityComparer<TKey>? comparer = null
        )
        where TKey : notnull
    {
        return new ConcurrentDictionary<TKey, TValue>(subject.Values.Select(x => clone(x)), comparer);
    }
}
