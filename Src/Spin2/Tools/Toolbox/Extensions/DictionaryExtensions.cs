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
    public static IReadOnlyDictionary<string, string?> ToDictionary(this string? subject, string propertyDelimiter = ";", string valueDelimiter = "=")
    {
        if (subject.IsEmpty()) return new Dictionary<string, string?>();

        return subject
            .Split(propertyDelimiter, StringSplitOptions.RemoveEmptyEntries)
            .Select(x => GetKeyValue(x, valueDelimiter).Return())
            .ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);
    }

    public static string ToDictionaryString(this IEnumerable<KeyValuePair<string, string?>> values) => (values ?? Array.Empty<KeyValuePair<string, string?>>())
        .Select(x => x.Value switch { null => x.Key, _ => $"{x.Key}={x.Value}" })
        .Join(';');

    public static Option<KeyValuePair<string, string?>> GetKeyValue(this string subject, string valueDelimiter = "=")
    {
        if (subject.IsEmpty()) return new Option<KeyValuePair<string, string?>>(StatusCode.BadRequest);

        return subject.IndexOf(valueDelimiter) switch
        {
            -1 => new KeyValuePair<string, string?>(subject, null),
            var index => new KeyValuePair<string, string?>(subject[0..index].Trim(), subject[(index + 1)..^0].Trim()),
        };
    }
}
