//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Hosting;
//using Microsoft.Extensions.Logging;
//using Toolbox.Data;
//using Toolbox.Extensions;
//using Toolbox.Store;
//using Toolbox.Tools;
//using Toolbox.Types;
//using Xunit.Abstractions;

//namespace Toolbox.Test.Data.IndexedCollection;

//public class IndexedCollectionTransactionTests
//{
//    private record TestRec
//    {
//        public int Id { get; init; }
//        public string Name { get; init; } = string.Empty;
//    }

//    private readonly ITestOutputHelper _outputHelper;

//    public IndexedCollectionTransactionTests(ITestOutputHelper output) => _outputHelper = output.NotNull();
//    protected virtual void AddStore(IServiceCollection services) => services.AddInMemoryFileStore();

//    public async Task<IHost> BuildService()
//    {
//        var option = new TransactionManagerOption
//        {
//            JournalKey = "transaction_journal"
//        };

//        var host = Host.CreateDefaultBuilder()
//        .ConfigureServices((context, services) =>
//        {
//            services.AddLogging(config => config.AddLambda(_outputHelper.WriteLine).AddDebug().AddFilter(x => true));
//            services.AddInMemoryFileStore();
//            services.AddListStore<DataChangeRecord>();
//            services.AddTransactionServices(option);
//        })
//        .Build();

//        await host.ClearStore<IndexedCollectionTransactionTests>();
//        return host;
//    }

//    [Fact]
//    public async Task EmptyTransaction()
//    {
//        var host = await BuildService();
//        var context = host.Services.CreateContext<IndexedCollectionTransactionTests>();

//        var c = new IndexedCollection<int, TestRec>(x => x.Id);

//        var trxMgr = host.Services.GetRequiredService<TransactionManager>()
//            .Register("indexedCollection", c);

//        await trxMgr.Start(context);

//        await trxMgr.Commit(context);

//        var fileList = host.Services.GetRequiredService<IListStore<DataChangeRecord>>();
//        var result = await fileList.Get("transaction_journal", context);
//        result.BeOk();
//        result.Return().Action(x =>
//        {
//            x.Count.Be(1);
//            x[0].Entries.Count.Be(0);
//        });
//    }

//    [Fact]
//    public async Task SimpleTransaction()
//    {
//        var host = await BuildService();
//        var context = host.Services.CreateContext<IndexedCollectionTransactionTests>();

//        var c = new IndexedCollection<int, TestRec>(x => x.Id);

//        var trxMgr = host.Services.GetRequiredService<TransactionManager>()
//            .Register("indexedCollection", c);

//        await trxMgr.Start(context);

//        c.TryAdd(new TestRec { Id = 1, Name = "A" }).BeTrue();
//        c.TryAdd(new TestRec { Id = 2, Name = "B" }).BeTrue();

//        c.TryGetValue(1, out var _).BeTrue();
//        c.TryGetValue(2, out var _).BeTrue();

//        await trxMgr.Commit(context);

//        c.TryGetValue(1, out var _).BeTrue();
//        c.TryGetValue(2, out var _).BeTrue();

//        var fileList = host.Services.GetRequiredService<IListStore<DataChangeRecord>>();
//        var result = await fileList.Get("transaction_journal", context);
//        result.BeOk();
//        result.Return().Action(x =>
//        {
//            x.Count.Be(1);
//            x[0].Action(y =>
//            {
//                y.Entries.Count.Be(2);
//                y.Entries.Count(e => e.SourceName == "indexedCollection").Be(2);
//                y.Entries.Select(x => x.ObjectId).SequenceEqual(["1", "2"]).BeTrue();
//            });
//        });
//    }

//    [Fact]
//    public async Task SimpleRollbackTransaction()
//    {
//        var host = await BuildService();
//        var context = host.Services.CreateContext<IndexedCollectionTransactionTests>();

//        var c = new IndexedCollection<int, TestRec>(x => x.Id);
//        c.TryAdd(new TestRec { Id = 1, Name = "A" }).BeTrue();
//        c.TryAdd(new TestRec { Id = 2, Name = "B" }).BeTrue();

