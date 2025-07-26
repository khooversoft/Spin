using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

internal class EdgeInstruction
{
    public static Option Process(GiEdge giEdge, GraphTrxContext pContext)
    {
        pContext.Context.Location().LogDebug("Process giEdge={giEdge}", giEdge);

        var result = giEdge.ChangeType switch
        {
            GiChangeType.Add => Add(giEdge, pContext),
            GiChangeType.Set => Set(giEdge, pContext),
            GiChangeType.Delete => Delete(giEdge, pContext),
            _ => throw new InvalidOperationException("Invalid change type"),
        };

        pContext.AddQueryResult(result);

        result.LogStatus(pContext.Context, "Completed processing of giEdge={giEdge}", [giEdge]);
        return result;
    }

    private static Option Add(GiEdge giEdge, GraphTrxContext pContext)
    {
        giEdge.NotNull();
        pContext.NotNull();

        pContext.Context.LogDebug("Adding giEdge={giEdge}", giEdge);

        var graphEdge = new GraphEdge(giEdge.From, giEdge.To, giEdge.Type, giEdge.Tags.RemoveDeleteCommands(), DateTime.UtcNow);

        var graphResult = pContext.GetMap().Edges.Add(graphEdge, pContext);
        if (graphResult.IsError()) return graphResult;

        pContext.Context.LogDebug("Added giEdge={giEdge}", giEdge);
        return StatusCode.OK;
    }

    private static Option Delete(GiEdge giEdge, GraphTrxContext pContext)
    {
        giEdge.NotNull();
        pContext.NotNull();

        pContext.Context.LogDebug("Deleting giEdge={giEdge}", giEdge);

        var graphResult = pContext.GetMap().Edges.Remove(giEdge.GetPrimaryKey(), pContext);
        if (!giEdge.IfExist && graphResult.IsError()) return graphResult;

        pContext.Context.LogDebug("Deleting giEdge={giEdge}", giEdge);
        return StatusCode.OK;
    }

    private static Option Set(GiEdge giEdge, GraphTrxContext pContext)
    {
        pContext.Context.LogDebug("Updating giEdge={giEdge}", giEdge);

        IReadOnlyDictionary<string, string?> tags = giEdge.Tags;
        if (pContext.GetMap().Edges.TryGetValue(giEdge.GetPrimaryKey(), out var readGraphEdge))
        {
            tags = TagsTool.ProcessTags(readGraphEdge.NotNull().Tags, giEdge.Tags);
        }

        var graphEdge = new GraphEdge(giEdge.From, giEdge.To, giEdge.Type, tags, DateTime.UtcNow);

        var updateOption = pContext.GetMap().Edges.Set(graphEdge, pContext);
        if (updateOption.IsError()) return updateOption.LogStatus(pContext.Context, "Failed to update node key={giEdge}", [giEdge]);

        return StatusCode.OK;
    }
}
