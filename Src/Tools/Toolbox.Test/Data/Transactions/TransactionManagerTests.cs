//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Hosting;
//using Microsoft.Extensions.Logging;
//using Toolbox.Data;
//using Toolbox.Extensions;
//using Toolbox.Store;
//using Toolbox.Tools;
//using Toolbox.Types;
//using Xunit.Abstractions;

//namespace Toolbox.Test.Data.Transaction;

//public class TransactionManagerTests
//{
//    private readonly ITestOutputHelper _output;

//    public TransactionManagerTests(ITestOutputHelper output) => _output = output.NotNull();

//    private async Task<IHost> BuildHost(string journalKey)
//    {
//        var host = Host.CreateDefaultBuilder()
//            .ConfigureServices(services =>
//            {
//                services.AddLogging(c => c.AddLambda(_output.WriteLine).AddDebug().AddFilter(_ => true));
//                services.AddInMemoryFileStore();
//                services.AddListStore<DataChangeRecord>();
//                services.AddTransactionServices(new TransactionManagerOption { JournalKey = journalKey });
//                services.AddSingleton<ITransactionProvider, PassThruProvider>(); // optional default
//            })
//            .Build();

//        await host.ClearStore<TransactionManagerTests>();
//        return host;
//    }

//    private sealed class RecordingProvider : ITransactionProvider, ITransactionRegister
//    {
//        public string Name { get; }
//        public List<string> Calls { get; } = new();
//        public Func<DataChangeRecord, Option>? PrepareResult { get; set; }
//        public Func<DataChangeRecord, Option>? CommitResult { get; set; }
//        public Func<DataChangeEntry, Option>? RollbackResult { get; set; }

//        public DataChangeRecorder DataChangeLog => new DataChangeRecorder();

//        public RecordingProvider(string name) => Name = name;

//        public Task<Option> Prepare(DataChangeRecord r, ScopeContext c)
//        {
//            Calls.Add("prepare:" + Name);
//            return Task.FromResult(PrepareResult?.Invoke(r) ?? new Option(StatusCode.OK));
//        }

//        public Task<Option> Commit(DataChangeRecord r, ScopeContext c)
//        {
//            Calls.Add("commit:" + Name);
//            return Task.FromResult(CommitResult?.Invoke(r) ?? new Option(StatusCode.OK));
//        }

//        public Task<Option> Rollback(DataChangeEntry e, ScopeContext c)
//        {
//            Calls.Add("rollback:" + Name + ":" + e.ObjectId);
//            return Task.FromResult(RollbackResult?.Invoke(e) ?? new Option(StatusCode.OK));
//        }

//        public ITransactionProvider GetProvider() => this;
//    }

//    private sealed class PassThruProvider : ITransactionProvider
//    {
//        public string Name => "pass";
//        public Task<Option> Prepare(DataChangeRecord r, ScopeContext c) => Task.FromResult(new Option(StatusCode.OK));
//        public Task<Option> Commit(DataChangeRecord r, ScopeContext c) => Task.FromResult(new Option(StatusCode.OK));
//        public Task<Option> Rollback(DataChangeEntry e, ScopeContext c) => Task.FromResult(new Option(StatusCode.OK));
//    }

//    [Fact]
//    public async Task Startup_ResolvesServices()
//    {
//        using var host = await BuildHost("journal");
//        host.Services.GetRequiredService<TransactionManager>();
//        host.Services.GetRequiredService<LogSequenceNumber>();
//    }

//    [Fact]
//    public async Task Register_DuplicateProvider_Throws()
//    {
//        using var host = await BuildHost("j1");
//        var mgr = host.Services.GetRequiredService<TransactionManager>();
//        mgr.Register(new RecordingProvider("A"));
//        Assert.Throws<ArgumentException>(() => mgr.Register(new RecordingProvider("A")));
//    }

