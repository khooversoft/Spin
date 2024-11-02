using Toolbox.Extensions;
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
    //public void RemovedNodeFromEdges(GraphNode graphNode, IGraphTrxContext? graphContext) =>
    //    GraphEdgeIndex.LookupByNodeKey([ graphNode.Key ]).ForEach(x => GraphEdgeIndex.Remove(x, graphContext));

    public bool RemovedNodeFromEdges(GraphNode graphNode, IGraphTrxContext? graphContext)
    {
        var edges = GraphEdgeIndex.LookupByNodeKey([graphNode.Key]);
        edges.ForEach(x => GraphEdgeIndex.Remove(x.GetPrimaryKey(), graphContext));

        string[] nodesToRemove = [.. edges.Select(x => x.FromKey), .. edges.Select(x => x.ToKey)];
        RemoveUniqueIndexNodes(nodesToRemove, graphContext);

        return nodesToRemove.Any();
    }

    public Func<string, bool> IsNodeExist => x => GraphNodeIndex.ContainsKey(x);

    public void RemoveUniqueIndexNodes(IReadOnlyList<string> removedNodes, IGraphTrxContext? graphContext)
    {
        var indexNodesToRemove = GraphNodeIndex.LookupTag(GraphConstants.UniqueIndexEdgeType)
            .Join(removedNodes, x => x, x => x, (o, i) => i);

        foreach (var nodeKey in indexNodesToRemove)
        {
            if (!GraphNodeIndex.TryGetValue(nodeKey, out GraphNode? node)) continue;
            if (!node.Tags.Has(GraphConstants.UniqueIndexEdgeType)) continue;

            GraphNodeIndex.Remove(nodeKey, graphContext);
        }
    }
}
