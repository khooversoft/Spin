using Toolbox.Extensions;

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
    public Tags(Tags tags) : base(tags, StringComparer.OrdinalIgnoreCase) { }
    public Tags(string? line) : this() { TagsTool.Parse(line).ThrowOnError().Return().ForEach(x => Add(x.Key, x.Value)); }
    public Tags(IEnumerable<KeyValuePair<string, string?>> values) : base(values, StringComparer.OrdinalIgnoreCase) { }

    public Tags Set(string? tags)
    {
        TagsTool.Parse(tags).ThrowOnError().Return().ForEach(x => Set(x.Key, x.Value));
        return this;
    }

    public Tags SetObject<T>(T value) where T : class
    {
        if (value == null) return this;

        var dict = value.ToDictionary();
        dict.ForEach(x => base[x.Key] = x.Value);

        return this;
    }

    public Tags Set(string key, string? value)
    {
        if (key.IsEmpty()) return this;

        if (key.Length > 1 && key[0] == '-' && value == null)
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
        var find = TagsTool.Parse(value).ThrowOnError().Return().All(x => Has(x.Key, x.Value));
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

    public static Tags operator +(Tags tags, string? value) => tags.Set(value);
    public static Tags operator +(Tags tags, (string key, string? value) subject) => tags.Set(subject.key, subject.value);

    public static implicit operator Tags(string? value) => new Tags(value);
    public static implicit operator string(Tags tags) => tags.ToString();

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
