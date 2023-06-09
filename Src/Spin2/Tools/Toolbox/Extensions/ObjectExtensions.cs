using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Extensions;

public static class ObjectExtensions
{
    public static string GetMethodName(this object obj, [CallerMemberName] string function = "") => $"{obj.GetType().Name}.{function}";

    public static IReadOnlyList<KeyValuePair<string, object?>> ToKeyValuePairs(this object obj) =>
        TypeDescriptor.GetProperties(obj)
        .OfType<PropertyDescriptor>()
        .Select(x => new KeyValuePair<string, object?>(x.Name, x.GetValue(obj)))
        .ToArray();

    public static string ToJson<T>(this T subject) => Json.Default.Serialize(subject);

    public static string ToSafeJson<T>(this T subject, ScopeContext context)
    {
        try
        {
            return subject switch
            {
                null => string.Empty,
                string v => v,
                var v => Json.Default.Serialize(v.ToJsonPascal()),
            };
        }
        catch (Exception ex)
        {
            context.Logger?.LogError(context.Location(), ex, "Json serialzation error");
            return string.Empty;
        }
    }

    public static string ToJsonPascal<T>(this T subject) => Json.Default.SerializePascal(subject);

    public static string? ToJsonPascalSafe<T>(this T subject, ScopeContext context)
    {
        try
        {
            return subject switch
            {
                null => string.Empty,
                string v => v,
                var v => v.ToJsonPascal(),
            };
        }
        catch (Exception ex)
        {
            context.Logger?.LogError(context.Location(), ex, "Json serialzation error");
            return string.Empty;
        }
    }

    public static T? ToObject<T>(this string json)
    {
        return json switch
        {
            string v when v.IsEmpty() => default,
            _ => Json.Default.Deserialize<T>(json),
        };
    }

    /// <summary>
    /// Convert to Json formatted
    /// </summary>
    /// <typeparam name="T">type</typeparam>
    /// <param name="subject">subject</param>
    /// <returns>json</returns>
    public static string ToJsonFormat<T>(this T subject) => Json.Default.SerializeFormat(subject);

    /// <summary>
    /// Simple cast
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="subject"></param>
    /// <returns></returns>
    public static T Cast<T>(this object? subject) => subject switch
    {
        null => default!,
        var v => (T)v,
    };
}
