using System.Diagnostics.CodeAnalysis;

namespace Toolbox.Data;

public sealed class GraphEdgeComparer<TKey, TEdge> : IEqualityComparer<TEdge>
    where TKey : notnull
    where TEdge : IGraphEdge<TKey>
{
    public bool Equals(TEdge? x, TEdge? y)
    {
        if (x == null || y == null) return true;

        return x.FromNodeKey.Equals(y.FromNodeKey) &&
            x.ToNodeKey.Equals(y.ToNodeKey) &&
            x.Tags.Equals(y.Tags);
    }

    public int GetHashCode([DisallowNull] TEdge obj) => HashCode.Combine(obj.FromNodeKey, obj.ToNodeKey);
}
