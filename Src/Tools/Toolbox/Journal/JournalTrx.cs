using Microsoft.Extensions.Logging;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Journal;

public interface IJournalTrx : IAsyncDisposable
{
    string TransactionId { get; }
    Task Commit();
    Task<Option> Write(IReadOnlyList<JournalEntry> journalEntries);
}

public class JournalTrx : IJournalTrx, IAsyncDisposable
{
    private readonly JournalFile _journalFile;
    private readonly string _trxId;
    private readonly ILogger _logger;
    private readonly ScopeContext _context;
    private int _commitFlag = 0;
    private int _started = 0;

    internal JournalTrx(JournalFile journalFile, string trxId, ILogger logger)
    {
        _journalFile = journalFile.NotNull();
        _trxId = trxId.NotEmpty();
        _logger = logger;
        _context = new ScopeContext(_logger);
    }

    public string TransactionId => _trxId;

    public async ValueTask DisposeAsync() => await Commit();

    public async Task Commit()
    {
        var read = Interlocked.CompareExchange(ref _commitFlag, 1, 0);
        if (read == 1) return;

        if (_started == 0)
        {
            _context.LogDebug("Trx not started, normally for query type instructions, trxId={trxId}", _trxId);
            return;
        }

        _context.LogDebug("Commit={trxId}", _trxId);

        var entry = new JournalEntry
        {
            TransactionId = _trxId,
            Type = JournalType.Commit,
        };

        await _journalFile.Write([entry], _context);
    }

    public async Task<Option> Write(IReadOnlyList<JournalEntry> journalEntries)
    {
        _commitFlag.Assert(x => x == 0, "Transaction has alrady signaled committed");
        journalEntries.NotNull();

        var read = Interlocked.CompareExchange(ref _started, 1, 0);
        if (read == 0)
        {
            _context.LogDebug("Starting trx={trxId}", _trxId);

            journalEntries = journalEntries.Prepend(new JournalEntry
            {
                TransactionId = _trxId,
                Type = JournalType.Start,
            }).ToArray();
        }

        _context.LogDebug("Writing trx={trxId}, count={count}", _trxId, journalEntries.Count);

        var entries = journalEntries
            .Select(x => x with { TransactionId = _trxId })
            .ToArray();

        return await _journalFile.Write(entries, _context);
    }
}
