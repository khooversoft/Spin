using System.Collections.Immutable;
using Toolbox.Extensions;

namespace Toolbox.TransactionLog;

public enum JournalType
{
    Action,
    Revert,
    StartTran,
    CommitTran,
    RollbackTran,
}


public sealed record JournalEntry
{
    public string LogSequenceNumber { get; init; } = null!;
    public string TransactionId { get; init; } = Guid.NewGuid().ToString();
    public DateTime TimeStamp { get; init; } = DateTime.UtcNow;
    public JournalType Type { get; init; }
    public IReadOnlyDictionary<string, string?> Data { get; init; } = ImmutableDictionary<string, string?>.Empty;

    public bool Equals(JournalEntry? subject) => subject is JournalEntry &&
        LogSequenceNumber == subject.LogSequenceNumber &&
        TransactionId == subject.TransactionId &&
        TimeStamp == subject.TimeStamp &&
        Type == subject.Type &&
        Data.DeepEquals(subject.Data);

    public override int GetHashCode() => HashCode.Combine(LogSequenceNumber, TransactionId, TimeStamp, Type, Data);

    public override string ToString() => $"Lsn={LogSequenceNumber}, TranId={TransactionId}, TimeStamp={TimeStamp}, Type={Type}, Data={Data.ToJson()}";
}


// Examples
// Edge Change
//   key="new:edge", value={jsonOfObject}}
//   key="current:edge", value={jsonOfObject}}
//   key="logKey", value={guid}
// Node Change
//   key="new:node", value={jsonOfObject}}
//   key="current:node", value={jsonOfObject}}
//   key="logKey", value={guid}
// Node Add
//   key="new:node", value={jsonOfObject}}
//   key="logKey", value={guid}