//        // State before transaction
//        c.Count.Be(2);
//        c.TryGetValue(1, out var _).BeTrue();
//        c.TryGetValue(2, out var _).BeTrue();

//        var trxMgr = host.Services.GetRequiredService<TransactionManager>()
//            .Register("indexedCollection", c);

//        await trxMgr.Start(context);

//        c.TryAdd(new TestRec { Id = 3, Name = "C" }).BeTrue();
//        c.TryAdd(new TestRec { Id = 4, Name = "D" }).BeTrue();

//        c.TryGetValue(1, out var _).BeTrue();
//        c.TryGetValue(2, out var _).BeTrue();
//        c.TryGetValue(3, out var _).BeTrue();
//        c.TryGetValue(4, out var _).BeTrue();

//        c.Count.Be(4);
//        c.Keys.IsEquivalent([1, 2, 3, 4]).BeTrue();

//        await trxMgr.Rollback(context);

//        c.Count.Be(2);
//        c.Keys.IsEquivalent([1, 2]).BeTrue();

//        // No logs, because transaction was rolled back
//        var fileList = host.Services.GetRequiredService<IListStore<DataChangeRecord>>();
//        var result = await fileList.Get("transaction_journal", context);
//        result.BeOk();
//        result.Return().Action(x =>
//        {
//            x.Count.Be(0);
//        });
//    }

//    [Fact]
//    public async Task Commit_AddUpdateDeleteTransaction()
//    {
//        using var host = await BuildService();
//        var context = host.Services.CreateContext<IndexedCollectionTransactionTests>();

//        var c = new IndexedCollection<int, TestRec>(x => x.Id);
//        var trxMgr = host.Services.GetRequiredService<TransactionManager>()
//            .Register("indexedCollection", c);

//        await trxMgr.Start(context);

//        var a = new TestRec { Id = 1, Name = "Alpha" };
//        var b = new TestRec { Id = 2, Name = "Beta" };
//        c.TryAdd(a).BeTrue();    // Add
//        c.TryAdd(b).BeTrue();    // Add

//        var b2 = new TestRec { Id = 2, Name = "Beta2" };
//        c.TryUpdate(b2, b).BeTrue(); // Update
//        c.TryRemove(1, out var _).BeTrue(); // Delete

//        await trxMgr.Commit(context);

//        // State after commit
//        c.Count.Be(1);
//        c.TryGetValue(2, out var vb).BeTrue();
//        vb!.Name.Be("Beta2");

//        // Journal
//        var fileList = host.Services.GetRequiredService<IListStore<DataChangeRecord>>();
//        var read = await fileList.Get("transaction_journal", context);
//        read.BeOk();

//        var record = read.Return().Single();
//        record.Entries.Count.Be(4); // Add 1, Add 2, Update 2, Delete 1

//        var add1 = record.Entries.First(x => x.ObjectId == "1" && x.Action == ChangeOperation.Add);
//        add1.Before.BeNull();
//        add1.After.NotNull();

//        var add2 = record.Entries.First(x => x.ObjectId == "2" && x.Action == ChangeOperation.Add);
//        var upd2 = record.Entries.First(x => x.ObjectId == "2" && x.Action == ChangeOperation.Update);
//        upd2.Before.NotNull();
//        upd2.After.NotNull();
//        upd2.Before!.Value.ETag.NotEmpty();
//        upd2.After!.Value.ETag.NotEmpty();

//        var del1 = record.Entries.First(x => x.ObjectId == "1" && x.Action == ChangeOperation.Delete);
//        del1.Before.NotNull(); del1.After.BeNull();
//    }

//    [Fact]
//    public async Task Commit_SetIndexer_AddAndThenUpdate()
//    {
//        using var host = await BuildService();
//        var context = host.Services.CreateContext<IndexedCollectionTransactionTests>();

