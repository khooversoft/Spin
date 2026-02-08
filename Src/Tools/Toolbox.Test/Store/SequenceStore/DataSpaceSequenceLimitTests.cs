using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Toolbox.Store;
using Toolbox.Telemetry;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Test.Store.SequenceStore;

public class DataSpaceSequenceLimitTests
{
    private readonly ITestOutputHelper _outputHelper;
    private const string _key = "sequence-limit-key";
    private record TestRecord(string Name, int Age);

    public DataSpaceSequenceLimitTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;
    protected virtual void AddStore(IServiceCollection services, string basePath) => services.AddInMemoryKeyStore();


    private async Task<IHost> BuildService(bool deferred, bool noLogging = false, [CallerMemberName] string function = "")
    {
        string basePath = nameof(DataSpaceSequenceLimitTests) + "/" + function;

        var host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                if (!noLogging) services.AddLogging(c => c.AddLambda(_outputHelper.WriteLine).AddDebug().AddFilter(_ => true));

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
                services.AddSequenceLimit<TestRecord>(config =>
                {
                    config.Key = _key;
                    config.MaxItems = 10;
                    config.CheckInterval = deferred ? TimeSpan.FromMilliseconds(1000) : TimeSpan.Zero;  // force realtime
                });
                services.AddSingleton<LogSequenceNumber>();
                services.AddTelemetry();

                var option = new TelemetryOption()
                    .AddCollector(_ => new TestTelemetryCollector());
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

    [Fact]
    public async Task SequentialAddShouldReturnAll()
    {
        using var host = await BuildService(false);
        SequenceSizeLimit<TestRecord> limit = host.Services.GetRequiredService<SequenceSizeLimit<TestRecord>>();
        ISequenceStore<TestRecord> sequenceStore = host.Services.GetRequiredService<ISequenceStore<TestRecord>>();
        ILogger<DataSpaceSequenceLimitTests> logger = host.Services.GetRequiredService<ILogger<DataSpaceSequenceLimitTests>>();

        TestRecord[] items = CreateTestEntries(10).ToArray();

        const int size = 10;
        int count = 0;

        while (count++ < size)
        {
            using (var scope = logger.LogDuration($"Count={count}"))
            {
                foreach (var item in items)
                {
                    (await sequenceStore.Add(_key, item)).BeOk();
                }

                await limit.Cleanup();

                var result = (await sequenceStore.Get(_key)).BeOk().Return();
                result.Count.Be(items.Length);
                result.SequenceEqual(items).BeTrue();

                (await sequenceStore.Delete(_key)).BeOk();
            }
        }
    }

    [Fact]
    public async Task ScaleSequentialAddShouldReturnAllNoLogging()
    {
        using var host = await BuildService(false, true);
        SequenceSizeLimit<TestRecord> limit = host.Services.GetRequiredService<SequenceSizeLimit<TestRecord>>();
        ISequenceStore<TestRecord> sequenceStore = host.Services.GetRequiredService<ISequenceStore<TestRecord>>();

        TestRecord[] items = CreateTestEntries(1000).ToArray();

        const int size = 10;
        int count = 0;

        while (count++ < size)
        {
            foreach (var item in items)
            {
                (await sequenceStore.Add(_key, item)).BeOk();
            }

            await limit.Cleanup();

            var result = (await sequenceStore.Get(_key)).BeOk().Return();
            result.Count.Be(10);
            result.SequenceEqual(items.Skip(items.Length - 10)).BeTrue();

            (await sequenceStore.Delete(_key)).BeOk();
        }
    }

    [Fact]
    public async Task ScaleSequentialWithDeferredAddShouldReturnAll()
    {
        using var host = await BuildService(true);
        SequenceSizeLimit<TestRecord> limit = host.Services.GetRequiredService<SequenceSizeLimit<TestRecord>>();
        ISequenceStore<TestRecord> sequenceStore = host.Services.GetRequiredService<ISequenceStore<TestRecord>>();

        TestRecord[] items = CreateTestEntries(100).ToArray();

        foreach (var item in items)
        {
            (await sequenceStore.Add(_key, item)).BeOk();
        }

        await limit.Cleanup();

        var result = (await sequenceStore.Get(_key)).BeOk().Return();
        result.Count.Be(10);
        result.SequenceEqual(items.Skip(items.Length - 10)).BeTrue();
    }

    private class TestTelemetryCollector : ITelemetryCollector
    {
        public List<TelemetryEvent> ReceivedEvents { get; } = new();

        public void Post(TelemetryEvent telemetryEvent)
        {
            ReceivedEvents.Add(telemetryEvent);
        }
    }
}
