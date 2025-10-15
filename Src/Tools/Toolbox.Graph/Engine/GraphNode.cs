using System.Collections.Frozen;
using System.Diagnostics;
using System.Text.Json.Serialization;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

[DebuggerDisplay("Key={Key}, Tags={TagsString}, DataMap={DataMapString}, Indexes={IndexesString}, ForeignKeys={ForeignKeysString}")]
public sealed record GraphNode : IGraphCommon
{
    public GraphNode(string key, string? tags = null, string? indexes = null, string? foreignKeys = null)
    {
        Key = key.NotNull();
        Tags = TagsTool.Parse(tags).ThrowOnError().Return().ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

        Indexes = SplitValues(indexes);
        ForeignKeys = TagsTool.Parse(foreignKeys).ThrowOnError().Return().ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

        IReadOnlyCollection<string> SplitValues(string? value) => value switch
        {
            null => FrozenSet<string>.Empty,

            string v => v
                .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToFrozenSet(StringComparer.OrdinalIgnoreCase),
        };
    }

    [JsonConstructor]
    public GraphNode(
        string key,
        IReadOnlyDictionary<string, string?> tags,
        DateTime createdDate,
        IReadOnlyDictionary<string, GraphLink> dataMap,
        IReadOnlyCollection<string> indexes,
        IReadOnlyDictionary<string, string?> foreignKeys
        )
    {
        Key = key.NotNull();

        Tags = tags?.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase) ?? FrozenDictionary<string, string?>.Empty;
        CreatedDate = createdDate;
        DataMap = dataMap?.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase) ?? FrozenDictionary<string, GraphLink>.Empty;

        Indexes = setCollection(indexes);
        ForeignKeys = foreignKeys?.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase) ?? FrozenDictionary<string, string?>.Empty;

        IReadOnlyCollection<string> setCollection(IReadOnlyCollection<string> value) => value switch
        {
            null => FrozenSet<string>.Empty,
            var v => v
                .Where(x => x.IsNotEmpty())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToFrozenSet(StringComparer.OrdinalIgnoreCase),
        };
    }

    public string Key { get; }
    public IReadOnlyDictionary<string, string?> Tags { get; }
    public DateTime CreatedDate { get; init; } = DateTime.UtcNow;
    public IReadOnlyDictionary<string, GraphLink> DataMap { get; } = FrozenDictionary<string, GraphLink>.Empty;
    public IReadOnlyCollection<string> Indexes { get; } = FrozenSet<string>.Empty;
    public IReadOnlyDictionary<string, string?> ForeignKeys { get; } = FrozenDictionary<string, string?>.Empty;
    [JsonIgnore] public string TagsString => Tags.ToTagsString();
    [JsonIgnore] public string DataMapString => DataMap.ToDataMapString();
    [JsonIgnore] public string IndexesString => Indexes.Join(',');
    [JsonIgnore] public string ForeignKeysString => ForeignKeys.ToTagsString();

    public bool Equals(GraphNode? obj)
    {
        if (ReferenceEquals(this, obj)) return true;
        if (obj is null) return false;

        bool result = Key.EqualsIgnoreCase(obj.Key) &&
            Tags.DeepEquals(obj.Tags, StringComparer.OrdinalIgnoreCase) &&
            CreatedDate == obj.CreatedDate &&
            DataMap.DeepEquals(obj.DataMap, StringComparer.OrdinalIgnoreCase) &&
            SetEqualsIgnoreCase(Indexes, obj.Indexes) &&
            ForeignKeys.DeepEquals(obj.ForeignKeys, StringComparer.OrdinalIgnoreCase);

        return result;
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();

        hash.Add(Key, StringComparer.Ordinal);
        hash.Add(CreatedDate);

        // Tags: order-independent, case-insensitive keys
        foreach (var kv in Tags.OrderBy(x => x.Key, StringComparer.OrdinalIgnoreCase))
        {
            hash.Add(kv.Key, StringComparer.OrdinalIgnoreCase);
            hash.Add(kv.Value ?? string.Empty, StringComparer.Ordinal);
        }

        // DataMap: order-independent, case-insensitive keys, rely on GraphLink's equality/hash
        foreach (var kv in DataMap.OrderBy(x => x.Key, StringComparer.OrdinalIgnoreCase))
        {
            hash.Add(kv.Key, StringComparer.OrdinalIgnoreCase);
            hash.Add(kv.Value);
        }

        // Indexes: order-independent, case-insensitive
        foreach (var v in Indexes.OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
        {
            hash.Add(v, StringComparer.OrdinalIgnoreCase);
        }

        // ForeignKeys: order-independent, case-insensitive keys
        foreach (var kv in ForeignKeys.OrderBy(x => x.Key, StringComparer.OrdinalIgnoreCase))
        {
            hash.Add(kv.Key, StringComparer.OrdinalIgnoreCase);
            hash.Add(kv.Value ?? string.Empty, StringComparer.Ordinal);
        }

        return hash.ToHashCode();
    }

    public override string ToString() => $"Key={Key}, Tags={TagsString}, DataMap={DataMapString}, Indexes={IndexesString}, ForeignKeys={ForeignKeysString}";

    public static IValidator<GraphNode> Validator { get; } = new Validator<GraphNode>()
        .RuleFor(x => x.Key).NotEmpty()
        .RuleFor(x => x.Tags).NotNull().Must(d => d.All(kv => kv.Key.IsNotEmpty()), _ => "Tag key must not be empty")
        .RuleFor(x => x.CreatedDate).ValidDateTime()
        .RuleFor(x => x.DataMap).NotNull().Must(x => x.All(y => y.Key.IsNotEmpty() && y.Value.Validate().IsOk()), _ => "Graph link does not validate")
        .RuleFor(x => x.Indexes).NotNull().Must(x => x.All(x => x.IsNotEmpty()), _ => "Indexed data key must not be empty")
        .RuleFor(x => x.ForeignKeys).NotNull()
        .Build();

    private static bool SetEqualsIgnoreCase(IReadOnlyCollection<string> a, IReadOnlyCollection<string> b)
    {
        if (ReferenceEquals(a, b)) return true;
        if (a.Count != b.Count) return false;

        if (b is FrozenSet<string> fb)
        {
            foreach (var x in a) if (!fb.Contains(x)) return false;
            return true;
        }

        var set = new HashSet<string>(b, StringComparer.OrdinalIgnoreCase);
        foreach (var x in a) if (!set.Contains(x)) return false;
        return true;
    }
}

public static class GraphNodeTool
{
    public static Option Validate(this GraphNode subject) => GraphNode.Validator.Validate(subject).ToOptionStatus();
}
