using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

internal static class DeleteInstruction
{
    public static async Task<Option> Process(GiDelete giDelete, GraphTrxContext pContext)
    {
        pContext.Context.Location().LogDebug("Process giDelete={giDelete}", giDelete);

        var selectResultOption = await SelectInstruction.Select(giDelete.Instructions, pContext);
        if (selectResultOption.IsError()) return selectResultOption;

        var lastQueryResult = pContext.GetLastQueryResult();
        if (lastQueryResult == null)
        {
            pContext.Context.LogError("No query result found for delete");
            return (StatusCode.BadRequest, "No query result found for delete");
        }

        var deleteData = await DeleteData(lastQueryResult.Nodes, pContext);
        if (deleteData.IsError()) return deleteData;

        var removeEdges = RemoveEdges(lastQueryResult.Edges, pContext);
        if (removeEdges.IsError()) return removeEdges;

        var removeNodes = RemoveNodes(lastQueryResult.Nodes, pContext);
        if (removeNodes.IsError()) return removeNodes;

        pContext.Context.LogDebug("Completed processing of giSelect={giSelect}", giDelete);
        return StatusCode.OK;
    }

    private static Option RemoveEdges(IReadOnlyList<GraphEdge> edges, GraphTrxContext pContext)
    {
        foreach (var edge in edges)
        {
            var success = pContext.GetMap().Edges.Remove(edge.GetPrimaryKey(), pContext);
            if (success.IsError())
            {
                pContext.Context.LogWarning("Cannot remove edge key={edge.Key}", edge);
                continue;
            }

            pContext.Context.LogDebug("Removed edge key={edge.Key}", edge);
        }

        return StatusCode.OK;
    }

    private static Option RemoveNodes(IReadOnlyList<GraphNode> nodes, GraphTrxContext pContext)
    {
        foreach (var node in nodes)
        {
            var result = pContext.GetMap().Nodes.Remove(node.Key, pContext);
            if (result.IsError())
            {
                pContext.Context.LogWarning("Cannot remove node key={node.Key}", node.Key);
                continue;
            }

            pContext.Context.LogDebug("Removed node key={node.Key}", node.Key);
        }

        return StatusCode.OK;
    }

    private static async Task<Option> DeleteData(IReadOnlyList<GraphNode> nodes, GraphTrxContext graphContext)
    {
        var linksToDelete = nodes.SelectMany(x => x.DataMap.Values.Select(y => y.FileId));
        foreach (var fileId in linksToDelete)
        {
            var result = await NodeDataTool.DeleteNodeData(fileId, graphContext);
            if (result.IsError()) return result;
        }

        return StatusCode.OK;
    }
}
