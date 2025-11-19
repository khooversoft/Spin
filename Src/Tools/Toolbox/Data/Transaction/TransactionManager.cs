using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Data;

public class TransactionManager
{
    private enum RunState
    {
        None,
        Transaction
    };

    private readonly ConcurrentQueue<DataChangeEntry> _queue = new();
    private readonly ConcurrentDictionary<string, ITransactionProvider> _providers = new(StringComparer.OrdinalIgnoreCase);
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private EnumState<RunState> _runState = new(RunState.None);

    private readonly TransactionManagerOption _transactionManagerOption;
    private readonly IListStore<DataChangeRecord> _changeClient;
    private readonly LogSequenceNumber _logSequenceNumber;
    private readonly ILogger<TransactionManager> _logger;

    public TransactionManager(
        TransactionManagerOption transactionManagerOption,
        LogSequenceNumber logSequenceNumber,
        IListStore<DataChangeRecord> changeClient,
        ILogger<TransactionManager> logger
        )
    {
        _transactionManagerOption = transactionManagerOption.NotNull().Action(x => x.Validate().ThrowOnError());
        _logSequenceNumber = logSequenceNumber.NotNull();
        _changeClient = changeClient.NotNull();
        _logger = logger.NotNull();
    }

    public string TransactionId { get; private set; } = Guid.NewGuid().ToString();

    public ITrxRecorder Register(string sourceName, ITransactionProvider provider)
    {
        sourceName.NotEmpty();
        provider.NotNull();
        _runState.Value.Assert(x => x != RunState.Transaction, "Transaction is already in progress");

        _providers.TryAdd(sourceName, provider).Assert(x => x == true, $"Provider {sourceName} already registered");
        return new TrxRecorder(this, sourceName);
    }

    public Task<Option> Start(ScopeContext context)
    {
        // Must succeed (true) when moving from None -> Transaction
        _runState.TryMove(RunState.None, RunState.Transaction).Assert(x => x == true, "Transaction is already in progress");
        _queue.Count.Assert(x => x == 0, "Transaction queue is not empty");

        // Begin a fresh transaction
        TransactionId = Guid.NewGuid().ToString();

        context = context.With(_logger);
        context.LogTrace("Starting transaction with id={transactionId}", TransactionId);
        return new Option(StatusCode.OK).ToTaskResult();
    }

    public void Enqueue(DataChangeEntry entry)
    {
        entry.NotNull().Validate().ThrowOnError();
        _logger.LogTrace("Enqueue Transaction Entry: {entry}", entry);
        _queue.Enqueue(entry);
    }

    public void Enqueue<T>(string sourceName, string objectId, string action, DataETag? before, DataETag? after)
    {
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

        Enqueue(entry);
    }

    public async Task<Option> Commit(ScopeContext context)
    {
        _runState.TryMove(RunState.Transaction, RunState.None).Assert(x => x == true, "Transaction is not in progress");

        context = context.With(_logger);
        context.LogTrace("Committing transaction with count={count}", _queue.Count);

        var dataChangeRecord = new DataChangeRecord
        {
            TransactionId = TransactionId,
            Entries = _queue.ToArray(),
        };

        _queue.Clear();

        context.LogTrace("Committing journal to store, count={count}", _queue.Count);
        var r1 = await CommitJournal(dataChangeRecord, context);
        if (r1.IsError()) return r1;

        context.LogTrace("Prepare transaction, with providers");
        var r2 = await PrepareProviders(dataChangeRecord, context);
        if (r2.IsError()) return r2;

        context.LogTrace("Committing transaction with providers");
        var r3 = await CommitProviders(dataChangeRecord, context);
        if (r3.IsError()) return r3;

        context.LogTrace("Completed committing journal to store");
        return StatusCode.OK;
    }

    public async Task<Option> Rollback(ScopeContext context)
    {
        _runState.TryMove(RunState.Transaction, RunState.None).Assert(x => x == true, "Transaction is not in progress");
        context = context.With(_logger);

        // snap shot queue and clear
        var queue = _queue.Reverse().ToArray();
        _queue.Clear();

        foreach (var journalEntry in queue)
        {
            _providers.TryGetValue(journalEntry.SourceName, out var providerInstance)
                .Assert(x => x == true, "No provider for sourceName={sourceName}", journalEntry.SourceName);

            var result = await providerInstance.NotNull().Rollback(journalEntry, context);
            if (result.IsError())
            {
                context.LogError("Failed rollback transaction with sourceName={sourceName}, statusCode={statusCode}, error={error}", journalEntry.SourceName, result.StatusCode, result.Error);
                return result;
            }
        }

        context.LogTrace("Completed rollback");
        return StatusCode.OK;
    }

    private async Task<Option> CommitJournal(DataChangeRecord dataChangeRecord, ScopeContext context)
    {
        dataChangeRecord.NotNull().Validate().ThrowOnError();
        context = context.With(_logger);

        var journalOption = await _changeClient.Append(_transactionManagerOption.JournalKey, [dataChangeRecord], context);
        if (journalOption.IsError()) return journalOption.LogStatus(context, "Failed to append to journal file").ToOptionStatus();

        return StatusCode.OK;
    }

    private async Task<Option> PrepareProviders(DataChangeRecord dataChangeRecord, ScopeContext context)
    {
        foreach (var provider in _providers.Values)
        {
            context.LogTrace("Preparing transaction with provider={provider}", provider.Name);

            var result = await provider.Prepare(dataChangeRecord, context);
            if (result.IsError())
            {
                context.LogError("Failed to prepare transaction with provider={provider}, statusCode={statusCode}, error={error}", provider.Name, result.StatusCode, result.Error);
                return result;
            }
        }

        return StatusCode.OK;
    }

    private async Task<Option> CommitProviders(DataChangeRecord dataChangeRecord, ScopeContext context)
    {
        foreach (var provider in _providers.Values)
        {
            context.LogTrace("Committing transaction with provider={provider}", provider.Name);

            var result = await provider.Commit(dataChangeRecord, context);
            if (result.IsError())
            {
                context.LogError("Failed to commit transaction with provider={provider}, statusCode={statusCode}, error={error}", provider.Name, result.StatusCode, result.Error);
                return result;
            }
        }

        return StatusCode.OK;
    }
}

