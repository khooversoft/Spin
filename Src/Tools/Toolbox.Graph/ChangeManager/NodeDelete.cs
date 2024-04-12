using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public record NodeDelete : IChangeLog
{
    public NodeDelete(GraphNode oldValue) => CurrentValue = oldValue.NotNull();

    public Guid LogKey { get; } = Guid.NewGuid();
    public GraphNode CurrentValue { get; }

    public Option Undo(GraphContext graphContext)
    {
        graphContext.NotNull();

        graphContext.Map.Nodes[CurrentValue.Key] = CurrentValue;
        graphContext.Context.LogInformation("Rollback: restored node logKey={logKey}, Node key={key}, value={value}", LogKey, CurrentValue.Key, CurrentValue.ToJson());
        return StatusCode.OK;
    }

    public ChangeTrx GetChangeTrx(Guid trxKey) => new ChangeTrx(ChangeTrxType.NodeDelete, trxKey, LogKey, CurrentValue, null);
    public ChangeTrx GetUndoChangeTrx(Guid trxKey) => new ChangeTrx(ChangeTrxType.UndoNodeDelete, trxKey, LogKey, CurrentValue, null);
}
