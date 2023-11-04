using System.Diagnostics.CodeAnalysis;
using Toolbox.Extensions;

namespace Toolbox.Data;

public sealed class GraphEdgeComparer : IEqualityComparer<GraphEdge>
{
    public bool Equals(GraphEdge? x, GraphEdge? y)
    {
        if (x == null || y == null) return true;

        return x.FromKey.EqualsIgnoreCase(y.FromKey) &&
            x.ToKey.EqualsIgnoreCase(y.ToKey) &&
            x.EdgeType.EqualsIgnoreCase(y.EdgeType);
    }

    public int GetHashCode([DisallowNull] GraphEdge obj) => HashCode.Combine(obj.FromKey, obj.ToKey);
}
