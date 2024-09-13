using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public class CmNodeAdd : IChangeLog
{
    public CmNodeAdd(GraphNode newValue) => NewValue = newValue.NotNull();

    public Guid LogKey { get; } = Guid.NewGuid();
    public GraphNode NewValue { get; }

    public Task<Option> Undo(IGraphTrxContext graphContext)
    {
        graphContext.NotNull();

        if (graphContext.Map.Nodes.Remove(NewValue.Key).IsError())
        {
            graphContext.Context.LogError("Rollback: logKey={logKey}, Failed to remove node key={key}", LogKey, NewValue.Key);
            return ((Option)(StatusCode.Conflict, $"Failed to remove node key={NewValue.Key}")).ToTaskResult();
        }

        graphContext.Context.LogInformation("Rollback: removed node logKey={logKey}, Node key={key}", LogKey, NewValue.Key);
        return ((Option)StatusCode.OK).ToTaskResult();
    }
}
