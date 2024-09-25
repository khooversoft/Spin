using System.Collections.Frozen;
using System.Collections.Immutable;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Types;

public static class TagsTool
{
    private static FrozenSet<string> _delimiters = new string[] { ",", "=" }.ToFrozenSet();
    private static FrozenSet<char> _allowCharacters = new char[] { '*', '-', '.', ':' }.ToFrozenSet();

    public static ImmutableDictionary<string, string?> ToTags(this string? tags) => TagsTool.Parse(tags)
        .ThrowOnError().Return()
        .ToImmutableDictionary(StringComparer.OrdinalIgnoreCase);

    public static ImmutableDictionary<string, string?> ToTags(this IReadOnlyDictionary<string, string?> tags) => tags
        .ToImmutableDictionary(StringComparer.OrdinalIgnoreCase);

    public static ImmutableDictionary<string, string?> ProcessTags(this IEnumerable<KeyValuePair<string, string?>> tags, IEnumerable<KeyValuePair<string, string?>> tagCommands)
    {
        tagCommands.NotNull();

        var dict = tags.NotNull().ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);

        tagCommands.Where(x => x.Key.IsNotEmpty()).ForEach(x => dict.ApplyTagCommand(x.Key, x.Value));

        return dict.ToImmutableDictionary(StringComparer.OrdinalIgnoreCase);
    }

    public static ImmutableDictionary<string, string?> Empty { get; } = ImmutableDictionary<string, string?>.Empty;

    public static void ApplyTagCommand(this Dictionary<string, string?> tags, string key, string? value = null)
    {
        if (key.IsEmpty()) return;

        var removeKey = GetTagDeleteCommand(key, value);
        if (removeKey.IsNotEmpty())
        {
            tags.Remove(removeKey);
            return;
        }

        tags[key] = value;
    }

    public static string? GetTagDeleteCommand(string key, string? value) => (key, value) switch
    {
        (string, null) when key.Length > 1 && key[0] == '-' => key[1..],
        _ => null,
    };

    public static ImmutableDictionary<string, string?> RemoveDeleteCommands(this IEnumerable<KeyValuePair<string, string?>> tags) => tags.NotNull()
        .Where(x => GetTagDeleteCommand(x.Key, x.Value).IsEmpty())
        .ToImmutableDictionary();

    public static IReadOnlyList<string> GetTagDeleteCommands(this IEnumerable<KeyValuePair<string, string?>> tags) => tags.NotNull()
        .Select(x => GetTagDeleteCommand(x.Key, x.Value))
        .OfType<string>()
        .ToImmutableArray();


    public static string? FormatTag(string key, string? value) => value.ToNullIfEmpty() switch
    {
        null => key,
        var v => v.All(x => char.IsLetterOrDigit(x) || char.IsSymbol(x) || char.IsPunctuation(x) || x == '-' || x == '.') switch
        {
            true => $"{key}={value}",
            false => $"{key}='{value}'",
        }
    };

    public static string ToTagsString(this IEnumerable<KeyValuePair<string, string?>> tags) => tags
        .OrderBy(x => x.Key, StringComparer.OrdinalIgnoreCase)
        .Select(x => TagsTool.FormatTag(x.Key, x.Value))
        .Join(',');

    public static bool Has(this IReadOnlyDictionary<string, string?> tags, string? value)
    {
        if (value.IsEmpty()) return false;
        if (value == "*") return true;

        var set = TagsTool.Parse(value).ThrowOnError().Return();

        // Must find all tags
        var find = set.All(x => tags.Has(x.Key, x.Value));
        return find;
    }

    public static bool Has(this IReadOnlyDictionary<string, string?> tags, IEnumerable<KeyValuePair<string, string?>> searchTags)
    {
        tags.NotNull();
        searchTags.NotNull();

        // Must find all tags
        var find = searchTags.All(x => tags.Has(x.Key, x.Value));
        return find;
    }

    public static bool Has(this IReadOnlyDictionary<string, string?> tags, string key, string? value) => key switch
    {
        "*" => true,
        _ => tags.TryGetValue(key, out var readValue) switch
        {
            false => false,
            true => value == null || value == readValue,
        },
    };

    public static Option<IReadOnlyList<KeyValuePair<string, string?>>> Parse(string? value) => PropertyStringSchema.Tags.Parse(value);
}