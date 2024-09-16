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
            GiChangeType.Set => await Set(giNode, pContext),
            GiChangeType.Delete => await Delete(giNode, pContext),
            _ => throw new InvalidOperationException("Invalid change type"),
        };

        pContext.AddQueryResult(result);

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

        var graphNode = new GraphNode(giNode.Key, giNode.Tags.RemoveDeleteCommands(), DateTime.UtcNow, dataMap);

        var graphResult = pContext.GraphContext.Map.Nodes.Add(graphNode, pContext.GraphContext);
        if (graphResult.IsError()) return graphResult;

        pContext.GraphContext.Context.LogInformation("Added giNode={giNode}", giNode);
        return StatusCode.OK;
    }

    private static async Task<Option> Delete(GiNode giNode, QueryExecutionContext pContext)
    {
        giNode.NotNull();
        pContext.NotNull();

        pContext.GraphContext.Context.LogInformation("Deleting giNode={giNode}", giNode);

        if (pContext.GraphContext.Map.Nodes.TryGetValue(giNode.Key, out var readGraphNode))
        {
            var dataMapOption = await NodeDataTool.DeleteData([readGraphNode], pContext.GraphContext);
            if (dataMapOption.IsError()) return dataMapOption;
        }

        var graphResult = pContext.GraphContext.Map.Nodes.Remove(giNode.Key, pContext.GraphContext);
        if (!giNode.IfExist && graphResult.IsError()) return graphResult;

        pContext.GraphContext.Context.LogInformation("Deleting giNode={giNode}", giNode);
        return StatusCode.OK;
    }

    private static async Task<Option> Set(GiNode giNode, QueryExecutionContext pContext)
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

        var tags = TagsTool.ProcessTags(currentGraphNode.Tags, giNode.Tags);
        var graphNode = new GraphNode(giNode.Key, tags, DateTime.UtcNow, dataMap);

        var updateOption = pContext.GraphContext.Map.Nodes.Set(graphNode, pContext.GraphContext);
        if (updateOption.IsError()) return updateOption.LogStatus(pContext.GraphContext.Context, $"Failed to upsert node key={giNode.Key}");

        return StatusCode.OK;
    }
}
