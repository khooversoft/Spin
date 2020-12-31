using System;

namespace Toolbox.Graph
{
    public interface IGraphNode<TKey> : IGraphCommon
    {
        TKey Key { get; }
    }
}
