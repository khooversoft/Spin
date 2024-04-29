using System.Collections.Immutable;
using System.Diagnostics;
using System.Text.Json.Serialization;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;


[DebuggerDisplay("Key={Key}, Tags={Tags.ToString()}, Links={LinksString}")]
public sealed record GraphNode : IGraphCommon
{
    public GraphNode() { }

    public GraphNode(string key, string? tags = null)
    {
        Key = key.NotNull();
        Tags = TagsTool.Parse(tags).ThrowOnError().Return().ToImmutableDictionary(StringComparer.OrdinalIgnoreCase);
    }

    public GraphNode(string key, IEnumerable<KeyValuePair<string, string?>> tags)
    {
        Key = key.NotNull();
        Tags = tags.NotNull().ToImmutableDictionary(StringComparer.OrdinalIgnoreCase);
    }

    public GraphNode(string key, IEnumerable<KeyValuePair<string, string?>> tags, ImmutableHashSet<string> links)
    {
        Key = key.NotNull();
        Tags = tags.NotNull().ToImmutableDictionary(StringComparer.OrdinalIgnoreCase);
        Links = links.NotNull();
    }

    [JsonConstructor]
    public GraphNode(string key, ImmutableDictionary<string, string?> tags, DateTime createdDate, ImmutableHashSet<string> links)
    {
        Key = key.NotNull();
        Tags = tags?.ToImmutableDictionary(StringComparer.OrdinalIgnoreCase) ?? ImmutableDictionary<string, string?>.Empty;
        CreatedDate = createdDate;
        Links = links?.ToImmutableHashSet(StringComparer.Ordinal) ?? ImmutableHashSet<string>.Empty;
    }

    public string Key { get; } = default!;
    public ImmutableDictionary<string, string?> Tags { get; private init; } = ImmutableDictionary<string, string?>.Empty;
    public DateTime CreatedDate { get; } = DateTime.UtcNow;
    public ImmutableHashSet<string> Links { get; private init; } = [];
    public string LinksString => Links.OrderBy(x => x).Join(',');

    public GraphNode Copy() => new GraphNode(Key, Tags, CreatedDate, Links);

    public GraphNode With(GraphNode node) => this with
    {
        Tags = TagsTool.ProcessTags(Tags, node.Tags),
        Links = GraphTool.ProcessLinks(Links, node.Links),
    };

    public GraphNode With(IEnumerable<KeyValuePair<string, string?>> tagCommands, IEnumerable<string> linkCommands) => this with
    {
        Tags = TagsTool.ProcessTags(Tags, tagCommands),
        Links = GraphTool.ProcessLinks(Links, linkCommands)
    };

    public bool Equals(GraphNode? obj)
    {
        bool result = obj is GraphNode subject &&
            Key == subject.Key &&
            Tags.DeepEquals(subject.Tags) &&
            CreatedDate == subject.CreatedDate &&
            Links.Count == subject.Links.Count &&
            Links.SequenceEqual(subject.Links);

        return result;
    }

    public override int GetHashCode() => HashCode.Combine(Key, Tags, CreatedDate);

    public static IValidator<GraphNode> Validator { get; } = new Validator<GraphNode>()
        .RuleFor(x => x.Key).NotNull()
        .RuleFor(x => x.Tags).NotNull()
        .RuleFor(x => x.Links).NotNull()
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