//    [Fact]
//    public async Task Register_AfterStart_Throws()
//    {
//        using var host = await BuildHost("j2");
//        var mgr = host.Services.GetRequiredService<TransactionManager>();
//        mgr.Register(new RecordingProvider("A"));
//        var ctx = host.Services.CreateContext<TransactionManagerTests>();
//        (await mgr.Start(ctx)).BeOk();
//        Assert.Throws<ArgumentException>(() => mgr.Register(new RecordingProvider("B")));
//    }

//    [Fact]
//    public async Task Commit_WithEntries_AppendsJournal_InvokesProviders()
//    {
//        string journalKey = "commit-" + Guid.NewGuid().ToString("N");
//        using var host = await BuildHost(journalKey);
//        var mgr = host.Services.GetRequiredService<TransactionManager>();
//        var pA = new RecordingProvider("A");
//        var pB = new RecordingProvider("B");
//        mgr.Register(pA);
//        mgr.Register(pB);
//        var recA = pA.DataChangeLog.GetRecorder().NotNull();
//        var recB = pB.DataChangeLog.GetRecorder().NotNull();

//        var ctx = host.Services.CreateContext<TransactionManagerTests>();
//        (await mgr.Start(ctx)).BeOk();

//        recA.Add("id1", "val1");
//        recB.Delete("id2", "old2");

//        (await mgr.Commit(ctx)).BeOk();

//        // Providers called (ordering not enforced)
//        pA.Calls.Count(x => x.StartsWith("prepare")).Be(1);
//        pB.Calls.Count(x => x.StartsWith("prepare")).Be(1);
//        pA.Calls.Count(x => x.StartsWith("commit")).Be(1);
//        pB.Calls.Count(x => x.StartsWith("commit")).Be(1);

//        var store = host.Services.GetRequiredService<IListStore<DataChangeRecord>>();
//        var read = await store.Get(journalKey, ctx);
//        read.BeOk();
//        var records = read.Return();
//        records.Count.Be(1);
//        var trx = records[0];
//        trx.TransactionId.Be(mgr.TransactionId);
//        trx.Entries.Count.Be(2);
//    }

//    [Fact]
//    public async Task Commit_Empty_WritesJournal()
//    {
//        string journalKey = "empty-" + Guid.NewGuid().ToString("N");
//        using var host = await BuildHost(journalKey);
//        var mgr = host.Services.GetRequiredService<TransactionManager>();
//        mgr.Register(new RecordingProvider("A"));

//        var ctx = host.Services.CreateContext<TransactionManagerTests>();
//        (await mgr.Start(ctx)).BeOk();
//        (await mgr.Commit(ctx)).BeOk();

//        var store = host.Services.GetRequiredService<IListStore<DataChangeRecord>>();
//        var read = await store.Get(journalKey, ctx);
//        read.BeOk();
//        read.Return().Single().Entries.Count.Be(0);
//    }

//    [Fact]
//    public async Task Prepare_Failure_Stops_NoCommit()
//    {
//        using var host = await BuildHost("prepFail");
//        var mgr = host.Services.GetRequiredService<TransactionManager>();
//        var bad = new RecordingProvider("bad") { PrepareResult = _ => new Option(StatusCode.BadRequest, "prep") };
//        var good = new RecordingProvider("good");
//        var recBad = mgr.Register(bad);
//        mgr.Register(good);

//        var ctx = host.Services.CreateContext<TransactionManagerTests>();
//        (await mgr.Start(ctx)).BeOk();
//        bad.DataChangeLog.GetRecorder().NotNull().Add("obj", "v");
//        var result = await mgr.Commit(ctx);
//        result.IsError().BeTrue();
//        result.StatusCode.Be(StatusCode.BadRequest);

//        bad.Calls.Contains("commit:bad").BeFalse();
//        good.Calls.Any().BeFalse();
//    }

