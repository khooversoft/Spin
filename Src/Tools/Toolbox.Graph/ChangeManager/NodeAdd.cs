using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public class NodeAdd : IChangeLog
{
    public NodeAdd(GraphNode newValue) => NewValue = newValue.NotNull();

    public Guid LogKey { get; } = Guid.NewGuid();
    public GraphNode NewValue { get; }

    public Option Undo(GraphContext graphContext)
    {
        graphContext.NotNull();

        if (!graphContext.Map.Nodes.Remove(NewValue.Key))
        {
            graphContext.Context.LogError("Rollback: logKey={logKey}, Failed to remove node key={key}", LogKey, NewValue.Key);
            return (StatusCode.Conflict, $"Failed to remove node key={NewValue.Key}");
        }

        graphContext.Context.LogInformation("Rollback: removed node logKey={logKey}, Node key={key}", LogKey, NewValue.Key);
        return StatusCode.OK;
    }

    public ChangeTrx GetChangeTrx(Guid trxKey) => new ChangeTrx(ChangeTrxType.NodeAdd, trxKey, LogKey, NewValue, null);
    public ChangeTrx GetUndoChangeTrx(Guid trxKey) => new ChangeTrx(ChangeTrxType.UndoNodeAdd, trxKey, LogKey, NewValue, null);
}
