using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Test.Store.Memory;

public class MemoryStoreTrx : MemoryStore
{
    public MemoryStoreTrx(IServiceProvider services) : base(services.GetRequiredService<ILogger<MemoryStore>>())
    {
    }
}

public class MemoryStoreTransactionTests
{
    private readonly ITestOutputHelper _output;


    public MemoryStoreTransactionTests(ITestOutputHelper output) => _output = output.NotNull();

    private IHost BuildHost()
    {
        var option = new TransactionManagerOption
        {
            JournalKey = "transaction_journal"
        };

        var host = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddLogging(c => c.AddLambda(_output.WriteLine).AddDebug().AddFilter(_ => true));
                services.AddInMemoryFileStore();
                services.AddListStore<DataChangeRecord>();
                services.AddTransactionServices(option);

                services.AddSingleton<MemoryStoreTrx>();
            })
            .Build();

        return host;
    }

    #region Add Operation Transaction Tests

    [Fact]
    public async Task GivenAdd_WhenCommit_ShouldPersistData()
    {
        using var host = BuildHost();
        var memoryStore = host.Services.GetRequiredService<MemoryStoreTrx>();
        var trxManager = host.Services.GetRequiredService<TransactionManager>();
        var context = host.Services.CreateContext<MemoryStoreTransactionTests>();

        // Register transaction provider
        trxManager.Register("memory", memoryStore);

        await trxManager.Start(context);

        const string path = "test/transaction/file1.txt";
        var data = new DataETag("test data".ToBytes());
        var addResult = memoryStore.Add(path, data, context);
        addResult.BeOk();

        await trxManager.Commit(context);

        // Verify data persists after commit
        memoryStore.Exist(path).BeTrue();
        var getResult = memoryStore.Get(path);
        getResult.BeOk();
        getResult.Return().Data.SequenceEqual(data.Data.ToArray()).BeTrue();
    }

    [Fact]
    public async Task GivenAdd_WhenRollback_ShouldRevertData()
    {
        using var host = BuildHost();
        var memoryStore = host.Services.GetRequiredService<MemoryStoreTrx>();
        var trxManager = host.Services.GetRequiredService<TransactionManager>();
        var context = host.Services.CreateContext<MemoryStoreTransactionTests>();

        trxManager.Register("memory", memoryStore);

        await trxManager.Start(context);

        const string path = "test/rollback/file1.txt";
        var data = new DataETag("test data".ToBytes());
        memoryStore.Add(path, data, context).BeOk();

        // Verify data exists before rollback
        memoryStore.Exist(path).BeTrue();

        await trxManager.Rollback(context);

        // Verify data is removed after rollback
        memoryStore.Exist(path).BeFalse();
    }

    [Fact]
    public async Task GivenMultipleAdds_WhenRollback_ShouldRevertAllInReverseOrder()
    {
        using var host = BuildHost();
        var memoryStore = host.Services.GetRequiredService<MemoryStoreTrx>();
        var trxManager = host.Services.GetRequiredService<TransactionManager>();
        var context = host.Services.CreateContext<MemoryStoreTransactionTests>();

        trxManager.Register("memory", memoryStore);

        await trxManager.Start(context);

        const string path1 = "test/rollback/file1.txt";
        const string path2 = "test/rollback/file2.txt";
        const string path3 = "test/rollback/file3.txt";

        memoryStore.Add(path1, new DataETag("data1".ToBytes()), context).BeOk();
        memoryStore.Add(path2, new DataETag("data2".ToBytes()), context).BeOk();
        memoryStore.Add(path3, new DataETag("data3".ToBytes()), context).BeOk();

        memoryStore.Exist(path1).BeTrue();
        memoryStore.Exist(path2).BeTrue();
        memoryStore.Exist(path3).BeTrue();

        await trxManager.Rollback(context);

        memoryStore.Exist(path1).BeFalse();
        memoryStore.Exist(path2).BeFalse();
        memoryStore.Exist(path3).BeFalse();
    }

    #endregion

    #region Update Operation Transaction Tests

    [Fact]
    public async Task GivenUpdate_WhenCommit_ShouldPersistChanges()
    {
        using var host = BuildHost();
        var memoryStore = host.Services.GetRequiredService<MemoryStoreTrx>();
        var trxManager = host.Services.GetRequiredService<TransactionManager>();
        var context = host.Services.CreateContext<MemoryStoreTransactionTests>();

        const string path = "test/update/file1.txt";
        var originalData = new DataETag("original data".ToBytes());

        // Add initial data outside transaction
        memoryStore.Add(path, originalData, context).BeOk();

        // Start transaction and update
        trxManager.Register("memory", memoryStore);
        await trxManager.Start(context);

        var updatedData = new DataETag("updated data".ToBytes());
        memoryStore.Set(path, updatedData, null, context).BeOk();

        await trxManager.Commit(context);

        // Verify updated data persists
        var result = memoryStore.Get(path);
        result.BeOk();
        result.Return().Data.SequenceEqual(updatedData.Data).BeTrue();
    }

    [Fact]
    public async Task GivenUpdate_WhenRollback_ShouldRevertToOriginal()
    {
        using var host = BuildHost();
        var memoryStore = host.Services.GetRequiredService<MemoryStoreTrx>();
        var trxManager = host.Services.GetRequiredService<TransactionManager>();
        var context = host.Services.CreateContext<MemoryStoreTransactionTests>();

        const string path = "test/update/rollback.txt";
        var originalData = new DataETag("original data".ToBytes());

        // Add initial data outside transaction
        memoryStore.Add(path, originalData, context).BeOk();

        // Start transaction and update
        trxManager.Register("memory", memoryStore);
        await trxManager.Start(context);

        var updatedData = new DataETag("updated data".ToBytes());
        memoryStore.Set(path, updatedData, null, context).BeOk();

        await trxManager.Rollback(context);

        // Verify original data is restored
        var result = memoryStore.Get(path);
        result.BeOk();
        result.Return().Data.SequenceEqual(originalData.Data).BeTrue();
    }

    #endregion

    #region Delete Operation Transaction Tests

    [Fact]
    public async Task GivenDelete_WhenCommit_ShouldRemoveData()
    {
        using var host = BuildHost();
        var memoryStore = host.Services.GetRequiredService<MemoryStoreTrx>();
        var trxManager = host.Services.GetRequiredService<TransactionManager>();
        var context = host.Services.CreateContext<MemoryStoreTransactionTests>();

        const string path = "test/delete/file1.txt";
        var data = new DataETag("data to delete".ToBytes());

        // Add initial data
        memoryStore.Add(path, data, context).BeOk();

        // Start transaction and delete
        trxManager.Register("memory", memoryStore);
        await trxManager.Start(context);

        memoryStore.Delete(path, null, context).BeOk();

        await trxManager.Commit(context);

        // Verify data is removed
        memoryStore.Exist(path).BeFalse();
    }

    [Fact]
    public async Task GivenDelete_WhenRollback_ShouldRestoreData()
    {
        using var host = BuildHost();
        var memoryStore = host.Services.GetRequiredService<MemoryStoreTrx>();
        var trxManager = host.Services.GetRequiredService<TransactionManager>();
        var context = host.Services.CreateContext<MemoryStoreTransactionTests>();

        const string path = "test/delete/rollback.txt";
        var originalData = new DataETag("data to restore".ToBytes());

        // Add initial data
        memoryStore.Add(path, originalData, context).BeOk();

        // Start transaction and delete
        trxManager.Register("memory", memoryStore);
        await trxManager.Start(context);

        memoryStore.Delete(path, null, context).BeOk();
        memoryStore.Exist(path).BeFalse();

        await trxManager.Rollback(context);

        // Verify data is restored
        memoryStore.Exist(path).BeTrue();
        var result = memoryStore.Get(path);
        result.BeOk();
        result.Return().Data.SequenceEqual(originalData.Data).BeTrue();
    }

    #endregion

    #region Mixed Operations Transaction Tests

    [Fact]
    public async Task GivenMixedOperations_WhenCommit_ShouldPersistAll()
    {
        using var host = BuildHost();
        var memoryStore = host.Services.GetRequiredService<MemoryStoreTrx>();
        var trxManager = host.Services.GetRequiredService<TransactionManager>();
        var context = host.Services.CreateContext<MemoryStoreTransactionTests>();

        const string path1 = "test/mixed/add.txt";
        const string path2 = "test/mixed/update.txt";
        const string path3 = "test/mixed/delete.txt";

        // Setup initial data for update and delete
        memoryStore.Add(path2, new DataETag("original".ToBytes()), context).BeOk();
        memoryStore.Add(path3, new DataETag("to delete".ToBytes()), context).BeOk();

        // Start transaction with mixed operations
        trxManager.Register("memory", memoryStore);
        await trxManager.Start(context);

        memoryStore.Add(path1, new DataETag("new data".ToBytes()), context).BeOk();
        memoryStore.Set(path2, new DataETag("updated".ToBytes()), null, context).BeOk();
        memoryStore.Delete(path3, null, context).BeOk();

        await trxManager.Commit(context);

        // Verify all operations persisted
        memoryStore.Exist(path1).BeTrue();
        memoryStore.Get(path2).Return().Data.SequenceEqual("updated".ToBytes()).BeTrue();
        memoryStore.Exist(path3).BeFalse();
    }

    [Fact]
    public async Task GivenMixedOperations_WhenRollback_ShouldRevertAll()
    {
        using var host = BuildHost();
        var memoryStore = host.Services.GetRequiredService<MemoryStoreTrx>();
        var trxManager = host.Services.GetRequiredService<TransactionManager>();
        var context = host.Services.CreateContext<MemoryStoreTransactionTests>();

        const string path1 = "test/mixed/add.txt";
        const string path2 = "test/mixed/update.txt";
        const string path3 = "test/mixed/delete.txt";

        // Setup initial data
        memoryStore.Add(path2, new DataETag("original".ToBytes()), context).BeOk();
        memoryStore.Add(path3, new DataETag("to delete".ToBytes()), context).BeOk();

        // Start transaction with mixed operations
        trxManager.Register("memory", memoryStore);
        await trxManager.Start(context);

        memoryStore.Add(path1, new DataETag("new data".ToBytes()), context).BeOk();
        memoryStore.Set(path2, new DataETag("updated".ToBytes()), null, context).BeOk();
        memoryStore.Delete(path3, null, context).BeOk();

        await trxManager.Rollback(context);

        // Verify all operations reverted
        memoryStore.Exist(path1).BeFalse();
        memoryStore.Get(path2).Return().Data.SequenceEqual("original".ToBytes()).BeTrue();
        memoryStore.Exist(path3).BeTrue();
    }

    #endregion

    #region Transaction with Lease Tests

    [Fact]
    public async Task GivenLeasedPath_WhenTransactionUpdate_ShouldFail()
    {
        using var host = BuildHost();
        var memoryStore = host.Services.GetRequiredService<MemoryStoreTrx>();
        var trxManager = host.Services.GetRequiredService<TransactionManager>();
        var context = host.Services.CreateContext<MemoryStoreTransactionTests>();

        const string path = "test/lease/file.txt";
        memoryStore.Add(path, new DataETag("data".ToBytes()), context).BeOk();

        // Acquire lease
        var leaseResult = memoryStore.AcquireLease(path, TimeSpan.FromMinutes(5), context);
        leaseResult.BeOk();

        // Start transaction and attempt update
        trxManager.Register("memory", memoryStore);
        await trxManager.Start(context);

        var setResult = memoryStore.Set(path, new DataETag("new data".ToBytes()), null, context);
        setResult.StatusCode.Be(StatusCode.Locked);

        await trxManager.Rollback(context);
    }

    [Fact]
    public async Task GivenLeasedPath_WhenTransactionWithLeaseId_ShouldSucceed()
    {
        using var host = BuildHost();
        var memoryStore = host.Services.GetRequiredService<MemoryStoreTrx>();
        var trxManager = host.Services.GetRequiredService<TransactionManager>();
        var context = host.Services.CreateContext<MemoryStoreTransactionTests>();

        const string path = "test/lease/update.txt";
        memoryStore.Add(path, new DataETag("data".ToBytes()), context).BeOk();

        // Acquire lease
        var leaseResult = memoryStore.AcquireLease(path, TimeSpan.FromMinutes(5), context);
        leaseResult.BeOk();
        var leaseId = leaseResult.Return().LeaseId;

        // Start transaction and update with lease ID
        trxManager.Register("memory", memoryStore);
        await trxManager.Start(context);

        var setResult = memoryStore.Set(path, new DataETag("new data".ToBytes()), leaseId, context);
        setResult.BeOk();

        await trxManager.Commit(context);

        memoryStore.Get(path).Return().Data.SequenceEqual("new data".ToBytes()).BeTrue();
    }

    #endregion

    #region Data Change Recorder Tests

    [Fact]
    public async Task GivenTransaction_WhenOperations_ShouldRecordChanges()
    {
        using var host = BuildHost();
        var memoryStore = host.Services.GetRequiredService<MemoryStoreTrx>();
        var trxManager = host.Services.GetRequiredService<TransactionManager>();
        var context = host.Services.CreateContext<MemoryStoreTransactionTests>();

        trxManager.Register("memory", memoryStore);
        await trxManager.Start(context);

        const string path = "test/recorder/file.txt";
        memoryStore.Add(path, new DataETag("data".ToBytes()), context).BeOk();

        // Access the recorder to verify it's set
        var recorder = memoryStore.DataChangeLog.GetRecorder();
        recorder.NotNull();

        await trxManager.Commit(context);
    }

    [Fact]
    public void GivenNoTransaction_WhenOperations_ShouldNotRecordChanges()
    {
        using var host = BuildHost();
        var memoryStore = host.Services.GetRequiredService<MemoryStoreTrx>();
        var context = host.Services.CreateContext<MemoryStoreTransactionTests>();

        const string path = "test/no-recorder/file.txt";
        memoryStore.Add(path, new DataETag("data".ToBytes()), context).BeOk();

        // Verify recorder is null when no transaction
        var recorder = memoryStore.DataChangeLog.GetRecorder();
        (recorder == null).BeTrue();
    }

    #endregion

    #region Journal Persistence Tests

    [Fact]
    public async Task GivenCommittedTransaction_ShouldWriteJournal()
    {
        using var host = BuildHost();
        var memoryStore = host.Services.GetRequiredService<MemoryStoreTrx>();
        var trxManager = host.Services.GetRequiredService<TransactionManager>();
        var journalStore = host.Services.GetRequiredService<IListStore<DataChangeRecord>>();
        var context = host.Services.CreateContext<MemoryStoreTransactionTests>();

        trxManager.Register("memory", memoryStore);
        await trxManager.Start(context);

        const string path = "test/journal/file.txt";
        memoryStore.Add(path, new DataETag("data".ToBytes()), context).BeOk();

        var transactionId = trxManager.TransactionId;
        await trxManager.Commit(context);

        // Verify journal entry exists
        var journalResult = await journalStore.Get("transaction_journal", context);
        journalResult.BeOk();
        var journals = journalResult.Return();

        var matchingJournal = journals.FirstOrDefault(j => j.TransactionId == transactionId);
        matchingJournal.NotNull();
        matchingJournal.Entries.Count.Be(1);
    }

    #endregion

    #region Edge Cases Tests

    [Fact]
    public async Task GivenEmptyTransaction_WhenCommit_ShouldSucceed()
    {
        using var host = BuildHost();
        var memoryStore = host.Services.GetRequiredService<MemoryStoreTrx>();
        var trxManager = host.Services.GetRequiredService<TransactionManager>();
        var context = host.Services.CreateContext<MemoryStoreTransactionTests>();

        trxManager.Register("memory", memoryStore);
        await trxManager.Start(context);

        var result = await trxManager.Commit(context);
        result.BeOk();
    }

    [Fact]
    public async Task GivenSetOnNonExistentPath_WhenRollback_ShouldRemove()
    {
        using var host = BuildHost();
        var memoryStore = host.Services.GetRequiredService<MemoryStoreTrx>();
        var trxManager = host.Services.GetRequiredService<TransactionManager>();
        var context = host.Services.CreateContext<MemoryStoreTransactionTests>();

        const string path = "test/edge/nonexistent.txt";

        trxManager.Register("memory", memoryStore);
        await trxManager.Start(context);

        // Set creates a new entry if path doesn't exist
        memoryStore.Set(path, new DataETag("data".ToBytes()), null, context).BeOk();
        memoryStore.Exist(path).BeTrue();

        await trxManager.Rollback(context);

        // Should be removed after rollback
        memoryStore.Exist(path).BeFalse();
    }

    [Fact]
    public async Task GivenMultipleTransactions_WhenSequential_ShouldSucceed()
    {
        using var host = BuildHost();
        var memoryStore = host.Services.GetRequiredService<MemoryStoreTrx>();
        var trxManager = host.Services.GetRequiredService<TransactionManager>();
        var context = host.Services.CreateContext<MemoryStoreTransactionTests>();

        trxManager.Register("memory", memoryStore);

        // First transaction
        await trxManager.Start(context);
        memoryStore.Add("test/seq/file1.txt", new DataETag("data1".ToBytes()), context).BeOk();
        await trxManager.Commit(context);

        // Second transaction
        await trxManager.Start(context);
        memoryStore.Add("test/seq/file2.txt", new DataETag("data2".ToBytes()), context).BeOk();
        await trxManager.Commit(context);

        // Verify both exist
        memoryStore.Exist("test/seq/file1.txt").BeTrue();
        memoryStore.Exist("test/seq/file2.txt").BeTrue();
    }

    #endregion
}
