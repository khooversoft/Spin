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

namespace Toolbox.Test.Transactions;

public class TransactionRecoveryTests
{
    private ITestOutputHelper _outputHelper;
    private record TestRecord(string Name, int Age);

    public TransactionRecoveryTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

    private IHost BuildService([CallerMemberName] string function = "")
    {
        string basePath = nameof(TransactionBindTests) + "/" + function;

        var host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddLogging(c => c.AddLambda(_outputHelper.WriteLine).AddDebug().AddFilter(_ => true));
                services.AddInMemoryKeyStore();

                services.AddDataSpace(cnfg =>
                {
                    cnfg.Spaces.Add(new SpaceDefinition
                    {
                        Name = "list",
                        ProviderName = "listStore",
                        BasePath = basePath + "/listBase",
                        SpaceFormat = SpaceFormat.List,
                    });
                    cnfg.Add<ListStoreProvider>("listStore");

                    cnfg.Spaces.Add(new SpaceDefinition
                    {
                        Name = "keyStore",
                        ProviderName = "fileStore",
                        BasePath = basePath + "/keyStore",
                        SpaceFormat = SpaceFormat.Key,
                        UseCache = false
                    });
                    cnfg.Add<KeyStoreProvider>("fileStore");
                });

                services.AddListStore<DataChangeRecord>("list");
                services.AddKeyStore<TestRecord>("keyStore");

                services.AddTransaction("default", config =>
                {
                    config.ListSpaceName = "list";
                    config.JournalKey = "TestJournal";
                    config.TrxProviders.Add(x => x.GetRequiredService<MemoryStore>());
                });
            })
            .Build();

        return host;
    }

    [Fact]
    public async Task StartWithoutErrorsThrows()
    {
        var host = BuildService();
        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");

        await transaction.Start();
    }

    [Fact]
    public async Task EmptyTransactionWithCommit()
    {
        var host = BuildService();
        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");
        IListStore<DataChangeRecord> listStore = host.Services.GetRequiredService<IListStore<DataChangeRecord>>();

        await transaction.Start();
        (await transaction.Commit()).BeOk();

        var records = (await listStore.Get("TestJournal")).BeOk();
        var data = records.Return();
        data.Count.Be(1);
        data[0].Entries.Count.Be(0);
    }

    [Fact]
    public async Task EmptyTransactionWithRollback()
    {
        var host = BuildService();
        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");
        IListStore<DataChangeRecord> listStore = host.Services.GetRequiredService<IListStore<DataChangeRecord>>();

        await transaction.Start();
        (await transaction.Rollback()).BeOk();

        var records = (await listStore.Get("TestJournal")).BeOk();
        records.Return().Count.Be(0);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(10)]
    public async Task SingleOrMultipleTransactionCommit(int trxCount)
    {
        var host = BuildService();
        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");
        IListStore<DataChangeRecord> listStore = host.Services.GetRequiredService<IListStore<DataChangeRecord>>();
        var keyStore = host.Services.GetRequiredService<IKeyStore<TestRecord>>();

        await transaction.Start();

        foreach (var i in Enumerable.Range(0, trxCount))
        {
            (await keyStore.Set("key1", new TestRecord($"Alice-{i}", 30 + i))).BeOk();
        }

        (await transaction.Commit()).BeOk();

        var records = (await listStore.Get("TestJournal")).BeOk();
        records.Return().Action(x =>
        {
            x.Count.Be(1);
            x[0].Entries.Count.Be(trxCount);
        });
    }

    [Fact]
    public async Task FailWithRecordAttachedAndTrxNotStarted()
    {
        var host = BuildService();
        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");
        var keyStore = host.Services.GetRequiredService<IKeyStore<TestRecord>>();

        await transaction.Start();
        (await keyStore.Set("key1", new TestRecord("Alice", 30))).BeOk();
        (await transaction.Commit()).BeOk();

        await Verify.ThrowsAsync<ArgumentException>(async () =>
        {
            (await keyStore.Set("key1", new TestRecord("Alice", 30))).BeOk();
        });
    }

    [Fact]
    public async Task SingleTransactionRecovery()
    {
        var host = BuildService();
        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");
        IListStore<DataChangeRecord> listStore = host.Services.GetRequiredService<IListStore<DataChangeRecord>>();
        var keyStore = host.Services.GetRequiredService<IKeyStore<TestRecord>>();
        var memoryStore = host.Services.GetRequiredService<MemoryStore>();

        const string aliceKey = "key1";
        TestRecord alice = new("Alice", 10);

        const string bobKey = "key2";
        TestRecord bob = new("Bob", 20);

        // First transaction
        await transaction.Start();
        (await keyStore.Set(aliceKey, alice)).BeOk();
        (await transaction.Commit()).BeOk();

        (await listStore.Get("TestJournal")).BeOk().Return().Count.Be(1);

        var snapshot1 = await memoryStore.GetSnapshot();
        var s1 = snapshot1.ToObject<MemoryStoreSerialization>().NotNull();

        // Second transaction
        await transaction.Start();
        (await keyStore.Set(bobKey, bob)).BeOk();
        (await transaction.Commit()).BeOk();
        (await listStore.Get("TestJournal")).BeOk().Return().Count.Be(2);

        var currentJournal = s1.DirectoryDetails.Where(x => x.PathDetail.Path.Like("*testjournal*")).First();

        var dd = s1.DirectoryDetails
            .Where(x => x.PathDetail.Path != currentJournal.PathDetail.Path)
            .Append(currentJournal)
            .ToArray();

        var newSnapshot = new MemoryStoreSerialization(dd, s1.LogSequenceNumber);

        // Store snapshot
        (await memoryStore.Restore(snapshot1)).BeOk();
        (await listStore.Get("TestJournal")).BeOk().Return().Count.Be(1);

        (await keyStore.Get(aliceKey)).BeOk().Return().Be(alice);
        (await keyStore.Get(bobKey)).BeNotFound();


    }
}
