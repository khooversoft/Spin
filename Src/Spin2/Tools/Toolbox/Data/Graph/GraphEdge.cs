using System.Diagnostics;
using System.Text.Json.Serialization;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace Toolbox.Data;

public enum EdgeDirection
{
    Both,
    Directed,
}


[DebuggerDisplay("Key={Key}, FromKey={FromKey}, ToKey={ToKey}, EdgeType={EdgeType}, Tags={Tags}")]
public sealed record GraphEdge : IGraphCommon
{
    public GraphEdge() { }

    public GraphEdge(string fromNodeKey, string toNodeKey, string? edgeType = null, string? tags = null, DateTime? createdDate = null)
    {
        FromKey = fromNodeKey.NotNull();
        ToKey = toNodeKey.NotNull();
        EdgeType = edgeType ?? "default";
        Tags = new Tags().Set(tags);
        CreatedDate = createdDate ?? DateTime.UtcNow;

        this.Validate().ThrowOnError("Edge is invalid");
    }

    [JsonConstructor]
    public GraphEdge(Guid key, string fromKey, string toKey, string edgeType, Tags tags, DateTime createdDate)
    {
        Key = key;
        FromKey = fromKey.NotNull();
        ToKey = toKey.NotNull();
        EdgeType = edgeType.NotEmpty();
        Tags = tags;
        CreatedDate = createdDate;

        this.Validate().ThrowOnError("Edge is invalid");
    }

    public Guid Key { get; init; } = Guid.NewGuid();
    public string FromKey { get; init; } = null!;
    public string ToKey { get; init; } = null!;
    public string EdgeType { get; init; } = "default";
    public Tags Tags { get; init; } = new Tags();
    public DateTime CreatedDate { get; init; } = DateTime.UtcNow;

    public GraphEdge Copy() => new GraphEdge(Key, FromKey, ToKey, EdgeType, Tags.Copy(), CreatedDate);

    public bool Equals(GraphEdge? obj) => obj is GraphEdge document &&
        Key.Equals(document.Key) &&
        FromKey.EqualsIgnoreCase(document.FromKey) &&
        ToKey.EqualsIgnoreCase(document.ToKey) &&
        EdgeType.Equals(document.EdgeType, StringComparison.OrdinalIgnoreCase) &&
        Tags.Equals(document.Tags) &&
        CreatedDate == document.CreatedDate;

    public override int GetHashCode() => HashCode.Combine(Key, FromKey, ToKey, EdgeType, CreatedDate);

    public static IValidator<GraphEdge> Validator { get; } = new Validator<GraphEdge>()
        .RuleFor(x => x.FromKey).NotNull()
        .RuleFor(x => x.ToKey).NotNull()
        .RuleFor(x => x.EdgeType).NotEmpty()
        .RuleForObject(x => x).Must(x => !x.FromKey.EqualsIgnoreCase(x.ToKey), _ => "From and to keys cannot be the same")
        .RuleFor(x => x.Tags).NotNull()
        .RuleFor(x => x.CreatedDate).ValidDateTime()
        .Build();
}


public static class GraphEdgeTool
{
    public static Option Validate(this GraphEdge subject) => GraphEdge.Validator.Validate(subject).ToOptionStatus();

    public static bool Validate(this GraphEdge subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }
}