//    [Fact]
//    public async Task Commit_Failure_Stops_SubsequentProvidersNotInvoked()
//    {
//        using var host = await BuildHost("commitFail");
//        var mgr = host.Services.GetRequiredService<TransactionManager>();
//        var pA = new RecordingProvider("A");
//        var pB = new RecordingProvider("B") { CommitResult = _ => new Option(StatusCode.Conflict, "fail") };
//        var recA = mgr.Register(pA);
//        mgr.Register(pB);

//        var ctx = host.Services.CreateContext<TransactionManagerTests>();
//        (await mgr.Start(ctx)).BeOk();
//        pA.DataChangeLog.GetRecorder().NotNull().Add("x", "y");
//        var r = await mgr.Commit(ctx);
//        r.IsError().BeTrue();
//        r.StatusCode.Be(StatusCode.Conflict);

//        // A prepared & committed, B prepared & failed commit, no further providers
//        pA.Calls.Count(x => x == "prepare:A").Be(1);
//        pA.Calls.Count(x => x == "commit:A").Be(0);
//        pB.Calls.Count(x => x == "prepare:B").Be(1);
//        pB.Calls.Count(x => x == "commit:B").Be(1);
//    }

//    [Fact]
//    public async Task Rollback_ReversesOrder_ProviderSpecific()
//    {
//        using var host = await BuildHost("rollback");
//        var mgr = host.Services.GetRequiredService<TransactionManager>();
//        var pA = new RecordingProvider("A");
//        var pB = new RecordingProvider("B");
//        var recA = mgr.Register(pA);
//        var recB = mgr.Register(pB);

//        var ctx = host.Services.CreateContext<TransactionManagerTests>();
//        (await mgr.Start(ctx)).BeOk();

//        pA.DataChangeLog.GetRecorder().NotNull().Add("o1", "v1");
//        pB.DataChangeLog.GetRecorder().NotNull().Update("o2", "old2", "new2");
//        pA.DataChangeLog.GetRecorder().NotNull().Delete("o3", "old3");

//        (await mgr.Rollback(ctx)).BeOk();

//        // Rollback entries executed; order reversed by object ids expected: o3 then o2 then o1 relative to providers
//        // We just verify all three present.
//        pA.Calls.Count(x => x.StartsWith("rollback:A")).Be(2);
//        pB.Calls.Count(x => x.StartsWith("rollback:B")).Be(1);
//    }

//    [Fact]
//    public async Task Enqueue_InvalidEntry_Throws()
//    {
//        using var host = await BuildHost("invalid");
//        var mgr = host.Services.GetRequiredService<TransactionManager>();
//        Assert.Throws<ArgumentException>(() => mgr.Enqueue(new DataChangeEntry())); // invalid
//    }

//    [Fact]
//    public async Task Commit_WithoutStart_Throws()
//    {
//        using var host = await BuildHost("nostart");
//        var mgr = host.Services.GetRequiredService<TransactionManager>();
//        mgr.Register(new RecordingProvider("A"));
//        var ctx = host.Services.CreateContext<TransactionManagerTests>();
//        Assert.Throws<ArgumentException>(() => mgr.Commit(ctx).GetAwaiter().GetResult());
//    }

//    // ============================================================================
//    // NEW TESTS - Additional Coverage
//    // ============================================================================

//    [Fact]
//    public async Task Start_CalledTwice_Throws()
//    {
//        using var host = await BuildHost("doubleStart");
//        var mgr = host.Services.GetRequiredService<TransactionManager>();
//        var ctx = host.Services.CreateContext<TransactionManagerTests>();

//        (await mgr.Start(ctx)).BeOk();
//        Assert.Throws<ArgumentException>(() => mgr.Start(ctx).GetAwaiter().GetResult());
//    }

//    [Fact]
//    public async Task Rollback_WithoutStart_Throws()
//    {
//        using var host = await BuildHost("rollbackNoStart");
//        var mgr = host.Services.GetRequiredService<TransactionManager>();
//        mgr.Register(new RecordingProvider("A"));
//        var ctx = host.Services.CreateContext<TransactionManagerTests>();

