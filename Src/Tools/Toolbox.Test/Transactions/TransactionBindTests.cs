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

public class TransactionBindTests
{
    private ITestOutputHelper _outputHelper;
    private record TestRecord(string Name, int Age);

    public TransactionBindTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

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

        var result = await transaction.Commit();
        result.BeOk();

        var records = await listStore.Get("TestJournal");
        records.BeOk();
        var data = records.Return();
        data.Count.Be(1);
        data[0].Entries.Count.Be(0);
    }

    [Fact]
    public async Task SingleTransactionWithCommit()
    {
        var host = BuildService();
        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");
        IListStore<DataChangeRecord> listStore = host.Services.GetRequiredService<IListStore<DataChangeRecord>>();
        var keyStore = host.Services.GetRequiredService<IKeyStore<TestRecord>>();

        await transaction.Start();
        var testData = new TestRecord("Alice", 30);
        (await keyStore.Set("key1", testData)).BeOk();

        var result = await transaction.Commit();
        result.BeOk();

        var records = await listStore.Get("TestJournal");
        records.BeOk();
        var data = records.Return();
        data.Count.Be(1);
        data[0].Action(x =>
        {
            x.Entries.Count.Be(1);
            x.Entries[0].Action(y =>
            {
                y.TypeName.Be(typeof(DirectoryDetail).Name);
                y.SourceName.Be("memoryStore");
                y.ObjectId.Be("transactionbindtests/singletransactionwithcommit/keystore/key1");
                y.Action.Be(ChangeOperation.Add);
                (y.Before == null).BeTrue();

                var jread = y.After?.ToObject<DirectoryDetail>() ?? throw new ArgumentException();
                jread.PathDetail.Path.Be("transactionbindtests/singletransactionwithcommit/keystore/key1");
                var storedData = jread.Data.ToObject<TestRecord>();
                storedData.Name.Be("Alice");
                storedData.Age.Be(30);
            });
        });
    }


    [Fact]
    public async Task UpdateTransactionWithCommit()
    {
        var host = BuildService();
        var keyStore = host.Services.GetRequiredService<IKeyStore<TestRecord>>();
        (await keyStore.Set("key1", new TestRecord("Alice", 30))).BeOk();

        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");
        IListStore<DataChangeRecord> listStore = host.Services.GetRequiredService<IListStore<DataChangeRecord>>();

        await transaction.Start();
        (await keyStore.Set("key1", new TestRecord("Alice-Updated", 31))).BeOk();

        var result = await transaction.Commit();
        result.BeOk();

        var current = await keyStore.Get("key1");
        current.BeOk();
        var currentValue = current.Return();
        currentValue.Name.Be("Alice-Updated");
        currentValue.Age.Be(31);

        var records = await listStore.Get("TestJournal");
        records.BeOk();
        var entry = records.Return()[0].Entries[0];
        entry.Action(y =>
        {
            y.Action.Be(ChangeOperation.Update);
            y.SourceName.Be("memoryStore");
            y.ObjectId.Be("transactionbindtests/updatetransactionwithcommit/keystore/key1");

            var beforeDetail = y.Before?.ToObject<DirectoryDetail>() ?? throw new ArgumentException();
            var afterDetail = y.After?.ToObject<DirectoryDetail>() ?? throw new ArgumentException();

            var beforeRecord = beforeDetail.Data.ToObject<TestRecord>();
            beforeRecord.Name.Be("Alice");
            beforeRecord.Age.Be(30);

            var afterRecord = afterDetail.Data.ToObject<TestRecord>();
            afterRecord.Name.Be("Alice-Updated");
            afterRecord.Age.Be(31);
        });
    }

    [Fact]
    public async Task DeleteTransactionWithCommit()
    {
        var host = BuildService();
        var keyStore = host.Services.GetRequiredService<IKeyStore<TestRecord>>();
        (await keyStore.Set("key1", new TestRecord("Alice", 30))).BeOk();

        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");
        IListStore<DataChangeRecord> listStore = host.Services.GetRequiredService<IListStore<DataChangeRecord>>();

        await transaction.Start();
        (await keyStore.Delete("key1")).BeOk();

        var result = await transaction.Commit();
        result.BeOk();

        var exists = await keyStore.Exists("key1");
        exists.StatusCode.Be(StatusCode.NotFound);

        var records = await listStore.Get("TestJournal");
        records.BeOk();
        var entry = records.Return()[0].Entries[0];
        entry.Action(y =>
        {
            y.Action.Be(ChangeOperation.Delete);
            y.SourceName.Be("memoryStore");
            y.ObjectId.Be("transactionbindtests/deletetransactionwithcommit/keystore/key1");
            (y.After == null).BeTrue();

            var beforeDetail = y.Before?.ToObject<DirectoryDetail>() ?? throw new ArgumentException();
            var beforeRecord = beforeDetail.Data.ToObject<TestRecord>();
            beforeRecord.Name.Be("Alice");
            beforeRecord.Age.Be(30);
        });
    }

    [Fact]
    public async Task AddTransactionWithRollbackRemovesItem()
    {
        var host = BuildService();
        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");
        var keyStore = host.Services.GetRequiredService<IKeyStore<TestRecord>>();

        await transaction.Start();
        (await keyStore.Set("key1", new TestRecord("Alice", 30))).BeOk();

        var result = await transaction.Rollback();
        result.BeOk();

        var exists = await keyStore.Exists("key1");
        exists.StatusCode.Be(StatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateTransactionWithRollbackRestoresPrevious()
    {
        var host = BuildService();
        var keyStore = host.Services.GetRequiredService<IKeyStore<TestRecord>>();
        (await keyStore.Set("key1", new TestRecord("Alice", 30))).BeOk();

        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");

        await transaction.Start();
        (await keyStore.Set("key1", new TestRecord("Bob", 40))).BeOk();

        var result = await transaction.Rollback();
        result.BeOk();

        var current = await keyStore.Get("key1");
        current.BeOk();
        var currentValue = current.Return();
        currentValue.Name.Be("Alice");
        currentValue.Age.Be(30);
    }

    [Fact]
    public async Task DeleteTransactionWithRollbackRestoresItem()
    {
        var host = BuildService();
        var keyStore = host.Services.GetRequiredService<IKeyStore<TestRecord>>();
        (await keyStore.Set("key1", new TestRecord("Alice", 30))).BeOk();

        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");

        await transaction.Start();
        (await keyStore.Delete("key1")).BeOk();

        var result = await transaction.Rollback();
        result.BeOk();

        var current = await keyStore.Get("key1");
        current.BeOk();
        var currentValue = current.Return();
        currentValue.Name.Be("Alice");
        currentValue.Age.Be(30);
    }
}
