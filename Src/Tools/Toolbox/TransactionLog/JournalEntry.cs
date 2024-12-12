using System.Collections.Frozen;
using Toolbox.Extensions;

namespace Toolbox.TransactionLog;

public enum JournalType
{
    Action,
    CommitTran,
}


public sealed record JournalEntry
{
    public string LogSequenceNumber { get; init; } = null!;
    public string TransactionId { get; init; } = Guid.NewGuid().ToString();
    public DateTime TimeStamp { get; init; } = DateTime.UtcNow;
    public JournalType Type { get; init; }
    public IReadOnlyDictionary<string, string?> Data { get; init; } = FrozenDictionary<string, string?>.Empty;

    public bool Equals(JournalEntry? subject) => subject is JournalEntry &&
        LogSequenceNumber == subject.LogSequenceNumber &&
        TransactionId == subject.TransactionId &&
        TimeStamp == subject.TimeStamp &&
        Type == subject.Type &&
        Data.DeepEqualsComparer(subject.Data);

    public override int GetHashCode() => HashCode.Combine(LogSequenceNumber, TransactionId, TimeStamp, Type, Data);

    public override string ToString() => $"Lsn={LogSequenceNumber}, TranId={TransactionId}, TimeStamp={TimeStamp}, Type={Type}, Data={Data.ToJson()}";

    public static JournalEntry Create(JournalType type, IEnumerable<KeyValuePair<string, string?>> data) => new JournalEntry
    {
        Type = type,
        Data = data.ToFrozenDictionary(),
    };
}

