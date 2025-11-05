using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Test.Data.Transaction;

public class TransactionManagerTests
{
    private readonly ITestOutputHelper _outputHelper;

    protected virtual void AddStore(IServiceCollection services) => services.AddInMemoryFileStore();

    public TransactionManagerTests(ITestOutputHelper output) => _outputHelper = output.NotNull();

    public async Task<IHost> BuildService(bool useQueue)
    {
        var host = Host.CreateDefaultBuilder()
        .ConfigureServices((context, services) =>
        {
            services.AddLogging(config => config.AddLambda(_outputHelper.WriteLine).AddDebug().AddFilter(x => true));
            AddStore(services);
            services.AddListStore<DataChangeRecord>(config => useQueue.IfTrue(() => config.AddBatchProvider()));
            services.AddTransactionServices();
        })
        .Build();

        await host.ClearStore<TransactionManagerTests>();
        return host;
    }

    [Fact]
    public void TransactionStartup()
    {
        new ServiceCollection().AddTransactionServices().BuildServiceProvider();
    }

    [Fact]
    public async Task Empty()
    {
        Assert.Throws<ArgumentNullException>(() => new TransactionManagerBuilder().Build());
        Assert.Throws<ArgumentNullException>(() => new TransactionManagerBuilder().SetJournalKey("key").Build());

        var host = await BuildService(false);
        Assert.Throws<ArgumentException>(() => new TransactionManagerBuilder().SetJournalKey("key").SetServiceProvider(host.Services).Build());
    }

