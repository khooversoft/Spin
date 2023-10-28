using System.Diagnostics.CodeAnalysis;
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

public interface IGraphEdge<TKey> : IGraphCommon
{
    Guid Key { get; }
    TKey FromKey { get; }
    TKey ToKey { get; }
    string EdgeType { get; }
    Tags Tags { get; }
    DateTime CreatedDate { get; }
}

public sealed record GraphEdge<TKey> : IGraphEdge<TKey> where TKey : notnull
{
    public GraphEdge() { }

    public GraphEdge(TKey fromNodeKey, TKey toNodeKey, string? edgeType = null, string? tags = null, DateTime? createdDate = null)
    {
        FromKey = fromNodeKey.NotNull();
        ToKey = toNodeKey.NotNull();
        EdgeType = edgeType ?? "default";
        Tags = new Tags().Set(tags);
        CreatedDate = createdDate ?? DateTime.UtcNow;

        this.Validate().ThrowOnError("Edge is invalid");
    }

    [JsonConstructor]
    public GraphEdge(Guid key, TKey fromKey, TKey toKey, string edgeType, Tags tags, DateTime createdDate)
    {
        Key = key;
        FromKey = fromKey.NotNull();
        ToKey = toKey.NotNull();
        EdgeType = edgeType.NotEmpty();
        Tags = tags;
        CreatedDate = createdDate;

        this.Validate().ThrowOnError("Edge is invalid");
    }

    public Guid Key { get; } = Guid.NewGuid();
    public TKey FromKey { get; init; } = default!;
    public TKey ToKey { get; init; } = default!;
    public string EdgeType { get; init; } = "default";
    public Tags Tags { get; init; } = new Tags();
    public DateTime CreatedDate { get; init; } = DateTime.UtcNow;

    public bool Equals(GraphEdge<TKey>? obj) => obj is GraphEdge<TKey> document &&
        Key.Equals(document.Key) &&
        GraphEdgeTool.IsKeysEqual(FromKey, document.FromKey) &&
        GraphEdgeTool.IsKeysEqual(ToKey, document.ToKey) &&
        EdgeType.Equals(document.EdgeType, StringComparison.OrdinalIgnoreCase) &&
        Tags.Equals(document.Tags) &&
        CreatedDate == document.CreatedDate;

    public override int GetHashCode() => HashCode.Combine(Key, FromKey, ToKey, EdgeType, CreatedDate);

    public static IValidator<IGraphEdge<TKey>> Validator { get; } = new Validator<IGraphEdge<TKey>>()
        .RuleFor(x => x.FromKey).NotNull()
        .RuleFor(x => x.ToKey).NotNull()
        .RuleFor(x => x.EdgeType).NotEmpty()
        .RuleForObject(x => x).Must(x => !GraphEdgeTool.IsKeysEqual(x.FromKey, x.ToKey), _ => "From and to keys cannot be the same")
        .RuleFor(x => x.Tags).NotNull()
        .RuleFor(x => x.CreatedDate).ValidDateTime()
        .Build();
}


public static class GraphEdgeTool
{
    public static Option Validate<TKey>(this IGraphEdge<TKey> subject) where TKey : notnull => GraphEdge<TKey>.Validator.Validate(subject).ToOptionStatus();

    public static bool Validate<TKey>(this IGraphEdge<TKey> subject, out Option result) where TKey : notnull
    {
        result = subject.Validate();
        return result.IsOk();
    }

    public static bool IsKeysEqual<TKey>(TKey key1, TKey key2) => (key1, key2) switch
    {
        ("*", _) => true,
        (_, "*") => true,
        (string from, string to) => StringComparer.OrdinalIgnoreCase.Equals(from, to),
        (var from, var to) => ComparerTool.ComparerFor<TKey>().Equals(from, to),
    };
}
