using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

internal class GraphRI
{
    public GraphNodeIndex GraphNodeIndex { get; set; } = null!;
    public GraphEdgeIndex GraphEdgeIndex { get; set; } = null!;

    public bool RemovedNodeFromEdges(GraphNode graphNode, GraphContext? graphContext)
    {
        IReadOnlyList<string> removedSet = GraphEdgeIndex.Remove(graphNode.NotNull().Key, graphContext);

        foreach (var node in removedSet)
        {
            if (!GraphNodeIndex.TryGetValue(node, out GraphNode? referencedNode)) continue;
            if (!referencedNode.Tags.Has(GraphConstants.UniqueIndexTag)) continue;

            var query = new GraphEdgeSearch { NodeKey = node };
            IReadOnlyList<GraphEdge> keys = GraphEdgeIndex.Query(query);
            if (keys.Count != 0) continue;

            GraphNodeIndex.Remove(node, graphContext);
        }

        return removedSet.Any();
    }

    public Func<string, bool> IsNodeExist => x => GraphNodeIndex.ContainsKey(x);
}
