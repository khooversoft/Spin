using System.Collections.Frozen;
using System.Collections.Immutable;
using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.Tools;

public static class TagsTool
{
    private readonly static FrozenSet<string> _delimiters = new string[] { ",", "=" }.ToFrozenSet();
    private readonly static FrozenSet<char> _allowCharacters = new char[] { '*', '-', '.', ':' }.ToFrozenSet();

    public static IReadOnlyDictionary<string, string?> ToTags(this string? tags) => Parse(tags)
        .ThrowOnError().Return()
        .ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

    public static IReadOnlyDictionary<string, string?> ToTags(this (string key, string? value) tag) => new KeyValuePair<string, string?>(tag.key, tag.value)
        .ToEnumerable()
        .ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

    public static IReadOnlyDictionary<string, string?> ProcessTags(this IEnumerable<KeyValuePair<string, string?>> tags, IEnumerable<KeyValuePair<string, string?>> tagCommands)
    {
        tagCommands.NotNull();

        var dict = tags.NotNull().ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);

        tagCommands.Where(x => x.Key.IsNotEmpty()).ForEach(x => dict.ApplyTagCommand(x.Key, x.Value));

        return dict.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
    }

    public static void ApplyTagCommand(this IDictionary<string, string?> tags, string key, string? value = null)
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

    public static IReadOnlyDictionary<string, string?> RemoveDeleteCommands(this IEnumerable<KeyValuePair<string, string?>> tags) => tags.NotNull()
        .Where(x => GetTagDeleteCommand(x.Key, x.Value).IsEmpty())
        .ToFrozenDictionary();

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

    public static string FormatTagKey(string edgeType, params string[] suffixes) => edgeType.NotEmpty().ToEnumerable()
        .Concat(suffixes)
        .Join('-');

    public static string ToTagsString(this IEnumerable<KeyValuePair<string, string?>> tags) => tags
        .OrderBy(x => x.Key, StringComparer.OrdinalIgnoreCase)
        .Select(x => FormatTag(x.Key, x.Value))
        .Join(',');

    public static bool Has(this IReadOnlyDictionary<string, string?> tags, string? value)
    {
        if (value.IsEmpty()) return false;
        if (value == "*") return true;

        var set = Parse(value).ThrowOnError().Return();

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

    public static IReadOnlyDictionary<string, string?> Merge(this IEnumerable<KeyValuePair<string, string?>> source1, IEnumerable<KeyValuePair<string, string?>>? source2)
    {
        source1.NotNull();
        IEnumerable<KeyValuePair<string, string?>> currentTags = source2 ??= Array.Empty<KeyValuePair<string, string?>>();

        var list = source1
            .Concat(source2)
            .GroupBy(x => x.Key, StringComparer.OrdinalIgnoreCase)
            .Select(x => x.First())
            .ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

        return list;
    }

    public static IReadOnlyDictionary<string, string?> MergeAndFilter(
        this IEnumerable<KeyValuePair<string, string?>> newTags,
        IEnumerable<KeyValuePair<string, string?>>? currentTagsValue
        )
    {
        newTags.NotNull();
        IEnumerable<KeyValuePair<string, string?>> currentTags = currentTagsValue ??= Array.Empty<KeyValuePair<string, string?>>();

        var removeTags = newTags.GetTagDeleteCommands();

        var list = newTags
            .Concat(currentTags)
            .Where(x => !HasRemoveFlag(x.Key))
            .Where(x => !removeTags.Contains(x.Key))
            .GroupBy(x => x.Key, StringComparer.OrdinalIgnoreCase)
            .Select(x => x.First())
            .ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

        return list;
    }

    public static bool HasRemoveFlag(string value) => value.IsNotEmpty() && value.Length > 1 && value[0] == '-';
}
