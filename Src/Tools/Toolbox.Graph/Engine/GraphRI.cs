using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

internal class GraphRI
{
    public GraphNodeIndex GraphNodeIndex { get; set; } = null!;
    public GraphEdgeIndex GraphEdgeIndex { get; set; } = null!;

    /// <summary>
    /// Remove all edges that are associated with the node.
    /// If the node is unique, remove the node if no edges are associated with it.
    /// </summary>
    /// <param name="graphNode"></param>
    /// <param name="graphContext"></param>
    /// <returns></returns>
    public bool RemovedNodeFromEdges(GraphNode graphNode, IGraphTrxContext? graphContext)
    {
        IReadOnlyList<string> removedSet = GraphEdgeIndex.Remove(graphNode.NotNull().Key, graphContext);

        RemoveUniqueIndexNodes(removedSet, graphContext);

        return removedSet.Any();
    }

    public Func<string, bool> IsNodeExist => x => GraphNodeIndex.ContainsKey(x);

    public void RemoveUniqueIndexNodes(IReadOnlyList<string> removedNodeKeySet, IGraphTrxContext? graphContext)
    {
        foreach (var nodeKey in removedNodeKeySet)
        {
            if (!GraphNodeIndex.TryGetValue(nodeKey, out GraphNode? node)) continue;
            if (!node.Tags.Has(GraphConstants.UniqueIndexTag)) continue;

            var query = new GraphEdgeSearch { NodeKey = nodeKey };
            IReadOnlyList<GraphEdge> keys = GraphEdgeIndex.Query(query);
            if (keys.Count != 0) continue;

            GraphNodeIndex.Remove(nodeKey, graphContext);
        }
    }
}
