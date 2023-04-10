using System.ComponentModel;
using System.Runtime.CompilerServices;
using Toolbox.Tools;

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

    public static string ToSafeJson<T>(this T subject)
    {
        try { return Json.Default.Serialize(subject); }
        catch { return "<serialzation failed>"; }
    }

    public static string ToJsonPascal<T>(this T subject) => Json.Default.SerializePascal(subject);

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
}
