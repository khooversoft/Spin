//using System.Collections.Concurrent;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Hosting;
//using Microsoft.Extensions.Logging;
//using Toolbox.Extensions;
//using Toolbox.Store;
//using Toolbox.Tools;
//using Toolbox.Types;
//using Xunit.Abstractions;

//namespace Toolbox.Test.Store;

//public class KeyStoreLockTests
//{
//    private readonly ITestOutputHelper _outputHelper;
//    private readonly ConcurrentQueue<string> _loggingMessageSave = new();
//    private record JournalEntry(string Name, int Age);
//    private record TraceEntry(DateTime Date, string Name, int Age);

//    public KeyStoreLockTests(ITestOutputHelper output) => _outputHelper = output.NotNull();

//    protected virtual void AddStore(IServiceCollection services) => services.AddInMemoryFileStore();

//    public async Task<IHost> BuildService()
//    {
//        TimeSpan cacheDuration = TimeSpan.FromSeconds(1);

//        var host = Host.CreateDefaultBuilder()
//        .ConfigureServices((context, services) =>
//        {
//            services.AddLogging(config =>
//            {
//                config.AddLambda(x =>
//                {
//                    _loggingMessageSave.Enqueue(x);
//                    _outputHelper.WriteLine(x);
//                });
//                config.AddDebug();
//                config.AddFilter(x => true);
//            });

//            AddStore(services);
//            services.AddKeyStore<JournalEntry>(FileSystemType.Key, config =>
//            {
//                config.AddLockProvider(LockMode.Exclusive);
//                config.AddKeyStore();
//            });
//            services.AddKeyStore<TraceEntry>(FileSystemType.Hash, config =>
//            {
//                config.AddLockProvider(LockMode.Exclusive);
//                config.AddKeyStore();
//            });
//        })
//        .Build();

//        await host.ClearStore<KeyStoreTests>();
//        return host;
//    }

//    [Fact]
//    public async Task SingleKey()
//    {
//        using var host = await BuildService();
//        var context = host.Services.CreateContext<KeyStoreTests>();
//        var fileStore = host.Services.GetRequiredService<IFileStore>();
//        var keyStore = host.Services.GetRequiredService<IKeyStore<JournalEntry>>();
//        var fileSystem = host.Services.GetRequiredService<IFileSystem<JournalEntry>>();

//        const string key = nameof(SingleKey);
//        string shouldMatch = fileSystem.PathBuilder(key);
//        var journalEntry = new JournalEntry("Test", 30);
//        var path = fileSystem.PathBuilder(key);

//        // Get should exclusive lock file, creates a file if not exist, zero length
//        (await keyStore.Get(key, context)).IsNoContent().BeTrue();

//        (await keyStore.Set(key, journalEntry, context)).BeOk();
//        (await fileStore.File(path).Set(journalEntry.ToDataETag(), context)).IsLocked().BeTrue();
//        (await keyStore.Get(key, context)).BeOk().Return().Action(x => (journalEntry == x).BeTrue());
//        (await fileStore.File(path).Set(journalEntry.ToDataETag(), context)).IsLocked().BeTrue();

//        journalEntry = new JournalEntry("Test2", 50);
//        (await keyStore.Set(key, journalEntry, context)).BeOk();
//        (await fileStore.File(path).Set(journalEntry.ToDataETag(), context)).IsLocked().BeTrue();

//        (await keyStore.Get(key, context)).BeOk().Return().Action(x => (journalEntry == x).BeTrue());
//        (await keyStore.ReleaseLock(key, context)).BeOk();
//        (await fileStore.File(path).Set(journalEntry.ToDataETag(), context)).BeOk();

//        // This will lock it again
//        (await keyStore.Get(key, context)).BeOk().Return().Action(x => (journalEntry == x).BeTrue());

//        var fileList = await fileStore.Search("**/*", context);
//        fileList.Count.Be(1);
//        fileList[0].Path.Be(shouldMatch);

