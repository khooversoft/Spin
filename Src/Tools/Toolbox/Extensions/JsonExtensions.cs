using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Extensions;

public static class JsonExtensions
{
    public static string ToJson<T>(this T subject) => Json.Default.Serialize(subject);

    public static string ToJsonSafe<T>(this T subject, ScopeContextLocation context)
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
            context.LogError(ex, "Json serialzation error");
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
            context.Location().LogError(ex, "Json serialzation error");
            return string.Empty;
        }
    }

    public static string ToJsonFormat<T>(this T subject) => Json.Default.SerializeFormat(subject);
}
