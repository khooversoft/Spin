using System.Text.Json;
using Microsoft.Extensions.Logging;
using Toolbox.Types;

namespace Toolbox.Tools;

public static class JsonExtensions
{
    public static string ToJson<T>(this T subject) => Json.Default.Serialize(subject);

    public static string ToJsonSafe<T>(this T subject, ILogger logger)
    {
        try
        {
            return subject switch
            {
                null => string.Empty,
                string v => v,
                var v => Json.Default.Serialize(v),
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Json serialization error");
            return string.Empty;
        }
    }

    public static string ToJsonPascal<T>(this T subject) => Json.Default.SerializePascal(subject);

    public static string? ToJsonPascalSafe<T>(this T subject, ILogger logger)
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
            logger.LogError(ex, "Json serialization error");
            return string.Empty;
        }
    }

    public static string ToJsonFormat<T>(this T subject) => Json.Default.SerializeFormat(subject);

    public static Option<JsonElement> Find(this JsonDocument subject, string path) => subject.RootElement.Find(path);

    public static Option<JsonElement> Find(this JsonElement element, string path)
    {
        element.NotNull();
        path.NotEmpty();

        JsonElement current = element;

        foreach (string propertyName in path.Split('/'))
        {
            if (current.ValueKind != JsonValueKind.Object) return StatusCode.NotFound;
            if (!current.TryGetProperty(propertyName, out current)) return StatusCode.NotFound;
        }

        return current;
    }
}
