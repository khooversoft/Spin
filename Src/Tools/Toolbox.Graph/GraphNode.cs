using System.Collections.Immutable;
using System.Diagnostics;
using System.Text.Json.Serialization;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;


[DebuggerDisplay("Key={Key}, Tags={Tags.ToString()}")]
public sealed record GraphNode : IGraphCommon
{
    public GraphNode() { }

    public GraphNode(string key, string? tags = null)
    {
        Key = key.NotNull();
        Tags = new Tags().Set(tags);
    }

    public GraphNode(string key, Tags tags)
    {
        Key = key.NotNull();
        Tags = tags.NotNull();
    }

    public GraphNode(string key, Tags tags, IEnumerable<string> links)
    {
        Key = key.NotNull();
        Tags = tags.NotNull();
        Links = ((string[])[.. links])
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x)
            .ToImmutableArray();
    }

    [JsonConstructor]
    public GraphNode(string key, Tags tags, DateTime createdDate, ImmutableArray<string> links)
    {
        Key = key.NotNull();
        Tags = tags.NotNull();
        CreatedDate = createdDate;
        Links = ((string[])[.. links])
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x)
            .ToImmutableArray();
    }

    public string Key { get; private set; } = default!;
    public Tags Tags { get; private set; } = new Tags();
    public DateTime CreatedDate { get; private set; } = DateTime.UtcNow;
    public ImmutableArray<string> Links { get; private set; } = [];

    public GraphNode Copy() => new GraphNode(Key, Tags.Clone(), CreatedDate, Links);

    public GraphNode WithMerged(GraphNode node) => this with
    {
        Tags = Tags.Clone().Set(node.Tags),
        Links = ((string[])[.. Links, .. node.Links])
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x)
            .ToImmutableArray(),
    };

    public GraphNode With(Tags tags, IEnumerable<string> linkCommands) => this with
    {
        Tags = Tags.Clone().Set(tags),
        Links = GraphNodeTool.ProcessLinks(Links, linkCommands)
    };

    public GraphNode WithLinks(IEnumerable<string> linkCommands) => this with
    {
        Links = GraphNodeTool.ProcessLinks(Links, linkCommands)
    };

    public GraphNode WithTags(string tags) => this with { Tags = Tags.Clone().Set(tags) };

    public bool Equals(GraphNode? obj)
    {
        bool result = obj is GraphNode subject &&
            Key == subject.Key &&
            Tags == subject.Tags &&
            CreatedDate == subject.CreatedDate &&
            Links.Length == subject.Links.Length &&
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

    public static ImmutableArray<string> ProcessLinks(ImmutableArray<string> links, IEnumerable<string> linkCommands)
    {
        var list = new HashSet<string>(links, StringComparer.OrdinalIgnoreCase);

        linkCommands.Where(x => x.Length > 0 && x[0] == '-').Select(x => x[1..]).ForEach(x => list.Remove(x));
        linkCommands.Where(x => x.Length > 0 && x[0] != '-').ForEach(x => list.Add(x));

        var result = list
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x)
            .ToImmutableArray();

        return result;
    }
}