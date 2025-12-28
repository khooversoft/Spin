using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Data;


public enum RunState
{
    None,
    Active,
    Committing,
    RollingBack,
    Failed,
}

public class Transaction
{
    private readonly ConcurrentQueue<DataChangeEntry> _queue = new();
    private readonly TransactionOption _trxOption;
    private readonly LogSequenceNumber _logSequenceNumber;
    private readonly IListStore2<DataChangeRecord> _changeClient;
    private readonly ILogger<Transaction> _logger;
    private EnumState<RunState> _runState = new(RunState.None);
    private TransactionEnlistments _enlistments;

    public Transaction(
        TransactionOption trxOption,
        LogSequenceNumber logSequenceNumber,
        IListStore2<DataChangeRecord> changeClient,
        ILogger<Transaction> logger
        )
    {
        _trxOption = trxOption.NotNull().Action(x => x.Validate().ThrowOnError());
        _logSequenceNumber = logSequenceNumber.NotNull();
        _changeClient = changeClient.NotNull();
        _logger = logger.NotNull();

        TrxRecorder = new TrxRecorder(this);
        _enlistments = new TransactionEnlistments(this, () => _runState.IfValue(RunState.None), _logger);
    }

    public string TransactionId { get; private set; } = Guid.NewGuid().ToString();
    public TrxRecorder TrxRecorder { get; }
    public RunState RunState => _runState.Value;
    public TransactionEnlistments Enlistments => _enlistments;

    public void Start()
    {
        _enlistments.Count.Assert(x => x > 0, "No rollback functions or enlistment registered");
        _queue.Count.Assert(x => x == 0, "Transaction queue is not empty");

        _runState.TryMove(RunState.None, RunState.Active).BeTrue("Active is already in progress");
        TransactionId = Guid.NewGuid().ToString();

        _logger.LogTrace("Starting transaction with id={transactionId}", TransactionId);
    }

    public void Enqueue(DataChangeEntry entry)
    {
        _runState.IfValue(RunState.Active).BeTrue("Transaction is not in progress");
        entry.NotNull().Validate().ThrowOnError();
        _logger.LogTrace("Enqueue Transaction Entry: {entry}", entry);

        _queue.Enqueue(entry);
    }

    public void Enqueue<T>(string sourceName, string objectId, string action, DataETag? before, DataETag? after)
    {
        _runState.IfValue(RunState.Active).BeTrue("Transaction is not in progress");
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

    public async Task<Option> Commit(ScopeContext context)
    {
        _runState.TryMove(RunState.Active, RunState.Committing).BeTrue("Transaction is not in progress");
        context = context.With(_logger);
        context.LogTrace("Committing transaction with count={count}", _queue.Count);

        var dataChangeRecord = new DataChangeRecord
        {
            TransactionId = TransactionId,
            Entries = _queue.ToArray(),
        };

        _queue.Clear();
        var r1 = await CommitJournal(dataChangeRecord, context);
        _runState.TryMove(RunState.Committing, RunState.None).BeTrue("Transaction is not in finalized");
        if (r1.IsError()) return r1;

        context.LogTrace("Completed committing journal to store");
        return StatusCode.OK;
    }

    public async Task<Option> Rollback(ScopeContext context)
    {
        _runState.TryMove(RunState.Active, RunState.RollingBack).BeTrue("Transaction is not in progress");
        context = context.With(_logger);
        context.LogTrace("Rolling back transaction with count={count}", _queue.Count);

        // snap shot queue and clear
        var queue = _queue.Reverse().ToArray();
        _queue.Clear();

        foreach (var journalEntry in queue)
        {
            var result = await _enlistments.Rollback(journalEntry);
            if (result.IsError())
            {
                _logger.LogError("Failed to rollback transaction entry: {entry}, error={error}", journalEntry, result.Error);
                _runState.TryMove(RunState.RollingBack, RunState.None).BeTrue("Transaction is aborted");
                return result;
            }
        }

        _runState.TryMove(RunState.RollingBack, RunState.None).BeTrue("Transaction is rollback");
        context.LogTrace("Completed rollback");
        return StatusCode.OK;
    }

    private async Task<Option> CommitJournal(DataChangeRecord dataChangeRecord, ScopeContext context)
    {
        dataChangeRecord.NotNull().Validate().ThrowOnError();
        context = context.With(_logger);

        context.LogTrace("Committing journal to store, entries={entries}", dataChangeRecord.Entries.Count);
        var journalOption = await _changeClient.Append(_trxOption.JournalKey, [dataChangeRecord], context);
        if (journalOption.IsError()) return journalOption.LogStatus(context, "Failed to append to journal file").ToOptionStatus();

        return StatusCode.OK;
    }
}
