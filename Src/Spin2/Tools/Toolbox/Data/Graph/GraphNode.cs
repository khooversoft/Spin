using System.Text.Json.Serialization;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Data;

public interface IGraphNode<TKey> : IGraphCommon
{
    TKey Key { get; }
    Tags Tags { get; }
}

public record GraphNode<TKey> : IGraphNode<TKey>
{
    public GraphNode(TKey key, string? tags = null)
    {
        Key = key.NotNull();
        Tags = new Tags().Set(tags);
    }

    [JsonConstructor]
    public GraphNode(TKey key, Tags tags)
    {
        Key = key.NotNull();
        Tags = tags;
    }

    public TKey Key { get; init; }
    public Tags Tags { get; init; } = new Tags();
}
