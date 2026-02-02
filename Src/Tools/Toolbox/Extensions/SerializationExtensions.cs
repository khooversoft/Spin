using System.Collections.Immutable;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Toolbox.Data;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Extensions;

public static class SerializationExtensions
{
    public static string ToJson<T>(this T subject)
    {
        if (subject is null) return "null";

        return subject switch
        {
            string s => s,
            byte[] bytes => bytes.Length == 0 ? string.Empty : Encoding.UTF8.GetString(bytes),

            _ => JsonSerializerContextRegistered.Find<T>() switch
            {
                { StatusCode: StatusCode.OK } v => JsonSerializer.Serialize(subject, v.Return()),
                _ => Json.Default.Serialize(subject),
            }
        };
    }

    public static T ToObject<T>(this object? source)
    {
        source.NotNull();
        if (source is T tValue) return tValue;

        try
        {
            ReadOnlySpan<byte> json = source switch
            {
                string s => Encoding.UTF8.GetBytes(s),
                byte[] bytes => bytes,
                ImmutableArray<byte> ia => ia.AsSpan(),
                DataETag etag => etag.Data.AsSpan(),
                DataObject dobj => Encoding.UTF8.GetBytes(dobj.JsonData),
                _ => throw new InvalidOperationException($"Unsupported source type {source.GetType().FullName}")
            };

            T? result = typeof(T) switch
            {
                var t when t == typeof(string) => (T)(object)Encoding.UTF8.GetString(json),
                var t when t == typeof(byte[]) => (T)(object)json.ToArray(),
                var t when t == typeof(ImmutableArray<byte>) => (T)(object)json.ToArray().ToImmutableArray(),

                _ => JsonSerializerContextRegistered.Find<T>() switch
                {
                    { StatusCode: StatusCode.OK } v => (T?)JsonSerializer.Deserialize(json, v.Return()),
                    _ => Json.Default.Deserialize<T>(Encoding.UTF8.GetString(json)),
                }
            };

            return result.NotNull("Deserialize error");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"ToObject parse failed, T={typeof(T).Name}, source.Type={source.GetType().Name}", ex);
        }
    }

    public static T ToObject<T>(this IEnumerable<KeyValuePair<string, string?>> values) where T : new()
    {
        values.NotNull();

        return new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build()
            .Get<T>()
            .NotNull();
    }
}
