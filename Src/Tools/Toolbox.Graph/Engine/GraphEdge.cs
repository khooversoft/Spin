using System.Collections.Immutable;
using System.Diagnostics;
using System.Text.Json.Serialization;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public enum EdgeDirection
{
    Both,
    Directed,
}


[DebuggerDisplay("Key={Key}, FromKey={FromKey}, ToKey={ToKey}, EdgeType={EdgeType}, Tags={Tags.ToString()}")]
public sealed record GraphEdge : IGraphCommon
{
    public GraphEdge(string fromKey, string toKey, string? edgeType = null, string? tags = null, DateTime? createdDate = null)
    {
        FromKey = fromKey.NotNull();
        ToKey = toKey.NotNull();
        EdgeType = edgeType ?? "default";
        Tags = TagsTool.Parse(tags).ThrowOnError().Return().ToImmutableDictionary(StringComparer.OrdinalIgnoreCase);
        CreatedDate = createdDate ?? DateTime.UtcNow;

        this.Validate().ThrowOnError("Edge is invalid");
    }

    [JsonConstructor]
    public GraphEdge(Guid key, string fromKey, string toKey, string edgeType, ImmutableDictionary<string, string?> tags, DateTime createdDate)
    {
        Key = key;
        FromKey = fromKey.NotNull();
        ToKey = toKey.NotNull();
        EdgeType = edgeType.NotEmpty();
        Tags = tags.NotNull().ToImmutableDictionary(StringComparer.OrdinalIgnoreCase);
        CreatedDate = createdDate;

        this.Validate().ThrowOnError("Edge is invalid");
    }

    public Guid Key { get; } = Guid.NewGuid();
    public string FromKey { get; }
    public string ToKey { get; }
    public string EdgeType { get; private init; } = "default";
    public ImmutableDictionary<string, string?> Tags { get; private init; } = ImmutableDictionary<string, string?>.Empty;
    public DateTime CreatedDate { get; } = DateTime.UtcNow;

    public GraphEdge With(GraphEdge edge) => this with
    {
        Tags = TagsTool.ProcessTags(Tags, edge.Tags),
    };

    public GraphEdge With(string? edgeType, IEnumerable<KeyValuePair<string, string?>> tagCommands) => this with
    {
        EdgeType = edgeType ?? EdgeType,
        Tags = TagsTool.ProcessTags(Tags, tagCommands),
    };

    public bool Equals(GraphEdge? obj) => obj is GraphEdge subject &&
        Key.Equals(subject.Key) &&
        FromKey.EqualsIgnoreCase(subject.FromKey) &&
        ToKey.EqualsIgnoreCase(subject.ToKey) &&
        EdgeType.Equals(subject.EdgeType, StringComparison.OrdinalIgnoreCase) &&
        Tags.DeepEquals(subject.Tags) &&
        CreatedDate == subject.CreatedDate;

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
