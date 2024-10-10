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

    public static IReadOnlyList<KeyValuePair<string, string>> ToDictionary<T>(this T subject) where T : class
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
            .Where(x => x.Value != null)
            .OfType<KeyValuePair<string, string>>()
            .OrderBy(x => x.Key)
            .ToArray();
    }

    /// <summary>
    /// 
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
            .Bind<T>();

        return result;
    }

    /// <summary>
    /// Comparis two dictionaries (key value pairs)
    /// </summary>
    /// <param name="source"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    //public static bool DeepEquals(this IEnumerable<KeyValuePair<string, string?>>? source, IEnumerable<KeyValuePair<string, string?>>? target)
    //{
    //    if (source == null && target == null) return true;
    //    if (source == null || target == null) return false;

    //    var sourceList = source.OrderBy(x => x.Key).ToArray();
    //    var targetList = target.OrderBy(x => x.Key).ToArray();
    //    if (sourceList.Length != targetList.Length) return false;

    //    var zip = sourceList.Zip(targetList, (x, y) => (source: x, target: y));
    //    var isEqual = zip.All(x => x.source.Key.Equals(x.target.Key, StringComparison.OrdinalIgnoreCase) switch
    //    {
    //        false => false,
    //        true => (x.source.Value, x.target.Value) switch
    //        {
    //            (null, null) => true,
    //            (string s1, string s2) => s1.Equals(s2, StringComparison.OrdinalIgnoreCase),
    //            _ => false,
    //        }
    //    });

    //    return isEqual;
    //}

    public static bool DeepEquals<TKey, TValue>(
        this IEnumerable<KeyValuePair<TKey, TValue>>? source, IEnumerable<KeyValuePair<TKey, TValue>>? target,
        IComparer<TKey>? keyComparer = null, IComparer<TValue>? valueComparer = null
        )
    {
        if (source == null && target == null) return true;
        if (source == null || target == null) return false;

        keyComparer = keyComparer.ComparerFor();
        valueComparer = valueComparer.ComparerFor();

        var sourceList = source.OrderBy(x => x.Key, keyComparer).ToArray();
        var targetList = target.OrderBy(x => x.Key).ToArray();
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
