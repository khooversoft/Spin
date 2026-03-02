using Microsoft.Extensions.Logging;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public partial class GraphQueryExecute
{
    private Option ProcessEdge(GiEdge giEdge, GraphTrxContext pContext)
    {
        _logger.LogDebug("Process giEdge={giEdge}", giEdge);

        var result = giEdge.ChangeType switch
        {
            GiChangeType.Add => Add(giEdge, pContext),
            GiChangeType.Set => Set(giEdge, pContext),
            GiChangeType.Delete => Delete(giEdge, pContext),
            _ => throw new InvalidOperationException("Invalid change type"),
        };

        pContext.AddQueryResult(result);

        _logger.LogStatus(result, "Completed processing of giEdge={giEdge}", [giEdge]);
        return result;
    }

    private Option Add(GiEdge giEdge, GraphTrxContext pContext)
    {
        giEdge.NotNull();
        pContext.NotNull();

        _logger.LogDebug("Adding giEdge={giEdge}", giEdge);

        var graphEdge = new GraphEdge(giEdge.From, giEdge.To, giEdge.Type, giEdge.Tags.RemoveDeleteCommands(), DateTime.UtcNow);

        var graphResult = _graphMapStore.GetMap().Edges.Add(graphEdge, pContext);
        if (graphResult.IsError()) return graphResult;

        _logger.LogDebug("Added giEdge={giEdge}", giEdge);
        return StatusCode.OK;
    }

    private Option Delete(GiEdge giEdge, GraphTrxContext pContext)
    {
        giEdge.NotNull();
        pContext.NotNull();

        _logger.LogDebug("Deleting giEdge={giEdge}", giEdge);

        var graphResult = _graphMapStore.GetMap().Edges.Remove(giEdge.GetPrimaryKey(), pContext);
        if (!giEdge.IfExist && graphResult.IsError()) return graphResult;

        _logger.LogDebug("Deleting giEdge={giEdge}", giEdge);
        return StatusCode.OK;
    }

    private Option Set(GiEdge giEdge, GraphTrxContext pContext)
    {
        _logger.LogDebug("Updating giEdge={giEdge}", giEdge);

        IReadOnlyDictionary<string, string?> tags = giEdge.Tags;
        if (_graphMapStore.GetMap().Edges.TryGetValue(giEdge.GetPrimaryKey(), out var readGraphEdge))
        {
            tags = TagsTool.ProcessTags(readGraphEdge.NotNull().Tags, giEdge.Tags);
        }

        var graphEdge = new GraphEdge(giEdge.From, giEdge.To, giEdge.Type, tags, DateTime.UtcNow);

        var updateOption = _graphMapStore.GetMap().Edges.Set(graphEdge, pContext);
        if (updateOption.IsError()) return _logger.LogStatus(updateOption, "Failed to update node key={giEdge}", [giEdge]);

        return StatusCode.OK;
    }
}
