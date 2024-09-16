using System.Diagnostics.CodeAnalysis;
using Toolbox.Extensions;

namespace Toolbox.Graph;

public sealed class GraphEdgeComparer : IEqualityComparer<GraphEdge>
{
    public bool Equals(GraphEdge? x, GraphEdge? y)
    {
        if (ReferenceEquals(x, y)) return true;

        if (x is null || y is null) return false;

        return x.FromKey.EqualsIgnoreCase(y.FromKey) &&
            x.ToKey.EqualsIgnoreCase(y.ToKey) &&
            x.EdgeType.EqualsIgnoreCase(y.EdgeType);
    }

    public int GetHashCode([DisallowNull] GraphEdge obj) => HashCode.Combine(obj.FromKey, obj.ToKey, obj.EdgeType);
}



