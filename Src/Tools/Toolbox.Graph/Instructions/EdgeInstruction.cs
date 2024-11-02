using Toolbox.Logging;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

internal class EdgeInstruction
{
    public static Option Process(GiEdge giEdge, QueryExecutionContext pContext)
    {
        pContext.TrxContext.Context.Location().LogInformation("Process giEdge={giEdge}", giEdge);

        var result = giEdge.ChangeType switch
        {
            GiChangeType.Add => Add(giEdge, pContext),
            GiChangeType.Set => Set(giEdge, pContext),
            GiChangeType.Delete => Delete(giEdge, pContext),
            _ => throw new InvalidOperationException("Invalid change type"),
        };

        pContext.AddQueryResult(result);

        result.LogStatus(pContext.TrxContext.Context, "Completed processing of giEdge={giEdge}", [giEdge]);
        return result;
    }

    private static Option Add(GiEdge giEdge, QueryExecutionContext pContext)
    {
        giEdge.NotNull();
        pContext.NotNull();

        pContext.TrxContext.Context.LogInformation("Adding giEdge={giEdge}", giEdge);

        var graphEdge = new GraphEdge(giEdge.From, giEdge.To, giEdge.Type, giEdge.Tags.RemoveDeleteCommands(), DateTime.UtcNow);

        var graphResult = pContext.TrxContext.Map.Edges.Add(graphEdge, pContext.TrxContext);
        if (graphResult.IsError()) return graphResult;

        pContext.TrxContext.Context.LogInformation("Added giEdge={giEdge}", giEdge);
        return StatusCode.OK;
    }

    private static Option Delete(GiEdge giEdge, QueryExecutionContext pContext)
    {
        giEdge.NotNull();
        pContext.NotNull();

        pContext.TrxContext.Context.LogInformation("Deleting giEdge={giEdge}", giEdge);

        var graphResult = pContext.TrxContext.Map.Edges.Remove(giEdge.GetPrimaryKey(), pContext.TrxContext);
        if (!giEdge.IfExist && graphResult.IsError()) return graphResult;

        pContext.TrxContext.Context.LogInformation("Deleting giEdge={giEdge}", giEdge);
        return StatusCode.OK;
    }

    private static Option Set(GiEdge giEdge, QueryExecutionContext pContext)
    {
        pContext.TrxContext.Context.LogInformation("Updating giEdge={giEdge}", giEdge);

        IReadOnlyDictionary<string, string?> tags = giEdge.Tags;
        if (pContext.TrxContext.Map.Edges.TryGetValue(giEdge.GetPrimaryKey(), out var readGraphEdge))
        {
            tags = TagsTool.ProcessTags(readGraphEdge.NotNull().Tags, giEdge.Tags);
        }

        var graphEdge = new GraphEdge(giEdge.From, giEdge.To, giEdge.Type, tags, DateTime.UtcNow);

        var updateOption = pContext.TrxContext.Map.Edges.Set(graphEdge, pContext.TrxContext);
        if (updateOption.IsError()) return updateOption.LogStatus(pContext.TrxContext.Context, "Failed to update node key={giEdge}", [giEdge]);

        return StatusCode.OK;
    }
}
