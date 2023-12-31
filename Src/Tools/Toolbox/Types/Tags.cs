using Toolbox.Extensions;
using Toolbox.Tools;
using System;

namespace Toolbox.Types;

public class Tags : Dictionary<string, string?>
{
    public Tags() : base(StringComparer.OrdinalIgnoreCase) { }
    public Tags(IEnumerable<KeyValuePair<string, string?>> values) : base(values, StringComparer.OrdinalIgnoreCase) { }
    public Tags(string? tags) : base(StringComparer.OrdinalIgnoreCase) => Set(tags);

    public Tags Set(string? tags)
    {
        if (tags.IsEmpty()) return this;

        var keyValuePairs = tags.ToDictionaryFromString();
        keyValuePairs.ForEach(x => SetValue(x.Key, x.Value));
        return this;
    }

    public Tags Set<T>(T? value) where T : class
    {
        if (value == null) return this;

        var dict = value.ToDictionary();
        dict.ForEach(x => SetValue(x.Key, x.Value));
        return this;
    }

    public Tags SetValue(string key) => this.Action(x => SetValue(key, null));

    public Tags SetValue(string key, string? value)
    {
        key.NotEmpty();

        if (key.Length > 1 && key[0] == '-')
        {
            Remove(key[1..]);
            return this;
        }

        this[key] = value;
        return this;
    }

    public Tags SetValues(IEnumerable<KeyValuePair<string, string?>> values)
    {
        values.NotNull().ForEach(x => SetValue(x.Key, x.Value));
        return this;
    }

    public bool Has(string? key) => key switch
    {
        string v => v.Func(_ => v.ToDictionaryFromString().All(x => TryGetValue(x.Key, out var value) switch
        {
            false => false,
            true => x.Value == null || x.Value == value,
        })),

        _ => false,
    };

    public bool Has(string key, string value)
    {
        return TryGetValue(key, out var readValue) switch
        {
            false => false,
            true => value == readValue,
        };
    }

    public override string ToString() => this
        .OrderBy(x => x.Key)
        .Select(x => x.Value.IsEmpty() ? x.Key : $"{x.Key}={x.Value}")
        .Join(';');

    public Tags Copy() => new Tags(ToString());

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
}
