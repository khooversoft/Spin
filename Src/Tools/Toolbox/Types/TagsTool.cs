using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Types;

public static class TagsTool
{
    public static T ToObject<T>(this Tags subject) where T : new()
    {
        subject.NotNull();

        var dict = subject
            .Select(x => new KeyValuePair<string, string>(x.Key, x.Value ?? "true"))
            .ToDictionary(x => x.Key, x => x.Value);

        var result = DictionaryExtensions.ToObject<T>(dict);
        return result;
    }

    public static bool HasTag(string? tags, string tag)
    {
        if (tags == null) return false;
        tag.NotEmpty();

        var memoryTag = tag.AsMemory();

        foreach (var item in tags.AsMemory().Split(';'))
        {
            foreach (ReadOnlyMemory<char> field in item.Split('='))
            {
                bool isEqual = field.Span.Equals(memoryTag.Span, StringComparison.OrdinalIgnoreCase);
                if (isEqual) return true;
            }
        }

        return false;
    }

    public static bool TryGetValue(string? tags, string tag, out string? value)
    {
        value = null;

        if (tags == null) return false;
        tag.NotEmpty();

        var memoryTag = tag.AsMemory();

        foreach (var item in tags.AsMemory().Split(';'))
        {
            bool first = true;
            foreach (ReadOnlyMemory<char> field in item.Split('='))
            {
                if (first)
                {
                    first = false;
                    bool isEqual = field.Span.Equals(memoryTag.Span, StringComparison.OrdinalIgnoreCase);
                    if (isEqual) continue;
                    break;
                }

                value = field.Span.ToString();
                return true;
            }
        }

        return false;
    }

    public static bool HasTag(string? tags, string tag, string value)
    {
        if (tags == null) return false;
        tag.NotEmpty();
        value.NotEmpty();

        var memoryTag = tag.AsMemory();
        var memoryValue = value.AsMemory();

        foreach (var item in tags.AsMemory().Split(';'))
        {
            foreach (var field in item.Split('=').WithIndex())
            {
                if (field.Index == 0 && !field.Item.Span.Equals(memoryTag.Span, StringComparison.OrdinalIgnoreCase)) break;
                if (field.Index == 1 && field.Item.Span.Equals(memoryValue.Span, StringComparison.OrdinalIgnoreCase)) return true;
            }
        }

        return false;
    }
}