//        var c = new IndexedCollection<int, TestRec>(x => x.Id);
//        var trxMgr = host.Services.GetRequiredService<TransactionManager>()
//            .Register("indexedCollection", c);

//        await trxMgr.Start(context);

//        c[10] = new TestRec { Id = 10, Name = "First" }; // Add via indexer
//        c[10] = new TestRec { Id = 10, Name = "Second" }; // Update via indexer

//        await trxMgr.Commit(context);

//        c.Count.Be(1);
//        c[10].Name.Be("Second");

//        var store = host.Services.GetRequiredService<IListStore<DataChangeRecord>>();
//        var read = await store.Get("transaction_journal", context);
//        read.BeOk();
//        var entries = read.Return().Single().Entries;
//        entries.Count.Be(2);
//        entries.Count(x => x.Action == ChangeOperation.Add).Be(1);
//        entries.Count(x => x.Action == ChangeOperation.Update).Be(1);
//    }

//    [Fact]
//    public async Task Rollback_Update_RevertsValue()
//    {
//        using var host = await BuildService();
//        var context = host.Services.CreateContext<IndexedCollectionTransactionTests>();

//        var c = new IndexedCollection<int, TestRec>(x => x.Id);
//        c.TryAdd(new TestRec { Id = 5, Name = "Original" }).BeTrue();

//        var trxMgr = host.Services.GetRequiredService<TransactionManager>()
//            .Register("indexedCollection", c);

//        await trxMgr.Start(context);

//        var updated = new TestRec { Id = 5, Name = "Changed" };
//        c.TryUpdate(updated, new TestRec { Id = 5, Name = "Original" }).BeTrue();

//        c[5].Name.Be("Changed");

//        await trxMgr.Rollback(context);

//        c[5].Name.Be("Original");

//        var store = host.Services.GetRequiredService<IListStore<DataChangeRecord>>();
//        (await store.Get("transaction_journal", context)).Return().Count.Be(0);
//    }

//    [Fact]
//    public async Task Rollback_Delete_RestoresDeletedItem()
//    {
//        using var host = await BuildService();
//        var context = host.Services.CreateContext<IndexedCollectionTransactionTests>();

//        var c = new IndexedCollection<int, TestRec>(x => x.Id);
//        var rec = new TestRec { Id = 7, Name = "KeepMe" };
//        c.TryAdd(rec).BeTrue();

//        var trxMgr = host.Services.GetRequiredService<TransactionManager>()
//            .Register("indexedCollection", c);

//        await trxMgr.Start(context);

//        c.TryRemove(7, out var _).BeTrue();
//        c.ContainsKey(7).BeFalse();

//        await trxMgr.Rollback(context);

//        c.ContainsKey(7).BeTrue();
//        c[7].Name.Be("KeepMe");
//    }

//    [Fact]
//    public async Task Rollback_MixedOperations()
//    {
//        using var host = await BuildService();
//        var context = host.Services.CreateContext<IndexedCollectionTransactionTests>();

//        var c = new IndexedCollection<int, TestRec>(x => x.Id);
//        c.TryAdd(new TestRec { Id = 1, Name = "A" }).BeTrue(); // Pre-existing

//        var trxMgr = host.Services.GetRequiredService<TransactionManager>()
//            .Register("indexedCollection", c);

//        await trxMgr.Start(context);

//        c.TryAdd(new TestRec { Id = 2, Name = "B" }).BeTrue(); // Add -> should be removed
//        c.TryUpdate(new TestRec { Id = 1, Name = "A2" }, new TestRec { Id = 1, Name = "A" }).BeTrue(); // Update -> revert
//        c.TryAdd(new TestRec { Id = 3, Name = "C" }).BeTrue();
//        c.TryRemove(3, out var _).BeTrue(); // Delete after Add -> sequence: Add then Delete (rollback should restore? Actually Add then Delete net zero -> provider rollback: Delete restores Before which was original Add's Before=null -> sets Before value? For Add then Delete entries, rollback executes in reverse: Delete -> restore Before (null) => Set(null?) => provider logic sets Before object on Delete; here Before is the item value. Works.)