    [Fact]
    public async Task Simple()
    {
        var host = await BuildService(false);

        var builder = new TransactionManagerBuilder()
            .SetJournalKey("key")
            .SetServiceProvider(host.Services)
            .AddProvider(new TestProvider())
            .Build();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Commit_Success_AppendsJournal_InvokesProviders(bool useQueue)
    {
        using var host = await BuildService(useQueue);
        var context = host.Services.CreateContext<TransactionManagerTests>();
        var listStore = host.Services.GetRequiredService<IListStore<DataChangeRecord>>();

        string journalKey = nameof(Commit_Success_AppendsJournal_InvokesProviders) + "-" + Guid.NewGuid().ToString("N");

        var callLog = new List<string>();
        var providerA = new FakeProvider("sourceA", callLog);
        var providerB = new FakeProvider("sourceB", callLog);

        var manager = new TransactionManagerBuilder()
            .SetJournalKey(journalKey)
            .SetServiceProvider(host.Services)
            .AddProvider(providerA)
            .AddProvider(providerB)
            .Build();

        var recA = manager.GetRecorder("sourceA");
        var recB = manager.GetRecorder("sourceB");

        recA.Add<string>("obj1", "v1");
        recB.Delete<string>("obj2", "old");

        var result = await manager.Commit(context);
        Assert.True(result.IsOk(), result.ToString());

        // Verify provider ordering: prepare then commit, A then B
        Assert.Equal(new[] { "prepare:sourceA", "prepare:sourceB", "commit:sourceA", "commit:sourceB" }, callLog);

        // Verify journal contains our transaction
        var read = await listStore.Get(journalKey, context);
        Assert.True(read.IsOk(), read.ToString());
        var record = read.Return().SingleOrDefault(x => x.TransactionId == manager.TransactionId);
        Assert.NotNull(record);
        Assert.Equal(2, record!.Entries.Count);

        var e1 = record.Entries[0];
        var e2 = record.Entries[1];

        Assert.Equal(manager.TransactionId, e1.TransactionId);
        Assert.Equal("sourceA", e1.SourceName);
        Assert.Equal(ChangeOperation.Add, e1.Action);
        Assert.NotNull(e1.After);
        Assert.Null(e1.Before);
        Assert.Equal("String", e1.TypeName);

        Assert.Equal(manager.TransactionId, e2.TransactionId);
        Assert.Equal("sourceB", e2.SourceName);
        Assert.Equal(ChangeOperation.Delete, e2.Action);
        Assert.NotNull(e2.Before);
        Assert.Null(e2.After);
        Assert.Equal("String", e2.TypeName);

        // Finalization: cannot enqueue or commit again
        Assert.Throws<ArgumentException>(() => recA.Add<string>("obj3", "v3"));
        await Assert.ThrowsAsync<ArgumentException>(async () => await manager.Commit(context));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Rollback_Success_InvokesProviderPerEntry(bool useQueue)
    {
        using var host = await BuildService(useQueue);
        var context = host.Services.CreateContext<TransactionManagerTests>();

        var providerA = new FakeProvider("sourceA");
        var providerB = new FakeProvider("sourceB");

        var manager = new TransactionManagerBuilder()
            .SetJournalKey(nameof(Rollback_Success_InvokesProviderPerEntry))
            .SetServiceProvider(host.Services)
            .AddProvider(providerA)
            .AddProvider(providerB)
            .Build();

        var recA = manager.GetRecorder("sourceA");
        var recB = manager.GetRecorder("sourceB");

        recA.Add<string>("obj1", "v1");
        recB.Update<string>("obj2", "old", "new");
        recA.Delete<string>("obj3", "oldA");

        var result = await manager.Rollback(context);
        Assert.True(result.IsOk(), result.ToString());

        // Each rollback goes to the provider associated with the entry
        Assert.Collection(providerA.RolledBack,
            e => Assert.Equal("obj1", e.ObjectId),
            e => Assert.Equal("obj3", e.ObjectId)
        );
        Assert.Collection(providerB.RolledBack,
            e => Assert.Equal("obj2", e.ObjectId)
        );

        // Finalization: cannot enqueue or commit/rollback again
        Assert.Throws<ArgumentException>(() => recA.Add<string>("objX", "v"));
        await Assert.ThrowsAsync<ArgumentException>(async () => await manager.Rollback(context));
        await Assert.ThrowsAsync<ArgumentException>(async () => await manager.Commit(context));
    }

    [Fact]
    public async Task PrepareFails_StopsAndReturnsError_NoCommitInvoked()
    {
        using var host = await BuildService(false);
        var context = host.Services.CreateContext<TransactionManagerTests>();

        var callLog = new List<string>();
        var badProvider = new FakeProvider("bad", callLog) { PrepareResultFactory = _ => new Option(StatusCode.BadRequest, "prep fail") };
        var goodProvider = new FakeProvider("good", callLog);

        var manager = new TransactionManagerBuilder()
            .SetJournalKey(nameof(PrepareFails_StopsAndReturnsError_NoCommitInvoked))
            .SetServiceProvider(host.Services)
            .AddProvider(badProvider)     // first -> should fail here
            .AddProvider(goodProvider)
            .Build();

        var rec = manager.GetRecorder("bad");
        rec.Add<string>("obj", "v");

        var result = await manager.Commit(context);
        Assert.True(result.IsError());
        Assert.Equal(StatusCode.BadRequest, result.StatusCode);

        // Only first prepare should have been called, no commits at all
        Assert.Equal(new[] { "prepare:bad" }, callLog);
        Assert.Empty(badProvider.Committed);
        Assert.Empty(goodProvider.Prepared);
        Assert.Empty(goodProvider.Committed);
    }

    [Fact]
    public async Task CommitFails_StopsAndReturnsError_SubsequentProvidersNotInvoked()
    {
        using var host = await BuildService(false);
        var context = host.Services.CreateContext<TransactionManagerTests>();

        var callLog = new List<string>();
        var providerA = new FakeProvider("A", callLog);
        var providerB = new FakeProvider("B", callLog) { CommitResultFactory = _ => new Option(StatusCode.Conflict, "commit fail") };

        var manager = new TransactionManagerBuilder()
            .SetJournalKey(nameof(CommitFails_StopsAndReturnsError_SubsequentProvidersNotInvoked))
            .SetServiceProvider(host.Services)
            .AddProvider(providerA) // prepare A ok
            .AddProvider(providerB) // prepare B ok, commit B fails
            .Build();

        var recA = manager.GetRecorder("A");
        recA.Add<string>("obj1", "v1");

        var result = await manager.Commit(context);
        Assert.True(result.IsError());
        Assert.Equal(StatusCode.Conflict, result.StatusCode);

        // Prepare A, Prepare B, Commit A, Commit B(fail). No commit calls after failure.
        Assert.Equal(new[] { "prepare:A", "prepare:B", "commit:A", "commit:B" }, callLog);
    }

    [Fact]
    public async Task GetRecorder_UnknownProvider_Throws()
    {
        using var host = await BuildService(false);

        var manager = new TransactionManagerBuilder()
            .SetJournalKey(nameof(GetRecorder_UnknownProvider_Throws))
            .SetServiceProvider(host.Services)
            .AddProvider(new FakeProvider("known"))
            .Build();

        Assert.Throws<ArgumentException>(() => manager.GetRecorder("unknown"));
    }

    [Fact]
    public async Task Enqueue_InvalidEntry_Throws()
    {
        using var host = await BuildService(false);

        var manager = new TransactionManagerBuilder()
            .SetJournalKey(nameof(Enqueue_InvalidEntry_Throws))
            .SetServiceProvider(host.Services)
            .AddProvider(new FakeProvider("p"))
            .Build();

        // Missing required fields triggers validation error
        var invalid = new DataChangeEntry();
        Assert.Throws<ArgumentException>(() => manager.Enqueue(invalid));
    }

    private class TestProvider : ITransactionProvider
    {
        public string Name => "test";

        public Task<Option> Commit(DataChangeRecord dataChangeEntry, ScopeContext context) => throw new NotImplementedException();
        public Task<Option> Prepare(DataChangeRecord dataChangeEntry, ScopeContext context) => throw new NotImplementedException();
        public Task<Option> Rollback(DataChangeEntry dataChangeEntry, ScopeContext context) => throw new NotImplementedException();
    }

    private class FakeProvider : ITransactionProvider
    {
        private readonly List<string>? _sharedLog;

        public FakeProvider(string name, List<string>? sharedLog = null)
        {
            Name = name.NotEmpty();
            _sharedLog = sharedLog;
        }

        public string Name { get; }

        public List<DataChangeRecord> Prepared { get; } = new();
        public List<DataChangeRecord> Committed { get; } = new();
        public List<DataChangeEntry> RolledBack { get; } = new();

        public Func<DataChangeRecord, Option>? PrepareResultFactory { get; set; }
        public Func<DataChangeRecord, Option>? CommitResultFactory { get; set; }
        public Func<DataChangeEntry, Option>? RollbackResultFactory { get; set; }

        public Task<Option> Prepare(DataChangeRecord dataChangeEntry, ScopeContext context)
        {
            Prepared.Add(dataChangeEntry);
            _sharedLog?.Add($"prepare:{Name}");
            var result = PrepareResultFactory?.Invoke(dataChangeEntry) ?? new Option(StatusCode.OK);
            return Task.FromResult(result);
        }

        public Task<Option> Commit(DataChangeRecord dataChangeEntry, ScopeContext context)
        {
            Committed.Add(dataChangeEntry);
            _sharedLog?.Add($"commit:{Name}");
            var result = CommitResultFactory?.Invoke(dataChangeEntry) ?? new Option(StatusCode.OK);
            return Task.FromResult(result);
        }

        public Task<Option> Rollback(DataChangeEntry dataChangeEntry, ScopeContext context)
        {
            RolledBack.Add(dataChangeEntry);
            var result = RollbackResultFactory?.Invoke(dataChangeEntry) ?? new Option(StatusCode.OK);
            return Task.FromResult(result);
        }
    }
}
