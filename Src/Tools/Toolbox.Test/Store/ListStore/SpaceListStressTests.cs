using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Test.Store.ListStore;

public class SpaceListStressTests
{
    private ITestOutputHelper _outputHelper;
    private record JournalEntry(string Name, int Age, string Data);

    public SpaceListStressTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;
    protected virtual void AddStore(IServiceCollection services, string basePath) => services.AddInMemoryKeyStore();

    private async Task<IHost> BuildService(int maxSize, [CallerMemberName] string function = "")
    {
        string basePath = nameof(SpaceListStressTests) + "/" + function + $"/{maxSize}";

        var host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddLogging(c => c.AddLambda(_outputHelper.WriteLine).AddDebug().AddFilter(_ => true));
                AddStore(services, basePath);
                services.AddSingleton<LogSequenceNumber>();

                services.AddDataSpace(cnfg =>
                {
                    cnfg.Spaces.Add(new SpaceDefinition
                    {
                        Name = "list",
                        ProviderName = "listStore",
                        BasePath = "listBase",
                        SpaceFormat = SpaceFormat.List,
                        MaxBlockSizeMB = maxSize,
                    });
                    cnfg.Add<ListStoreProvider>("listStore");
                });

                services.AddListStore<JournalEntry>("list");
            })
            .Build();

        // Clear the store before running tests, this includes any locked files
        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();
        await keyStore.DeleteFolder(basePath);
        (await keyStore.Search($"{basePath}/***")).Count().Be(0);

        return host;
    }

    private static IReadOnlyList<JournalEntry> CreateTestEntries(int start, int count) => Enumerable.Range(0, count)
        .Select(x => (i: x, index: start + x))
        .Select(i => new JournalEntry($"Person{i.index}", i.index, GenerateRandomData()))
        .ToArray();

    private static string GenerateRandomData()
    {
        int size = RandomNumberGenerator.GetInt32(5000, 10000);
        int newSize = (size & 1) == 0 ? size : size - 1; // ensure even
        string data = RandomTool.GenerateRandomSequence(newSize);
        int dataLength = $"{data.Length}".Length + 1 + data.Length;
        return $"{dataLength}:{data}";
    }

    [Fact]
    public async Task AppendLessThenSplitSize_ShouldStoreAllItems()
    {
        const int count = 1;
        const int loop = 5;
        using var host = await BuildService(4);
        var listStore = host.Services.GetRequiredService<IListStore<JournalEntry>>();
        var logger = host.Services.GetRequiredService<ILogger<JournalEntry>>();

        const string key = nameof(AppendMultipleItems_ShouldStoreAllItems);
        var list = new List<JournalEntry>();
        var startTime = Stopwatch.GetTimestamp();

        int total = 0;
        for (int i = 0; i < loop; i++)
        {
            var entries = CreateTestEntries(0, count);
            total += entries.Count;

            (await listStore.Append(key, entries)).BeOk();
            list.AddRange(entries);

            (await listStore.Get(key)).BeOk().Return().Action(x =>
            {
                x.Count.Be(total);
                x.SequenceEqual(list).BeTrue();
            });
        }

        var end = Stopwatch.GetElapsedTime(startTime);
        logger.LogInformation("Duration: ms={end}", end);

        await listStore.Delete(key);
    }

    [Theory]
    [InlineData(4)]
    [InlineData(8)]
    [InlineData(16)]
    [InlineData(32)]
    public async Task AppendMultipleItems_ShouldStoreAllItems(int maxSize)
    {
        using var host = await BuildService(maxSize);
        var listStore = host.Services.GetRequiredService<IListStore<JournalEntry>>();
        var logger = host.Services.GetRequiredService<ILogger<SpaceListStressTests>>();

        const string key = nameof(AppendMultipleItems_ShouldStoreAllItems);
        var list = new List<JournalEntry>();

        var startTime = Stopwatch.GetTimestamp();
        var disposeScope = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        int total = 0;
        int loop = RandomNumberGenerator.GetInt32(50, maxSize * 100);
        for (int i = 0; i < loop && !disposeScope.IsCancellationRequested; i++)
        {
            var count = RandomNumberGenerator.GetInt32(maxSize * 10, maxSize * 100);
            var entries = CreateTestEntries(total, count);
            total += entries.Count;
            list.AddRange(entries);

            logger.LogTrace("[Appending] entries={entriesCount}, listCount={listCount}", entries.Count, list.Count);
            (await listStore.Append(key, entries)).BeOk();
            logger.LogTrace("[Appended] entries={entriesCount}, listCount={listCount}", entries.Count, list.Count);

            (await listStore.Get(key)).BeOk().Return().Action(x =>
            {
                logger.LogTrace("[Get] count={count}, listCount={listCount}", x.Count, list.Count);
                x.Count.Be(total);
                x.SequenceEqual(list).BeTrue();
            });
        }

        var files = await listStore.Search(key);
        logger.LogInformation("Number of partitions, count={count}", files.Count);

        var fileDetails = await ((ListSpace<JournalEntry>)listStore).ReadListFiles(files);
        files.Count.Be(fileDetails.Count);

        var fileOrder = fileDetails.OrderBy(x => x.StorePathDetail.Path).ToArray();
        foreach (var file in fileOrder)
        {
            logger.LogInformation("File={path}, size={size}, items.Count={count}", file.StorePathDetail.Path, file.StorePathDetail.ContentLength, file.Items.Count);
        }

        var end = Stopwatch.GetElapsedTime(startTime);
        logger.LogInformation("Duration: span={end}, tps={tsp}, total={total}", end, total / end.TotalSeconds, total);
    }
}
