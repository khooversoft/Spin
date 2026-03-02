using Microsoft.Extensions.Logging;
using Toolbox.Types;

namespace Toolbox.Graph;

public partial class GraphQueryExecute
{
    private async Task<Option> ProcessDelete(GiDelete giDelete, GraphTrxContext pContext)
    {
        _logger.LogDebug("Process giDelete={giDelete}", giDelete);

        var selectResultOption = await Select(giDelete.Instructions, pContext);
        if (selectResultOption.IsError()) return selectResultOption;

        var lastQueryResult = pContext.GetLastQueryResult();
        if (lastQueryResult == null)
        {
            _logger.LogError("No query result found for delete");
            return (StatusCode.BadRequest, "No query result found for delete");
        }

        var deleteData = await DeleteData(lastQueryResult.Nodes, pContext);
        if (deleteData.IsError()) return deleteData;

        var removeEdges = RemoveEdges(lastQueryResult.Edges, pContext);
        if (removeEdges.IsError()) return removeEdges;

        var removeNodes = RemoveNodes(lastQueryResult.Nodes, pContext);
        if (removeNodes.IsError()) return removeNodes;

        _logger.LogDebug("Completed processing of giSelect={giSelect}", giDelete);
        return StatusCode.OK;
    }

    private Option RemoveEdges(IReadOnlyList<GraphEdge> edges, GraphTrxContext pContext)
    {
        foreach (var edge in edges)
        {
            var success = _graphMapStore.GetMap().Edges.Remove(edge.GetPrimaryKey(), pContext);
            if (success.IsError())
            {
                _logger.LogWarning("Cannot remove edge key={edge.Key}", edge);
                continue;
            }

            _logger.LogDebug("Removed edge key={edge.Key}", edge);
        }

        return StatusCode.OK;
    }

    private Option RemoveNodes(IReadOnlyList<GraphNode> nodes, GraphTrxContext pContext)
    {
        foreach (var node in nodes)
        {
            var result = _graphMapStore.GetMap().Nodes.Remove(node.Key, pContext);
            if (result.IsError())
            {
                _logger.LogWarning("Cannot remove node key={node.Key}", node.Key);
                continue;
            }

            _logger.LogDebug("Removed node key={node.Key}", node.Key);
        }

        return StatusCode.OK;
    }
}