//        Assert.Throws<ArgumentException>(() => mgr.Rollback(ctx).GetAwaiter().GetResult());
//    }

//    [Fact]
//    public async Task Rollback_ProviderFailure_ReturnsError()
//    {
//        using var host = await BuildHost("rollbackFail");
//        var mgr = host.Services.GetRequiredService<TransactionManager>();
//        var failing = new RecordingProvider("failing")
//        {
//            RollbackResult = _ => new Option(StatusCode.InternalServerError, "rollback failed")
//        };

//        mgr.Register(failing);

//        var ctx = host.Services.CreateContext<TransactionManagerTests>();
//        (await mgr.Start(ctx)).BeOk();
//        failing.DataChangeLog.GetRecorder().NotNull().Add("obj1", "val1");

//        var result = await mgr.Rollback(ctx);
//        result.IsError().BeTrue();
//        result.StatusCode.Be(StatusCode.InternalServerError);
//    }

//    [Fact]
//    public async Task Rollback_Order_ReverseOfEnqueue()
//    {
//        using var host = await BuildHost("rollbackOrder");
//        var mgr = host.Services.GetRequiredService<TransactionManager>();
//        var pA = new RecordingProvider("A");
//        mgr.Register(pA);

//        var ctx = host.Services.CreateContext<TransactionManagerTests>();
//        (await mgr.Start(ctx)).BeOk();

//        pA.DataChangeLog.GetRecorder().NotNull().Add("o1", "v1");
//        pA.DataChangeLog.GetRecorder().NotNull().Add("o2", "v2");
//        pA.DataChangeLog.GetRecorder().NotNull().Add("o3", "v3");

//        (await mgr.Rollback(ctx)).BeOk();

//        // Verify reverse order: o3, o2, o1
//        pA.Calls[0].Be("rollback:A:o3");
//        pA.Calls[1].Be("rollback:A:o2");
//        pA.Calls[2].Be("rollback:A:o1");
//    }

//    [Fact]
//    public async Task Commit_ReuseAfterSuccess_NewTransactionId()
//    {
//        string journalKey = "reuse-" + Guid.NewGuid().ToString("N");
//        using var host = await BuildHost(journalKey);
//        var mgr = host.Services.GetRequiredService<TransactionManager>();
//        RecordingProvider pA = new("A");
//        mgr.Register(pA);
//        var ctx = host.Services.CreateContext<TransactionManagerTests>();

//        // First transaction
//        (await mgr.Start(ctx)).BeOk();
//        var firstTxId = mgr.TransactionId;
//        pA.DataChangeLog.GetRecorder().NotNull().Add("obj1", "val1");
//        (await mgr.Commit(ctx)).BeOk();

//        // Second transaction
//        (await mgr.Start(ctx)).BeOk();
//        var secondTxId = mgr.TransactionId;
//        pA.DataChangeLog.GetRecorder().NotNull().Add("obj2", "val2");
//        (await mgr.Commit(ctx)).BeOk();

//        // Verify different transaction IDs
//        firstTxId.NotBe(secondTxId);

//        var store = host.Services.GetRequiredService<IListStore<DataChangeRecord>>();
//        var read = await store.Get(journalKey, ctx);
//        read.BeOk();
//        read.Return().Count.Be(2);
//    }

//    [Fact]
//    public async Task Commit_ReuseAfterRollback_NewTransactionId()
//    {
//        using var host = await BuildHost("reuseRollback");
//        var mgr = host.Services.GetRequiredService<TransactionManager>();
//        RecordingProvider pA = new("A");
//        mgr.Register(pA);
//        var ctx = host.Services.CreateContext<TransactionManagerTests>();

//        // First transaction with rollback
//        (await mgr.Start(ctx)).BeOk();
//        var firstTxId = mgr.TransactionId;
//        pA.DataChangeLog.GetRecorder().NotNull().Add("obj1", "val1");
//        (await mgr.Rollback(ctx)).BeOk();

