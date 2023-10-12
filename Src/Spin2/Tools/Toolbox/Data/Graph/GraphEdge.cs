using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Data;

public interface IGraphEdge<TKey> : IGraphCommon
{
    Guid Key { get; }
    TKey FromNodeKey { get; }
    TKey ToNodeKey { get; }
    IReadOnlyDictionary<string, string?> Tags { get; }
}

public record GraphEdge<TKey> : IGraphEdge<TKey>
{
    private static IReadOnlyDictionary<string, string?> _default() => new Dictionary<string, string?>();

    public GraphEdge(TKey fromNodeKey, TKey toNodeKey, string? tags = null)
    {
        FromNodeKey = fromNodeKey.NotNull();
        ToNodeKey = toNodeKey.NotNull();

        Tags = tags != null ? new Tags().Set(tags) : _default();
        this.Verify();
    }


    [JsonConstructor]
    public GraphEdge(Guid key, TKey fromNodeKey, TKey toNodeKey, IReadOnlyDictionary<string, string?> tags)
    {
        Key = key;
        FromNodeKey = fromNodeKey.NotNull();
        ToNodeKey = toNodeKey.NotNull();
        Tags = tags.ToDictionary(x => x.Key, x => x.Value);
    }

    public Guid Key { get; } = Guid.NewGuid();
    public TKey FromNodeKey { get; init; }
    public TKey ToNodeKey { get; init; }
    public IReadOnlyDictionary<string, string?> Tags { get; init; } = _default();
}


public static class GraghEdgeExtensions
{
    public static void Verify<TKey>(this IGraphEdge<TKey> subject)
    {
        subject.NotNull();
        var keyCompare = ComparerTool.ComparerFor<TKey>(null);
        keyCompare.Equals(subject.FromNodeKey, subject.ToNodeKey).Assert(x => !x, "From and to keys cannot be the same");
    }
}
