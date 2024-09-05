using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

internal static class NodeInstruction
{
    public static async Task<Option> Process(GiNode giNode, QueryExecutionContext pContext)
    {
        pContext.GraphContext.Context.Location().LogInformation("Process giNode={giNode}", giNode);

        var result = giNode.ChangeType switch
        {
            GiChangeType.Add => await Add(giNode, pContext),
            GiChangeType.Update => await Update(giNode, pContext),
            GiChangeType.Upsert => await Upsert(giNode, pContext),
            _ => throw new InvalidOperationException("Invalid change type"),
        };

        result.LogStatus(pContext.GraphContext.Context, $"Completed processing of giNode={giNode}");
        return result;
    }

    private static async Task<Option> Add(GiNode giNode, QueryExecutionContext pContext)
    {
        giNode.NotNull();
        pContext.NotNull();

        pContext.GraphContext.Context.LogInformation("Adding giNode={giNode}", giNode);

        var dataMapOption = await NodeDataTool.AddData(giNode, pContext);
        if (dataMapOption.IsError()) return dataMapOption.ToOptionStatus();
        var dataMap = dataMapOption.Return().ToDictionary(x => x.Name, x => x);

        var graphNode = new GraphNode(giNode.Key, giNode.Tags, DateTime.UtcNow, dataMap);

        var graphResult = pContext.GraphContext.Map.Nodes.Add(graphNode, pContext.GraphContext);
        if (graphResult.IsError()) return graphResult;

        pContext.GraphContext.Context.LogInformation("Added giNode={giNode}", giNode);
        return StatusCode.OK;
    }

    private static async Task<Option> Update(GiNode giNode, QueryExecutionContext pContext)
    {
        if (!pContext.GraphContext.Map.Nodes.TryGetValue(giNode.Key, out var currentGraphNode)) return (StatusCode.NotFound, $"Node key={giNode.Key} not found");

        pContext.GraphContext.Context.LogInformation("Updating giNode={giNode}", giNode);

        var dataMapOption = await NodeDataTool.MergeData(giNode, currentGraphNode, pContext);
        if (dataMapOption.IsError()) return dataMapOption.ToOptionStatus();
        var dataMap = dataMapOption.Return().ToDictionary(x => x.Name, x => x);

        var graphNode = new GraphNode(giNode.Key, giNode.Tags, DateTime.UtcNow, dataMap);

        var updateOption = pContext.GraphContext.Map.Nodes.Update(graphNode, graphNode.Tags, dataMap, pContext.GraphContext);
        if (updateOption.IsError()) return updateOption.LogStatus(pContext.GraphContext.Context, $"Failed to update node key={giNode.Key}");

        return StatusCode.OK;
    }

    private static async Task<Option> Upsert(GiNode giNode, QueryExecutionContext pContext)
    {
        if (!pContext.GraphContext.Map.Nodes.TryGetValue(giNode.Key, out var currentGraphNode))
        {
            pContext.GraphContext.Context.LogInformation("Node key={key} not found, adding node for upsert", giNode.Key);
            return await Add(giNode, pContext);
        }

        pContext.GraphContext.Context.LogInformation("Updating giNode={giNode}", giNode);

        var dataMapOption = await NodeDataTool.MergeData(giNode, currentGraphNode, pContext);
        if (dataMapOption.IsError()) return dataMapOption.ToOptionStatus();
        var dataMap = dataMapOption.Return().ToDictionary(x => x.Name, x => x);

        var graphNode = new GraphNode(giNode.Key, giNode.Tags, DateTime.UtcNow, dataMap);

        var updateOption = pContext.GraphContext.Map.Nodes.Update(graphNode, graphNode.Tags, dataMap, pContext.GraphContext);
        if (updateOption.IsError()) return updateOption.LogStatus(pContext.GraphContext.Context, $"Failed to update node key={giNode.Key}");

        return StatusCode.OK;
    }
}