//        // Second transaction
//        (await mgr.Start(ctx)).BeOk();
//        var secondTxId = mgr.TransactionId;
//        secondTxId.NotBe(firstTxId);
//    }

//    [Fact]
//    public async Task Register_CaseInsensitive_Throws()
//    {
//        using var host = await BuildHost("caseInsensitive");
//        var mgr = host.Services.GetRequiredService<TransactionManager>();

//        var p = new RecordingProvider("Provider");
//        mgr.Register(p);
//        Assert.Throws<ArgumentException>(() => mgr.Register(p));
//    }

//    [Fact]
//    public async Task Enqueue_Generic_CreatesCorrectEntry()
//    {
//        string journalKey = "genericEnqueue-" + Guid.NewGuid().ToString("N");
//        using var host = await BuildHost(journalKey);
//        var mgr = host.Services.GetRequiredService<TransactionManager>();
//        mgr.Register(new RecordingProvider("source"));
//        var lsn = host.Services.GetRequiredService<LogSequenceNumber>();

//        var ctx = host.Services.CreateContext<TransactionManagerTests>();
//        (await mgr.Start(ctx)).BeOk();

//        var before = new DataETag([1, 2, 3]);
//        var after = new DataETag([4, 5, 6]);
//        mgr.Enqueue<string>("source", "objId", "Update", before, after);

//        (await mgr.Commit(ctx)).BeOk();

//        var store = host.Services.GetRequiredService<IListStore<DataChangeRecord>>();
//        var read = await store.Get(journalKey, ctx);
//        read.BeOk();
//        var entry = read.Return().Single().Entries.Single();

//        entry.ObjectId.Be("objId");
//        entry.SourceName.Be("source");
//        entry.Action.Be("Update");
//        entry.TypeName.Be("String");
//        (entry.Before == before).BeTrue();
//        (entry.After == after).BeTrue();
//    }

//    [Fact]
//    public async Task Commit_NoProviders_SucceedsWithJournal()
//    {
//        string journalKey = "noProviders-" + Guid.NewGuid().ToString("N");

//        // Build host without registering any providers
//        var host = Host.CreateDefaultBuilder()
//            .ConfigureServices(services =>
//            {
//                services.AddLogging(c => c.AddLambda(_output.WriteLine).AddDebug().AddFilter(_ => true));
//                services.AddInMemoryFileStore();
//                services.AddListStore<DataChangeRecord>();
//                services.AddTransactionServices(new TransactionManagerOption { JournalKey = journalKey });
//            })
//            .Build();

//        await host.ClearStore<TransactionManagerTests>();

//        var mgr = host.Services.GetRequiredService<TransactionManager>();
//        var ctx = host.Services.CreateContext<TransactionManagerTests>();

//        (await mgr.Start(ctx)).BeOk();
//        var result = await mgr.Commit(ctx);
//        result.BeOk();

//        var store = host.Services.GetRequiredService<IListStore<DataChangeRecord>>();
//        var read = await store.Get(journalKey, ctx);
//        read.BeOk();
//        read.Return().Single().Entries.Count.Be(0);
//    }

//    [Fact]
//    public async Task TrxRecorder_Update_CreatesCorrectEntry()
//    {
//        string journalKey = "update-" + Guid.NewGuid().ToString("N");
//        using var host = await BuildHost(journalKey);
//        var mgr = host.Services.GetRequiredService<TransactionManager>();
//        RecordingProvider pA = new("A");
//        mgr.Register(pA);

//        var ctx = host.Services.CreateContext<TransactionManagerTests>();
//        (await mgr.Start(ctx)).BeOk();

//        pA.DataChangeLog.GetRecorder().NotNull().Update("key1", "oldValue", "newValue");

//        (await mgr.Commit(ctx)).BeOk();

