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

            string v => v.Split(',')
                .Select(x => x.Trim())
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
            var v => v.Distinct(StringComparer.OrdinalIgnoreCase).ToFrozenSet(StringComparer.OrdinalIgnoreCase),
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
        bool result = obj is GraphNode subject &&
            Key == subject.Key &&
            Tags.DeepEqualsComparer(subject.Tags) &&
            CreatedDate == subject.CreatedDate &&
            DataMap.DeepEquals(subject.DataMap) &&
            Enumerable.SequenceEqual(Indexes.OrderBy(x => x), subject.Indexes.OrderBy(x => x)) &&
            Enumerable.SequenceEqual(ForeignKeys.OrderBy(x => x), subject.ForeignKeys.OrderBy(x => x));

        return result;
    }

    public override int GetHashCode() => HashCode.Combine(Key, Tags, CreatedDate);

    public override string ToString() => $"Key={Key}, Tags={TagsString}, DataMap={DataMapString}, Indexes={IndexesString}, ForeignKeys={ForeignKeysString}";

    public static IValidator<GraphNode> Validator { get; } = new Validator<GraphNode>()
        .RuleFor(x => x.Key).NotEmpty()
        .RuleFor(x => x.Tags).NotNull()
        .RuleFor(x => x.CreatedDate).ValidDateTime()
        .RuleFor(x => x.DataMap).NotNull().Must(x => x.All(y => y.Key.IsNotEmpty() && y.Value.Validate().IsOk()), _ => "Graph link does not validate")
        .RuleFor(x => x.Indexes).NotNull().Must(x => x.All(x => x.IsNotEmpty()), _ => "Indexed data key must not be empty")
        .RuleFor(x => x.ForeignKeys).NotNull()
        .Build();
}

public static class GraphNodeTool
{
    public static Option Validate(this GraphNode subject) => GraphNode.Validator.Validate(subject).ToOptionStatus();
}
