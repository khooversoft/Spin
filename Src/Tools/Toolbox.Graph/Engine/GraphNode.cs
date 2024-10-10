using System.Collections.Frozen;
using System.Diagnostics;
using System.Text.Json.Serialization;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;


[DebuggerDisplay("Key={Key}, Tags={TagsString}, DataMap={DataMapString}")]
public sealed record GraphNode : IGraphCommon
{
    public GraphNode(string key, string? tags = null, string? indexes = null)
    {
        Key = key.NotNull();
        Tags = TagsTool.Parse(tags).ThrowOnError().Return().ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
        Indexes = indexes?.Split(',')?.ToFrozenSet(StringComparer.OrdinalIgnoreCase) ?? FrozenSet<string>.Empty;
    }

    [JsonConstructor]
    public GraphNode(
        string key,
        IReadOnlyDictionary<string, string?> tags,
        DateTime createdDate,
        IReadOnlyDictionary<string, GraphLink> dataMap,
        IReadOnlyCollection<string> indexes
        )
    {
        Key = key.NotNull();
        Tags = tags?.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase) ?? FrozenDictionary<string, string?>.Empty;
        CreatedDate = createdDate;
        DataMap = dataMap?.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase) ?? FrozenDictionary<string, GraphLink>.Empty;
        Indexes = indexes?.ToFrozenSet(StringComparer.OrdinalIgnoreCase) ?? FrozenSet<string>.Empty;
    }

    public string Key { get; }
    public IReadOnlyDictionary<string, string?> Tags { get; }
    public DateTime CreatedDate { get; } = DateTime.UtcNow;
    public IReadOnlyDictionary<string, GraphLink> DataMap { get; } = FrozenDictionary<string, GraphLink>.Empty;
    public IReadOnlyCollection<string> Indexes { get; } = FrozenSet<string>.Empty;
    [JsonIgnore] public string TagsString => Tags.ToTagsString();
    [JsonIgnore] public string DataMapString => DataMap.ToDataMapString();

    public bool Equals(GraphNode? obj)
    {
        bool result = obj is GraphNode subject &&
            Key == subject.Key &&
            Tags.DeepEquals(subject.Tags) &&
            CreatedDate == subject.CreatedDate &&
            DataMap.DeepEquals(subject.DataMap) &&
            Enumerable.SequenceEqual(Indexes.OrderBy(x => x), subject.Indexes.OrderBy(x => x));

        return result;
    }

    public override int GetHashCode() => HashCode.Combine(Key, Tags, CreatedDate);

    public static IValidator<GraphNode> Validator { get; } = new Validator<GraphNode>()
        .RuleFor(x => x.Key).NotEmpty()
        .RuleFor(x => x.Tags).NotNull()
        .RuleFor(x => x.CreatedDate).ValidDateTime()
        .RuleFor(x => x.DataMap).NotNull().Must(x => x.All(y => y.Key.IsNotEmpty() && y.Value.Validate().IsOk()), _ => "Graph link does not validate")
        .RuleFor(x => x.Indexes).NotNull().Must(x => x.All(x => x.IsNotEmpty()), _ => "Indexed data key must not be empty")
        .Build();
}

public static class GraphNodeTool
{
    public static Option Validate(this GraphNode subject) => GraphNode.Validator.Validate(subject).ToOptionStatus();

    public static bool Validate(this GraphNode subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }
}
