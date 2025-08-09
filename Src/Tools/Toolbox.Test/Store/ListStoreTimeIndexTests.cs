using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Models;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Test.Store;

public class ListStoreTimeIndexTests
{
    private readonly ITestOutputHelper _outputHelper;
    private record JournalEntry(string Name, int Age);
    public ListStoreTimeIndexTests(ITestOutputHelper output) => _outputHelper = output.NotNull();

    protected virtual void AddStore(IServiceCollection services)
    {
        services.AddInMemoryFileStore();
        services.AddSingleton<IListFileSystem<DataChangeEntry>, ListSecondFileSystem<DataChangeEntry>>();
    }

    public async Task<IHost> BuildService(bool useQueue)
    {
        var host = Host.CreateDefaultBuilder()
        .ConfigureServices((context, services) =>
        {
            services.AddLogging(config => config.AddLambda(_outputHelper.WriteLine).AddDebug().AddFilter(x => true));
            AddStore(services);
            services.AddListStore<DataChangeEntry>(config => useQueue.IfTrue(() => config.AddBatchProvider()));

        })
        .Build();

        await host.ClearStore<ListStoreTimeIndexTests>();
        return host;
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GetHistoryByTimeIndex(bool useQueue)
    {
        using var host = await BuildService(useQueue);
        var context = host.Services.CreateContext<ListStoreTimeIndexTests>();
        var fileStore = host.Services.GetRequiredService<IFileStore>();
        var listStore = host.Services.GetRequiredService<IListStore<DataChangeEntry>>();
        var fileSystem = host.Services.GetRequiredService<IListFileSystem<DataChangeEntry>>();
        var lsn = new LogSequenceNumber();

        const string key = nameof(GetHistoryByTimeIndex);

        var token = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        int count = 0;
        var entrySequence = new Sequence<DataChangeEntry>();
        var journalSequence = new Sequence<JournalEntry>();

        while (!token.IsCancellationRequested)
        {
            var entry = new JournalEntry($"Test{count}", 30 + count);

            var journalEntry = new DataChangeEntry
            {
                LogSequenceNumber = lsn.Next(),
                TransactionId = Guid.NewGuid().ToString(),
                TypeName = nameof(JournalEntry),
                SourceName = "source",
                ObjectId = "Test-" + count++,
                Action = "add",
                After = entry.ToDataETag(),
            };

            entrySequence += journalEntry;
            journalSequence += entry;

            (await listStore.Append(key, [journalEntry], context)).BeOk();
        }

        (await listStore.Get(key, context)).BeOk().Return().SequenceEqual(entrySequence).BeTrue();

        await entrySequence.First().Func(async selectedEntry => await testEntry(selectedEntry));
        await entrySequence.Last().Func(async selectedEntry => await testEntry(selectedEntry));
        await entrySequence.Shuffle().First().Func(async selectedEntry => await testEntry(selectedEntry));

        (await listStore.Delete(key, context)).BeOk();
        (await listStore.Search(key, "**/*", context)).Count.Be(0);
        (await fileStore.Search("**/*", context)).Count.Be(0);

        async Task testEntry(DataChangeEntry selectedEntry)
        {
            DateTime timeIndex = LogSequenceNumber.ConvertToDateTime(selectedEntry.LogSequenceNumber);

            var readOption = await listStore.GetHistory(key, timeIndex, context);

            var list = readOption.BeOk().Return()
                .FirstOrDefault(x => x.LogSequenceNumber == selectedEntry.LogSequenceNumber)
                .NotNull($"Did not find log sequence number, lsn={selectedEntry.LogSequenceNumber}");
        }
    }
}
