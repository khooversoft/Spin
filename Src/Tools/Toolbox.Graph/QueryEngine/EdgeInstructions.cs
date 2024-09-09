//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace Toolbox.Graph.QueryEngine;

//internal class EdgeInstructions
//{
//    public static async Task<Option> Process(GiEdge giEdge, QueryExecutionContext pContext)
//    {
//        pContext.GraphContext.Context.Location().LogInformation("Process giEdge={giEdge}", giEdge);

//        var result = giEdge.ChangeType switch
//        {
//            GiChangeType.Add => await Add(giEdge, pContext),
//            GiChangeType.Update => await Update(giEdge, pContext),
//            GiChangeType.Upsert => await Upsert(giEdge, pContext),
//            _ => throw new InvalidOperationException("Invalid change type"),
//        };

//        pContext.AddQueryResult(result);

//        result.LogStatus(pContext.GraphContext.Context, $"Completed processing of giEdge={giEdge}");
//        return result;
//    }

//    private static async Task<Option> Add(GiEdge giEdge, QueryExecutionContext pContext)
//    {
//        giEdge.NotNull();
//        pContext.NotNull();

//        pContext.GraphContext.Context.LogInformation("Adding giEdge={giEdge}", giEdge);

//        var graphEdge = new GraphEdge(giEdge.From, giEdge.To, giEdge.Type, giEdge.Tags, DateTime.UtcNow);

//        var graphResult = pContext.GraphContext.Map.Edges.Add(graphEdge, pContext.GraphContext);
//        if (graphResult.IsError()) return graphResult;

//        pContext.GraphContext.Context.LogInformation("Added giEdge={giEdge}", giEdge);
//        return StatusCode.OK;
//    }

//    private static async Task<Option> Update(GiEdge giEdge, QueryExecutionContext pContext)
//    {
//        if (!pContext.GraphContext.Map.Nodes.TryGetValue(giEdge.Key, out var currentGraphNode)) return (StatusCode.NotFound, $"Node key={giEdge.Key} not found");

//        pContext.GraphContext.Context.LogInformation("Updating giEdge={giEdge}", giEdge);

//        var dataMapOption = await NodeDataTool.MergeData(giEdge, currentGraphNode, pContext);
//        if (dataMapOption.IsError()) return dataMapOption.ToOptionStatus();
//        var dataMap = dataMapOption.Return().ToDictionary(x => x.Name, x => x);

//        var graphNode = new GraphNode(giEdge.Key, giEdge.Tags, DateTime.UtcNow, dataMap);

//        var updateOption = pContext.GraphContext.Map.Nodes.Update(graphNode, graphNode.Tags, dataMap, pContext.GraphContext);
//        if (updateOption.IsError()) return updateOption.LogStatus(pContext.GraphContext.Context, $"Failed to update node key={giEdge.Key}");

//        return StatusCode.OK;
//    }

//    private static async Task<Option> Upsert(GiEdge giEdge, QueryExecutionContext pContext)
//    {
//        if (!pContext.GraphContext.Map.Nodes.TryGetValue(giEdge.Key, out var currentGraphNode))
//        {
//            pContext.GraphContext.Context.LogInformation("Node key={key} not found, adding node for upsert", giEdge.Key);
//            return await Add(giEdge, pContext);
//        }

//        pContext.GraphContext.Context.LogInformation("Updating giEdge={giEdge}", giEdge);

//        var dataMapOption = await NodeDataTool.MergeData(giEdge, currentGraphNode, pContext);
//        if (dataMapOption.IsError()) return dataMapOption.ToOptionStatus();
//        var dataMap = dataMapOption.Return().ToDictionary(x => x.Name, x => x);

//        var graphNode = new GraphNode(giEdge.Key, giEdge.Tags, DateTime.UtcNow, dataMap);

//        var updateOption = pContext.GraphContext.Map.Nodes.Update(graphNode, graphNode.Tags, dataMap, pContext.GraphContext);
//        if (updateOption.IsError()) return updateOption.LogStatus(pContext.GraphContext.Context, $"Failed to update node key={giEdge.Key}");

//        return StatusCode.OK;
//    }

//}
