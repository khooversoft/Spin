using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public interface IGraphClient
{
    Task<Option<QueryResult>> Execute(string command, ScopeContext context);
    Task<Option<QueryBatchResult>> ExecuteBatch(string command, ScopeContext context);
}


public static class GraphClientExtensions
{
    public static async Task<Option<T>> GetNode<T>(this IGraphClient graphClient, string nodeKey, ScopeContext context)
    {
        var cmd = new SelectCommandBuilder()
            .AddNodeSearch(x => x.SetNodeKey(nodeKey))
            .AddDataName("entity")
            .Build();

        var result = await graphClient.Execute(cmd, context);
        if (result.IsError())
        {
            context.LogError("Failed to find by tag={tag}", nodeKey);
            return result.LogStatus(context, $"tag={nodeKey}").ToOptionStatus<T>();
        }

        return result.Return().DataLinkToObject<T>("entity");
    }

    public static async Task<Option<T>> GetByTag<T>(this IGraphClient graphClient, string tag, ScopeContext context)
    {
        var cmd = new SelectCommandBuilder()
            .AddNodeSearch(x => x.AddTag(tag))
            .AddDataName("entity")
            .Build();

        var result = await graphClient.Execute(cmd, context);
        if (result.IsError())
        {
            context.LogError("Failed to find by tag={tag}", tag);
            return result.LogStatus(context, $"tag={tag}").ToOptionStatus<T>();
        }

        return result.Return().DataLinkToObject<T>("entity");
    }

    public static async Task<Option> DeleteNode(this IGraphClient graphClient, string nodeKey, ScopeContext context)
    {
        nodeKey.NotEmpty();
        context = context.With(context.Logger);

        var cmd = new DeleteCommandBuilder()
            .SetIfExist()
            .SetNodeKey(nodeKey)
            .Build();

        var result = await graphClient.Execute(cmd, context);
        if (result.IsError())
        {
            context.LogError("Failed to delete nodeKey={nodeKey}", nodeKey);
            return result.LogStatus(context, $"nodeKey={nodeKey}").ToOptionStatus();
        }

        result.LogStatus(context, $"Deleting nodeKey={nodeKey}");
        return result.ToOptionStatus();
    }
}