using System;

namespace Toolbox.Graph
{
    public record GraphEdge<TKey> : IGraphEdge<TKey>
    {
        public GraphEdge(TKey fromNodeKey, TKey toNodeKey)
        {
            FromNodeKey = fromNodeKey;
            ToNodeKey = toNodeKey;
        }

        public Guid Key { get; } = Guid.NewGuid();

        public TKey FromNodeKey { get; init; }

        public TKey ToNodeKey { get; init; }
    }
}