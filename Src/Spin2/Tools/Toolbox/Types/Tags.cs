﻿using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Types;

public class Tags : Dictionary<string, string?>
{
    public Tags() : base(StringComparer.OrdinalIgnoreCase) { }
    public Tags(IEnumerable<KeyValuePair<string, string?>> values) : base(values, StringComparer.OrdinalIgnoreCase) { }

    public Tags Set(string? tags)
    {
        if (tags.IsEmpty()) return this;

        var keyValuePairs = tags.ToDictionaryFromString();
        keyValuePairs.ForEach(x => this[x.Key] = x.Value);
        return this;
    }

    public Tags SetValue(string key) => this.Action(x => this[key.NotEmpty()] = null);
    public Tags SetValue(string key, string value) => this.Action(x => this[key.NotEmpty()] = value);

    public bool Has(string? key) => key switch
    {
        string v => v.Func(x => v.ToDictionaryFromString().All(x => TryGetValue(x.Key, out var value) switch
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
        .Select(x => x.Value.IsEmpty() ? x.Key : $"{x.Key}={x.Value}")
        .Join(';');

    public string ToString(bool order) => order switch
    {
        false => this.ToString(),
        true => this.OrderBy(x => x.Key).Select(x => x.Value.IsEmpty() ? x.Key : $"{x.Key}={x.Value}").Join(';'),
    };

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

    public static Tags Parse(string? subject) => new Tags().Set(subject);

    public static Tags Create<T>(T value) where T : class
    {
        var dict = value.ToDictionary();
        return new Tags(dict.OfType<KeyValuePair<string, string?>>());
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
