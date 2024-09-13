using Toolbox.Extensions;
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
    public void RemovedNodeFromEdges(GraphNode graphNode, IGraphTrxContext? graphContext) =>
        GraphEdgeIndex.LookupByNodeKey(graphNode.Key).ForEach(x => GraphEdgeIndex.Remove(x, graphContext));

    public Func<string, bool> IsNodeExist => x => GraphNodeIndex.ContainsKey(x);
}
