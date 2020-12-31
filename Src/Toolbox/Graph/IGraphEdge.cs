using System;

namespace Toolbox.Graph
{
    public interface IGraphEdge<TKey> : IGraphCommon
    {
        Guid Key { get; }

        TKey FromNodeKey { get; }

        TKey ToNodeKey { get; }
    }
}