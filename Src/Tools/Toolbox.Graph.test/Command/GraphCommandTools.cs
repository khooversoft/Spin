using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph.test.Command;

public static class GraphCommandTools
{
    public static IReadOnlyList<IGraphCommon> CompareMap(GraphMap currentMap, GraphMap newMap, bool ignoreDate = false)
    {
        currentMap.NotNull();
        newMap.NotNull();

        var nodeDelta2 = newMap.Nodes.Where(x =>
        {
            if (!currentMap.Nodes.TryGetValue(x.Key, out var node)) return true;
            if (ignoreDate) x = x with { CreatedDate = node.CreatedDate };
            return x != node;
        }).ToArray();

        var nodeDelta1 = currentMap.Nodes.Where(x =>
        {
            if (nodeDelta2.Any(y => y.Key == x.Key)) return false;

            if (!newMap.Nodes.TryGetValue(x.Key, out var node)) return true;
            if (ignoreDate) x = x with { CreatedDate = node.CreatedDate };
            return x != node;
        }).ToArray();

        var edgeDelta2 = newMap.Edges.Where(x =>
        {
            if (!currentMap.Edges.TryGetValue(x.GetPrimaryKey(), out var edge)) return true;
            if (ignoreDate) x = x with { CreatedDate = edge.CreatedDate };
            return x != edge;
        }).ToArray();

        var edgeDelta1 = currentMap.Edges.Where(x =>
        {
            if (edgeDelta2.Any(y => y.GetPrimaryKey() == x.GetPrimaryKey())) return false;

            if (!newMap.Edges.TryGetValue(x.GetPrimaryKey(), out var edge)) return true;
            if (ignoreDate) x = x with { CreatedDate = edge.CreatedDate };
            return x != edge;
        }).ToArray();

        return new Sequence<IGraphCommon>() + nodeDelta1 + edgeDelta1 + nodeDelta2 + edgeDelta2;
    }

}
