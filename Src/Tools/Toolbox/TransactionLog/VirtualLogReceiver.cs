using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.TransactionLog;

public interface ILogicalTrx : IDisposable
{
    string TransactionId { get; }
    Task<Option> Write(JournalEntry journalEntry);
    Task<Option> CommitTransaction();
    Task<Option> RollbackTransaction();
    Task<Option> Reverted(JournalEntry journalEntry);
}


internal class VirtualLogReceiver : ILogicalTrx, IDisposable
{
    private readonly string _transactionId;
    private readonly ScopeContext _context;
    private readonly Func<JournalEntry, ScopeContext, Task<Option>> _writeJournal;
    private int _active = 1;
    private const int stopped = 0;
    private const int running = 1;

    public VirtualLogReceiver(string transactionId, Func<JournalEntry, ScopeContext, Task<Option>> writeJournal, ScopeContext context)
    {
        _transactionId = transactionId.NotEmpty();
        _writeJournal = writeJournal.NotNull();
        _context = context.NotNull();
    }

    public string TransactionId => _transactionId;

    public async Task<Option> CommitTransaction()
    {
        var current = Interlocked.Exchange(ref _active, stopped);
        if (current == stopped)
        {
            _context.LogError("Failed to commit because transaction is marked closed, transactionId={transactionId}", _transactionId);
            throw new InvalidOperationException($"Failed to commit because transaction is marked closed, transactionId={_transactionId}");
        }

        _context.LogInformation("Commit transaction, transactionId={transactionId}", _transactionId);

        return await _writeJournal(new JournalEntry
        {
            TransactionId = _transactionId,
            Type = JournalType.CommitTran,
        }, _context);
    }

    public Task<Option> RollbackTransaction()
    {
        var current = Interlocked.Exchange(ref _active, stopped);
        if (current == stopped)
        {
            _context.LogError("Failed rollback because transaction is marked closed, transactionId={transactionId}", _transactionId);
            throw new InvalidOperationException($"Failed rollback because transaction is marked closed, transactionId={_transactionId}");
        }

        _context.LogInformation("Commit transaction, transactionId={transactionId}", _transactionId);

        return _writeJournal(new JournalEntry
        {
            TransactionId = _transactionId,
            Type = JournalType.RollbackTran,
        }, _context);
    }

    public Task<Option> Write(JournalEntry journalEntry)
    {
        if (_active != running && journalEntry.Type != JournalType.Revert)
        {
            _context.LogError("Cannot write to transaction becuase it closed, transactionId={transactionId}", _transactionId);
            throw new InvalidOperationException($"Cannot write to transaction becuase it closed, transactionId={_transactionId}");
        }

        _context.LogInformation("Write, transactionId={transactionId}", _transactionId);
        return _writeJournal(journalEntry with { TransactionId = _transactionId }, _context);
    }

    public Task<Option> Reverted(JournalEntry journalEntry)
    {
        _context.LogInformation("Reverting transaction, transactionId={transactionId}", _transactionId);

        return _writeJournal(journalEntry with
        {
            TransactionId = _transactionId,
            Type = JournalType.Revert,
        }, _context);
    }

    public void Dispose()
    {
        if (_active != stopped)
        {
            _context.LogError("Dispose an active transaction, transactionId={transactionId}", _transactionId);
            throw new InvalidOperationException($"Dispose an active transaction, transactionId={_transactionId}");
        }
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
