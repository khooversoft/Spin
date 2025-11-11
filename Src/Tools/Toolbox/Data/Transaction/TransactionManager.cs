using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Data;

public class TransactionManager
{
    private readonly ConcurrentQueue<DataChangeEntry> _queue = new();
    private bool _isFinalized = false;

    private readonly TransactionConfiguration _transactionConfiguration;
    private readonly IListStore<DataChangeRecord> _changeClient;
    private readonly LogSequenceNumber _logSequenceNumber;
    private readonly ILogger<TransactionManager> _logger;

    public TransactionManager(
        TransactionConfiguration transactionProviders,
        LogSequenceNumber logSequenceNumber,
        IListStore<DataChangeRecord> changeClient,
        ILogger<TransactionManager> logger
        )
    {
        _transactionConfiguration = transactionProviders.NotNull();
        _logSequenceNumber = logSequenceNumber.NotNull();
        _changeClient = changeClient.NotNull();
        _logger = logger.NotNull();
    }

    public string TransactionId { get; } = Guid.NewGuid().ToString();

    public void Enqueue(DataChangeEntry entry)
    {
        EnsureNotFinalized();
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

    public TrxRecorder GetRecorder(string sourceName)
    {
        EnsureNotFinalized();

        _transactionConfiguration.Providers.TryGetValue(sourceName, out var provider)
            .Assert(x => x == true, "No transaction data provider registered with name={name}", sourceName);

        return new TrxRecorder(this, sourceName);
    }

    public async Task<Option> Commit(ScopeContext context)
    {
        SealTransaction();

        context = context.With(_logger);
        context.LogTrace("Committing transaction with count={count}", _queue.Count);

        var dataChangeRecord = new DataChangeRecord
        {
            TransactionId = TransactionId,
            Entries = _queue.ToArray(),
        };

        context.LogTrace("Committing journal to store, count={count}", _queue.Count);
        var r1 = await CommitJournal(dataChangeRecord, context);
        if (r1.IsError()) return r1;

        context.LogTrace("Prepare transaction ,with providers");
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
        SealTransaction();
        context = context.With(_logger);

        foreach (var journalEntry in _queue)
        {
            _transactionConfiguration.Providers.TryGetValue(journalEntry.SourceName, out var providerInstance)
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

    private void EnsureNotFinalized() => Volatile.Read(ref _isFinalized).Assert(x => x == false, "Transaction already committed or rolled back");
    private void SealTransaction() => Interlocked.CompareExchange(ref _isFinalized, true, false).Assert(x => x == false, "Transaction already committed or rolled back");

    private async Task<Option> CommitJournal(DataChangeRecord dataChangeRecord, ScopeContext context)
    {
        dataChangeRecord.NotNull().Validate().ThrowOnError();
        context = context.With(_logger);

        var journalOption = await _changeClient.Append(_transactionConfiguration.JournalKey, [dataChangeRecord], context);
        if (journalOption.IsError()) return journalOption.LogStatus(context, "Failed to append to journal file").ToOptionStatus();

        return StatusCode.OK;
    }

    private async Task<Option> PrepareProviders(DataChangeRecord dataChangeRecord, ScopeContext context)
    {
        foreach (var provider in _transactionConfiguration.ProviderList)
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
        foreach (var provider in _transactionConfiguration.ProviderList)
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

