using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public class CmEdgeAdd : IChangeLog
{
    public CmEdgeAdd(GraphEdge newValue) => NewValue = newValue.NotNull();

    public Guid LogKey { get; } = Guid.NewGuid();
    public GraphEdge NewValue { get; }

    public Task<Option> Undo(IGraphTrxContext graphContext)
    {
        graphContext.NotNull();

        if (!graphContext.Map.Edges.Remove(NewValue.Key))
        {
            graphContext.Context.LogError("Rollback Edge: logKey={logKey}, Failed to remove node key={key}", NewValue.Key);
            return ((Option)(StatusCode.Conflict, $"Failed to remove edge edgeKey={NewValue.Key}")).ToTaskResult();
        }

        graphContext.Context.LogInformation("Rollback Edge: removed edge logKey={logKey}, Edge edgeKey={key} ", LogKey, NewValue.Key);
        return ((Option)StatusCode.OK).ToTaskResult();
    }
}