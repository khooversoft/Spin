using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Extensions;

public static class DictionaryExtensions
{

    /// <summary>
    /// Convert property string ex "property1=value1;property2=value2";
    /// </summary>
    /// <param name="subject"></param>
    /// <param name="propertyDelimiter"></param>
    /// <param name="valueDelimiter"></param>
    /// <returns></returns>
    public static IReadOnlyDictionary<string, string?> ToDictionaryFromString(this string? subject, string propertyDelimiter = ";", string valueDelimiter = "=")
    {
        if (subject.IsEmpty()) return new Dictionary<string, string?>();

        return subject
            .Split(propertyDelimiter, StringSplitOptions.RemoveEmptyEntries)
            .Select(x => GetKeyValue(x, valueDelimiter).Return())
            .ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);
    }

    public static Option<KeyValuePair<string, string?>> GetKeyValue(this string subject, string valueDelimiter = "=")
    {
        if (subject.IsEmpty()) return new Option<KeyValuePair<string, string?>>(StatusCode.BadRequest);

        return subject.IndexOf(valueDelimiter) switch
        {
            -1 => new KeyValuePair<string, string?>(subject, null),
            var index => new KeyValuePair<string, string?>(subject[0..index].Trim(), subject[(index + 1)..^0].Trim()),
        };
    }

    public static IReadOnlyList<KeyValuePair<string, string?>> ToKeyValuePairs<T>(this T subject)
    {
        subject.NotNull();

        var options = new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
        string json = JsonSerializer.Serialize(subject, options).NotEmpty(name: "Serialization failed");

        byte[] byteArray = Encoding.UTF8.GetBytes(json);
        using MemoryStream stream = new MemoryStream(byteArray);

        IConfiguration config = new ConfigurationBuilder()
            .AddJsonStream(stream)
            .Build();

        return config
            .AsEnumerable()
            .Append(new KeyValuePair<string, string?>("$type", subject.GetType().Name))
            .Where(x => x.Value != null)
            .OrderBy(x => x.Key)
            .ToArray();
    }

    /// <summary>
    /// To object based on type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="values"></param>
    /// <returns></returns>
    public static T ToObject<T>(this IEnumerable<KeyValuePair<string, string?>> values) where T : new()
    {
        values.NotNull();
        var dict = values.ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);

        T result = new ConfigurationBuilder()
            .AddInMemoryCollection(values!)
            .Build()
            .Get<T>().NotNull();

        return result;
    }

    /// <summary>
    /// Deep equality for sequences of key/value pairs.
    /// If both sides are real dictionaries and no custom key comparer is provided, a fast path is used.
    /// Otherwise treats the inputs as multisets of (key,value) pairs (supports duplicate keys).
    /// </summary>
    /// <typeparam name="TKey">Key type</typeparam>
    /// <typeparam name="TValue">Value type</typeparam>
    /// <param name="source">Source sequence</param>
    /// <param name="target">Target sequence</param>
    /// <param name="keyComparer">Optional key equality comparer (defaults to EqualityComparer&lt;TKey&gt;.Default)</param>
    /// <returns>true if equal, otherwise false</returns>
    public static bool DeepEquals<TKey, TValue>(
        this IEnumerable<KeyValuePair<TKey, TValue>>? source,
        IEnumerable<KeyValuePair<TKey, TValue>>? target,
        IEqualityComparer<TKey>? keyComparer = null,
        IEqualityComparer<TValue>? valueComparer = null)
    {
        // Null handling
        if (source == null && target == null) return true;
        if (source == null || target == null) return false;
        if (ReferenceEquals(source, target)) return true;

        var keyComp = keyComparer ?? EqualityComparer<TKey>.Default;
        var valueComp = valueComparer ?? EqualityComparer<TValue>.Default;

        // Fast path only when no external comparer is supplied (otherwise dictionary internal comparer may differ)
        if (keyComparer == null && source is IDictionary<TKey, TValue> sDict && target is IDictionary<TKey, TValue> tDict)
        {
            if (sDict.Count != tDict.Count) return false;

            foreach (var kvp in sDict)
            {
                if (!tDict.TryGetValue(kvp.Key, out var targetValue)) return false;
                if (!valueComp.Equals(kvp.Value, targetValue)) return false;
            }

            return true;
        }

        // General path: multiset compare with custom key comparer support
        var pairComparer = new KeyValuePairComparer<TKey, TValue>(keyComp, valueComp);

        var sourceCounts = new Dictionary<KeyValuePair<TKey, TValue>, int>(pairComparer);
        foreach (var kvp in source)
        {
            sourceCounts.TryGetValue(kvp, out int count);
            sourceCounts[kvp] = count + 1;
        }

        var targetCounts = new Dictionary<KeyValuePair<TKey, TValue>, int>(pairComparer);
        foreach (var kvp in target)
        {
            targetCounts.TryGetValue(kvp, out int count);
            targetCounts[kvp] = count + 1;
        }

        if (sourceCounts.Count != targetCounts.Count) return false;

        foreach (var entry in sourceCounts)
        {
            if (!targetCounts.TryGetValue(entry.Key, out int targetCount)) return false;
            if (targetCount != entry.Value) return false;
        }

        return true;
    }

    private sealed class KeyValuePairComparer<TKey, TValue> : IEqualityComparer<KeyValuePair<TKey, TValue>>
    {
        private readonly IEqualityComparer<TKey> _keyComparer;
        private readonly IEqualityComparer<TValue> _valueComparer;

        public KeyValuePairComparer(IEqualityComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer)
        {
            _keyComparer = keyComparer;
            _valueComparer = valueComparer;
        }

        public bool Equals(KeyValuePair<TKey, TValue> x, KeyValuePair<TKey, TValue> y) =>
            _keyComparer.Equals(x.Key, y.Key) && _valueComparer.Equals(x.Value, y.Value);

        public int GetHashCode(KeyValuePair<TKey, TValue> obj)
        {
            int h1 = obj.Key is null ? 0 : _keyComparer.GetHashCode(obj.Key);
            int h2 = obj.Value is null ? 0 : _valueComparer.GetHashCode(obj.Value);
            return HashCode.Combine(h1, h2);
        }
    }
}
