using System.Diagnostics;
using System.Text.Json.Serialization;
using Toolbox.Tools;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace Toolbox.Data;


[DebuggerDisplay("Key={Key}, Tags={Tags}")]
public record GraphNode : IGraphCommon
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

    public GraphNode Copy() => new GraphNode(Key, Tags.Copy(), CreatedDate);

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