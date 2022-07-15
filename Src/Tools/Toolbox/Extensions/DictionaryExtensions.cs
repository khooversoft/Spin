using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Monads;
using Toolbox.Tools;

namespace Toolbox.Extensions;

public static class DictionaryExtensions
{
    public static bool IsEqual(this IEnumerable<KeyValuePair<string, string>> left, IEnumerable<KeyValuePair<string, string>> right)
    {
        if (left == null || right == null) return false;
        if (left.Count() != right.Count()) return false;

        return left.OrderBy(x => x.Key)
            .Zip(right.OrderBy(x => x.Key), (o, i) => (o, i))
            .All(x => x.o.Key == x.i.Key && x.o.Value == x.i.Value);
    }


    // ////////////////////////////////////////////////////////////////////////////////////////////
    // Generic

    public static Option<TValue> Get<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, TKey key)
    {
        dictionary.NotNull();

        bool found = dictionary.TryGetValue(key, out TValue? value);
        return (found, value).Option();
    }


    // ////////////////////////////////////////////////////////////////////////////////////////////
    // String key

    public static Option<T> Get<T>(this IReadOnlyDictionary<string, T> dictionary, string key)
    {
        dictionary.NotNull();
        key.NotEmpty();

        bool found = dictionary.TryGetValue(key, out T? value);
        return (found, value).Option();
    }

    public static Option<T> Get<T>(this IReadOnlyDictionary<string, T> dictionary)
    {
        dictionary.NotNull();

        bool found = dictionary.TryGetValue(typeof(T).Name, out T? value);
        return (found, value).Option();
    }


    // ////////////////////////////////////////////////////////////////////////////////////////////
    // Embedded json object

    public static Option<string> Get(this IReadOnlyDictionary<string, string> dictionary, string key)
    {
        dictionary.NotNull();
        key.NotEmpty();

        bool found = dictionary.TryGetValue(key, out string? value);
        return (found, value).Option();
    }

    public static Option<T> Get<T>(this IReadOnlyDictionary<string, string> dictionary)
    {
        dictionary.NotNull();

        bool found = dictionary.TryGetValue(typeof(T).Name, out string? data);
        if (!found) return Option<T>.None;

        var value = Json.Default.Deserialize<T>(data!)
            .Assert(x => x != null, "Deserialize error");

        return value.Option();
    }

    public static void Set<T>(this IDictionary<string, string> dictionary, T value)
    {
        dictionary.NotNull();

        string json = Json.Default.Serialize(value);
        dictionary[typeof(T).Name] = json;
    }
}