//        await trxMgr.Rollback(context);

//        c.Keys.OrderBy(x => x).SequenceEqual(new[] { 1 }).BeTrue();
//        c[1].Name.Be("A");
//    }

//    [Fact]
//    public async Task GetOrAdd_DoesNotDuplicateExisting_InTransaction()
//    {
//        using var host = await BuildService();
//        var context = host.Services.CreateContext<IndexedCollectionTransactionTests>();

//        var c = new IndexedCollection<int, TestRec>(x => x.Id);
//        var trxMgr = host.Services.GetRequiredService<TransactionManager>()
//            .Register("indexedCollection", c);

//        await trxMgr.Start(context);

//        var first = new TestRec { Id = 100, Name = "First" };
//        c.GetOrAdd(first); // Add

//        var second = new TestRec { Id = 100, Name = "Second" };
//        c.GetOrAdd(second); // Should not create new journal entry

//        await trxMgr.Commit(context);

//        c.Count.Be(1);
//        c[100].Name.Be("First");

//        var store = host.Services.GetRequiredService<IListStore<DataChangeRecord>>();
//        var record = (await store.Get("transaction_journal", context)).Return().Single();
//        record.Entries.Count.Be(1);
//        record.Entries[0].Action.Be(ChangeOperation.Add);
//        record.Entries[0].ObjectId.Be("100");
//    }

//    [Fact]
//    public async Task MultipleCollections_Commit_JournalContainsBothSources()
//    {
//        using var host = await BuildService();
//        var context = host.Services.CreateContext<IndexedCollectionTransactionTests>();

//        var c1 = new IndexedCollection<int, TestRec>(x => x.Id);
//        var c2 = new IndexedCollection<int, TestRec>(x => x.Id);

//        var trxMgr = host.Services.GetRequiredService<TransactionManager>()
//            .Register("collectionA", c1)
//            .Register("collectionB", c2);

//        await trxMgr.Start(context);

//        c1.TryAdd(new TestRec { Id = 1, Name = "A1" }).BeTrue();
//        c2.TryAdd(new TestRec { Id = 2, Name = "B2" }).BeTrue();

//        await trxMgr.Commit(context);

//        var store = host.Services.GetRequiredService<IListStore<DataChangeRecord>>();
//        var record = (await store.Get("transaction_journal", context)).Return().Single();

//        record.Entries.Count.Be(2);
//        record.Entries.Count(x => x.SourceName == "collectionA").Be(1);
//        record.Entries.Count(x => x.SourceName == "collectionB").Be(1);
//    }

//    [Fact]
//    public async Task SequenceNumber_Order_Preserved()
//    {
//        using var host = await BuildService();
//        var context = host.Services.CreateContext<IndexedCollectionTransactionTests>();

//        var c = new IndexedCollection<int, TestRec>(x => x.Id);
//        var trxMgr = host.Services.GetRequiredService<TransactionManager>()
//            .Register("indexedCollection", c);

//        await trxMgr.Start(context);

//        for (int i = 1; i <= 5; i++) c.TryAdd(new TestRec { Id = i, Name = $"N{i}" }).BeTrue();

//        await trxMgr.Commit(context);

//        var store = host.Services.GetRequiredService<IListStore<DataChangeRecord>>();
//        var record = (await store.Get("transaction_journal", context)).Return().Single();
//        var seq = record.Entries.Select(x => x.LogSequenceNumber).ToArray();
//        seq.Length.Be(5);
//        seq.SequenceEqual(seq.OrderBy(x => x)).BeTrue();
//        seq.Distinct().Count().Be(5);
//    }

//    [Fact]
//    public async Task UpdateRollback_SecondaryIndexRetainsNewKey_CurrentBehavior()
//    {
//        using var host = await BuildService();
//        var context = host.Services.CreateContext<IndexedCollectionTransactionTests>();

//        var c = new IndexedCollection<int, TestRec>(x => x.Id);
//        var unique = c.SecondaryIndexes.CreateUniqueIndex("byName", x => x.Name, StringComparer.OrdinalIgnoreCase).BeOk().Return();

