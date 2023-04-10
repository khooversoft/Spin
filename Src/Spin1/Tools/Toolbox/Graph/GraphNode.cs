namespace Toolbox.Graph;

public record GraphNode<TKey> : IGraphNode<TKey>
{
    public GraphNode(TKey key) => Key = key;

    public TKey Key { get; init; }
}