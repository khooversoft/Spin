using System.Collections.Concurrent;
using Toolbox.Extensions;
using Toolbox.Journal;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.TransactionLog;

public interface ITransactionLog
{
    Task<Option<ILogicalTrx>> StartTransaction(ScopeContext context);
    Task<IReadOnlyList<JournalEntry>> ReadJournals(string name, ScopeContext context);
}


public class TransactionLogProvider : ITransactionLog
{
    private readonly ConcurrentDictionary<string, ITransactionLogWriter> _journals = new(StringComparer.OrdinalIgnoreCase);
    private readonly LogSequenceNumber _logSequenceNumber = new LogSequenceNumber();
    private readonly object _lock = new object();

    public TransactionLogProvider(IEnumerable<ITransactionLogWriter> writers)
    {
        writers.NotNull().ForEach(x => Add(x));
    }

    public IReadOnlyDictionary<string, ITransactionLogWriter> Journals => _journals;

    public void Add(ITransactionLogWriter transactionLogWriter)
    {
        transactionLogWriter.NotNull();

        var result = _journals.TryAdd(transactionLogWriter.Name, transactionLogWriter);
        result.Assert(x => x == true, $"Attempting to add duplicate journal name={transactionLogWriter.Name}");
    }

    public Task<Option<ILogicalTrx>> StartTransaction(ScopeContext context)
    {
        var trxId = Guid.NewGuid().ToString();
        var reader = new VirtualLogReceiver(trxId, Write, context);

        var option = reader.ToOption<ILogicalTrx>();
        return option.ToTaskResult();
    }

    public async Task<IReadOnlyList<JournalEntry>> ReadJournals(string name, ScopeContext context)
    {
        if (!_journals.TryGetValue(name, out ITransactionLogWriter? journal))
        {
            context.LogError("Journal not found, name={name}", name);
            return Array.Empty<JournalEntry>();
        }

        return await journal.ReadJournals(context);
    }

    private async Task<Option> Write(IReadOnlyList<JournalEntry> journalEntries, ScopeContext context)
    {
        _journals.Assert(x => x.Count > 0, "No journals to write to");

        var queue = new ConcurrentQueue<Option>();

        var journalEntriesWithLsn = journalEntries
            .Select(x => x with { LogSequenceNumber = _logSequenceNumber.Next() })
            .ToList();

        string journalEntriesWithLsnString = journalEntriesWithLsn.Select(x => x.ToString()).Join(';');
        context.LogInformation("Writing journalEntry to all journals, journalEntriesWithLsn={journalEntriesWithLsn}", journalEntriesWithLsnString);

        Option status = StatusCode.OK;
        foreach (var item in _journals.Values)
        {
            context.LogTrace("Writing journalEntry to journal, journalName={journalName}", item.Name);
            var result = await item.Write(journalEntriesWithLsn, context);
            if (result.IsError()) status = result;
        }

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