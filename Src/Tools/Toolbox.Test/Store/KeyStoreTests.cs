using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Test.Store;

public class KeyStoreTests
{
    private readonly ITestOutputHelper _outputHelper;
    private record JournalEntry(int index, string Name, int Age);
    private record TraceEntry(int index, DateTime date, string Name, int Age);
    private record LedgerEntry(int index, DateTime date, string AccountId, double amount);

    public KeyStoreTests(ITestOutputHelper output) => _outputHelper = output.NotNull();

    protected virtual void AddStore(IServiceCollection services) => services.AddInMemoryFileStore();

    public async Task<IHost> BuildService(bool useCache, bool useHash, ConcurrentQueue<string>? loggingMessageSave)
    {
        TimeSpan cacheDuration = TimeSpan.FromSeconds(1);
        FileSystemType fileSystemType = useHash ? FileSystemType.Hash : FileSystemType.Key;

        var host = Host.CreateDefaultBuilder()
        .ConfigureServices((context, services) =>
        {
            services.AddLogging(config =>
            {
                config.AddLambda(x =>
                {
                    loggingMessageSave?.Enqueue(x);
                    _outputHelper.WriteLine(x);
                });
                config.AddDebug();
                config.AddFilter(x => true);
            });

            AddStore(services);
            services.AddKeyStore<JournalEntry>(fileSystemType, config => useCache.IfTrue(() => config.AddCacheProvider(cacheDuration)));
            services.AddKeyStore<TraceEntry>(fileSystemType, config => useCache.IfTrue(() => config.AddCacheProvider(cacheDuration)));
            services.AddKeyStore<LedgerEntry>(fileSystemType, config => useCache.IfTrue(() => config.AddCacheProvider(cacheDuration)));
        })
        .Build();

        await host.ClearStore<KeyStoreTests>();
        return host;
    }

