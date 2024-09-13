using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Tools;

namespace Toolbox.Graph;

public static class GraphQuery
{
    public static IReadOnlyList<GraphEdge> GetEdgesForNode(this GraphMap map, string nodeKey, IGraphTrxContext? graphContext = null)
    {
        var nodeKeys = map.Edges.LookupByNodeKey(nodeKey);

        var result = nodeKeys
            .Select(x => map.Edges.TryGetValue(x, out var value) ? value : default)
            .OfType<GraphEdge>()
            .ToImmutableArray();

        return result;
    }
}
