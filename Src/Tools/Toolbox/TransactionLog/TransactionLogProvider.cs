using System.Collections.Concurrent;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.TransactionLog;

public interface ITransactionLog
{
    Task<Option<ILogicalTrx>> StartTransaction(ScopeContext context);
}

public interface ITransactionLogWriter
{
    Task<Option> Write(JournalEntry journalEntry, ScopeContext context);
}


public class TransactionLogProvider : ITransactionLog
{
    private readonly ConcurrentDictionary<string, ITransactionLogWriter> _journals = new(StringComparer.OrdinalIgnoreCase);
    private readonly LogSequenceNumber _logSequenceNumber = new LogSequenceNumber();
    private readonly object _lock = new object();

    public IReadOnlyDictionary<string, ITransactionLogWriter> Journals => _journals;

    public void Add(string name, ITransactionLogWriter transactionLogWriter)
    {
        name.NotEmpty();
        transactionLogWriter.NotNull();

        var result = _journals.TryAdd(name, transactionLogWriter);
        result.Assert(x => x == true, $"Attempting to add duplicate journal name={name}");
    }

    public async Task<Option<ILogicalTrx>> StartTransaction(ScopeContext context)
    {
        var trxId = Guid.NewGuid().ToString();

        var writeStatus = await Write(new JournalEntry
        {
            TransactionId = trxId,
            Type = JournalType.StartTran,
        }, context);

        if (writeStatus.IsError()) return writeStatus.ToOptionStatus<ILogicalTrx>();

        return new VirtualLogReceiver(trxId, Write, context);
    }

    private async Task<Option> Write(JournalEntry journalEntry, ScopeContext context)
    {
        _journals.Assert(x => x.Count > 0, "No journals to write to");

        var queue = new ConcurrentQueue<Option>();
        var journalEntryWithLsn = journalEntry with { LogSequenceNumber = _logSequenceNumber.Next() };
        context.LogInformation("Writing journalEntry to all journals, journalEntry={journalEntry}", journalEntryWithLsn);

        await Parallel.ForEachAsync(
            _journals.Values,
            context.CancellationToken,
            async (x, c) => queue.Append(await x.Write(journalEntryWithLsn, context))
            );

        var status = queue.FirstOrDefault(x => x.IsError(), StatusCode.OK);
        return status;
    }
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