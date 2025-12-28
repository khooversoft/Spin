//using System.Collections.Concurrent;
//using Microsoft.Extensions.Logging;
//using Toolbox.Extensions;
//using Toolbox.Store;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace Toolbox.Data;

///// <summary>
///// Transaction manager is per transaction
///// </summary>
//public class TransactionManager
//{
//    private enum RunState
//    {
//        None,
//        Transaction,
//        Finalized
//    };

//    private readonly ConcurrentQueue<DataChangeEntry> _queue = new();
//    private readonly ConcurrentDictionary<string, ITransactionProvider> _providers = new(StringComparer.OrdinalIgnoreCase);
//    private readonly ConcurrentDictionary<string, ITransaction> _transactions = new(StringComparer.OrdinalIgnoreCase);
//    private readonly SemaphoreSlim _semaphore = new(1, 1);
//    private readonly TransactionManagerOption _transactionManagerOption;
//    private readonly IListStore<DataChangeRecord> _changeClient;
//    private readonly LogSequenceNumber _logSequenceNumber;
//    private readonly ILogger<TransactionManager> _logger;

//    private EnumState<RunState> _runState = new(RunState.None);
//    private DataChangeRecorder _trxChangeRecorder = new();

//    public TransactionManager(
//        TransactionManagerOption transactionManagerOption,
//        LogSequenceNumber logSequenceNumber,
//        IListStore<DataChangeRecord> changeClient,
//        IServiceProvider serviceProvider,
//        ILogger<TransactionManager> logger
//        )
//    {
//        _transactionManagerOption = transactionManagerOption.NotNull().Action(x => x.Validate().ThrowOnError());
//        _logSequenceNumber = logSequenceNumber.NotNull();
//        _changeClient = changeClient.NotNull();
//        _logger = logger.NotNull();
//    }

//    public string TransactionId { get; private set; } = Guid.NewGuid().ToString();

//    public Option Enlist(ITransactionProvider provider, ScopeContext context)
//    {
//        _runState.Value.Assert(x => x != RunState.None, "Transaction is already in progress");

//        var name = provider.NotNull().Name.NotEmpty();
//        _providers.TryAdd(name, provider).Assert(x => x == true, $"Provider name={name} already registered");
//        return StatusCode.OK;
//    }

//    public void Enqueue(DataChangeEntry entry)
//    {
//        entry.NotNull().Validate().ThrowOnError();
//        _logger.LogTrace("Enqueue Transaction Entry: {entry}", entry);
//        _queue.Enqueue(entry);
//    }

//    public void Enqueue<T>(string sourceName, string objectId, string action, DataETag? before, DataETag? after)
//    {
//        var entry = new DataChangeEntry
//        {
//            LogSequenceNumber = _logSequenceNumber.Next(),
//            TransactionId = TransactionId,
//            ObjectId = objectId.NotEmpty(),
//            Date = DateTime.UtcNow,
//            TypeName = typeof(T).Name,
//            SourceName = sourceName.NotEmpty(),
//            Action = action,
//            Before = before,
//            After = after,
//        };

//        Enqueue(entry);
//    }

//    //public Task<Option<DataChangeRecorder>> Start(ScopeContext context)
//    //{
//    //    _runState.TryMove(RunState.None, RunState.Transaction).Assert(x => x == true, "Transaction is already in progress");
//    //    _providers.Count.Assert(x => x > 0, "No transaction providers enlisted");
//    //    _queue.Count.Assert(x => x == 0, "Transaction queue is not empty");

//    //    TransactionId = Guid.NewGuid().ToString();

//    //    _transactions.Clear();
//    //    foreach (var provider in _providers.Values)
//    //    {
//    //        ITransaction trx = provider.CreateTransaction();
//    //        _transactions.TryAdd(trx.SourceName, trx)
//    //            .Assert(x => x == true, "Transaction for sourceName={sourceName} already exists", trx.SourceName);
//    //    }

//    //    var trxRecorder = new TrxRecorder(this);
//    //    _trxChangeRecorder.Set(trxRecorder);

//    //    context = context.With(_logger);
//    //    context.LogTrace("Starting transaction with id={transactionId}", TransactionId);
//    //    return new Option<DataChangeRecorder>(_trxChangeRecorder).ToTaskResult();
//    //}

//    public async Task<Option> Commit(ScopeContext context)
//    {
//        _runState.TryMove(RunState.Transaction, RunState.Finalized).Assert(x => x == true, "Transaction is not in progress");
//        _trxChangeRecorder.Clear();

//        context = context.With(_logger);
//        context.LogTrace("Committing transaction with count={count}", _queue.Count);

//        var dataChangeRecord = new DataChangeRecord
//        {
//            TransactionId = TransactionId,
//            Entries = _queue.ToArray(),
//        };

//        _queue.Clear();
//        var r1 = await CommitJournal(dataChangeRecord, context);
//        if (r1.IsError()) return r1;

//        foreach (var trxProvider in _transactions.Values)
//        {
//            context.LogTrace("Committing transaction with sourceName={sourceName}", trxProvider.SourceName);

//            var result = await trxProvider.Commit(dataChangeRecord, context);
//            if (result.IsError())
//            {
//                context.LogError("Failed to commit transaction with sourceName={sourceName}, statusCode={statusCode}, error={error}", trxProvider.SourceName, result.StatusCode, result.Error);
//                return result;
//            }
//        }

//        _runState.TryMove(RunState.Finalized, RunState.None).Assert(x => x == true, "Transaction is not in finalized");
//        context.LogTrace("Completed committing journal to store");
//        return StatusCode.OK;
//    }

//    public async Task<Option> Rollback(ScopeContext context)
//    {
//        _runState.TryMove(RunState.Transaction, RunState.Finalized).Assert(x => x == true, "Transaction is not in progress");
//        context = context.With(_logger);
//        _trxChangeRecorder.Clear();

//        // snap shot queue and clear
//        var queue = _queue.Reverse().ToArray();
//        _queue.Clear();

//        foreach (var journalEntry in queue)
//        {
//            _transactions.TryGetValue(journalEntry.SourceName, out var providerTrx)
//                .Assert(x => x == true, "No provider for sourceName={sourceName}", journalEntry.SourceName);

//            var result = await providerTrx.NotNull().Rollback(journalEntry, context);
//            if (result.IsError())
//            {
//                context.LogError("Failed rollback transaction with sourceName={sourceName}, statusCode={statusCode}, error={error}", journalEntry.SourceName, result.StatusCode, result.Error);
//                return result;
//            }
//        }

//        _runState.TryMove(RunState.Finalized, RunState.None).Assert(x => x == true, "Transaction is not in finalized");
//        context.LogTrace("Completed rollback");
//        return StatusCode.OK;
//    }

//    private async Task<Option> CommitJournal(DataChangeRecord dataChangeRecord, ScopeContext context)
//    {
//        dataChangeRecord.NotNull().Validate().ThrowOnError();
//        context = context.With(_logger);

//        context.LogTrace("Committing journal to store, count={count}", _queue.Count);
//        var journalOption = await _changeClient.Append(_transactionManagerOption.JournalKey, [dataChangeRecord], context);
//        if (journalOption.IsError()) return journalOption.LogStatus(context, "Failed to append to journal file").ToOptionStatus();

//        return StatusCode.OK;
//    }
//}

