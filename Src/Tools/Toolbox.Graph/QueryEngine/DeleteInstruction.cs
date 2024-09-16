using Toolbox.Types;

namespace Toolbox.Graph;

internal static class DeleteInstruction
{
    public static async Task<Option> Process(GiDelete giDelete, QueryExecutionContext pContext)
    {
        pContext.GraphContext.Context.Location().LogInformation("Process giDelete={giDelete}", giDelete);

        var selectResultOption = await SelectInstruction.Select(giDelete.Instructions, pContext);
        if (selectResultOption.IsError()) return selectResultOption;

        var lastQueryResult = pContext.GetLastQueryResult();
        if (lastQueryResult == null)
        {
            pContext.GraphContext.Context.LogError("No query result found for delete");
            return (StatusCode.BadRequest, "No query result found for delete");
        }

        var deleteData = await DeleteData(lastQueryResult.Nodes, pContext.GraphContext);
        if (deleteData.IsError()) return deleteData;

        var removeEdges = RemoveEdges(lastQueryResult.Edges, pContext);
        if (removeEdges.IsError()) return removeEdges;

        var removeNodes = RemoveNodes(lastQueryResult.Nodes, pContext);
        if (removeNodes.IsError()) return removeNodes;

        pContext.GraphContext.Context.LogInformation("Completed processing of giSelect={giSelect}", giDelete);
        return StatusCode.OK;
    }

    private static Option RemoveEdges(IReadOnlyList<GraphEdge> edges, QueryExecutionContext pContext)
    {
        foreach (var edge in edges)
        {
            var success = pContext.GraphContext.Map.Edges.Remove(edge.GetPrimaryKey(), pContext.GraphContext);
            if (success.IsError())
            {
                pContext.GraphContext.Context.LogWarning("Cannot remove edge key={edge.Key}", edge);
                continue;
            }

            pContext.GraphContext.Context.LogInformation("Removed edge key={edge.Key}", edge);
        }

        return StatusCode.OK;
    }

    private static Option RemoveNodes(IReadOnlyList<GraphNode> nodes, QueryExecutionContext pContext)
    {
        foreach (var node in nodes)
        {
            var result = pContext.GraphContext.Map.Nodes.Remove(node.Key, pContext.GraphContext);
            if (result.IsError())
            {
                pContext.GraphContext.Context.LogWarning("Cannot remove node key={node.Key}", node.Key);
                continue;
            }

            pContext.GraphContext.Context.LogInformation("Removed node key={node.Key}", node.Key);
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
