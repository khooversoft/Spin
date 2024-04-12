using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public class NodeChange : IChangeLog
{
    public NodeChange(GraphNode currentValue, GraphNode newValue)
    {
        CurrentValue = currentValue.NotNull();
        NewValue = newValue.NotNull();
        (CurrentValue.Key == NewValue.Key).Assert(x => x == true, "Node Key must be the same");
    }

    public Guid LogKey { get; } = Guid.NewGuid();
    public GraphNode CurrentValue { get; }
    public GraphNode NewValue { get; }

    public Option Undo(GraphContext graphContext)
    {
        graphContext.NotNull();

        graphContext.Map.Nodes[CurrentValue.Key] = CurrentValue;
        graphContext.Context.LogInformation("Rollback Node: restored node logKey={logKey}, key={key}, value={value}", LogKey, CurrentValue.Key, CurrentValue.ToJson());
        return StatusCode.OK;
    }

    public ChangeTrx GetChangeTrx(Guid trxKey) => new ChangeTrx(ChangeTrxType.NodeChange, trxKey, LogKey, CurrentValue, NewValue);
    public ChangeTrx GetUndoChangeTrx(Guid trxKey) => new ChangeTrx(ChangeTrxType.UndoNodeChange, trxKey, LogKey, CurrentValue, NewValue);
}
