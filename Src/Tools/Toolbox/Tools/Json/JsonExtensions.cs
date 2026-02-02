using System.Text.Json;
using Toolbox.Types;

namespace Toolbox.Tools;

public static class JsonExtensions
{
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