//        (await keyStore.ReleaseLock(key, context)).BeOk();
//        (await keyStore.Delete(key, context)).BeOk();
//        (await fileStore.Search("**/*", context)).Count.Be(0);
//    }

//    [Fact]
//    public async Task TwoKeyStoreSameKey()
//    {
//        using var host = await BuildService();
//        var context = host.Services.CreateContext<KeyStoreTests>();
//        var fileStore = host.Services.GetRequiredService<IFileStore>();
//        var journalStore = host.Services.GetRequiredService<IKeyStore<JournalEntry>>();
//        var traceStore = host.Services.GetRequiredService<IKeyStore<TraceEntry>>();
//        var journalFileSystem = host.Services.GetRequiredService<IFileSystem<JournalEntry>>();
//        var traceFileSystem = host.Services.GetRequiredService<IFileSystem<TraceEntry>>();

//        const string key = nameof(TwoKeyStoreSameKey);
//        var now = DateTime.Now;
//        var journalEntry = new JournalEntry("Test", 30);
//        var traceEntry = new TraceEntry(now, "Test", 30);
//        var failEntry = new JournalEntry("fail", 50);

//        var journalPath = journalFileSystem.PathBuilder(key);
//        var tracePath = traceFileSystem.PathBuilder(key);

//        // Get should exclusive lock file, creates a file if not exist, zero length
//        (await journalStore.Get(key, context)).IsNoContent().BeTrue();
//        (await traceStore.Get(key, context)).IsNoContent().BeTrue();

//        (await journalStore.Set(key, journalEntry, context)).BeOk();
//        (await traceStore.Set(key, traceEntry, context)).BeOk();

//        (await fileStore.File(journalPath).Set(failEntry.ToDataETag(), context)).IsLocked().BeTrue();
//        (await journalStore.Get(key, context)).BeOk().Return().Action(x => (journalEntry == x).BeTrue());

//        (await fileStore.File(journalPath).Set(failEntry.ToDataETag(), context)).IsLocked().BeTrue();
//        (await fileStore.File(tracePath).Set(failEntry.ToDataETag(), context)).IsLocked().BeTrue();

//        var newJournalEntry = new JournalEntry("Test2", 50);
//        (await journalStore.Set(key, newJournalEntry, context)).BeOk();
//        (await journalStore.Get(key, context)).BeOk().Return().Action(x => (newJournalEntry == x).BeTrue());
//        (await fileStore.File(journalPath).Set(failEntry.ToDataETag(), context)).IsLocked().BeTrue();

//        (await journalStore.Get(key, context)).BeOk().Return().Action(x => (newJournalEntry == x).BeTrue());
//        (await journalStore.ReleaseLock(key, context)).BeOk();
//        (await fileStore.File(journalPath).Set(failEntry.ToDataETag(), context)).BeOk();

//        // This will lock it again
//        (await journalStore.Get(key, context)).BeOk().Return().Action(x => (failEntry == x).BeTrue());
//        (await traceStore.Get(key, context)).BeOk().Return().Action(x => (traceEntry == x).BeTrue());

//        var fileList = await fileStore.Search("**/*", context);
//        fileList.Count.Be(2);

//        var fileParts = new string[]
//            {
//                journalFileSystem.PathBuilder(key),
//                traceFileSystem.PathBuilder(key)
//            }.OrderBy(x => x)
//            .ToArray();

//        fileList.OrderBy(x => x.Path).All(x => fileParts.Any(z => x.Path.Contains(z))).BeTrue();

//        (await journalStore.ReleaseLock(key, context)).BeOk();
//        (await traceStore.ReleaseLock(key, context)).BeOk();
//        (await journalStore.Delete(key, context)).BeOk();
//        (await traceStore.Delete(key, context)).BeOk();
//        (await fileStore.Search("**/*", context)).Count.Be(0);
//    }
//}
