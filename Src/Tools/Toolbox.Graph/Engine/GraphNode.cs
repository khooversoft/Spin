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
        ImmutableDictionary<string, string?> tags,
        DateTime createdDate,
        ImmutableDictionary<string, GraphDataLink> dataMap
        )
    {
        Key = key.NotNull();
        Tags = tags?.ToImmutableDictionary(StringComparer.OrdinalIgnoreCase) ?? ImmutableDictionary<string, string?>.Empty;
        CreatedDate = createdDate;
        DataMap = dataMap?.ToImmutableDictionary(StringComparer.OrdinalIgnoreCase) ?? ImmutableDictionary<string, GraphDataLink>.Empty;
    }

    public string Key { get; }
    public ImmutableDictionary<string, string?> Tags { get; private init; }
    public string TagsString => Tags.ToTagsString();
    public DateTime CreatedDate { get; } = DateTime.UtcNow;
    public ImmutableDictionary<string, GraphDataLink> DataMap { get; private set; } = ImmutableDictionary<string, GraphDataLink>.Empty;
    public string DataMapString => DataMap.ToDataMapString();

    public GraphNode With(GraphNode node) => this with
    {
        Tags = TagsTool.ProcessTags(Tags, node.Tags),
        DataMap = DataMap.Concat(node.DataMap).Distinct().ToImmutableDictionary(StringComparer.OrdinalIgnoreCase),
    };

    public GraphNode With(
        IEnumerable<KeyValuePair<string, string?>> tagCommands,
        IEnumerable<KeyValuePair<string, GraphDataLink>> dataMap
        ) => this with
        {
            Tags = TagsTool.ProcessTags(Tags, tagCommands),
            DataMap = dataMap.ToImmutableDictionary(StringComparer.OrdinalIgnoreCase),
        };

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
        .RuleFor(x => x.Key).NotNull()
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
