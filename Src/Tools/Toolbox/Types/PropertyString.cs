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

    public static PropertyString<string?> ConnectionString { get; } = new PropertyString<string?>(false, [";", "="], x => x == ";", x => x switch
    {
        null => false,
        string v => TestKey(v, _connectionStringAllowCharacters),
    });

    public static PropertyString<string?> Tags { get; } = new PropertyString<string?>(false, [",", "="], x => x == ",", x => x switch
    {
        null => false,
        string key => key switch
        {
            { Length: 1 } v when v == "*" => true,
            var v => (v[0] == '-' || char.IsLetterOrDigit(v[0])) && v.Skip(1).All(x => char.IsLetterOrDigit(x) || _connectionStringAllowCharacters.Contains(x)),
        },
    });

    public static PropertyString<string> KeyValuePair { get; } = new PropertyString<string>(true, [",", "="], x => x == ",", x => x switch
        {
            null => false,
            string key => key switch
            {
                { Length: 1 } v when v == "*" => true,
                var v => TestKey(v, _connectionStringAllowCharacters),
            },
        }
    );

    public static PropertyString<string?> FileSearch { get; } = new PropertyString<string?>(false, [";", "="], x => x == ";", x => x switch
    {
        null => false,
        string key => key.All(x => char.IsLetterOrDigit(x) || _fileSearchAllowCharacters.Contains(x)),
    });

    private static bool TestKey(string key, FrozenSet<char> set) => key switch
    {
        { Length: 0 } => false,
        { Length: 1 } v when char.IsLetterOrDigit(v[0]) => true,
        var v => char.IsLetterOrDigit(v[0]) && v.Skip(1).All(x => char.IsLetterOrDigit(x) || set.Contains(x)) switch
        {
            true => true,
            false => false,
        },
    };
}

public class PropertyString<TValue>
{
    private readonly FrozenSet<string> _tokens;
    private readonly Func<string, bool> _isDelimiter;
    private readonly Func<string, bool> _isValidKey;
    private readonly bool _isValueRequired;

    public PropertyString(bool isValueRequired, IEnumerable<string> tokens, Func<string, bool> isDelimiter, Func<string, bool> isValidKey)
    {
        _isValueRequired = isValueRequired;
        _tokens = tokens.ToFrozenSet();
        _isDelimiter = isDelimiter.NotNull();
        _isValidKey = isValidKey;
    }

    public Option<IReadOnlyList<KeyValuePair<string, TValue>>> Parse(string? value)
    {
        var result = new Dictionary<string, TValue>(StringComparer.Ordinal);

        foreach (var item in Tokens(value))
        {
            if (item.IsError()) return item.ToOptionStatus<IReadOnlyList<KeyValuePair<string, TValue>>>();
            KeyValuePair<string, TValue> r = item.Return();

            result.Add(r.Key, r.Value);
        }

        return result.ToArray();
    }

    private IEnumerable<Option<KeyValuePair<string, TValue>>> Tokens(string? value)
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

        bool check = false;
        while (tokens.Count > 0)
        {
            if (check)
            {
                check = false;
                var delimiter = tokens.Pop();
                if (!_isDelimiter(delimiter.Value)) yield return (StatusCode.BadRequest, $"Invalid delimiter={delimiter}");
                continue;
            }

            check = true;
            var next = ParseAssignment(tokens);
            if (next.IsOk())
            {
                yield return next;
                continue;
            }

            if (_isValueRequired) yield return (StatusCode.BadRequest, "Value is required");

            string key = tokens.Pop().Value;
            if (!IsKeyValid(key)) yield return (StatusCode.BadRequest, $"Invalid key={key}");
            yield return new KeyValuePair<string, TValue>(key, default!);
        }
    }

    private bool IsKeyValid(string key) => !_isDelimiter(key) && _isValidKey(key);

    private static Option<KeyValuePair<string, TValue>> ParseAssignment(Stack<IToken> tokens)
    {
        if (tokens.Count < 3) return (StatusCode.BadRequest, "No key=value");
        if (tokens.Skip(1).First().Value != "=") return (StatusCode.BadRequest, "No equal '='");

        string key = tokens.Pop().Value;
        tokens.Pop(); // Remove "="
        string value = tokens.Pop().Value;

        return new KeyValuePair<string, TValue>(key, (TValue)(object)value);
    }
}