    [Fact]
    public async Task VerifyHash()
    {
        using var host = await BuildService(false, true, null);

        foreach (var index in Enumerable.Range(0, 10))
        {
            IFileSystem<JournalEntry> fileSystem = host.Services.GetRequiredService<IFileSystem<JournalEntry>>();
            var testPath = fileSystem.PathBuilder("nodes/node2/node2___contract");

            string dataFilePath = "56/d5/nodes/node2/node2___contract.journalentry.json";
            (testPath == dataFilePath).BeTrue();
        }
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task SingleKey(bool useCache, bool useHash)
    {
        var loggingMessageSave = useCache ? new ConcurrentQueue<string>() : null;
        using var host = await BuildService(useCache, useHash, loggingMessageSave);
        var context = host.Services.CreateContext<KeyStoreTests>();
        var fileStore = host.Services.GetRequiredService<IFileStore>();
        var keyStore = host.Services.GetRequiredService<IKeyStore<JournalEntry>>();
        var journalFileStore = host.Services.GetRequiredService<IFileSystem<JournalEntry>>();

        const string key = nameof(SingleKey);
        string shouldMatch = journalFileStore.PathBuilder(key);
        var journalEntry = new JournalEntry(1, "Test", 30);

        (await keyStore.Get(key, context)).BeNotFound();
        loggingMessageSave?.Action(x => x.Count(x => x.Contains("CacheMiss")).Be(1));

        (await keyStore.Set(key, journalEntry, context)).BeOk();
        loggingMessageSave?.Action(x => x.Count(x => x.Contains("CacheSet")).Be(1));

        (await keyStore.Get(key, context)).BeOk().Return().Action(x => (journalEntry == x).BeTrue());
        loggingMessageSave?.Action(x => x.Count(x => x.Contains("CacheHit")).Be(1));

        var fileList = await fileStore.Search("**/*", context);
        fileList.Count.Be(1);
        fileList.First().Path.Be(shouldMatch);

        (await keyStore.Delete(key, context)).BeOk();
        (await keyStore.Get(key, context)).IsNotFound().BeTrue();
        (await fileStore.Search("**/*", context)).Count.Be(0);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task TwoTypes(bool useCache, bool useHash)
    {
        DateTime now = DateTime.Now;

        var loggingMessageSave = useCache ? new ConcurrentQueue<string>() : null;
        using var host = await BuildService(useCache, useHash, loggingMessageSave);
        var context = host.Services.CreateContext<KeyStoreTests>();
        var fileStore = host.Services.GetRequiredService<IFileStore>();
        var journalStore = host.Services.GetRequiredService<IKeyStore<JournalEntry>>();
        var journalSystem = host.Services.GetRequiredService<IFileSystem<JournalEntry>>();

        var traceStore = host.Services.GetRequiredService<IKeyStore<TraceEntry>>();
        var traceSystem = host.Services.GetRequiredService<IFileSystem<TraceEntry>>();

        const string key = nameof(SingleKey);
        string journalKey = journalSystem.PathBuilder(key);
        string traceKey = traceSystem.PathBuilder(key);

        var journalEntry = new JournalEntry(2, "Test", 30);
        var traceEntry = new TraceEntry(3, now, "Test", 30);

        (await journalStore.Set(key, journalEntry, context)).BeOk();
        (await traceStore.Set(key, traceEntry, context)).BeOk();
        loggingMessageSave?.Action(x => x.Count(x => x.Contains("CacheSet")).Be(2));

        (await journalStore.Get(key, context)).BeOk().Return().Assert(x => x == journalEntry);
        (await traceStore.Get(key, context)).BeOk().Return().Assert(x => x == traceEntry);
        loggingMessageSave?.Action(x => x.Count(x => x.Contains("CacheHit")).Be(2));

        loggingMessageSave?.Action(x => x.Count(x => x.Contains("CacheMiss")).Be(0));

        (await fileStore.Search("**/*", context)).Action(x =>
        {
            x.Count.Be(2);

            string[] files = [journalSystem.PathBuilder(key), traceSystem.PathBuilder(key)];
            x.Select(x => x.Path).OrderBy(x => x).SequenceEqual(files.OrderBy(x => x)).BeTrue();
        });

        (await journalStore.Delete(key, context)).BeOk();
        (await journalStore.Get(key, context)).IsNotFound().BeTrue();

        (await traceStore.Delete(key, context)).BeOk();
        (await traceStore.Get(key, context)).IsNotFound().BeTrue();

        (await fileStore.Search("**/*", context)).Count.Be(0);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task StressTestSameKey(bool useCache, bool useHash)
    {
        var loggingMessageSave = useCache ? new ConcurrentQueue<string>() : null;
        using var host = await BuildService(useCache, useHash, loggingMessageSave);
        var context = host.Services.CreateContext<KeyStoreTests>();
        var fileStore = host.Services.GetRequiredService<IFileStore>();

        int count = 0;
        DateTime start = DateTime.Now;

        var t1 = Task.Run(() => run("journalEntry", () => nextCount().Func(x => new JournalEntry(x, $"journal{x}", x + 100))));
        var t2 = Task.Run(() => run("traceEntry", () => nextCount().Func(x => new TraceEntry(x, DateTime.Now, $"trace{x}", x + 100))));
        var t3 = Task.Run(() => run("LedgerEntry", () => nextCount().Func(x => new LedgerEntry(x, DateTime.Now, $"trace{x}", x + 100))));

        await Task.WhenAll(t1, t2, t3);

        var list = await fileStore.Search("**/*", context);
        list.Count.Be(3);

        var time = DateTime.Now - start;
        double tps = count / time.TotalSeconds;
        context.LogInformation("Performance: {count} updates, tps={tps}", count, tps);

        (await host.Services.GetRequiredService<IKeyStore<JournalEntry>>().Delete("journalEntry", context)).BeOk();
        (await host.Services.GetRequiredService<IKeyStore<TraceEntry>>().Delete("traceEntry", context)).BeOk();
        (await host.Services.GetRequiredService<IKeyStore<LedgerEntry>>().Delete("LedgerEntry", context)).BeOk();

        if (loggingMessageSave != null)
        {
            var setCount = loggingMessageSave.Count(x => x.Contains("CacheSet"));
            var hitCount = loggingMessageSave.Count(x => x.Contains("CacheHit"));
            setCount.Assert(x => x >= hitCount, x => $"hit counter {x} is less than {hitCount}");
            setCount.Assert(x => x >= count, x => $"set counter {x} less than {count}");
            loggingMessageSave.Count(x => x.Contains("CacheMiss")).Be(0);
        }

        int nextCount() => Interlocked.Increment(ref count);

        async Task run<T>(string key, Func<T> createEntry)
        {
            var store = host.Services.GetRequiredService<IKeyStore<T>>();
            var token = new CancellationTokenSource(TimeSpan.FromSeconds(1));

            while (!token.IsCancellationRequested)
            {
                T data = createEntry();
                (await store.Set(key, data, context)).BeOk();
                (await store.Get(key, context)).BeOk().Return().Assert(x => x.NotNull().Equals(data));
            }
        }
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task StressTestMultiKey(bool useCache, bool useHash)
    {
        var loggingMessageSave = useCache ? new ConcurrentQueue<string>() : null;
        using var host = await BuildService(useCache, useHash, loggingMessageSave);
        var context = host.Services.CreateContext<KeyStoreTests>();
        var fileStore = host.Services.GetRequiredService<IFileStore>();

        DateTime start = DateTime.Now;
        var getQueue = new ConcurrentQueue<Func<Task>>();
        var deleteQueue = new ConcurrentQueue<Func<Task>>();

        var totalCount = await measure("Test load", async () =>
        {
            var t1 = Task.Run(() => run("journalEntry", x => new JournalEntry(x, $"journal{x}", x + 100)));
            var t2 = Task.Run(() => run("traceEntry", x => new TraceEntry(x, DateTime.Now, $"trace{x}", x + 100)));
            var t3 = Task.Run(() => run("LedgerEntry", x => new LedgerEntry(x, DateTime.Now, $"trace{x}", x + 100)));

            var counts = await Task.WhenAll(t1, t2, t3);
            int totalCount = counts.Sum();
            return totalCount;
        });

        var list = await fileStore.Search("**/*", context);
        list.Count.Be(totalCount);

        await measure("Get performance", async () =>
        {
            await Parallel.ForEachAsync(getQueue, async (item, token) => await item());
            return getQueue.Count;
        });

        await measure("Delete performance", async () =>
        {
            await Parallel.ForEachAsync(deleteQueue, async (item, token) => await item());
            return deleteQueue.Count;
        });

        if (loggingMessageSave != null)
        {
            await Task.Delay(TimeSpan.FromSeconds(1));
            loggingMessageSave.Count(x => x.Contains("CacheSet")).BeWithinPercentage(getQueue.Count * 2, 20);
            loggingMessageSave.Count(x => x.Contains("CacheDelete")).BeWithinPercentage(deleteQueue.Count, 20);

            var hitCount = loggingMessageSave.Count(x => x.Contains("CacheHit"));
            var missCount = loggingMessageSave.Count(x => x.Contains("CacheMiss"));
            (hitCount + missCount).Assert(x => x > 0, "No hit or miss counts");
        }

        (await fileStore.Search("**/*", context)).Count.Be(0);

        async Task<int> run<T>(string baseKey, Func<int, T> createEntry)
        {
            var store = host.Services.GetRequiredService<IKeyStore<T>>();
            var token = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            int count = 0;

            while (!token.IsCancellationRequested)
            {
                Interlocked.Increment(ref count);
                string key = $"{baseKey}_{count}";
                T data = createEntry(count);
                (await store.Set(key, data, context)).BeOk();

                getQueue.Enqueue(async () => (await store.Get(key, context)).BeOk().Return().Assert(x => x.NotNull().Equals(data)));
                deleteQueue.Enqueue(async () => await store.Delete(key, context));
            }

            return count;
        }

        async Task<int> measure(string title, Func<Task<int>> func)
        {
            DateTime start = DateTime.Now;
            int count = await func();

            var time = DateTime.Now - start;
            double tps = count / time.TotalSeconds;
            context.LogInformation("{title} performance: count={count}, time={time}, tps={tps}", title, count, time, tps);

            return count;
        }
    }
}
