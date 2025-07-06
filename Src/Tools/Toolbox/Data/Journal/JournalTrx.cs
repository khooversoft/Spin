using Microsoft.Extensions.Logging;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Data;

public interface IJournalTrx : IAsyncDisposable
{
    string TransactionId { get; }
    Task Commit(ScopeContext context);
    Task<Option> Write(IReadOnlyList<JournalEntry> journalEntries, ScopeContext context);
}

public class JournalTrx : IJournalTrx, IAsyncDisposable
{
    private readonly IDataClient<JournalEntry> _dataClient;
    private readonly string _trxId;
    private readonly LogSequenceNumber _logSequence;
    private readonly ILogger<JournalTrx> _logger;
    private readonly string _key;
    private int _commitFlag = 0;
    private int _started = 0;

    public JournalTrx(IDataClient<JournalEntry> dataClient, LogSequenceNumber logSequence, string key, string trxId, ILogger<JournalTrx> logger)
    {
        _dataClient = dataClient.NotNull();
        _logSequence = logSequence.NotNull();
        _logger = logger.NotNull();

        _key = key.NotEmpty();
        _trxId = trxId.NotEmpty();
    }

    public string TransactionId => _trxId;

    public async ValueTask DisposeAsync()
    {
        _logger.ToScopeContext().LogDebug("Disposing transaction trxId={trxId}", TransactionId);
        await Commit(_logger.ToScopeContext());
    }

    public async Task Commit(ScopeContext context)
    {
        context = context.With(_logger);
        var read = Interlocked.CompareExchange(ref _commitFlag, 1, 0);
        if (read == 1) return;
        if (_started == 0) return;

        var entry = new JournalEntry
        {
            TransactionId = _trxId,
            LogSequenceNumber = _logSequence.Next(),
            Type = JournalType.Commit,
        };

        await _dataClient.AppendList(_key, [entry], context);
        context.LogDebug("Commit={trxId}", _trxId);
    }

    public async Task<Option> Write(IReadOnlyList<JournalEntry> journalEntries, ScopeContext context)
    {
        context = context.With(_logger);
        _commitFlag.Assert(x => x == 0, "Transaction has already signaled committed");
        journalEntries.NotNull();

        var list = (IEnumerable<JournalEntry>)journalEntries;

        var read = Interlocked.CompareExchange(ref _started, 1, 0);
        if (read == 0)
        {
            context.LogDebug("Starting trx={trxId}", _trxId);

            list = list.Prepend(new JournalEntry
            {
                TransactionId = _trxId,
                LogSequenceNumber = _logSequence.Next(),
                Type = JournalType.Start,
            });
        }

        context.LogDebug("Writing trx={trxId}, count={count}", _trxId, journalEntries.Count);

        var entries = list
            .Select(x => x with
            {
                TransactionId = _trxId,
                LogSequenceNumber = _logSequence.Next()
            })
            .ToArray();

        return await _dataClient.AppendList(_key, entries, context);
    }
}