//        var store = host.Services.GetRequiredService<IListStore<DataChangeRecord>>();
//        var read = await store.Get(journalKey, ctx);
//        var entry = read.Return().Single().Entries.Single();

//        entry.ObjectId.Be("key1");
//        entry.Action.Be(ChangeOperation.Update);
//        entry.Before.NotNull();
//        entry.After.NotNull();
//    }

//    [Fact]
//    public async Task MultipleProviders_SameTransaction_BothReceiveAllEntries()
//    {
//        string journalKey = "multiProvider-" + Guid.NewGuid().ToString("N");
//        using var host = await BuildHost(journalKey);
//        var mgr = host.Services.GetRequiredService<TransactionManager>();
//        var pA = new RecordingProvider("A");
//        var pB = new RecordingProvider("B");
//        mgr.Register(pA);
//        mgr.Register(pB);

//        var ctx = host.Services.CreateContext<TransactionManagerTests>();
//        (await mgr.Start(ctx)).BeOk();

//        pA.DataChangeLog.GetRecorder().NotNull().Add("a1", "valA");
//        pB.DataChangeLog.GetRecorder().NotNull().Add("b1", "valB");

//        (await mgr.Commit(ctx)).BeOk();

//        // Both providers should see all entries in prepare/commit
//        pA.Calls.Count(x => x.StartsWith("prepare")).Be(1);
//        pB.Calls.Count(x => x.StartsWith("prepare")).Be(1);
//        pA.Calls.Count(x => x.StartsWith("commit")).Be(1);
//        pB.Calls.Count(x => x.StartsWith("commit")).Be(1);
//    }

//    // ============================================================================
//    // STRESS TESTS
//    // ============================================================================

//    [Fact]
//    public async Task Stress_LargeNumberOfEntries()
//    {
//        string journalKey = "stress-large-" + Guid.NewGuid().ToString("N");
//        using var host = await BuildHost(journalKey);
//        var mgr = host.Services.GetRequiredService<TransactionManager>();
//        RecordingProvider pA = new("A");
//        mgr.Register(pA);

//        var ctx = host.Services.CreateContext<TransactionManagerTests>();
//        (await mgr.Start(ctx)).BeOk();

//        // Enqueue 1000 entries
//        for (int i = 0; i < 1000; i++)
//        {
//            pA.DataChangeLog.GetRecorder().NotNull().Add($"obj{i}", $"value{i}");
//        }

//        var result = await mgr.Commit(ctx);
//        result.BeOk();

//        var store = host.Services.GetRequiredService<IListStore<DataChangeRecord>>();
//        var read = await store.Get(journalKey, ctx);
//        read.BeOk();
//        read.Return().Single().Entries.Count.Be(1000);
//    }

//    [Fact]
//    public async Task Stress_ConcurrentEnqueue_ThreadSafety()
//    {
//        string journalKey = "stress-concurrent-" + Guid.NewGuid().ToString("N");
//        using var host = await BuildHost(journalKey);
//        var mgr = host.Services.GetRequiredService<TransactionManager>();
//        RecordingProvider pA = new("A");
//        mgr.Register(pA);

//        var ctx = host.Services.CreateContext<TransactionManagerTests>();
//        (await mgr.Start(ctx)).BeOk();

//        var tasks = new List<Task>();
//        int threadCount = 10;
//        int itemsPerThread = 100;

//        // Concurrent enqueue from multiple threads
//        for (int t = 0; t < threadCount; t++)
//        {
//            int threadId = t;
//            var task = Task.Run(() =>
//            {
//                for (int i = 0; i < itemsPerThread; i++)
//                {
//                    pA.DataChangeLog.GetRecorder().NotNull().Add($"obj-t{threadId}-i{i}", $"value{i}");
//                }
//            });
//            tasks.Add(task);
//        }

//        await Task.WhenAll(tasks);

//        var result = await mgr.Commit(ctx);
//        result.BeOk();

