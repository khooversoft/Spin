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
        var host = await BuildService(false);

        Verify.Throw<ArgumentNullException>(() => new TransactionManagerFactory(null!).Build());
        Verify.Throw<ArgumentNullException>(() => new TransactionManagerFactory(host.Services).SetJournalKey("key").Build());
    }

    [Fact]
    public async Task Simple()
    {
        var host = await BuildService(false);

        var builder = host.Services.GetRequiredService<TransactionManagerFactory>()
            .SetJournalKey("key")
            .AddProvider(new TestProvider())
            .Build();
    }

    [Theory]
    [InlineData(false)]
    //[InlineData(true)]  // TODO: Queue does not work
    public async Task Commit_Success_AppendsJournal_InvokesProviders(bool useQueue)
    {
        using var host = await BuildService(useQueue);
        var context = host.Services.CreateContext<TransactionManagerTests>();
        var listStore = host.Services.GetRequiredService<IListStore<DataChangeRecord>>();

        string journalKey = nameof(Commit_Success_AppendsJournal_InvokesProviders) + "-" + Guid.NewGuid().ToString("N");

        var callLog = new List<string>();
        var providerA = new FakeProvider("sourceA", callLog);
        var providerB = new FakeProvider("sourceB", callLog);

        var manager = host.Services.GetRequiredService<TransactionManagerFactory>()
            .SetJournalKey(journalKey)
            .AddProvider(providerA)
            .AddProvider(providerB)
            .Build();

        var recA = manager.GetRecorder("sourceA");
        var recB = manager.GetRecorder("sourceB");

        recA.Add("obj1", "v1");
        recB.Delete("obj2", "old");

        var result = await manager.Commit(context);
        result.BeOk();

        // Verify provider ordering: prepare then commit, A then B
        callLog.SequenceEqual(["prepare:sourceA", "prepare:sourceB", "commit:sourceA", "commit:sourceB"]).BeTrue();

        // Verify journal contains our transaction
        var read = await listStore.Get(journalKey, context);
        read.BeOk();
        var record = read.Return().SingleOrDefault(x => x.TransactionId == manager.TransactionId);
        record.NotNull();
        record!.Entries.Count.Be(2);

        var e1 = record.Entries[0];
        var e2 = record.Entries[1];

        manager.TransactionId.Be(e1.TransactionId);
        "sourceA".Be(e1.SourceName);
        ChangeOperation.Add.Be(e1.Action);
        e1.After.NotNull();
        Assert.Null(e1.Before);
        "String".Be(e1.TypeName);

        manager.TransactionId.Be(e2.TransactionId);
        "sourceB".Be(e2.SourceName);
        ChangeOperation.Delete.Be(e2.Action);
        e2.Before.NotNull();
        e2.After.BeNull();
        "String".Be(e2.TypeName);

        // Finalization: cannot enqueue or commit again
        Verify.Throw<ArgumentException>(() => recA.Add("obj3", "v3"));
        await Verify.ThrowAsync<ArgumentException>(async () => await manager.Commit(context));
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

        var manager = host.Services.GetRequiredService<TransactionManagerFactory>()
            .SetJournalKey(nameof(Rollback_Success_InvokesProviderPerEntry))
            .AddProvider(providerA)
            .AddProvider(providerB)
            .Build();

        var recA = manager.GetRecorder("sourceA");
        var recB = manager.GetRecorder("sourceB");

        recA.Add("obj1", "v1");
        recB.Update("obj2", "old", "new");
        recA.Delete("obj3", "oldA");

        var result = await manager.Rollback(context);
        result.BeOk();

        // Each rollback goes to the provider associated with the entry
        Assert.Collection(providerA.RolledBack,
            e => "obj1".Be(e.ObjectId),
            e => "obj3".Be(e.ObjectId)
        );
        Assert.Collection(providerB.RolledBack,
            e => "obj2".Be(e.ObjectId)
        );

        // Finalization: cannot enqueue or commit/rollback again
        Verify.Throw<ArgumentException>(() => recA.Add("objX", "v"));
        await Verify.ThrowAsync<ArgumentException>(async () => await manager.Rollback(context));
        await Verify.ThrowAsync<ArgumentException>(async () => await manager.Commit(context));
    }

    [Fact]
    public async Task PrepareFails_StopsAndReturnsError_NoCommitInvoked()
    {
        using var host = await BuildService(false);
        var context = host.Services.CreateContext<TransactionManagerTests>();

        var callLog = new List<string>();
        var badProvider = new FakeProvider("bad", callLog) { PrepareResultFactory = _ => new Option(StatusCode.BadRequest, "prep fail") };
        var goodProvider = new FakeProvider("good", callLog);

        var manager = host.Services.GetRequiredService<TransactionManagerFactory>()
            .SetJournalKey(nameof(PrepareFails_StopsAndReturnsError_NoCommitInvoked))
            .SetServiceProvider(host.Services)
            .AddProvider(badProvider)     // first -> should fail here
            .AddProvider(goodProvider)
            .Build();

        var rec = manager.GetRecorder("bad");
        rec.Add("obj", "v");

        var result = await manager.Commit(context);
        result.BeError();
        result.StatusCode.Be(StatusCode.BadRequest);

        // Only first prepare should have been called, no commits at all
        callLog.SequenceEqual(["prepare:bad"]).BeTrue();
        badProvider.Committed.Count.Be(0);
        goodProvider.Prepared.Count.Be(0);
        goodProvider.Committed.Count.Be(0);
    }

    [Fact]
    public async Task CommitFails_StopsAndReturnsError_SubsequentProvidersNotInvoked()
    {
        using var host = await BuildService(false);
        var context = host.Services.CreateContext<TransactionManagerTests>();

        var callLog = new List<string>();
        var providerA = new FakeProvider("A", callLog);
        var providerB = new FakeProvider("B", callLog) { CommitResultFactory = _ => new Option(StatusCode.Conflict, "commit fail") };

        var manager = host.Services.GetRequiredService<TransactionManagerFactory>()
            .SetJournalKey(nameof(CommitFails_StopsAndReturnsError_SubsequentProvidersNotInvoked))
            .AddProvider(providerA) // prepare A ok
            .AddProvider(providerB) // prepare B ok, commit B fails
            .Build();

        var recA = manager.GetRecorder("A");
        recA.Add("obj1", "v1");

        var result = await manager.Commit(context);
        result.BeError();
        result.StatusCode.Be(StatusCode.Conflict);

        // Prepare A, Prepare B, Commit A, Commit B(fail). No commit calls after failure.
        Assert.Equal(new[] { "prepare:A", "prepare:B", "commit:A", "commit:B" }, callLog);
    }

    [Fact]
    public async Task GetRecorder_UnknownProvider_Throws()
    {
        using var host = await BuildService(false);

        var manager = host.Services.GetRequiredService<TransactionManagerFactory>()
            .SetJournalKey(nameof(GetRecorder_UnknownProvider_Throws))
            .AddProvider(new FakeProvider("known"))
            .Build();

        Verify.Throw<ArgumentException>(() => manager.GetRecorder("unknown"));
    }

    [Fact]
    public async Task Enqueue_InvalidEntry_Throws()
    {
        using var host = await BuildService(false);

        var manager = host.Services.GetRequiredService<TransactionManagerFactory>()
            .SetJournalKey(nameof(Enqueue_InvalidEntry_Throws))
            .AddProvider(new FakeProvider("p"))
            .Build();

        // Missing required fields triggers validation error
        var invalid = new DataChangeEntry();
        Verify.Throw<ArgumentException>(() => manager.Enqueue(invalid));
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
