//using System.Collections;
//using System.Collections.Immutable;
//using System.Diagnostics.CodeAnalysis;
//using System.Text.Json.Serialization;
//using Toolbox.Extensions;
//using Toolbox.Tools;

//namespace Toolbox.Types;

//public sealed class ImmutableTags : IReadOnlyDictionary<string, string?>, IEquatable<ImmutableTags?>
//{
//    public ImmutableDictionary<string, string?> _tags;

//    public ImmutableTags() => _tags = ImmutableDictionary<string, string?>.Empty;

//    [JsonConstructor]
//    public ImmutableTags(IReadOnlyDictionary<string, string?> tags)
//    {
//        _tags = tags.NotNull().ToImmutableDictionary(StringComparer.OrdinalIgnoreCase);
//    }

//    public ImmutableTags(string? tags)
//    {
//        if( tags.IsEmpty())
//        {
//            _tags = ImmutableDictionary<string, string?>.Empty;
//            return;
//        }

//        _tags = TagsTool
//            .Parse(tags).ThrowOnError().Return()
//            .ToImmutableDictionary(StringComparer.OrdinalIgnoreCase);
//    }

//    public IEnumerable<KeyValuePair<string, string?>> Tags => _tags;

//    [JsonIgnore] public int Count => _tags.Count;
//    public IEnumerable<string> Keys => _tags.Keys;
//    public IEnumerable<string?> Values => _tags.Values;

//    public string? this[string key] => _tags[key];
//    public bool ContainsKey(string key) => _tags.ContainsKey(key);

//    public bool Has(string? value)
//    {
//        if (value.IsEmpty()) return false;
//        if (value == "*") return true;

//        // Must find all tags
//        var find = TagsTool.Parse(value).ThrowOnError().Return().All(x => Has(x.Key, x.Value));
//        return find;
//    }

//    public bool Has(string key, string? value)
//    {
//        return _tags.TryGetValue(key, out var readValue) switch
//        {
//            false => false,
//            true => value == null || value == readValue,
//        };
//    }

//    public override string ToString() => _tags
//        .OrderBy(x => x.Key)
//        .Select(x => TagsTool.FormatTag(x.Key, x.Value))
//        .Join(',');

//    public override bool Equals(object? obj) => Equals(obj as ImmutableTags);
//    public bool Equals(ImmutableTags? other) => other is not null && _tags.DeepEquals(other._tags);
//    public override int GetHashCode() => _tags.GetHashCode();

//    public bool TryGetValue(string key, [MaybeNullWhen(false)] out string? value)
//    {
//        throw new NotImplementedException();
//    }

//    public IEnumerator<KeyValuePair<string, string?>> GetEnumerator() => _tags.GetEnumerator();
//    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

//    public static bool operator ==(ImmutableTags? left, ImmutableTags? right) => left?.Equals(right) == true;
//    public static bool operator !=(ImmutableTags? left, ImmutableTags? right) => !(left == right);

//    public static implicit operator ImmutableTags(string? value) => new ImmutableTags(value);
//    public static implicit operator string(ImmutableTags tags) => tags.ToString();
//}

