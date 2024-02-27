using System.Collections.Frozen;
using Toolbox.Extensions;
using Toolbox.LangTools;
using Toolbox.Tools;

namespace Toolbox.Types;

public static class TagsTool
{
    private static FrozenSet<string> _delimiters = new string[] { ",", "=" }.ToFrozenSet();
    private static FrozenSet<char> _allowCharacters = new char[] { '*', '-', '.', ':' }.ToFrozenSet();

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

        var result = new Sequence<KeyValuePair<string, string?>>();

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
                result.Add(assignment.Value);
                continue;
            }

            string tag = tokens.Pop().Value;
            if (_delimiters.Contains(tag)) return (StatusCode.BadRequest, $"Invalid token={tag}");
            if (!IsKeyValid(tag, out Option v2)) return v2.ToOptionStatus<IReadOnlyList<KeyValuePair<string, string?>>>();

            result.Add(new KeyValuePair<string, string?>(tag, null));
        }

        return result;

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

    public static T ToObject<T>(this Tags subject) where T : new()
    {
        subject.NotNull();

        var dict = subject
            .Select(x => new KeyValuePair<string, string>(x.Key, x.Value ?? "true"))
            .ToDictionary(x => x.Key, x => x.Value);

        var result = DictionaryExtensions.ToObject<T>(dict);
        return result;
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