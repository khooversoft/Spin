using System.Text.Json.Serialization;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Graph;

public enum ChangeTrxType
{
    None,
    BatchStart,
    BatchEnd,
    NodeAdd,
    NodeDelete,
    NodeChange,
    EdgeAdd,
    EdgeDelete,
    EdgeChange,
    UndoNodeAdd,
    UndoNodeDelete,
    UndoNodeChange,
    UndoEdgeAdd,
    UndoEdgeDelete,
    UndoEdgeChange,
}

public readonly struct ChangeTrx : IEquatable<ChangeTrx>
{
    public ChangeTrx(ChangeTrxType trxType, Guid trxId, Guid logKey, GraphNode currentValue, GraphNode? updateValue, DateTime? date = null)
    {
        Date = date ?? DateTime.UtcNow;
        TrxType = trxType.Action(x => x.IsEnumValid().Assert(y => y == true, "Enum is invalid"));
        TrxId = trxId;
        LogKey = logKey;
        CurrentNodeValue = currentValue.NotNull();
        UpdateNodeValue = updateValue.Assert(x => trxType != ChangeTrxType.NodeChange || x != null, "Update value must be set for NodeChange");
    }

    public ChangeTrx(ChangeTrxType trxType, Guid trxId, Guid logKey, GraphEdge currentValue, GraphEdge? updateValue, DateTime? date = null)
    {
        Date = date ?? DateTime.UtcNow;
        TrxType = trxType.Action(x => x.IsEnumValid().Assert(y => y == true, "Enum is invalid"));
        TrxId = trxId;
        LogKey = logKey;
        CurrentEdgeValue = currentValue.NotNull();
        UpdateEdgeValue = updateValue.Assert(x => trxType != ChangeTrxType.EdgeChange || x != null, "Update value must be set for EdgeChange");
    }

    [JsonConstructor]
    public ChangeTrx(DateTime date, ChangeTrxType trxType, Guid trxId, Guid logKey, GraphNode currentNodeValue, GraphNode? updateNodeValue, GraphEdge currentEdgeValue, GraphEdge? updateEdgeValue)
    {
        Date = date;
        TrxType = trxType;
        TrxId = trxId;
        LogKey = logKey;
        CurrentNodeValue = currentNodeValue;
        UpdateNodeValue = updateNodeValue;
        CurrentEdgeValue = currentEdgeValue;
        UpdateEdgeValue = updateEdgeValue;
    }

    public DateTime Date { get; }
    public ChangeTrxType TrxType { get; }
    public Guid TrxId { get; }
    public Guid LogKey { get; }
    public GraphNode? CurrentNodeValue { get; }
    public GraphNode? UpdateNodeValue { get; }
    public GraphEdge? CurrentEdgeValue { get; }
    public GraphEdge? UpdateEdgeValue { get; }

    public override bool Equals(object? obj) => obj is ChangeTrx trx && Equals(trx);

    public bool Equals(ChangeTrx other)
    {
        bool result = Date == other.Date &&
            TrxType == other.TrxType &&
            TrxId == other.TrxId &&
            LogKey == other.LogKey &&
            Test(CurrentNodeValue, other.CurrentNodeValue) &&
            Test(UpdateNodeValue, other.UpdateNodeValue) &&
            Test(CurrentEdgeValue, other.CurrentEdgeValue) &&
            Test(UpdateEdgeValue, other.UpdateEdgeValue);

        return result;
    }

    public override int GetHashCode() => HashCode.Combine(Date, TrxType, TrxId, LogKey, CurrentNodeValue, UpdateNodeValue, CurrentEdgeValue, UpdateEdgeValue);
    public static bool operator ==(ChangeTrx left, ChangeTrx right) => left.Equals(right);
    public static bool operator !=(ChangeTrx left, ChangeTrx right) => !(left == right);

    private static bool Test(object? current, object? update) => (current, update) switch
    {
        (null, null) => true,
        (object, object) v => v.Item1?.Equals(v.Item2) == true,
        _ => false,
    };
}
