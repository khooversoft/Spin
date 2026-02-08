using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Store;
using Toolbox.Test.Store.ListStore;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Test.Store.SequenceStore;

public class DataSpaceSequenceTests
{
    private ITestOutputHelper _outputHelper;
    private record TestRecord(string Name, int Age);

    public DataSpaceSequenceTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;
    protected virtual void AddStore(IServiceCollection services, string basePath) => services.AddInMemoryKeyStore();


    private async Task<IHost> BuildService([CallerMemberName] string function = "")
    {
        string basePath = nameof(DataSpaceListTests) + "/" + function;

        var host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddLogging(c => c.AddLambda(_outputHelper.WriteLine).AddDebug().AddFilter(_ => true));
                AddStore(services, basePath);

                services.AddDataSpace(cnfg =>
                {
                    cnfg.Spaces.Add(new SpaceDefinition
                    {
                        Name = "sequence",
                        ProviderName = "sequenceStore",
                        BasePath = "sequenceBase",
                        SpaceFormat = SpaceFormat.Sequence,
                    });
                    cnfg.Add<SequenceSpaceProvider>("sequenceStore");
                });

                services.AddSequenceStore<TestRecord>("sequence");
                services.AddSingleton<LogSequenceNumber>();
            })
            .Build();

        // Clear the store before running tests, this includes any locked files
        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();
        await keyStore.DeleteFolder(basePath);
        (await keyStore.Search($"{basePath}/***")).Count().Be(0); ;

        return host;
    }

    private static IEnumerable<TestRecord> CreateTestEntries(int count) => Enumerable.Range(1, count)
        .Select(i => new TestRecord($"Person{i}", 20 + i));

    private static string BuildSequencePath(SequenceKeySystem<TestRecord> keySystem, string key, DateTime timestamp, int counter, string random = "abcd") =>
        $"{keySystem.GetPathPrefix()}/{key}/{key}-{new DateTimeOffset(timestamp).ToUnixTimeMilliseconds():D15}-{counter:D6}-{random}.{typeof(TestRecord).Name}.json"
            .ToLowerInvariant();

    [Fact]
    public async Task SingleItemInSequence()
    {
        using var host = await BuildService();
        ISequenceStore<TestRecord> sequenceStore = host.Services.GetRequiredService<ISequenceStore<TestRecord>>();

        var ls = sequenceStore as SequenceSpace<TestRecord> ?? throw new ArgumentException();
        var fileStore = ls.SequenceKeySystem;

        const string key = nameof(SingleItemInSequence);

        string pathPrefix = fileStore.GetPathPrefix();
        string fullPath = fileStore.PathBuilder(key);
        string shouldMatch = fullPath.Replace($"{pathPrefix}/", string.Empty);

        var testRecord = new TestRecord("Test", 30);
        (await sequenceStore.Add(key, testRecord)).BeOk();

        (await sequenceStore.Get(key)).BeOk().Return().Action(x =>
        {
            x.Count.Be(1);
            x.SequenceEqual([testRecord]).BeTrue();
        });

        (await sequenceStore.Delete(key)).BeOk();
        (await sequenceStore.Get(key)).Return().Count.Be(0);
    }

    [Fact]
    public async Task SearchShouldStripBasePath()
    {
        using var host = await BuildService();
        ISequenceStore<TestRecord> sequenceStore = host.Services.GetRequiredService<ISequenceStore<TestRecord>>();
        var ls = sequenceStore as SequenceSpace<TestRecord> ?? throw new ArgumentException();
        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();

        const string key = "search-strip";
        (await sequenceStore.Add(key, new TestRecord("One", 1))).BeOk();

        IReadOnlyList<StorePathDetail> raw = await keyStore.Search(ls.SequenceKeySystem.BuildKeySearch(key));
        raw.Count.Be(1);

        IReadOnlyList<StorePathDetail> search = await sequenceStore.GetDetails(key);
        search.Count.Be(1);

        string prefix = ls.SequenceKeySystem.GetPathPrefix();
        search.Single().Path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase).BeFalse();
        search.Single().Path.Be(raw.Single().Path.Replace($"{prefix}/", string.Empty, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task DeleteMissingKeyShouldReturnNotFound()
    {
        using var host = await BuildService();
        ISequenceStore<TestRecord> sequenceStore = host.Services.GetRequiredService<ISequenceStore<TestRecord>>();

        Option deleteResult = await sequenceStore.Delete("missing-key");
        deleteResult.StatusCode.Be(StatusCode.NotFound);
    }

    [Fact]
    public async Task GetHistoryShouldReturnFromPreviousBoundary()
    {
        using var host = await BuildService();
        ISequenceStore<TestRecord> sequenceStore = host.Services.GetRequiredService<ISequenceStore<TestRecord>>();
        var ls = sequenceStore as SequenceSpace<TestRecord> ?? throw new ArgumentException();
        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();

        const string key = "history-boundary";
        DateTime baseTime = new DateTime(2023, 11, 15, 9, 0, 0, DateTimeKind.Utc);

        var entries = new[]
        {
            (timestamp: baseTime, record: new TestRecord("First", 10)),
            (timestamp: baseTime.AddHours(1), record: new TestRecord("Second", 20)),
            (timestamp: baseTime.AddHours(2), record: new TestRecord("Third", 30)),
        };

        int counter = 1;
        foreach (var entry in entries)
        {
            string path = BuildSequencePath(ls.SequenceKeySystem, key, entry.timestamp, counter++);
            (await keyStore.Add(path, entry.record.ToJson().ToDataETag())).BeOk();
        }

        var history = (await sequenceStore.GetHistory(key, entries[2].timestamp)).BeOk().Return();
        history.Count.Be(2);
        history.Any(x => x.Name == "Second").BeTrue();
        history.Any(x => x.Name == "Third").BeTrue();
        history.Any(x => x.Name == "First").BeFalse();
    }

    [Fact]
    public async Task GetHistoryInFutureReturnsEmpty()
    {
        using var host = await BuildService();
        ISequenceStore<TestRecord> sequenceStore = host.Services.GetRequiredService<ISequenceStore<TestRecord>>();

        const string key = "future-history";
        (await sequenceStore.Add(key, new TestRecord("Now", 42))).BeOk();

        var result = (await sequenceStore.GetHistory(key, DateTime.UtcNow.AddHours(2))).BeOk().Return();
        result.Count.Be(0);
    }

    [Fact]
    public async Task ScaleSequentialAddShouldReturnAll()
    {
        using var host = await BuildService();
        ISequenceStore<TestRecord> sequenceStore = host.Services.GetRequiredService<ISequenceStore<TestRecord>>();

        const string key = "scale-sequential";
        TestRecord[] items = CreateTestEntries(1000).ToArray();

        foreach (var item in items)
        {
            (await sequenceStore.Add(key, item)).BeOk();
        }

        var result = (await sequenceStore.Get(key)).BeOk().Return();
        result.Count.Be(items.Length);
        result.SequenceEqual(items).BeTrue();
    }

    [Fact]
    public async Task ParallelAddsShouldRemainConsistentAndOrdered()
    {
        using var host = await BuildService();
        ISequenceStore<TestRecord> sequenceStore = host.Services.GetRequiredService<ISequenceStore<TestRecord>>();
        var ls = sequenceStore as SequenceSpace<TestRecord> ?? throw new ArgumentException();

        const string key = "scale-parallel";
        int writers = Math.Min(Environment.ProcessorCount, 8);
        int perWriter = 250;
        var expected = new ConcurrentBag<TestRecord>();

        var tasks = Enumerable.Range(0, writers).Select(async writer =>
        {
            for (int i = 0; i < perWriter; i++)
            {
                var record = new TestRecord($"P{writer}-#{i}", writer * 10_000 + i);
                expected.Add(record);
                (await sequenceStore.Add(key, record)).BeOk();
            }
        });

        await Task.WhenAll(tasks);

        var data = (await sequenceStore.Get(key)).BeOk().Return();
        data.Count.Be(writers * perWriter);

        data.ToHashSet().SetEquals(expected.ToHashSet()).BeTrue(); // All unique entries present

        IReadOnlyList<StorePathDetail> search = await sequenceStore.GetDetails(key);
        search.Count.Be(data.Count);

        // Ensure chronological order from the underlying key layout
        var times = search.Select(x => PartitionSchemas.ExtractSequenceNumberIndex(x.Path).LogTime).ToArray();
        times.Zip(times.Skip(1), (a, b) => a <= b).All(x => x).BeTrue();
    }

    [Fact]
    public async Task GetMissingKeyReturnsEmpty()
    {
        using var host = await BuildService();
        ISequenceStore<TestRecord> sequenceStore = host.Services.GetRequiredService<ISequenceStore<TestRecord>>();

        var result = (await sequenceStore.Get("missing")).BeOk().Return();
        result.Count.Be(0);
    }

    [Fact]
    public async Task GetHistoryBeforeFirstReturnsAll()
    {
        using var host = await BuildService();
        ISequenceStore<TestRecord> sequenceStore = host.Services.GetRequiredService<ISequenceStore<TestRecord>>();

        const string key = "history-before-first";
        var items = CreateTestEntries(3).ToArray();

        foreach (var item in items)
        {
            (await sequenceStore.Add(key, item)).BeOk();
        }

        // Earlier than any recorded entry
        var history = (await sequenceStore.GetHistory(key, DateTime.UtcNow.AddHours(-12))).BeOk().Return();
        history.Count.Be(items.Length);
        history.SequenceEqual(items).BeTrue();
    }
}
