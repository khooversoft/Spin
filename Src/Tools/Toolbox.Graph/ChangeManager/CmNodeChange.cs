using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public class CmNodeChange : IChangeLog
{
    public CmNodeChange(GraphNode currentValue, GraphNode newValue)
    {
        CurrentValue = currentValue.NotNull();
        NewValue = newValue.NotNull();
        (CurrentValue.Key == NewValue.Key).Assert(x => x == true, "Node Key must be the same");
    }

    public Guid LogKey { get; } = Guid.NewGuid();
    public GraphNode CurrentValue { get; }
    public GraphNode NewValue { get; }

    public Task<Option> Undo(IGraphTrxContext graphContext)
    {
        graphContext.NotNull();

        graphContext.Map.Nodes[CurrentValue.Key] = CurrentValue;
        graphContext.Context.LogInformation("Rollback Node: restored node logKey={logKey}, key={key}, value={value}", LogKey, CurrentValue.Key, CurrentValue.ToJson());
        return ((Option)StatusCode.OK).ToTaskResult();
    }
}
