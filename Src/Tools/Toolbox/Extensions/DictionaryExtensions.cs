﻿using System.Text;
using System.Text.Json;
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

        string json = JsonSerializer.Serialize(subject).NotEmpty(name: "Serialization failed");

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
    public static T ToObject<T>(this IEnumerable<KeyValuePair<string, string>> values) where T : new()
    {
        values.NotNull();
        var dict = values.ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);

        T result = new ConfigurationBuilder()
            .AddInMemoryCollection(values!)
            .Build()
            .Bind<T>();

        return result;
    }
}
