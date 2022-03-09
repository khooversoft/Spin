using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Tools;

namespace Toolbox.Extensions;

public static class KeyValuePairExtensions
{
    public static KeyValuePair<string, string> ToKeyValuePair(this (string name, string value) subject) => new KeyValuePair<string, string>(subject.name, subject.value);

    public static KeyValuePair<string, string> ToKeyValuePair(this string subject, char delimiter = '=')
    {
        const string msg = "Syntax error";

        subject.VerifyNotEmpty(subject);

        int index = subject.IndexOf('=').VerifyAssert(x => x > 0, msg);

        string key = subject.Substring(0, index).Trim().VerifyNotEmpty(msg);
        string value = subject.Substring(index + 1).Trim().VerifyNotEmpty(msg); ;

        return new KeyValuePair<string, string>(key, value);
    }

    public static string? GetValue(this IEnumerable<string> subject, string name, char delimiter = '=') =>
        subject.VerifyNotNull(nameof(subject))
        .Select(x => x.ToKeyValuePair(delimiter))
        .Where(x => x.Key.Equals(name, StringComparison.OrdinalIgnoreCase))
        .Select(x => x.Value)
        .FirstOrDefault();

    public static string ToProperty(this KeyValuePair<string, string> subject) => $"{subject.Key}={subject.Value}";
}
