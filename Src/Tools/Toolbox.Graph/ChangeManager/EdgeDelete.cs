using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public record EdgeDelete : IChangeLog
{
    public EdgeDelete(GraphEdge oldValue) => CurrentValue = oldValue.NotNull();

    public Guid LogKey { get; } = Guid.NewGuid();
    private GraphEdge CurrentValue { get; }

    public Option Undo(IGraphTrxContext graphContext)
    {
        graphContext.NotNull();

        graphContext.Map.Add(CurrentValue);
        graphContext.Context.LogInformation("Rollback: restored edge logKey={logKey}, Edge key={key}, value={value}", LogKey, CurrentValue.Key, CurrentValue.ToJson());
        return StatusCode.OK;
    }

    public ChangeTrx GetChangeTrx(Guid trxKey) => new ChangeTrx(ChangeTrxType.EdgeDelete, trxKey, LogKey, CurrentValue, null);
    public ChangeTrx GetUndoChangeTrx(Guid trxKey) => new ChangeTrx(ChangeTrxType.UndoEdgeDelete, trxKey, LogKey, CurrentValue, null);
}
