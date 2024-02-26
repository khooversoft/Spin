using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.LangTools;
using Toolbox.Tools;

namespace Toolbox.Types;

/// <summary>
/// Tags format
///   "t" = single tag
///   "t1,t2" = 2 single tags
///   "t1=3,t4" = 2 tags, first with value, second single
///   "t1=3,t4='hello there'" = 2 tags, first with value, second with string value
/// </summary>
public class Tags : Dictionary<string, string?>
{
    public Tags() : base(StringComparer.OrdinalIgnoreCase) { }
    public Tags(string? line) : this() { Tags2Tool.Parse(line).ThrowOnError().Return().ForEach(x => Add(x.Key, x.Value)); }
    public Tags(IEnumerable<KeyValuePair<string, string?>> values) : base(values, StringComparer.OrdinalIgnoreCase) { }

    public Tags Set(string? tags)
    {
        Tags2Tool.Parse(tags).ThrowOnError().Return().ForEach(x => Set(x.Key, x.Value));
        return this;
    }

    public Tags Set<T>(T value) where T : class
    {
        if (value == null) return this;

        var dict = value.ToDictionary();
        dict.ForEach(x => base[x.Key] = x.Value);

        return this;
    }

    public Tags Set(string key, string? value)
    {
        if (key.IsEmpty()) return this;

        if (key.Length > 1 && key[0] == '-')
        {
            Remove(key[1..]);
            return this;
        }

        base[key] = value;
        return this;
    }

    public bool Has(string? value)
    {
        if (value.IsEmpty()) return false;
        if (value == "*") return true;

        // Must find all tags
        var find = Tags2Tool.Parse(value).ThrowOnError().Return().All(x => Has(x.Key, x.Value));

        return find;
    }

    public bool Has(string key, string? value)
    {
        return TryGetValue(key, out var readValue) switch
        {
            false => false,
            true => value == null || value == readValue,
        };
    }

    public Tags Clone() => new Tags(this);

    public override string ToString() => this
        .OrderBy(x => x.Key)
        .Select(x => Fmt(x.Key, x.Value))
        .Join(',');

    public bool Equals(Tags? other) => other is not null &&
        this.Count == other.Count &&
        this.All(x => other.TryGetValue(x.Key, out var subject) && (x.Value, subject) switch
        {
            (null, null) => true,
            (string s1, string s2) => s1.Equals(s1, StringComparison.OrdinalIgnoreCase),
            _ => false,
        });

    public override bool Equals(object? obj) => Equals(obj as Tags);
    public override int GetHashCode() => base.GetHashCode();
    public static bool operator ==(Tags? left, Tags? right) => EqualityComparer<Tags>.Default.Equals(left, right);
    public static bool operator !=(Tags? left, Tags? right) => !(left == right);

    public static implicit operator Tags(string? value) => new Tags(value);
    public static Tags operator +(Tags tags, string? value) => tags.Set(value);
    public static Tags operator +(Tags tags, (string key, string? value) subject) => tags.Set(subject.key, subject.value);

    private static string? Fmt(string key, string? value) => value.ToNullIfEmpty() switch
    {
        null => key,
        var v => v.All(x => char.IsLetterOrDigit(x) || x == '-' || x == '.') switch
        {
            true => $"{key}={value}",
            false => $"{key}='{value}'",
        }
    };
}



public static class Tags2Tool
{
    private static FrozenSet<string> _delimiters = new string[] { ",", "=" }.ToFrozenSet();
    private static FrozenSet<char> _allowCharacters = new char[] { '-', '.', ':' }.ToFrozenSet();

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