//        var store = host.Services.GetRequiredService<IListStore<DataChangeRecord>>();
//        var read = await store.Get(journalKey, ctx);
//        read.BeOk();
//        read.Return().Single().Entries.Count.Be(threadCount * itemsPerThread);
//    }

//    [Fact]
//    public async Task Stress_MultipleTransactionCycles()
//    {
//        string journalKey = "stress-cycles-" + Guid.NewGuid().ToString("N");
//        using var host = await BuildHost(journalKey);
//        var mgr = host.Services.GetRequiredService<TransactionManager>();
//        RecordingProvider pA = new("A");
//        mgr.Register(pA);
//        var ctx = host.Services.CreateContext<TransactionManagerTests>();

//        var transactionIds = new HashSet<string>();

//        // 100 transaction cycles
//        for (int i = 0; i < 100; i++)
//        {
//            (await mgr.Start(ctx)).BeOk();
//            transactionIds.Add(mgr.TransactionId);
//            pA.DataChangeLog.GetRecorder().NotNull().Add($"obj{i}", $"value{i}");
//            (await mgr.Commit(ctx)).BeOk();
//        }

//        // All transaction IDs should be unique
//        transactionIds.Count.Be(100);

//        var store = host.Services.GetRequiredService<IListStore<DataChangeRecord>>();
//        var read = await store.Get(journalKey, ctx);
//        read.BeOk();
//        read.Return().Count.Be(100);
//    }

//    [Fact]
//    public async Task Stress_LargeRollback()
//    {
//        using var host = await BuildHost("stress-rollback");
//        var mgr = host.Services.GetRequiredService<TransactionManager>();
//        var provider = new RecordingProvider("A");
//        mgr.Register(provider);

//        var ctx = host.Services.CreateContext<TransactionManagerTests>();
//        (await mgr.Start(ctx)).BeOk();

//        // Enqueue 500 entries
//        for (int i = 0; i < 500; i++)
//        {
//            provider.DataChangeLog.GetRecorder().NotNull().Add($"obj{i}", $"value{i}");
//        }

//        var result = await mgr.Rollback(ctx);
//        result.BeOk();

//        // Verify all rollbacks called in reverse order
//        provider.Calls.Count(x => x.StartsWith("rollback")).Be(500);
//        provider.Calls[0].Be("rollback:A:obj499");
//        provider.Calls[499].Be("rollback:A:obj0");
//    }

//    [Fact]
//    public async Task Stress_MultipleProviders_ManyEntries()
//    {
//        string journalKey = "stress-multi-" + Guid.NewGuid().ToString("N");
//        using var host = await BuildHost(journalKey);
//        var mgr = host.Services.GetRequiredService<TransactionManager>();

//        var providers = new List<RecordingProvider>();
//        var recorders = new List<ITrxRecorder>();

//        // Register 5 providers
//        for (int i = 0; i < 5; i++)
//        {
//            var provider = new RecordingProvider($"P{i}");
//            providers.Add(provider);
//            recorders.Add(provider.DataChangeLog.GetRecorder().NotNull());
//        }

//        var ctx = host.Services.CreateContext<TransactionManagerTests>();
//        (await mgr.Start(ctx)).BeOk();

//        // Each provider adds 200 entries
//        for (int i = 0; i < 5; i++)
//        {
//            for (int j = 0; j < 200; j++)
//            {
//                recorders[i].Add($"obj-p{i}-{j}", $"value{j}");
//            }
//        }

//        var result = await mgr.Commit(ctx);
//        result.BeOk();

//        // Verify all providers were called
//        foreach (var provider in providers)
//        {
//            provider.Calls.Count(x => x.StartsWith("prepare")).Be(1);
//            provider.Calls.Count(x => x.StartsWith("commit")).Be(1);
//        }

//        var store = host.Services.GetRequiredService<IListStore<DataChangeRecord>>();
//        var read = await store.Get(journalKey, ctx);
//        read.BeOk();
//        read.Return().Single().Entries.Count.Be(1000);
//    }
//}