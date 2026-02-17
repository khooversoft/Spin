using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Data;

public enum TrxRunState
{
    None,
    Active,
    Committing,
    RollingBack,
    Recovery,
    Failed,
}

public partial class Transaction
{
    private readonly ConcurrentQueue<DataChangeEntry> _queue = new();
    private readonly TransactionOption _trxOption;
    private readonly LogSequenceNumber _logSequenceNumber;
    private readonly IListStore<DataChangeRecord> _changeClient;
    private readonly ILogger<Transaction> _logger;
    private readonly TransactionProviders _providers;
    private EnumState<TrxRunState> _runState = new(TrxRunState.None);

    public Transaction(
        TransactionOption trxOption,
        LogSequenceNumber logSequenceNumber,
        IListStore<DataChangeRecord> changeClient,
        ILogger<Transaction> logger
        )
    {
        _trxOption = trxOption.NotNull().Action(x => x.Validate().ThrowOnError());
        _logSequenceNumber = logSequenceNumber.NotNull();
        _changeClient = changeClient.NotNull();
        _logger = logger.NotNull();

        Action testRunning = () => _runState.IfValue(TrxRunState.None).BeTrue("Transaction most not be started");

        TrxRecorder = new TrxRecorder(this);
        _providers = new TransactionProviders(this, testRunning, _logger);
    }

    public string TransactionId { get; private set; } = Guid.NewGuid().ToString();
    public TrxRecorder TrxRecorder { get; }
    public TrxRunState RunState => _runState.Value;
    public TransactionProviders Providers => _providers;

    public async Task<IAsyncDisposable> Start()
    {
        _providers.Count.Assert(x => x > 0, "No providers registered");
        _queue.Count.Assert(x => x == 0, "Transaction queue is not empty");

        _runState.TryMove(TrxRunState.None, TrxRunState.Active).BeTrue("Active is already in progress");
        TransactionId = Guid.NewGuid().ToString();
        await Providers.Start();

        _logger.LogTrace("Starting transaction with id={transactionId}", TransactionId);

        var scope = new CommitTrxDispose(this);
        return scope;
    }

    public void Enqueue(DataChangeEntry entry)
    {
        _runState.IfValue(TrxRunState.Active).BeTrue("Transaction is not in progress");
        entry.NotNull().Validate().ThrowOnError();
        _logger.LogTrace("Enqueue Transaction Entry: {entry}", entry);

        _queue.Enqueue(entry);
    }

    public void Enqueue<T>(string sourceName, string objectId, string action, DataETag? before, DataETag? after)
    {
        if (_runState.IfValue(TrxRunState.RollingBack) || _runState.IfValue(TrxRunState.Recovery)) return;

        _runState.IfValue(TrxRunState.Active).BeTrue("Transaction is not in progress");
        _logger.LogTrace("Enqueue Transaction sourceName={sourceName}, objectId={objectId}", sourceName, objectId);

        var entry = new DataChangeEntry
        {
            LogSequenceNumber = _logSequenceNumber.Next(),
            TransactionId = TransactionId,
            ObjectId = objectId.NotEmpty(),
            Date = DateTime.UtcNow,
            TypeName = typeof(T).Name,
            SourceName = sourceName.NotEmpty(),
            Action = action,
            Before = before,
            After = after,
        };

        entry.NotNull().Validate().ThrowOnError();
        Enqueue(entry);
    }

    public async Task<Option> Commit()
    {
        _runState.TryMove(TrxRunState.Active, TrxRunState.Committing).BeTrue("Transaction is not in progress");
        _logger.LogTrace("Committing transaction with count={count}", _queue.Count);

        var dataChangeRecord = new DataChangeRecord
        {
            TransactionId = TransactionId,
            Entries = _queue.ToArray(),
        };

        _queue.Clear();
        var r1 = await CommitJournal(dataChangeRecord);
        _runState.TryMove(TrxRunState.Committing, TrxRunState.None).BeTrue("Transaction is not in finalized");
        if (r1.IsError()) return r1;

        await Providers.Commit(dataChangeRecord);
        _logger.LogTrace("Completed committing journal to store");

        await Providers.Checkpoint();
        _logger.LogTrace("Completed committing checkpoint to store");

        return StatusCode.OK;
    }

    public async Task<Option> Rollback()
    {
        _runState.TryMove(TrxRunState.Active, TrxRunState.RollingBack).BeTrue("Transaction is not in progress");
        _logger.LogTrace("Rolling back transaction with count={count}", _queue.Count);

        // snap shot queue and clear
        var queue = _queue.Reverse().ToArray();
        _queue.Clear();

        foreach (var journalEntry in queue)
        {
            var result = await _providers.Rollback(journalEntry);
            if (result.IsError())
            {
                _logger.LogError("Failed to rollback transaction entry: {entry}, error={error}", journalEntry, result.Error);
                _runState.TryMove(TrxRunState.RollingBack, TrxRunState.None).BeTrue("Transaction is aborted");
                return result;
            }
        }

        _runState.TryMove(TrxRunState.RollingBack, TrxRunState.None).BeTrue("Transaction is rollback");
        _logger.LogTrace("Completed rollback");
        return StatusCode.OK;
    }

    private async Task<Option> CommitJournal(DataChangeRecord dataChangeRecord)
    {
        dataChangeRecord.NotNull().Validate().ThrowOnError();

        _logger.LogTrace("Committing journal to store, entries={entries}", dataChangeRecord.Entries.Count);
        var journalOption = await _changeClient.Append(_trxOption.JournalKey, [dataChangeRecord]);
        if (journalOption.IsError())
        {
            _logger.LogError("Failed to commit journal to store, statusCode={statusCode}, error={error}", journalOption.StatusCode, journalOption.Error);
            return journalOption.ToOptionStatus();
        }

        return StatusCode.OK;
    }

    private class CommitTrxDispose : IAsyncDisposable
    {
        private readonly Transaction _trx;
        public CommitTrxDispose(Transaction trx) => _trx = trx;
        public async ValueTask DisposeAsync() => await _trx.Commit();
    }
}
