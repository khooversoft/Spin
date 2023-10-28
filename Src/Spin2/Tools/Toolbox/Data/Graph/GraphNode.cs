using System.Text.Json.Serialization;
using Toolbox.Tools;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace Toolbox.Data;

public interface IGraphNode<TKey> : IGraphCommon
{
    TKey Key { get; }
    Tags Tags { get; }
    DateTime CreatedDate { get; }
}

public record GraphNode<TKey> : IGraphNode<TKey>
{
    public GraphNode() { }

    public GraphNode(TKey key, string? tags = null)
    {
        Key = key.NotNull();
        Tags = new Tags().Set(tags);
    }

    [JsonConstructor]
    public GraphNode(TKey key, Tags tags, DateTime createdDate)
    {
        Key = key.NotNull();
        Tags = tags;
        CreatedDate = createdDate;
    }

    public TKey Key { get; init; } = default!;
    public Tags Tags { get; init; } = new Tags();
    public DateTime CreatedDate { get; init; } = DateTime.UtcNow;

    public static IValidator<IGraphNode<TKey>> Validator { get; } = new Validator<IGraphNode<TKey>>()
        .RuleFor(x => x.Key).NotNull()
        .RuleFor(x => x.Tags).NotNull()
        .RuleFor(x => x.CreatedDate).ValidDateTime()
        .Build();
}

public static class GraphNodeTool
{
    public static Option Validate<TKey>(this IGraphNode<TKey> subject) where TKey : notnull => GraphNode<TKey>.Validator.Validate(subject).ToOptionStatus();

    public static bool Validate<TKey>(this IGraphNode<TKey> subject, out Option result) where TKey : notnull
    {
        result = subject.Validate();
        return result.IsOk();
    }
}