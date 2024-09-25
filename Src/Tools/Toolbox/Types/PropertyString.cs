using System.Collections.Frozen;
using Toolbox.Extensions;
using Toolbox.LangTools;
using Toolbox.Tools;

namespace Toolbox.Types;

public static class PropertyStringSchema
{
    private static FrozenSet<char> _connectionStringAllowCharacters = new[] { '-', '.', ':', '/' }.ToFrozenSet();
    private static FrozenSet<char> _tagAllowCharacters = new[] { '*', '-', '.', ':' }.ToFrozenSet();
    private static FrozenSet<char> _fileSearchAllowCharacters = new[] { '-', '.', ':', '/', '*' }.ToFrozenSet();

    public static PropertyString ConnectionString { get; } = new PropertyString([";", "="], x => x == ";", x => x switch
    {
        null => false,
        string key => key switch
        {
            { Length: 1 } v when _connectionStringAllowCharacters.Contains(v[0]) => false,
            var v => v.All(x => char.IsLetterOrDigit(x) || _connectionStringAllowCharacters.Contains(x)) switch
            {
                true => true,
                false => false,
            },
        },
    });

    public static PropertyString Tags { get; } = new PropertyString([",", "="], x => x == ",", x => x switch
    {
        null => false,
        string key => key switch
        {
            { Length: 1 } v when v == "*" => true,
            { Length: 1 } v when _connectionStringAllowCharacters.Contains(v[0]) => false,
            var v => v.All(x => char.IsLetterOrDigit(x) || _connectionStringAllowCharacters.Contains(x)) switch
            {
                true => true,
                false => false,
            },
        },
    });

    public static PropertyString FileSearch { get; } = new PropertyString([";", "="], x => x == ";", x => x switch
    {
        null => false,
        string key => key switch
        {
            { Length: 1 } v when v == "*" => true,
            { Length: 1 } v when _fileSearchAllowCharacters.Contains(v[0]) => false,
            var v => v.All(x => char.IsLetterOrDigit(x) || _fileSearchAllowCharacters.Contains(x)) switch
            {
                true => true,
                false => false,
            },
        },
    });
}

public class PropertyString
{
    private readonly FrozenSet<string> _tokens;
    private readonly Func<string, bool> _isDelimiter;
    private readonly Func<string, bool> _isValidKey;

    public PropertyString(IEnumerable<string> tokens, Func<string, bool> isDelimiter, Func<string, bool> isValidKey)
    {
        _tokens = tokens.ToFrozenSet();
        _isDelimiter = isDelimiter.NotNull();
        _isValidKey = isValidKey;
    }

    public Option<IReadOnlyList<KeyValuePair<string, string?>>> Parse(string? value)
    {
        var tokens = new StringTokenizer()
            .UseDoubleQuote()
            .UseSingleQuote()
            .UseCollapseWhitespace()
            .Add(_tokens)
            .Parse(value)
            .Where(x => x.Value.IsNotEmpty())
            .Reverse()
            .ToStack();

        var result = new Dictionary<string, string?>(StringComparer.Ordinal);

        bool check = false;
        while (tokens.Count > 0)
        {
            if (check)
            {
                check = false;
                var delimiter = tokens.Pop();
                if (!_isDelimiter(delimiter.Value)) return (StatusCode.BadRequest, $"Invalid delimiter={delimiter}");
                continue;
            }
            check = true;

            var assignment = ParseAssignment(tokens);
            if (assignment != null)
            {
                if (!_isValidKey(assignment.Value.Key)) return (StatusCode.BadRequest, $"Invalid token={assignment.Value.Key}");
                result.Add(assignment.Value.Key, assignment.Value.Value);
                continue;
            }

            string key = tokens.Pop().Value;
            if (!_isValidKey(key)) return (StatusCode.BadRequest, $"Invalid token={key}");
            result.Add(key, null);
        }

        var dict = result.ToArray();
        return dict;
    }

    private static KeyValuePair<string, string?>? ParseAssignment(Stack<IToken> tokens)
    {
        if (tokens.Count < 3) return null;
        if (tokens.Skip(1).First().Value != "=") return null;

        string key = tokens.Pop().Value;
        tokens.Pop(); // Remove "="
        string value = tokens.Pop().Value;

        return new KeyValuePair<string, string?>(key, value);
    }
}
