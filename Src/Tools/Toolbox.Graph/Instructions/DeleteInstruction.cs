using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

internal static class DeleteInstruction
{
    public static async Task<Option> Process(GiDelete giDelete, QueryExecutionContext pContext)
    {
        pContext.TrxContext.Context.Location().LogTrace("Process giDelete={giDelete}", giDelete);

        var selectResultOption = await SelectInstruction.Select(giDelete.Instructions, pContext);
        if (selectResultOption.IsError()) return selectResultOption;

        var lastQueryResult = pContext.GetLastQueryResult();
        if (lastQueryResult == null)
        {
            pContext.TrxContext.Context.LogError("No query result found for delete");
            return (StatusCode.BadRequest, "No query result found for delete");
        }

        var deleteData = await DeleteData(lastQueryResult.Nodes, pContext.TrxContext);
        if (deleteData.IsError()) return deleteData;

        var removeEdges = RemoveEdges(lastQueryResult.Edges, pContext);
        if (removeEdges.IsError()) return removeEdges;

        var removeNodes = RemoveNodes(lastQueryResult.Nodes, pContext);
        if (removeNodes.IsError()) return removeNodes;

        pContext.TrxContext.Context.LogTrace("Completed processing of giSelect={giSelect}", giDelete);
        return StatusCode.OK;
    }

    private static Option RemoveEdges(IReadOnlyList<GraphEdge> edges, QueryExecutionContext pContext)
    {
        foreach (var edge in edges)
        {
            var success = pContext.TrxContext.Map.Edges.Remove(edge.GetPrimaryKey(), pContext.TrxContext);
            if (success.IsError())
            {
                pContext.TrxContext.Context.LogWarning("Cannot remove edge key={edge.Key}", edge);
                continue;
            }

            pContext.TrxContext.Context.LogTrace("Removed edge key={edge.Key}", edge);
        }

        return StatusCode.OK;
    }

    private static Option RemoveNodes(IReadOnlyList<GraphNode> nodes, QueryExecutionContext pContext)
    {
        foreach (var node in nodes)
        {
            var result = pContext.TrxContext.Map.Nodes.Remove(node.Key, pContext.TrxContext);
            if (result.IsError())
            {
                pContext.TrxContext.Context.LogWarning("Cannot remove node key={node.Key}", node.Key);
                continue;
            }

            pContext.TrxContext.Context.LogTrace("Removed node key={node.Key}", node.Key);
        }

        return StatusCode.OK;
    }

    private static async Task<Option> DeleteData(IReadOnlyList<GraphNode> nodes, IGraphTrxContext graphContext)
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
