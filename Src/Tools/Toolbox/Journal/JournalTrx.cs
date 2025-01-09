using Microsoft.Extensions.Logging;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Journal;

public class JournalTrx : IAsyncDisposable
{
    private readonly JournalFile _journalFile;
    private readonly string _trxId;
    private readonly ILogger _logger;
    private int _commitFlag = 0;

    internal JournalTrx(JournalFile journalFile, string trxId, ILogger logger)
    {
        _journalFile = journalFile.NotNull();
        _trxId = trxId.NotEmpty();
        _logger = logger;
    }

    public string TransactionId => _trxId;

    public async ValueTask DisposeAsync()
    {
        var context = new ScopeContext(_logger);
        await Commit(context);
    }

    public async Task Commit(ScopeContext context)
    {
        var read = Interlocked.CompareExchange(ref _commitFlag, 1, 0);
        if (read == 1) return;

        var entry = new JournalEntry
        {
            TransactionId = _trxId,
            Type = JournalType.Commit,
        };

        await _journalFile.Write([entry], context);
    }

    public Task<Option> Write(IReadOnlyList<JournalEntry> journalEntries, ScopeContext context)
    {
        _commitFlag.Assert(x => x == 0, "Transaction has alrady signaled committed");
        journalEntries.NotNull();

        var entries = journalEntries
            .Select(x => x with { TransactionId = _trxId })
            .ToArray();

        return _journalFile.Write(entries, context);
    }
}
