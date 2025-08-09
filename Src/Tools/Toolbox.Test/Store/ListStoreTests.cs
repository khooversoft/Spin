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

    public async Task<IHost> BuildService(bool useQueue)
    {
        var host = Host.CreateDefaultBuilder()
        .ConfigureServices((context, services) =>
        {
            services.AddLogging(config => config.AddLambda(_outputHelper.WriteLine).AddDebug().AddFilter(x => true));
            AddStore(services);
            services.AddListStore<JournalEntry>(config => useQueue.IfTrue(() => config.AddBatchProvider()));
        })
        .Build();

        await host.ClearStore<ListStoreTests>();
        return host;
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SingleItemInList(bool useQueue)
    {
        using var host = await BuildService(useQueue);
        var context = host.Services.CreateContext<ListStoreTests>();
        var fileStore = host.Services.GetRequiredService<IFileStore>();
        var listStore = host.Services.GetRequiredService<IListStore<JournalEntry>>();
        var fileSystem = host.Services.GetRequiredService<IListFileSystem<JournalEntry>>();

        const string key = nameof(SingleItemInList);

        string shouldMatch = fileSystem.PathBuilder(key);

        var journalEntry = new JournalEntry("Test", 30);
        (await listStore.Append(key, [journalEntry], context)).BeOk();
        (await listStore.Search(key, "**/*", context)).Action(x =>
        {
            x.Count.Be(1);
            x.First().Path.EndsWith(shouldMatch).BeTrue();
        });

        (await listStore.Get(key, context)).BeOk().Return().Action(x =>
        {
            x.Count.Be(1);
            x.SequenceEqual([journalEntry]).BeTrue();
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

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task MultipleItemDailySchema(bool useQueue)
    {
        using var host = await BuildService(useQueue);
        var context = host.Services.CreateContext<ListStoreTests>();
        var fileStore = host.Services.GetRequiredService<IFileStore>();
        var listStore = host.Services.GetRequiredService<IListStore<JournalEntry>>();
        var fileSystem = host.Services.GetRequiredService<IListFileSystem<JournalEntry>>();

        const string key = nameof(SingleItemInList);

        var token = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        int count = 0;
        var sequence = new Sequence<JournalEntry>();

        while (!token.IsCancellationRequested)
        {
            string shouldMatch = fileSystem.PathBuilder(key);

            var journalEntry = new JournalEntry($"Test{count++}", 30 + count);
            sequence += journalEntry;

            (await listStore.Append(key, [journalEntry], context)).BeOk();

            (await listStore.Search(key, "**/*", context)).Action(x =>
            {
                x.Count.Be(1);
                x.First().Path.EndsWith(shouldMatch).BeTrue();
            });

            (await listStore.Get(key, context)).BeOk().Return().Action(x =>
            {
                x.Count.Be(sequence.Count);
                x.SequenceEqual(sequence).BeTrue();
            });

            IReadOnlyList<IStorePathDetail> searchList = await listStore.Search(key, "**/*", context);
            searchList.NotNull().Count.Be(1);
            searchList[0].Action(x =>
            {
                x.Path.Be(shouldMatch);
            });

            (await fileStore.Search("**/*", context)).Count.Be(1);
        }

        (await fileStore.Search("**/*", context)).Count.Be(1);

        (await listStore.Delete(key, context)).BeOk();
        (await listStore.Search(key, "**/*", context)).Count.Be(0);
        (await fileStore.Search("**/*", context)).Count.Be(0);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task MultipleItemSecondSchema(bool useQueue)
    {
        using var host = await BuildService(useQueue);
        var context = host.Services.CreateContext<ListStoreTests>();
        var fileStore = host.Services.GetRequiredService<IFileStore>();
        var listStore = host.Services.GetRequiredService<IListStore<JournalEntry>>();
        var fileSystem = host.Services.GetRequiredService<IListFileSystem<JournalEntry>>();

        const string key = nameof(SingleItemInList);

        var token = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        int count = 0;
        var sequence = new Sequence<JournalEntry>();

        while (!token.IsCancellationRequested)
        {
            string shouldMatch = fileSystem.PathBuilder(key);

            var journalEntry = new JournalEntry($"Test{count++}", 30 + count);
            sequence += journalEntry;

            (await listStore.Append(key, [journalEntry], context)).BeOk();

            (await listStore.Search(key, "**/*", context)).Action(x =>
            {
                x.Count.Assert(x => x > 0 && x <= 6, x => $"{x} Journal file count should be 1 => && 6 <=");
            });

            var list = (await listStore.Get(key, context)).BeOk().Return();

            list.SequenceEqual(sequence).BeTrue();
            context.LogDebug("Returned list count={count}", list.Count);

            (await fileStore.Search("**/*", context)).Count.Assert(x => x > 0 && x <= 6, "Journal file count should be 1 => && 6 <=");
        }

        context.LogDebug("Final Count={count}, sequence.count={seqCount}", count, sequence.Count);

        (await listStore.Delete(key, context)).BeOk();
        (await listStore.Search(key, "**/*", context)).Count.Be(0);
        (await fileStore.Search("**/*", context)).Count.Be(0);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Performance(bool useQueue)
    {
        using var host = await BuildService(useQueue);
        var context = host.Services.CreateContext<ListStoreTests>();
        var fileStore = host.Services.GetRequiredService<IFileStore>();
        var listStore = host.Services.GetRequiredService<IListStore<JournalEntry>>();
        var fileSystem = host.Services.GetRequiredService<IListFileSystem<JournalEntry>>();

        const string key = nameof(Performance);

        var token = new CancellationTokenSource(TimeSpan.FromSeconds(1));
        int count = 0;
        var sequence = new Sequence<JournalEntry>();
        DateTime start = DateTime.Now;

        while (!token.IsCancellationRequested)
        {
            var journalEntry = new JournalEntry($"Test{count++}", 30 + count);
            sequence += journalEntry;

            (await listStore.Append(key, [journalEntry], context)).BeOk();
        }

        (await listStore.Get(key, context)).BeOk().Return().Action(x =>
        {
            x.Count.Be(count);
            x.SequenceEqual(sequence).BeTrue();
        });

        TimeSpan timing = DateTime.Now - start;
        double tps = count / timing.TotalSeconds;
        context.LogDebug("Performance: Count={count}, sequence.count={seqCount}, timing={timing}, tps={tps}", count, sequence.Count, timing, tps);

        //(await listStore.Delete(key, context)).BeOk();
        //(await listStore.Search(key, "**/*", context)).Count.Be(0);
        //(await fileStore.Search("**/*", context)).Count.Be(0);
    }
}
