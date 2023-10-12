using System.Text.Json.Serialization;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Data;

public interface IGraphNode<TKey> : IGraphCommon
{
    TKey Key { get; }
    IReadOnlyDictionary<string, string?> Tags { get; }
}

public record GraphNode<TKey> : IGraphNode<TKey>
{
    private static IReadOnlyDictionary<string, string?> _default() => new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

    public GraphNode(TKey key, string? tags = null)
    {
        Key = key.NotNull();
        Tags = tags != null ? new Tags().Set(tags) : _default();
    }

    [JsonConstructor]
    public GraphNode(TKey key, IReadOnlyDictionary<string, string?> tags)
    {
        Key = key.NotNull();
        Tags = tags.ToDictionary(x => x.Key, x => x.Value);
    }

    public TKey Key { get; init; }
    public IReadOnlyDictionary<string, string?> Tags { get; init; } = _default();
}