//        c.TryAdd(new TestRec { Id = 1, Name = "Alpha" }).BeTrue();

//        var trxMgr = host.Services.GetRequiredService<TransactionManager>()
//            .Register("indexedCollection", c);

//        await trxMgr.Start(context);

//        c.TryUpdate(new TestRec { Id = 1, Name = "Beta" }, new TestRec { Id = 1, Name = "Alpha" }).BeTrue();
//        unique.TryGetValue("beta", out var vBeta).BeTrue();

//        await trxMgr.Rollback(context);

//        // Value reverted
//        c[1].Name.Be("Alpha");

//        // Current implementation: secondary index still has "Beta"
//        unique.TryGetValue("beta", out var stillBeta).BeTrue();

//        // Original key also exists
//        unique.TryGetValue("alpha", out var vAlpha).BeTrue();

//        // This documents existing behavior; if design changes to purge new keys on rollback, adjust expectations.
//    }

//    [Fact]
//    public async Task Rollback_Update_NoJournal()
//    {
//        var host = await BuildService();
//        var context = host.Services.CreateContext<IndexedCollectionTransactionTests>();

//        var c = new IndexedCollection<int, TestRec>(x => x.Id);
//        c.TryAdd(new TestRec { Id = 1, Name = "Alpha" }).BeTrue();

//        var trxMgr = host.Services.GetRequiredService<TransactionManager>()
//            .Register("indexedCollection", c);

//        await trxMgr.Start(context);

//        c.TryUpdate(new TestRec { Id = 1, Name = "Beta" }, new TestRec { Id = 1, Name = "Alpha" }).BeTrue();

//        await trxMgr.Rollback(context);

//        c[1].Name.Be("Alpha");

//        var store = host.Services.GetRequiredService<IListStore<DataChangeRecord>>();
//        (await store.Get("transaction_journal", context)).Return().Count.Be(0);
//    }

//    [Fact]
//    public async Task Rollback_Delete_NoJournal()
//    {
//        var host = await BuildService();
//        var context = host.Services.CreateContext<IndexedCollectionTransactionTests>();

//        var c = new IndexedCollection<int, TestRec>(x => x.Id);
//        c.TryAdd(new TestRec { Id = 2, Name = "ToDelete" }).BeTrue();

//        var trxMgr = host.Services.GetRequiredService<TransactionManager>()
//            .Register("indexedCollection", c);

//        await trxMgr.Start(context);

//        c.TryRemove(2, out _).BeTrue();
//        c.ContainsKey(2).BeFalse();

//        await trxMgr.Rollback(context);

//        c.ContainsKey(2).BeTrue();

//        var store = host.Services.GetRequiredService<IListStore<DataChangeRecord>>();
//        (await store.Get("transaction_journal", context)).Return().Count.Be(0);
//    }

//    [Fact]
//    public async Task TransactionCommit_AllowsAdditionalEntriesButFailsWhenStarted()
//    {
//        using var host = await BuildService();
//        var context = host.Services.CreateContext<IndexedCollectionTransactionTests>();

//        var c = new IndexedCollection<int, TestRec>(x => x.Id);

//        // Start first transaction
//        var trxMgr1 = host.Services.GetRequiredService<TransactionManager>()
//            .Register("indexedCollection", c);

//        await trxMgr1.Start(context);

//        c.TryAdd(new TestRec { Id = 1, Name = "A" }).BeTrue();
//        await trxMgr1.Commit(context);

//        // Collection remains usable after commit; recorder cleared so mutations are not recorded by trxMgr1.
//        c.TryAdd(new TestRec { Id = 2, Name = "B" }).BeTrue();
//        c.ContainsKey(1).BeTrue();
//        c.ContainsKey(2).BeTrue();

//        // Reusing the same manager for another commit should work.
//        await Verify.ThrowsAsync<ArgumentException>(async () => await trxMgr1.Commit(context));
//        await Verify.ThrowsAsync<ArgumentException>(async () => await trxMgr1.Rollback(context));

