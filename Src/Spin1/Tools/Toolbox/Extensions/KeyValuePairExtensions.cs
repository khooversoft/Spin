using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Tools;

namespace Toolbox.Extensions;

public static class KeyValuePairExtensions
{
    public static KeyValuePair<string, string> ToKeyValuePair(this string subject, char delimiter = '=')
    {
        const string msg = "Syntax error";

        subject.NotEmpty();

        int index = subject.IndexOf('=').Assert(x => x > 0, msg);

        string key = subject.Substring(0, index).Trim().NotEmpty(name: msg);
        string value = subject.Substring(index + 1).Trim().NotEmpty(name: msg); ;

        return new KeyValuePair<string, string>(key, value);
    }

    public static string? GetValue(this IEnumerable<string> subject, string name, char delimiter = '=') =>
        subject.NotNull()
        .Select(x => x.ToKeyValuePair(delimiter))
        .Where(x => x.Key.Equals(name, StringComparison.OrdinalIgnoreCase))
        .Select(x => x.Value)
        .FirstOrDefault();

    public static IReadOnlyList<KeyValuePair<string, string>> ToTags(this string? line) => line.ToNullIfEmpty() switch
    {
        null => Array.Empty<KeyValuePair<string, string>>(),
        string v => v
            .Split(';', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.ToTag())
            .OfType<KeyValuePair<string, string>>()
            .ToArray(),
    };

    public static KeyValuePair<string, string>? ToTag(this string? subject, char delimiter = '=')
    {
        return subject.ToNullIfEmpty() switch
        {
            null => null,

            string sub => sub.IndexOf('=') switch
            {
                -1 => null,

                int index => (Key: sub[..index].Trim(), Value: sub[(index + 1)..].Trim()) switch
                {
                    (null, null) => null,
                    ("", "") => null,
                    (string, string) v => new KeyValuePair<string, string>(v.Key, v.Value),

                    _ => throw new NotImplementedException(),
                },
            }
        };
    }

    public static string? FindTag(this string? line, string name) => line switch
    {
        null => null,
        string v => v.ToTags().FindTag(name),
    };

    public static string? FindTag(this IEnumerable<KeyValuePair<string, string>> subject, string name) =>
        subject.NotNull()
        .Where(x => x.Key.Equals(name, StringComparison.OrdinalIgnoreCase))
        .Select(x => x.Value)
        .FirstOrDefault();

    public static string KeyValueSerialize(this IEnumerable<KeyValuePair<string, string>> subject) => subject
        .NotNull()
        .Select(x => x.ToKeyValueString())
        .Join(";");

    public static string ToKeyValueString(this KeyValuePair<string, string> subject) => $"{subject.Key}={subject.Value}";
    public static string ToKeyValueString(this (string Key, string Value) subject) => subject.NotNull().ToKeyValuePair().ToKeyValueString();
    public static KeyValuePair<string, string> ToKeyValuePair(this (string Key, string Value) subject) =>
        new KeyValuePair<string, string>(subject.Key.NotEmpty(), subject.Value.NotEmpty());
}
