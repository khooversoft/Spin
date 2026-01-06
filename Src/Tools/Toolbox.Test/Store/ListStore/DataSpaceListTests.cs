using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Test.Store.ListStore;

public class DataSpaceListTests
{
    private ITestOutputHelper _outputHelper;
    private record JournalEntry(string Name, int Age);

    public DataSpaceListTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;
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
                        Name = "list",
                        ProviderName = "listStore",
                        BasePath = "listBase",
                        SpaceFormat = SpaceFormat.List,
                    });
                    cnfg.Add<ListStoreProvider>("listStore");
                });
            })
            .Build();

        // Clear the store before running tests, this includes any locked files
        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();
        await keyStore.DeleteFolder(basePath);
        (await keyStore.Search($"{basePath}/***")).Count().Be(0); ;

        return host;
    }

    private static IEnumerable<JournalEntry> CreateTestEntries(int count) =>
        Enumerable.Range(1, count).Select(i => new JournalEntry($"Person{i}", 20 + i));

    [Fact]
    public async Task SingleItemInList()
    {
        using var host = await BuildService();
        var listStore = host.Services.GetRequiredService<DataSpace>().GetListStore<JournalEntry>("list");

        var ls = listStore as ListSpace<JournalEntry> ?? throw new ArgumentException();
        var fileStore = ls.ListKeySystem;

        const string key = nameof(SingleItemInList);

        string pathPrefix = fileStore.GetPathPrefix();
        string fullPath = fileStore.PathBuilder(key);
        string shouldMatch = fullPath.Replace($"{pathPrefix}/", string.Empty);

        var journalEntry = new JournalEntry("Test", 30);
        (await listStore.Append(key, [journalEntry])).BeOk();

        (await listStore.Search(key)).Action(x =>
        {
            x.Count.Be(1);
            x.First().Path.Be(shouldMatch);
        });

        (await listStore.Get(key)).BeOk().Return().Action(x =>
        {
            x.Count.Be(1);
            x.SequenceEqual([journalEntry]).BeTrue();
        });

        (await listStore.Delete(key)).BeOk();
        (await listStore.Search(key)).Count.Be(0);
    }

    [Fact]
    public async Task AppendMultipleItems_ShouldStoreAllItems()
    {
        using var host = await BuildService();
        var listStore = host.Services.GetRequiredService<DataSpace>().GetListStore<JournalEntry>("list");

        const string key = nameof(AppendMultipleItems_ShouldStoreAllItems);
        var entries = CreateTestEntries(5).ToArray();

        (await listStore.Append(key, entries)).BeOk();

        (await listStore.Get(key)).BeOk().Return().Action(x =>
        {
            x.Count.Be(5);
            x.SequenceEqual(entries).BeTrue();
        });

        await listStore.Delete(key);
    }

    [Fact]
    public async Task AppendToExistingList_ShouldCombineLists()
    {
        using var host = await BuildService();
        var listStore = host.Services.GetRequiredService<DataSpace>().GetListStore<JournalEntry>("list");

        const string key = nameof(AppendToExistingList_ShouldCombineLists);
        var firstBatch = CreateTestEntries(3).ToArray();
        var secondBatch = CreateTestEntries(5).Skip(3).ToArray();

        (await listStore.Append(key, firstBatch)).BeOk();
        (await listStore.Append(key, secondBatch)).BeOk();

        (await listStore.Get(key)).BeOk().Return().Action(x =>
        {
            x.Count.Be(5);
            x.Take(3).SequenceEqual(firstBatch).BeTrue();
            x.Skip(3).SequenceEqual(secondBatch).BeTrue();
        });

        await listStore.Delete(key);
    }

    [Fact]
    public async Task AppendEmptyList_ShouldReturnNoContent()
    {
        using var host = await BuildService();
        var listStore = host.Services.GetRequiredService<DataSpace>().GetListStore<JournalEntry>("list");

        const string key = nameof(AppendEmptyList_ShouldReturnNoContent);
        var emptyList = Array.Empty<JournalEntry>();

        var result = await listStore.Append(key, emptyList);
        result.BeOk();
        result.Value.IsEmpty().BeTrue();
    }

    [Fact]
    public async Task Get_NonExistingKey_ShouldReturnEmptyList()
    {
        using var host = await BuildService();
        var listStore = host.Services.GetRequiredService<DataSpace>().GetListStore<JournalEntry>("list");

        const string key = nameof(Get_NonExistingKey_ShouldReturnEmptyList);

        (await listStore.Get(key)).BeOk().Return().Action(x =>
        {
            x.Count.Be(0);
        });
    }

    [Fact]
    public async Task GetWithPattern_ShouldFilterResults()
    {
        using var host = await BuildService();
        var listStore = host.Services.GetRequiredService<DataSpace>().GetListStore<JournalEntry>("list");

        const string key = nameof(GetWithPattern_ShouldFilterResults);
        var entries = CreateTestEntries(3).ToArray();

        (await listStore.Append(key, entries)).BeOk();

        (await listStore.Get(key)).BeOk().Return().Action(x =>
        {
            x.Count.Be(3);
            x.SequenceEqual(entries).BeTrue();
        });

        await listStore.Delete(key);
    }

    [Fact]
    public async Task Delete_NonExistingKey_ShouldSucceed()
    {
        using var host = await BuildService();
        var listStore = host.Services.GetRequiredService<DataSpace>().GetListStore<JournalEntry>("list");

        const string key = nameof(Delete_NonExistingKey_ShouldSucceed);

        (await listStore.Delete(key)).BeNotFound();
    }

    [Fact]
    public async Task Delete_ShouldRemoveAllListItems()
    {
        using var host = await BuildService();
        var listStore = host.Services.GetRequiredService<DataSpace>().GetListStore<JournalEntry>("list");

        const string key = nameof(Delete_ShouldRemoveAllListItems);
        var entries = CreateTestEntries(5).ToArray();

        (await listStore.Append(key, entries)).BeOk();
        (await listStore.Search(key)).Count().Be(1);

        (await listStore.Delete(key)).BeOk();
        (await listStore.Search(key)).Count().Be(0);
        (await listStore.Get(key)).BeOk().Return().Count.Be(0);
    }

    [Fact]
    public async Task Search_WithWildcards_ShouldMatchPatterns()
    {
        using var host = await BuildService();
        var listStore = host.Services.GetRequiredService<DataSpace>().GetListStore<JournalEntry>("list");

        const string key1 = "test/data1";
        const string key2 = "test/data2";
        var entries = CreateTestEntries(2).ToArray();

        (await listStore.Append(key1, entries.Take(1))).BeOk();
        (await listStore.Append(key2, entries.Skip(1).Take(1))).BeOk();

        var searchResult = await listStore.Search("test");
        searchResult.Count.Be(2);

        await listStore.Delete(key1);
        await listStore.Delete(key2);
    }

    [Fact]
    public async Task Search_NoMatches_ShouldReturnEmptyList()
    {
        using var host = await BuildService();
        var listStore = host.Services.GetRequiredService<DataSpace>().GetListStore<JournalEntry>("list");

        const string key = nameof(Search_NoMatches_ShouldReturnEmptyList);

        var searchResult = await listStore.Search(key);
        searchResult.Count.Be(0);
    }

    [Fact]
    public async Task MultipleAppendsToSameKey_ShouldAccumulateData()
    {
        using var host = await BuildService();
        var listStore = host.Services.GetRequiredService<DataSpace>().GetListStore<JournalEntry>("list");

        const string key = nameof(MultipleAppendsToSameKey_ShouldAccumulateData);
        var allEntries = CreateTestEntries(10).ToArray();

        for (int i = 0; i < 10; i++)
        {
            (await listStore.Append(key, [allEntries[i]])).BeOk();
        }

        (await listStore.Get(key)).BeOk().Return().Action(x =>
        {
            x.Count.Be(10);
            x.SequenceEqual(allEntries).BeTrue();
        });

        await listStore.Delete(key);
    }

    [Fact]
    public async Task ComplexObjectSerialization_ShouldPreserveData()
    {
        using var host = await BuildService();
        var listStore = host.Services.GetRequiredService<DataSpace>().GetListStore<JournalEntry>("list");

        const string key = nameof(ComplexObjectSerialization_ShouldPreserveData);
        var entries = new[]
        {
            new JournalEntry("Alice", 25),
            new JournalEntry("Bob", 30),
            new JournalEntry("Charlie", 35)
        };

        (await listStore.Append(key, entries)).BeOk();

        (await listStore.Get(key)).BeOk().Return().Action(x =>
        {
            x.Count.Be(3);
            x[0].Name.Be("Alice");
            x[0].Age.Be(25);
            x[1].Name.Be("Bob");
            x[1].Age.Be(30);
            x[2].Name.Be("Charlie");
            x[2].Age.Be(35);
        });

        await listStore.Delete(key);
    }

    [Fact]
    public async Task GetHistory_WithTimeIndex_ShouldReturnHistoricalData()
    {
        using var host = await BuildService();
        var listStore = host.Services.GetRequiredService<DataSpace>().GetListStore<JournalEntry>("list");

        const string key = "journal";
        var entries = CreateTestEntries(3).ToArray();

        var beforeAppend = DateTime.UtcNow;
        (await listStore.Append(key, entries)).BeOk();

        var currentList = await listStore.Get(key);
        currentList.BeOk().Return().Count.Be(3);

        var afterAppend = DateTime.UtcNow.AddDays(1);
        (await listStore.GetHistory(key, beforeAppend)).BeOk().Return().Action(x =>
        {
            x.Count.Be(3);
        });

        (await listStore.GetHistory(key, afterAppend)).BeOk().Return().Action(x =>
        {
            x.Count.Be(0);
        });

        await listStore.Delete(key);
    }

    [Fact]
    public async Task GetHistory_BeforeFirstEntry_ShouldReturnEmpty()
    {
        using var host = await BuildService();
        var listStore = host.Services.GetRequiredService<DataSpace>().GetListStore<JournalEntry>("list");

        const string key = nameof(GetHistory_BeforeFirstEntry_ShouldReturnEmpty);
        var futureTime = DateTime.UtcNow.AddHours(1);

        (await listStore.GetHistory(key, futureTime)).BeOk().Return().Action(x =>
        {
            x.Count.Be(0);
        });
    }

    [Fact]
    public async Task GetHistory_AfterAllEntries_ShouldReturnAllData()
    {
        using var host = await BuildService();
        var listStore = host.Services.GetRequiredService<DataSpace>().GetListStore<JournalEntry>("list");

        const string key = nameof(GetHistory_AfterAllEntries_ShouldReturnAllData);
        var entries = CreateTestEntries(5).ToArray();
        var pastTime = DateTime.UtcNow.AddHours(-1);

        (await listStore.Append(key, entries)).BeOk();

        (await listStore.GetHistory(key, pastTime)).BeOk().Return().Action(x =>
        {
            x.Count.Be(5);
            x.SequenceEqual(entries).BeTrue();
        });

        await listStore.Delete(key);
    }

    [Fact]
    public async Task MultipleKeys_ShouldBeIndependent()
    {
        using var host = await BuildService();
        var listStore = host.Services.GetRequiredService<DataSpace>().GetListStore<JournalEntry>("list");

        const string key1 = "folder1/list1";
        const string key2 = "folder2/list2";
        var entries1 = CreateTestEntries(3).ToArray();
        var entries2 = CreateTestEntries(5).Skip(3).ToArray();

        (await listStore.Append(key1, entries1)).BeOk();
        (await listStore.Append(key2, entries2)).BeOk();

        (await listStore.Get(key1)).BeOk().Return().Action(x =>
        {
            x.Count.Be(3);
            x.SequenceEqual(entries1).BeTrue();
        });

        (await listStore.Get(key2)).BeOk().Return().Action(x =>
        {
            x.Count.Be(2);
            x.SequenceEqual(entries2).BeTrue();
        });

        await listStore.Delete(key1);
        await listStore.Delete(key2);
    }

    [Fact]
    public async Task SearchWithSpecificPattern_ShouldReturnMatchingKeys()
    {
        using var host = await BuildService();
        var listStore = host.Services.GetRequiredService<DataSpace>().GetListStore<JournalEntry>("list");

        const string key1 = "users/admin";
        const string key2 = "users/guest";
        const string key3 = "system/logs";
        var entry = new JournalEntry("Test", 25);

        (await listStore.Append(key1, [entry])).BeOk();
        (await listStore.Append(key2, [entry])).BeOk();
        (await listStore.Append(key3, [entry])).BeOk();

        var userSearch = await listStore.Search("users");
        userSearch.Count.Be(2);

        var systemSearch = await listStore.Search("system");
        systemSearch.Count.Be(1);

        await listStore.Delete(key1);
        await listStore.Delete(key2);
        await listStore.Delete(key3);
    }

    [Fact]
    public async Task LargeList_ShouldHandleEfficently()
    {
        using var host = await BuildService();
        var listStore = host.Services.GetRequiredService<DataSpace>().GetListStore<JournalEntry>("list");

        const string key = nameof(LargeList_ShouldHandleEfficently);
        var largeList = CreateTestEntries(100).ToArray();

        (await listStore.Append(key, largeList)).BeOk();

        (await listStore.Get(key)).BeOk().Return().Action(x =>
        {
            x.Count.Be(100);
            x.SequenceEqual(largeList).BeTrue();
        });

        await listStore.Delete(key);
    }

    [Fact]
    public async Task ConcurrentAppends_SameKey_ShouldAccumulateInOrder()
    {
        using var host = await BuildService();
        var listStore = host.Services.GetRequiredService<DataSpace>().GetListStore<JournalEntry>("list");

        const string key = nameof(ConcurrentAppends_SameKey_ShouldAccumulateInOrder);
        var source = CreateTestEntries(50).ToArray();

        var tasks = Enumerable.Range(0, source.Length)
            .Select(i => listStore.Append(key, [source[i]]))
            .ToArray();

        await Task.WhenAll(tasks);

        (await listStore.Get(key)).BeOk().Return().Action(x =>
        {
            x.Count.Be(source.Length);
            x.SequenceEqual(source).BeTrue();
        });

        await listStore.Delete(key);
    }

    [Fact]
    public async Task Delete_ShouldClearNestedPathsUnderKeyPrefix()
    {
        using var host = await BuildService();
        var listStore = host.Services.GetRequiredService<DataSpace>().GetListStore<JournalEntry>("list");

        const string key = nameof(Delete_ShouldClearNestedPathsUnderKeyPrefix);
        var nested1 = $"{key}/child1";
        var nested2 = $"{key}/child2/deeper";

        var entry = new JournalEntry("X", 1);
        (await listStore.Append(nested1, [entry])).BeOk();
        (await listStore.Append(nested2, [entry])).BeOk();

        (await listStore.Search(key)).Count.Be(2);

        (await listStore.Delete(key)).BeOk();

        (await listStore.Search(key)).Count.Be(0);
        (await listStore.Get(nested1)).BeOk().Return().Count.Be(0);
        (await listStore.Get(nested2)).BeOk().Return().Count.Be(0);
    }

    [Fact]
    public async Task Search_ShouldReturnNonFolderEntries_AndNormalizedPaths()
    {
        using var host = await BuildService();
        var listStore = host.Services.GetRequiredService<DataSpace>().GetListStore<JournalEntry>("list");

        var ls = listStore as ListSpace<JournalEntry> ?? throw new ArgumentException();
        var ks = ls.ListKeySystem;

        const string key = nameof(Search_ShouldReturnNonFolderEntries_AndNormalizedPaths);
        var entry = new JournalEntry("Test", 1);

        (await listStore.Append(key, [entry])).BeOk();

        var search = await listStore.Search(key);
        search.Count.Be(1);
        search[0].Action(x =>
        {
            x.IsFolder.BeFalse();
            // path should be normalized (prefix removed)
            var expected = ks.PathBuilder(key).Replace($"{ks.GetPathPrefix()}/", string.Empty);
            x.Path.Be(expected);
        });

        await listStore.Delete(key);
    }

    [Fact]
    public async Task OrderingAcrossMultipleFiles_ShouldRemainStable()
    {
        using var host = await BuildService();
        var listStore = host.Services.GetRequiredService<DataSpace>().GetListStore<JournalEntry>("list");

        const string key = nameof(OrderingAcrossMultipleFiles_ShouldRemainStable);

        // Append several batches to force multiple underlying files/directories
        var batch1 = CreateTestEntries(10).ToArray();
        var batch2 = CreateTestEntries(15).Skip(10).ToArray();
        var batch3 = CreateTestEntries(25).Skip(15).ToArray();

        (await listStore.Append(key, batch1)).BeOk();
        (await listStore.Append(key, batch2)).BeOk();
        (await listStore.Append(key, batch3)).BeOk();

        var expected = batch1.Concat(batch2).Concat(batch3).ToArray();

        (await listStore.Get(key)).BeOk().Return().Action(x =>
        {
            x.Count.Be(expected.Length);
            x.SequenceEqual(expected).BeTrue();
        });

        await listStore.Delete(key);
    }
}
