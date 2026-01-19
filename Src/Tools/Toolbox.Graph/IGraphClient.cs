using Microsoft.Extensions.Logging;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public interface IGraphClient
{
    Task<Option<QueryResult>> Execute(string command);
    Task<Option<QueryBatchResult>> ExecuteBatch(string command);
}

public static class GraphClientTool
{
    public static async Task<Option<T>> GetNode<T>(this IGraphClient graphClient, string nodeKey, ILogger logger)
    {
        var cmd = new SelectCommandBuilder()
            .AddNodeSearch(x => x.SetNodeKey(nodeKey))
            .AddDataName("entity")
            .Build();

        var resultOption = await graphClient.Execute(cmd);
        if (resultOption.IsError()) return logger.LogStatus(resultOption, "Failed to find nodeKey={nodeKey}", [nodeKey]).ToOptionStatus<T>();

        var result = resultOption.Return();
        if (result.Nodes.Count == 0) return (StatusCode.NotFound, "Node not found");

        return result.DataLinkToObject<T>("entity");
    }

    public static async Task<Option<T>> GetByTag<T>(this IGraphClient graphClient, string tag, ILogger logger)
    {
        var cmd = new SelectCommandBuilder()
            .AddNodeSearch(x => x.AddTag(tag))
            .AddDataName("entity")
            .Build();

        var resultOption = await graphClient.Execute(cmd);
        if (resultOption.IsError()) return logger.LogStatus(resultOption, "Failed to find by tag={tag}", [tag]).ToOptionStatus<T>();

        var result = resultOption.Return();
        if (result.Nodes.Count == 0) return (StatusCode.NotFound, "Node not found");

        return result.DataLinkToObject<T>("entity");
    }

    public static async Task<Option> DeleteNode(this IGraphClient graphClient, string nodeKey, ILogger logger)
    {
        nodeKey.NotEmpty();

        var cmd = new DeleteCommandBuilder()
            .SetIfExist()
            .SetNodeKey(nodeKey)
            .Build();

        var result = await graphClient.Execute(cmd);
        logger.LogStatus(result, "Delete nodeKey={nodeKey}", [nodeKey]);
        return result.ToOptionStatus();
    }
}