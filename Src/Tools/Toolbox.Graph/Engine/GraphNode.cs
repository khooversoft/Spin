using System.Collections.Immutable;
using System.Diagnostics;
using System.Text.Json.Serialization;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;


[DebuggerDisplay("Key={Key}, Tags={TagsString}, DataMap={DataMapString}")]
public sealed record GraphNode : IGraphCommon
{
    public GraphNode(string key, string? tags = null)
    {
        Key = key.NotNull();
        Tags = TagsTool.Parse(tags).ThrowOnError().Return().ToImmutableDictionary(StringComparer.OrdinalIgnoreCase);
    }

    [JsonConstructor]
    public GraphNode(
        string key,
        IReadOnlyDictionary<string, string?> tags,
        DateTime createdDate,
        IReadOnlyDictionary<string, GraphLink> dataMap
        )
    {
        Key = key.NotNull();
        Tags = tags?.ToImmutableDictionary(StringComparer.OrdinalIgnoreCase) ?? ImmutableDictionary<string, string?>.Empty;
        CreatedDate = createdDate;
        DataMap = dataMap?.ToImmutableDictionary(StringComparer.OrdinalIgnoreCase) ?? ImmutableDictionary<string, GraphLink>.Empty;
    }

    public string Key { get; }
    public IReadOnlyDictionary<string, string?> Tags { get; private init; }
    public string TagsString => Tags.ToTagsString();
    public DateTime CreatedDate { get; } = DateTime.UtcNow;
    public IReadOnlyDictionary<string, GraphLink> DataMap { get; private set; } = ImmutableDictionary<string, GraphLink>.Empty;
    [JsonIgnore] public string DataMapString => DataMap.ToDataMapString();

    public bool Equals(GraphNode? obj)
    {
        bool result = obj is GraphNode subject &&
            Key == subject.Key &&
            Tags.DeepEquals(subject.Tags) &&
            CreatedDate == subject.CreatedDate &&
            DataMap.DeepEquals(subject.DataMap);

        return result;
    }

    public override int GetHashCode() => HashCode.Combine(Key, Tags, CreatedDate);

    public static IValidator<GraphNode> Validator { get; } = new Validator<GraphNode>()
        .RuleFor(x => x.Key).NotEmpty()
        .RuleFor(x => x.Tags).NotNull()
        .RuleFor(x => x.CreatedDate).ValidDateTime()
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
