﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

internal class EdgeInstruction
{
    public static Option Process(GiEdge giEdge, QueryExecutionContext pContext)
    {
        pContext.GraphContext.Context.Location().LogInformation("Process giEdge={giEdge}", giEdge);

        var result = giEdge.ChangeType switch
        {
            GiChangeType.Add => Add(giEdge, pContext),
            GiChangeType.Set => Set(giEdge, pContext),
            GiChangeType.Delete => Delete(giEdge, pContext),
            _ => throw new InvalidOperationException("Invalid change type"),
        };

        pContext.AddQueryResult(result);

        result.LogStatus(pContext.GraphContext.Context, $"Completed processing of giEdge={giEdge}");
        return result;
    }

    private static Option Add(GiEdge giEdge, QueryExecutionContext pContext)
    {
        giEdge.NotNull();
        pContext.NotNull();

        pContext.GraphContext.Context.LogInformation("Adding giEdge={giEdge}", giEdge);

        var graphEdge = new GraphEdge(giEdge.From, giEdge.To, giEdge.Type, giEdge.Tags, DateTime.UtcNow);

        var graphResult = pContext.GraphContext.Map.Edges.Add(graphEdge, pContext.GraphContext);
        if (graphResult.IsError()) return graphResult;

        pContext.GraphContext.Context.LogInformation("Added giEdge={giEdge}", giEdge);
        return StatusCode.OK;
    }

    private static Option Delete(GiEdge giEdge, QueryExecutionContext pContext)
    {
        giEdge.NotNull();
        pContext.NotNull();

        pContext.GraphContext.Context.LogInformation("Deleting giEdge={giEdge}", giEdge);

        var graphResult = pContext.GraphContext.Map.Edges.Remove(giEdge.GetPrimaryKey(), pContext.GraphContext);
        if (graphResult.IsError()) return graphResult;

        pContext.GraphContext.Context.LogInformation("Deleting giEdge={giEdge}", giEdge);
        return StatusCode.OK;
    }

    private static Option Set(GiEdge giEdge, QueryExecutionContext pContext)
    {
        pContext.GraphContext.Context.LogInformation("Updating giEdge={giEdge}", giEdge);

        IReadOnlyDictionary<string, string?> tags = giEdge.Tags;
        if(pContext.GraphContext.Map.Edges.TryGetValue(giEdge.GetPrimaryKey(), out var readGraphEdge))
        {
            tags = TagsTool.ProcessTags(readGraphEdge.NotNull().Tags, giEdge.Tags);
        }

        var graphEdge = new GraphEdge(giEdge.From, giEdge.To, giEdge.Type, tags, DateTime.UtcNow);

        var updateOption = pContext.GraphContext.Map.Edges.Set(graphEdge, pContext.GraphContext);
        if (updateOption.IsError()) return updateOption.LogStatus(pContext.GraphContext.Context, $"Failed to update node key={giEdge}");

        return StatusCode.OK;
    }
}
