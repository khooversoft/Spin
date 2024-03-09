using System.Diagnostics;
using System.Text.Json.Serialization;
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

    [JsonConstructor]
    public GraphNode(string key, Tags tags, DateTime createdDate)
    {
        Key = key.NotNull();
        Tags = tags;
        CreatedDate = createdDate;
    }

    public string Key { get; init; } = default!;
    public Tags Tags { get; init; } = new Tags();
    public DateTime CreatedDate { get; init; } = DateTime.UtcNow;

    public GraphNode Copy() => new GraphNode(Key, Tags.Clone(), CreatedDate);

    public bool Equals(GraphNode? obj)
    {
        bool result = obj is GraphNode subject &&
            Key == subject.Key &&
            Tags == subject.Tags &&
            CreatedDate == subject.CreatedDate;

        return result;
    }

    public override int GetHashCode() => HashCode.Combine(Key, Tags, CreatedDate);

    public static IValidator<GraphNode> Validator { get; } = new Validator<GraphNode>()
        .RuleFor(x => x.Key).NotNull()
        .RuleFor(x => x.Tags).NotNull()
        .RuleFor(x => x.CreatedDate).ValidDateTime()
        .Build();
}

public static class GraphNodeExtensions
{
    public static Option Validate(this GraphNode subject) => GraphNode.Validator.Validate(subject).ToOptionStatus();

    public static bool Validate(this GraphNode subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }
}