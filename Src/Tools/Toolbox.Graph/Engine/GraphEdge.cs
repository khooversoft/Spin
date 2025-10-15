using System.Collections.Frozen;
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

[DebuggerDisplay("FromKey={FromKey}, ToKey={ToKey}, EdgeType={EdgeType}, Tags={TagsString}")]
public sealed record GraphEdge : IGraphCommon
{
    public GraphEdge(string fromKey, string toKey, string edgeType, string? tags = null, DateTime? createdDate = null)
    {
        FromKey = fromKey.NotEmpty();
        ToKey = toKey.NotEmpty();
        EdgeType = edgeType.NotEmpty();
        Tags = TagsTool.Parse(tags).ThrowOnError().Return().ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
        CreatedDate = createdDate ?? DateTime.UtcNow;

        this.Validate().ThrowOnError("Edge is invalid");
    }

    [JsonConstructor]
    public GraphEdge(string fromKey, string toKey, string edgeType, IReadOnlyDictionary<string, string?>? tags, DateTime createdDate)
    {
        FromKey = fromKey.NotEmpty();
        ToKey = toKey.NotEmpty();
        EdgeType = edgeType.NotEmpty();
        Tags = (tags ?? FrozenDictionary<string, string?>.Empty).ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
        CreatedDate = createdDate == default ? DateTime.UtcNow : createdDate;

        this.Validate().ThrowOnError("Edge is invalid");
    }

    [JsonIgnore] public string Key => $"{FromKey}:{ToKey}:{EdgeType}";
    public string FromKey { get; }
    public string ToKey { get; }
    public string EdgeType { get; }
    public IReadOnlyDictionary<string, string?> Tags { get; init; } = FrozenDictionary<string, string?>.Empty;
    public DateTime CreatedDate { get; init; } = DateTime.UtcNow;
    [JsonIgnore] public string TagsString => Tags.ToTagsString();

    public bool Equals(GraphEdge? obj) => obj is GraphEdge subject &&
        FromKey.EqualsIgnoreCase(subject.FromKey) &&
        ToKey.EqualsIgnoreCase(subject.ToKey) &&
        EdgeType.EqualsIgnoreCase(subject.EdgeType) &&
        Tags.DeepEquals(subject.Tags) &&
        CreatedDate == subject.CreatedDate;

    public override int GetHashCode()
    {
        var hc = new HashCode();
        hc.Add(FromKey, StringComparer.OrdinalIgnoreCase);
        hc.Add(ToKey, StringComparer.OrdinalIgnoreCase);
        hc.Add(EdgeType, StringComparer.OrdinalIgnoreCase);

        // Order-independent fold of Tags to keep consistency with Equals without sorting
        if (Tags.Count > 0)
        {
            int tagsHash = 0;
            foreach (var kvp in Tags)
            {
                int keyHash = StringComparer.OrdinalIgnoreCase.GetHashCode(kvp.Key);
                int valueHash = kvp.Value is null ? 0 : StringComparer.Ordinal.GetHashCode(kvp.Value);
                tagsHash ^= HashCode.Combine(keyHash, valueHash);
            }
            hc.Add(tagsHash);
        }

        hc.Add(CreatedDate);
        return hc.ToHashCode();
    }

    public override string ToString() => $"{{ FromKey={FromKey} -> ToKey={ToKey} ({EdgeType}) }}";

    public static IValidator<GraphEdge> Validator { get; } = new Validator<GraphEdge>()
        .RuleFor(x => x.FromKey).NotEmpty()
        .RuleFor(x => x.ToKey).NotEmpty()
        .RuleFor(x => x.EdgeType).NotEmpty()
        .RuleForObject(x => x).Must(x => !x.FromKey.EqualsIgnoreCase(x.ToKey), _ => "From and to keys cannot be the same")
        .RuleFor(x => x.Tags).NotNull()
        .RuleFor(x => x.CreatedDate).ValidDateTime()
        .Build();
}

public static class GraphEdgeTool
{
    public static Option Validate(this GraphEdge subject) => GraphEdge.Validator.Validate(subject).ToOptionStatus();
}
