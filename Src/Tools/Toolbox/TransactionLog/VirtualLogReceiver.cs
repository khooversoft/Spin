using Toolbox.Journal;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.TransactionLog;

public interface ILogicalTrx
{
    string TransactionId { get; }
    Task<Option> Write(IReadOnlyList<JournalEntry> journalEntry);
}


internal class VirtualLogReceiver : ILogicalTrx
{
    private readonly string _transactionId;
    private readonly ScopeContext _context;
    private readonly Func<IReadOnlyList<JournalEntry>, ScopeContext, Task<Option>> _writeJournal;

    public VirtualLogReceiver(string transactionId, Func<IReadOnlyList<JournalEntry>, ScopeContext, Task<Option>> writeJournal, ScopeContext context)
    {
        _transactionId = transactionId.NotEmpty();
        _writeJournal = writeJournal.NotNull();
        _context = context.NotNull();
    }

    public string TransactionId => _transactionId;

    public Task<Option> Write(IReadOnlyList<JournalEntry> journalEntries)
    {
        journalEntries = journalEntries
            .Select(x => x with { TransactionId = _transactionId })
            .Append(new JournalEntry { Type = JournalType.Commit })
            .ToArray();

        _context.LogInformation("Write, transactionId={transactionId}", _transactionId);
        return _writeJournal(journalEntries, _context);
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
