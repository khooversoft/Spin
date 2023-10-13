using System.Text.Json.Serialization;
using Toolbox.Extensions;
using Toolbox.Tools;
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
    TKey FromNodeKey { get; }
    TKey ToNodeKey { get; }
    Tags Tags { get; }
}

public sealed record GraphEdge<TKey> : IGraphEdge<TKey> where TKey : notnull
{
    public GraphEdge(TKey fromNodeKey, TKey toNodeKey, string? tags = null)
    {
        FromNodeKey = fromNodeKey.NotNull();
        ToNodeKey = toNodeKey.NotNull();

        Tags = new Tags().Set(tags);
        this.Verify();
    }

    [JsonConstructor]
    public GraphEdge(Guid key, TKey fromNodeKey, TKey toNodeKey, Tags tags)
    {
        Key = key;
        FromNodeKey = fromNodeKey.NotNull();
        ToNodeKey = toNodeKey.NotNull();
        Tags = tags;
    }

    public Guid Key { get; } = Guid.NewGuid();
    public TKey FromNodeKey { get; init; }
    public TKey ToNodeKey { get; init; }
    public Tags Tags { get; init; } = new Tags();

    public bool Equals(GraphEdge<TKey>? obj) => obj is GraphEdge<TKey> document &&
        Key.Equals(document.Key) &&
        FromNodeKey.Equals(document.FromNodeKey) &&
        ToNodeKey.Equals(document.ToNodeKey) &&
        Tags.Equals(document.Tags);

    public override int GetHashCode() => HashCode.Combine(Key, FromNodeKey, ToNodeKey, Tags);
}


public static class GraghEdgeExtensions
{
    public static void Verify<TKey>(this IGraphEdge<TKey> subject)
    {
        subject.NotNull();
        var option = subject.IsValid().ThrowOnError("Edge is invalid");
    }

    public static Option IsValid<TKey>(this IGraphEdge<TKey> subject)
    {
        subject.NotNull();

        var keyCompare = ComparerTool.ComparerFor<TKey>(null);
        if (keyCompare.Equals(subject.FromNodeKey, subject.ToNodeKey)) return (StatusCode.BadRequest, "From and to keys cannot be the same");

        return StatusCode.OK;
    }

    public static bool IsValid<TKey>(this IGraphEdge<TKey> subject, out Option result)
    {
        result = subject.IsValid();
        return result.IsOk();
    }
}
