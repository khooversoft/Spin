using System.Diagnostics.CodeAnalysis;

namespace Toolbox.Data;

public sealed class GraphEdgeComparer<TKey, TEdge> : IEqualityComparer<TEdge>
    where TKey : notnull
    where TEdge : IGraphEdge<TKey>
{
    public bool Equals(TEdge? x, TEdge? y)
    {
        if (x == null || y == null) return true;

        return GraphEdgeTool.IsKeysEqual(x.FromKey, y.FromKey) &&
            GraphEdgeTool.IsKeysEqual(x.ToKey, y.ToKey) &&
            x.EdgeType.Equals(y.EdgeType, StringComparison.OrdinalIgnoreCase);
    }

    public int GetHashCode([DisallowNull] TEdge obj) => HashCode.Combine(obj.FromKey, obj.ToKey);
}
