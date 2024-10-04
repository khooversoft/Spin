using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
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


    public static T? ToObject<T>(this string json)
    {
        return json switch
        {
            string v when v.IsEmpty() => default,
            _ => Json.Default.Deserialize<T>(json),
        };
    }

    public static T SafeCast<T>(this object subject)
    {
        return subject is T to ? to : default!;
    }

    public static T Cast<T>(this object subject)
    {
        return subject is T to ? to : throw new ArgumentException($"Cannot cast to type={typeof(T).FullName}");
    }

    /// <summary>
    /// Compute hash for multiple objects, using string representations
    /// </summary>
    /// <param name="values">values</param>
    /// <returns>hash bytes</returns>
    public static byte[] ComputeHash(this IEnumerable<object?> values)
    {
        values.NotNull();

        var ms = new MemoryStream();

        values.Select(x => x switch
        {
            null => null,
            string v => v.ToBytes(),
            byte[] v => v,

            var v => throw new InvalidDataException($"Not supported type={(v?.GetType()?.Name ?? "<null>")}"),
        })
        .ForEach(x => ms.Write(x));

        ms.Seek(0, SeekOrigin.Begin);
        return MD5.Create().ComputeHash(ms);
    }
}
