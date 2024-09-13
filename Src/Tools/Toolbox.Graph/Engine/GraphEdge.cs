﻿using System.Collections.Immutable;
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

    public GraphEdge(string fromKey, string toKey, string edgeType, IReadOnlyDictionary<string, string?> tags, DateTime? createdDate)
    {
        FromKey = fromKey.NotEmpty();
        ToKey = toKey.NotEmpty();
        EdgeType = edgeType.NotEmpty();
        Tags = tags?.ToImmutableDictionary(StringComparer.OrdinalIgnoreCase) ?? ImmutableDictionary<string, string?>.Empty;
        CreatedDate = createdDate ?? DateTime.UtcNow;

        this.Validate().ThrowOnError("Edge is invalid");
    }

    [JsonConstructor]
    public GraphEdge(string fromKey, string toKey, string edgeType, ImmutableDictionary<string, string?> tags, DateTime createdDate)
    {
        FromKey = fromKey.NotNull();
        ToKey = toKey.NotNull();
        EdgeType = edgeType.NotEmpty();
        Tags = tags.NotNull().ToImmutableDictionary(StringComparer.OrdinalIgnoreCase);
        CreatedDate = createdDate;

        this.Validate().ThrowOnError("Edge is invalid");
    }

    public string FromKey { get; }
    public string ToKey { get; }
    public string EdgeType { get; private init; }
    public ImmutableDictionary<string, string?> Tags { get; init; } = ImmutableDictionary<string, string?>.Empty;
    public DateTime CreatedDate { get; } = DateTime.UtcNow;

    public bool Equals(GraphEdge? obj) => obj is GraphEdge subject &&
        FromKey.EqualsIgnoreCase(subject.FromKey) &&
        ToKey.EqualsIgnoreCase(subject.ToKey) &&
        EdgeType.EqualsIgnoreCase(subject.EdgeType) &&
        Tags.DeepEquals(subject.Tags) &&
        CreatedDate == subject.CreatedDate;

    public override int GetHashCode() => HashCode.Combine(FromKey, ToKey, EdgeType, CreatedDate);

    public override string ToString() => $"{{ FromKey={FromKey} -> ToKey={ToKey} ({EdgeType}) }}";

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
