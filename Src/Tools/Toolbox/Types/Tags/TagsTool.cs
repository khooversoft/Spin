using System.Collections.Frozen;
using System.Collections.Immutable;
using Toolbox.Extensions;
using Toolbox.LangTools;
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
        (string k, null) when key.Length > 1 && key[0] == '-' && value.IsEmpty() => key[1..],
        _ => null,
    };

    public static ImmutableDictionary<string, string?> RemoveCommands(this IEnumerable<KeyValuePair<string, string?>> tags) => tags.NotNull()
        .Where(x => GetTagDeleteCommand(x.Key, x.Value).IsEmpty())
        .ToImmutableDictionary();

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

    public static Option<IReadOnlyList<KeyValuePair<string, string?>>> Parse(string? value)
    {
        var tokens = new StringTokenizer()
            .UseDoubleQuote()
            .UseSingleQuote()
            .UseCollapseWhitespace()
            .Add(_delimiters)
            .Parse(value)
            .Where(x => x.Value.IsNotEmpty())
            .Reverse()
            .ToStack();

        var result = new Dictionary<string, string?>(StringComparer.Ordinal);

        while (tokens.Count > 0)
        {
            if (tokens.Peek().Value == ",")
            {
                tokens.Pop();
                continue;
            }

            var assignment = parseAssignment(tokens);
            if (assignment != null)
            {
                if (!IsKeyValid(assignment.Value.Key, out Option v1)) return v1.ToOptionStatus<IReadOnlyList<KeyValuePair<string, string?>>>();
                result.Add(assignment.Value.Key, assignment.Value.Value);
                continue;
            }

            string tag = tokens.Pop().Value;
            if (_delimiters.Contains(tag)) return (StatusCode.BadRequest, $"Invalid token={tag}");
            if (!IsKeyValid(tag, out Option v2)) return v2.ToOptionStatus<IReadOnlyList<KeyValuePair<string, string?>>>();

            result.Add(tag, null);
        }

        var dict = result.ToArray();
        return dict;

        KeyValuePair<string, string?>? parseAssignment(Stack<IToken> tokens)
        {
            if (tokens.Count < 3) return null;
            if (tokens.Skip(1).First().Value != "=") return null;

            string key = tokens.Pop().Value;
            tokens.Pop(); // Remove "="
            string value = tokens.Pop().Value;

            return new KeyValuePair<string, string?>(key, value);
        }
    }

    public static bool IsKeyValid(string? key, out Option result)
    {
        result = key.IsEmpty() switch
        {
            true => (StatusCode.BadRequest, "Key is empty"),
            false => key switch
            {
                { Length: 1 } v when v == "*" => StatusCode.OK,
                { Length: 1 } v when _allowCharacters.Contains(v[0]) => (StatusCode.BadRequest, $"Invalid key={key}"),

                var v => v.All(x => char.IsLetterOrDigit(x) || _allowCharacters.Contains(x)) switch
                {
                    true => StatusCode.OK,
                    false => (StatusCode.BadRequest, $"Invalid key={key}"),
                },
            }
        };


        return result.IsOk();
    }
}