//        // Start a new transaction manager instance and register recorder again
//        await Verify.ThrowsAsync<ArgumentException>(async () => await trxMgr1.Start(context));
//    }

//    [Fact]
//    public async Task TransactionFinalization_AllowsNewTransactions()
//    {
//        using var host = await BuildService();
//        var context = host.Services.CreateContext<IndexedCollectionTransactionTests>();

//        var c = new IndexedCollection<int, TestRec>(x => x.Id);

//        // Start first transaction
//        var trxMgr1 = host.Services.GetRequiredService<TransactionManager>()
//            .Register("indexedCollection", c);

//        await trxMgr1.Start(context);

//        c.TryAdd(new TestRec { Id = 1, Name = "A" }).BeTrue();
//        await trxMgr1.Commit(context);

//        c.ContainsKey(1).BeTrue();

//        // Reusing the same manager for another commit should work.
//        await Verify.ThrowsAsync<ArgumentException>(async () => await trxMgr1.Commit(context));
//        await Verify.ThrowsAsync<ArgumentException>(async () => await trxMgr1.Rollback(context));

//        // Start a new transaction manager instance and register recorder again
//        await trxMgr1.Start(context);

//        c.TryAdd(new TestRec { Id = 2, Name = "B" }).BeTrue();
//        c.ContainsKey(1).BeTrue();
//        c.ContainsKey(2).BeTrue();

//        // No new mutations; commit should capture changes since recorder was set (i.e., item 2 is not captured earlier, so it won't be in trx1; only trx2 will capture operations done after registration)
//        await trxMgr1.Commit(context);

//        // Verify journal has two records (one per transaction)
//        var store = host.Services.GetRequiredService<IListStore<DataChangeRecord>>();
//        var records = (await store.Get("transaction_journal", context)).Return();

//        records.Count.Be(2);
//        records[0].Entries.Count.Be(1);
//        records[1].Entries.Count.Be(1);

//        // Check data
//        records
//            .SelectMany(x => x.Entries)
//            .Select(e => (e.ObjectId, e.Action))
//            .SequenceEqual([("1", ChangeOperation.Add), ("2", ChangeOperation.Add)])
//            .BeTrue();
//    }

//    [Fact]
//    public async Task MultipleTransactions_RollbackThenCommit_JournalReflectsOnlyCommittedTxn()
//    {
//        using var host = await BuildService();
//        var context = host.Services.CreateContext<IndexedCollectionTransactionTests>();
//        var c = new IndexedCollection<int, TestRec>(x => x.Id);

//        // Txn 1: rollback
//        var tm1 = host.Services.GetRequiredService<TransactionManager>()
//            .Register("indexedCollection", c);

//        await tm1.Start(context);

//        c.TryAdd(new TestRec { Id = 20, Name = "RollbackMe" }).BeTrue();
//        await tm1.Rollback(context);

//        c.ContainsKey(20).BeFalse();

//        await tm1.Start(context);

//        c.TryAdd(new TestRec { Id = 21, Name = "CommitMe" }).BeTrue();
//        await tm1.Commit(context);

//        var store = host.Services.GetRequiredService<IListStore<DataChangeRecord>>();
//        var records = (await store.Get("transaction_journal", context)).Return();

//        records.Count.Be(1);
//        records[0].Entries.Select(e => (e.ObjectId, e.Action)).SequenceEqual(new[] { ("21", ChangeOperation.Add) }).BeTrue();

//        c.Keys.SequenceEqual(new[] { 21 }).BeTrue();
//    }

//    [Fact]
//    public async Task MultipleTransactions_SequentialCommits_AppendTwoJournalRecords()
//    {
//        using var host = await BuildService();
//        var context = host.Services.CreateContext<IndexedCollectionTransactionTests>();
//        var c = new IndexedCollection<int, TestRec>(x => x.Id);

