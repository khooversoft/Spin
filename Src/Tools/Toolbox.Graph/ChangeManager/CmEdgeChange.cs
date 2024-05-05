using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public class CmEdgeChange : IChangeLog
{
    public CmEdgeChange(GraphEdge currentValue, GraphEdge newValue)
    {
        CurrentValue = currentValue.NotNull();
        NewValue = newValue.NotNull();
        (CurrentValue.FromKey == NewValue.FromKey).Assert(x => x == true, "Edge FromKey must be the same");
        (CurrentValue.ToKey == NewValue.ToKey).Assert(x => x == true, "Edge ToKey must be the same");
    }

    public Guid LogKey { get; } = Guid.NewGuid();
    private GraphEdge CurrentValue { get; }
    private GraphEdge NewValue { get; }

    public Task<Option> Undo(IGraphTrxContext graphContext)
    {
        graphContext.NotNull();

        graphContext.Map.Edges[CurrentValue.Key] = CurrentValue;
        graphContext.Context.LogInformation("Rollback Edge: restored edge logKey={logKey}, edgeKey={key}, value={value}", LogKey, CurrentValue.Key, CurrentValue.ToJson());

        return ((Option)StatusCode.OK).ToTaskResult();
    }
}