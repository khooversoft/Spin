using System.Collections.Frozen;
using System.Diagnostics;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Journal;

public enum JournalType
{
    Action,
    Start,
    Commit,
}

[DebuggerDisplay("{ToString()}")]
public sealed record JournalEntry
{
    public string LogSequenceNumber { get; init; } = null!;
    public string TransactionId { get; init; } = Guid.NewGuid().ToString();
    public DateTime Date { get; init; } = DateTime.UtcNow;
    public JournalType Type { get; init; }
    public IReadOnlyDictionary<string, string?> Data { get; init; } = FrozenDictionary<string, string?>.Empty;

    public bool Equals(JournalEntry? subject) => subject is JournalEntry &&
        LogSequenceNumber == subject.LogSequenceNumber &&
        TransactionId == subject.TransactionId &&
        Date == subject.Date &&
        Type == subject.Type &&
        Data.DeepEqualsComparer(subject.Data);

    public override int GetHashCode() => HashCode.Combine(LogSequenceNumber, TransactionId, Date, Type, Data);

    public override string ToString() => $"Lsn={LogSequenceNumber}, TranId={TransactionId}, Date={Date}, Type={Type}, Data={Data.ToJson()}";

    public static JournalEntry Create(JournalType type, IEnumerable<KeyValuePair<string, string?>> data) => new JournalEntry
    {
        Type = type,
        Data = data.NotNull().ToFrozenDictionary(),
    };
}