//        // Txn 1
//        var tm1 = host.Services.GetRequiredService<TransactionManager>()
//            .Register("indexedCollection", c);

//        await tm1.Start(context);

//        c.TryAdd(new TestRec { Id = 10, Name = "T1" }).BeTrue();
//        await tm1.Commit(context);

//        await tm1.Start(context);

//        c.TryAdd(new TestRec { Id = 11, Name = "T2" }).BeTrue();
//        await tm1.Commit(context);

//        // Verify journal has both transactions appended
//        var store = host.Services.GetRequiredService<IListStore<DataChangeRecord>>();
//        var records = (await store.Get("transaction_journal", context)).Return();

//        records.Count.Be(2);
//        records[0].Entries.Select(e => (e.ObjectId, e.Action)).SequenceEqual(new[] { ("10", ChangeOperation.Add) }).BeTrue();
//        records[1].Entries.Select(e => (e.ObjectId, e.Action)).SequenceEqual(new[] { ("11", ChangeOperation.Add) }).BeTrue();

//        // Collection contains both items
//        c.Keys.OrderBy(x => x).SequenceEqual(new[] { 10, 11 }).BeTrue();
//    }

//    [Fact]
//    public async Task Start_TransitionsToTransactionState()
//    {
//        var host = await BuildService();
//        var context = host.Services.CreateContext<IndexedCollectionTransactionTests>();
//        var c = new IndexedCollection<int, TestRec>(x => x.Id);

//        var trxMgr = host.Services.GetRequiredService<TransactionManager>()
//            .Register("indexedCollection", c);

//        var result = await trxMgr.Start(context);
//        result.BeOk();
//    }

//    [Fact]
//    public async Task Start_CalledTwice_ThrowsInvalidOperation()
//    {
//        var host = await BuildService();
//        var context = host.Services.CreateContext<IndexedCollectionTransactionTests>();
//        var c = new IndexedCollection<int, TestRec>(x => x.Id);

//        var trxMgr = host.Services.GetRequiredService<TransactionManager>()
//            .Register("indexedCollection", c);

//        await trxMgr.Start(context);
//        await Verify.ThrowsAsync<ArgumentException>(async () => await trxMgr.Start(context));
//    }

//    [Fact]
//    public async Task Commit_WithoutStart_Throws()
//    {
//        var host = await BuildService();
//        var context = host.Services.CreateContext<IndexedCollectionTransactionTests>();
//        var c = new IndexedCollection<int, TestRec>(x => x.Id);

//        var trxMgr = host.Services.GetRequiredService<TransactionManager>()
//            .Register("indexedCollection", c);

//        await Verify.ThrowsAsync<ArgumentException>(async () => await trxMgr.Commit(context));
//    }

//    [Fact]
//    public async Task Register_DuringTransaction_Throws()
//    {
//        var host = await BuildService();
//        var context = host.Services.CreateContext<IndexedCollectionTransactionTests>();
//        var c1 = new IndexedCollection<int, TestRec>(x => x.Id);
//        var c2 = new IndexedCollection<int, TestRec>(x => x.Id);

//        var trxMgr = host.Services.GetRequiredService<TransactionManager>();
//        trxMgr.Register("c1", c1);
//        await trxMgr.Start(context);

//        Verify.Throws<ArgumentException>(() => trxMgr.Register("c2", c2));
//    }

//    [Fact]
//    public async Task TransactionId_ChangesAfterEachStart()
//    {
//        var host = await BuildService();
//        var context = host.Services.CreateContext<IndexedCollectionTransactionTests>();
//        var c = new IndexedCollection<int, TestRec>(x => x.Id);

//        var trxMgr1 = host.Services.GetRequiredService<TransactionManager>()
//            .Register("c", c);

//        await trxMgr1.Start(context);
//        var id1 = trxMgr1.TransactionId;
//        await trxMgr1.Commit(context);

//        await trxMgr1.Start(context);
//        var id2 = trxMgr1.TransactionId;
//        await trxMgr1.Commit(context);

//        id1.NotBe(id2);
//    }
//}
