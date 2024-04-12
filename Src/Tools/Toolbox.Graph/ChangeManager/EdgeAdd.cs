using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public class EdgeAdd : IChangeLog
{
    public EdgeAdd(GraphEdge newValue) => NewValue = newValue.NotNull();

    public Guid LogKey { get; } = Guid.NewGuid();
    public GraphEdge NewValue { get; }

    public Option Undo(GraphContext graphContext)
    {
        graphContext.NotNull();

        if (!graphContext.Map.Edges.Remove(NewValue.Key))
        {
            graphContext.Context.LogError("Rollback Edge: logKey={logKey}, Failed to remove node key={key}", NewValue.Key);
            return (StatusCode.Conflict, $"Failed to remove edge edgeKey={NewValue.Key}");
        }

        graphContext.Context.LogInformation("Rollback Edge: removed edge logKey={logKey}, Edge edgeKey={key} ", LogKey, NewValue.Key);
        return StatusCode.OK;
    }

    public ChangeTrx GetChangeTrx(Guid trxKey) => new ChangeTrx(ChangeTrxType.EdgeAdd, trxKey, LogKey, NewValue, null);
    public ChangeTrx GetUndoChangeTrx(Guid trxKey) => new ChangeTrx(ChangeTrxType.UndoEdgeAdd, trxKey, LogKey, NewValue, null);
}
