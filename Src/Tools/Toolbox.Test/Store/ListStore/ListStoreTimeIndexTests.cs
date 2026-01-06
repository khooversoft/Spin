using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Test.Store.ListStore;

public class ListStoreTimeIndexTests
{
    private ITestOutputHelper _outputHelper;
    private record JournalEntry(string Name, int Age);
    public ListStoreTimeIndexTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

    protected virtual void AddStore(IServiceCollection services, string basePath) => services.AddInMemoryKeyStore();

    private async Task<IHost> BuildService([CallerMemberName] string function = "")
    {
        string basePath = nameof(ListStoreTimeIndexTests) + "/" + function;

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

    [Fact]
    public async Task GetHistoryByTimeIndex()
    {
        using var host = await BuildService();
        var listStore = host.Services.GetRequiredService<DataSpace>().GetListStore<DataChangeEntry>("list");
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
                //ObjectId = "Test-" + count++,
                Action = "add",
                After = entry.ToDataETag(),
            };

            entrySequence += journalEntry;
            journalSequence += entry;

            (await listStore.Append(key, [journalEntry])).BeOk();
        }

        (await listStore.Get(key)).BeOk().Return().SequenceEqual(entrySequence).BeTrue();

        await entrySequence.First().Func(async selectedEntry => await testEntry(selectedEntry));

        await entrySequence.Last().Func(async selectedEntry => await testEntry(selectedEntry));
        await entrySequence.Shuffle().First().Func(async selectedEntry => await testEntry(selectedEntry));

        (await listStore.Delete(key)).BeOk();
        (await listStore.Search(key)).Count.Be(0);

        async Task testEntry(DataChangeEntry selectedEntry)
        {
            DateTime timeIndex = LogSequenceNumber.ConvertToDateTime(selectedEntry.LogSequenceNumber);

            var readOption = await listStore.GetHistory(key, timeIndex);

            var list = readOption.BeOk().Return()
                .FirstOrDefault(x => x.LogSequenceNumber == selectedEntry.LogSequenceNumber)
                .NotNull($"Did not find log sequence number, lsn={selectedEntry.LogSequenceNumber}");
        }
    }
}
