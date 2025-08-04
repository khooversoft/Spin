using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Test.Store;

public class ListStoreTests
{
    private readonly ITestOutputHelper _outputHelper;
    private record JournalEntry(string Name, int Age);
    public ListStoreTests(ITestOutputHelper output) => _outputHelper = output.NotNull();

    protected virtual void AddStore(IServiceCollection services) => services.AddInMemoryFileStore();

    public async Task<IHost> BuildService()
    {
        var host = Host.CreateDefaultBuilder()
        .ConfigureServices((context, services) =>
        {
            services.AddLogging(config => config.AddLambda(_outputHelper.WriteLine).AddDebug().AddFilter(x => true));
            AddStore(services);
            services.AddSingleton<IListStore, ListStore>();
            services.AddSingleton<IListFileSystem, ListFileSystem>();
        })
        .Build();

        await host.ClearStore<ListStoreTests>();
        return host;
    }

    [Fact]
    public async Task SingleItemInList()
    {
        using var host = await BuildService();
        var context = host.Services.CreateContext<ListStoreTests>();
        var fileStore = host.Services.GetRequiredService<IFileStore>();
        var listStore = host.Services.GetRequiredService<IListStore>();
        var fileSystem = host.Services.GetRequiredService<IListFileSystem>();

        const string key = nameof(SingleItemInList);
        const string listType = "TestList";

        string shouldMatch = fileSystem.PathBuilder(key, listType);

        var journalEntry = new JournalEntry("Test", 30);
        (await listStore.Append(key, listType, [journalEntry.ToDataETag()], context)).BeOk();
        (await listStore.Search(key, "**/*", context)).Action(x =>
        {
            x.Count.Be(1);
            x.First().Path.EndsWith(shouldMatch).BeTrue();
        });

        (await listStore.Get(key, context)).BeOk().Return().Action(x =>
        {
            x.Count.Be(1);
            x[0].Action(y =>
            {
                y.PathDetail.Path.Be(shouldMatch);
                JournalEntry[] dataItems = y.Data.Select(z => z.ToObject<JournalEntry>()).ToArray();

                dataItems.Length.Be(1);
                var x = dataItems.SequenceEqual([journalEntry]).BeTrue();
            });
        });

        IReadOnlyList<IStorePathDetail> searchList = await listStore.Search(key, "**/*", context);
        searchList.NotNull().Count.Be(1);
        searchList[0].Action(x =>
        {
            x.Path.Be(shouldMatch);
        });

        (await fileStore.Search("**/*", context)).Count.Be(1);

        (await listStore.Delete(key, context)).BeOk();
        (await listStore.Search(key, "**/*", context)).Count.Be(0);

        (await fileStore.Search("**/*", context)).Count.Be(0);
    }

    [Fact]
    public async Task MultipleItemDailySchema()
    {
        using var host = await BuildService();
        var context = host.Services.CreateContext<ListStoreTests>();
        var fileStore = host.Services.GetRequiredService<IFileStore>();
        var listStore = host.Services.GetRequiredService<IListStore>();
        var fileSystem = host.Services.GetRequiredService<IListFileSystem>();

        const string key = nameof(SingleItemInList);
        const string listType = "TestList";

        var token = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        int count = 0;
        var sequence = new Sequence<JournalEntry>();

        while (!token.IsCancellationRequested)
        {
            string shouldMatch = fileSystem.PathBuilder(key, listType);

            var journalEntry = new JournalEntry($"Test{count++}", 30 + count);
            sequence += journalEntry;

            (await listStore.Append(key, listType, [journalEntry.ToDataETag()], context)).BeOk();

            (await listStore.Search(key, "**/*", context)).Action(x =>
            {
                x.Count.Be(1);
                x.First().Path.EndsWith(shouldMatch).BeTrue();
            });

            (await listStore.Get(key, context)).BeOk().Return().Action(x =>
            {
                x.Count.Be(1);
                x[0].Action(y =>
                {
                    y.PathDetail.Path.Be(shouldMatch);
                    JournalEntry[] dataItems = y.Data.Select(z => z.ToObject<JournalEntry>()).ToArray();

                    dataItems.Length.Be(sequence.Count);
                    dataItems.SequenceEqual(sequence).BeTrue();
                });
            });

            IReadOnlyList<IStorePathDetail> searchList = await listStore.Search(key, "**/*", context);
            searchList.NotNull().Count.Be(1);
            searchList[0].Action(x =>
            {
                x.Path.Be(shouldMatch);
            });

            (await fileStore.Search("**/*", context)).Count.Be(1);
        }

        context.LogDebug("Final Count={count}, sequence.count={seqCount}", count, sequence.Count);
        (await fileStore.Search("**/*", context)).Count.Be(1);

        (await listStore.Delete(key, context)).BeOk();
        (await listStore.Search(key, "**/*", context)).Count.Be(0);
        (await fileStore.Search("**/*", context)).Count.Be(0);
    }

    [Fact]
    public async Task MultipleItemSecondSchema()
    {
        using var host = await BuildService();
        var context = host.Services.CreateContext<ListStoreTests>();
        var fileStore = host.Services.GetRequiredService<IFileStore>();
        var listStore = host.Services.GetRequiredService<IListStore>();
        var fileSystem = host.Services.GetRequiredService<IListFileSystem>();

        const string key = nameof(SingleItemInList);
        const string listType = "TestList";

        var token = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        int count = 0;
        var sequence = new Sequence<JournalEntry>();

        while (!token.IsCancellationRequested)
        {
            string shouldMatch = fileSystem.PathBuilder(key, listType);

            var journalEntry = new JournalEntry($"Test{count++}", 30 + count);
            sequence += journalEntry;

            (await listStore.Append(key, listType, [journalEntry.ToDataETag()], context)).BeOk();

            (await listStore.Search(key, "**/*", context)).Action(x =>
            {
                x.Count.Assert(x => x > 0 && x <= 6, x => $"{x} Journal file count should be 1 => && 6 <=");
            });

            var list = (await listStore.Get(key, context))
                .BeOk().Return()
                .SelectMany(z => z.Data)
                .Select(x => x.ToObject<JournalEntry>())
                .ToArray();

            list.SequenceEqual(sequence).BeTrue();
            context.LogDebug("Returned list count={count}", list.Length);

            (await fileStore.Search("**/*", context)).Count.Assert(x => x > 0 && x <= 6, "Journal file count should be 1 => && 6 <=");
        }

        context.LogDebug("Final Count={count}, sequence.count={seqCount}", count, sequence.Count);

        (await listStore.Delete(key, context)).BeOk();
        (await listStore.Search(key, "**/*", context)).Count.Be(0);
        (await fileStore.Search("**/*", context)).Count.Be(0);
    }
}
