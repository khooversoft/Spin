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
    /// Dictionary equals
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="source"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    public static bool DeepEquals<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>>? source, IEnumerable<KeyValuePair<TKey, TValue>>? target)
    {
        if (source == null && target == null) return true;
        if (source == null || target == null) return false;

        var keyResult = source.Select(x => x.Key).SequenceEqual(target.Select(x => x.Key));
        var valueResult = source.OrderBy(x => x.Key).SequenceEqual(target.OrderBy(x => x.Key));

        return keyResult && valueResult;
    }

    public static bool DeepEqualsComparer<TKey, TValue>(
        this IEnumerable<KeyValuePair<TKey, TValue>>? source,
        IEnumerable<KeyValuePair<TKey, TValue>>? target,
        IComparer<TKey>? keyComparer = null,
        IComparer<TValue>? valueComparer = null
        )
    {
        if (source == null && target == null) return true;
        if (source == null || target == null) return false;

        keyComparer = keyComparer.ComparerFor();
        valueComparer = valueComparer.ComparerFor();

        var sourceList = source.OrderBy(x => x.Key, keyComparer).ToArray();
        var targetList = target.OrderBy(x => x.Key, keyComparer).ToArray();
        if (sourceList.Length != targetList.Length) return false;

        var zip = sourceList.Zip(targetList, (x, y) => (source: x, target: y));
        var isEqual = zip.All(x => keyComparer.Compare(x.source.Key, x.target.Key) switch
        {
            0 => valueComparer.Compare(x.source.Value, x.target.Value) switch
            {
                0 => true,
                _ => false,
            },
            _ => false,
        });

        return isEqual;
    }
}
