using System.Diagnostics.CodeAnalysis;
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
    EdgeDirection Direction { get; }
    Tags Tags { get; }
}

public record GraphEdge<TKey> : IGraphEdge<TKey>
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
    public EdgeDirection Direction { get; init; } = EdgeDirection.Both;
    public Tags Tags { get; init; } = new Tags();
}


public static class GraghEdgeExtensions
{
    public static void Verify<TKey>(this IGraphEdge<TKey> subject)
    {
        subject.NotNull();
        var option = subject.IsValid();
        if (option.IsError()) throw new ArgumentException(option.Error);
    }

    public static Option IsValid<TKey>(this IGraphEdge<TKey> subject)
    {
        subject.NotNull();
        if (!subject.Direction.IsEnumValid()) return (StatusCode.BadRequest, "Invalid direction");

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
