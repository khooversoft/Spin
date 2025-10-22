using System.Collections.Frozen;
using Toolbox.Extensions;
using Toolbox.LangTools;
using Toolbox.Tools;

namespace Toolbox.Types;

public static class PropertyStringSchema
{
    private readonly static FrozenSet<char> _connectionStringAllowCharacters = new[] { '-', '.', ':', '/' }.ToFrozenSet();
    private readonly static FrozenSet<char> _tagAllowCharacters = new[] { '*', '-', '.', ':' }.ToFrozenSet();
    private readonly static FrozenSet<char> _fileSearchAllowCharacters = new[] { '-', '_', '.', ':', '/', '*' }.ToFrozenSet();

    public static PropertyString<string?> ConnectionString { get; } = new PropertyString<string?>(false, [";", "="], x => x == ";", x => x switch
    {
        null => false,
        string v => TestKey(v, _connectionStringAllowCharacters),
    });

    public static PropertyString<string?> Tags { get; } = new PropertyString<string?>(false, [",", "="], x => x == ",", x => x switch
    {
        null => false,
        string key => IsValidTagKey(key),
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
        string key => IsValidFileSearchKey(key),
    });

    private static bool TestKey(string key, FrozenSet<char> set)
    {
        if (string.IsNullOrEmpty(key)) return false;
        if (!char.IsLetterOrDigit(key[0])) return false;

        for (int i = 1; i < key.Length; i++)
        {
            char c = key[i];
            if (!char.IsLetterOrDigit(c) && !set.Contains(c)) return false;
        }

        return true;
    }

    private static bool IsValidTagKey(string key)
    {
        // "*" allowed
        if (key.Length == 1 && key[0] == '*') return true;
        if (key.Length == 0) return false;

        char c0 = key[0];
        if (!(c0 == '-' || char.IsLetterOrDigit(c0))) return false;

        for (int i = 1; i < key.Length; i++)
        {
            char c = key[i];
            if (!char.IsLetterOrDigit(c) && !_tagAllowCharacters.Contains(c)) return false;
        }
        return true;
    }

    private static bool IsValidFileSearchKey(string key)
    {
        if (key.Length == 0) return false;

        for (int i = 0; i < key.Length; i++)
        {
            char c = key[i];
            if (!char.IsLetterOrDigit(c) && !_fileSearchAllowCharacters.Contains(c)) return false;
        }
        return true;
    }
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
        _isValidKey = isValidKey.NotNull();
    }

    public Option<IReadOnlyList<KeyValuePair<string, TValue>>> Parse(string? value)
    {
        var result = new Dictionary<string, TValue>(StringComparer.Ordinal);

        foreach (var item in Tokens(value))
        {
            if (item.IsError()) return item.ToOptionStatus<IReadOnlyList<KeyValuePair<string, TValue>>>();
            KeyValuePair<string, TValue> r = item.Return();

            if (!result.TryAdd(r.Key, r.Value)) return (StatusCode.BadRequest, $"Duplicate key={r.Key}");
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

    private Option<KeyValuePair<string, TValue>> ParseAssignment(Stack<IToken> tokens)
    {
        if (tokens.Count < 3) return (StatusCode.BadRequest, "No key=value");

        var keyToken = tokens.Pop();
        var eqToken = tokens.Pop();

        if (eqToken.Value != "=")
        {
            tokens.Push(eqToken);
            tokens.Push(keyToken);
            return (StatusCode.BadRequest, "No equal '='");
        }

        var valueToken = tokens.Pop();

        string key = keyToken.Value;
        if (!IsKeyValid(key)) return (StatusCode.BadRequest, $"Invalid key={key}");

        return new KeyValuePair<string, TValue>(key, (TValue)(object)valueToken.Value);
    }
}
