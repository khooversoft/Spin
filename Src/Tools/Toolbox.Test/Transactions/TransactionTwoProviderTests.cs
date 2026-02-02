//using System.Runtime.CompilerServices;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Hosting;
//using Microsoft.Extensions.Logging;
//using Toolbox.Data;
//using Toolbox.Extensions;
//using Toolbox.Store;
//using Toolbox.Tools;
//using Toolbox.Types;
//using Xunit.Abstractions;

//namespace Toolbox.Test.Transactions;

//public class TransactionTwoProviderTests
//{
//    private ITestOutputHelper _outputHelper;
//    private record TestRecord(string Name, int Age);

//    public TransactionTwoProviderTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

//    private IHost BuildService([CallerMemberName] string function = "")
//    {
//        string basePath = nameof(TransactionTwoProviderTests) + "/" + function;

//        var host = Host.CreateDefaultBuilder()
//            .ConfigureServices((context, services) =>
//            {
//                services.AddLogging(c => c.AddLambda(_outputHelper.WriteLine).AddDebug().AddFilter(_ => true));
//                services.AddInMemoryKeyStore();
//                services.AddSingleton<Dictionary2<string, TestRecord>>(sp =>
//                {
//                    var dict = new Dictionary2<string, TestRecord>(x => x.Name);
//                    return dict;
//                });

//                services.AddDataSpace(cnfg =>
//                {
//                    cnfg.Spaces.Add(new SpaceDefinition
//                    {
//                        Name = "list",
//                        ProviderName = "listStore",
//                        BasePath = basePath + "/listBase",
//                        SpaceFormat = SpaceFormat.List,
//                    });
//                    cnfg.Add<ListStoreProvider>("listStore");

//                    cnfg.Spaces.Add(new SpaceDefinition
//                    {
//                        Name = "keyStore",
//                        ProviderName = "fileStore",
//                        BasePath = basePath + "/keyStore",
//                        SpaceFormat = SpaceFormat.Key,
//                        UseCache = false
//                    });
//                    cnfg.Add<KeyStoreProvider>("fileStore");
//                });

//                services.AddListStore<DataChangeRecord>("list");
//                services.AddKeyStore<TestRecord>("keyStore");

//                services.AddTransaction("default", config =>
//                {
//                    config.ListSpaceName = "list";
//                    config.JournalKey = "TestJournal";
//                    config.Providers.Add(x => (ITrxProvider)x.GetRequiredService<MemoryStore>());
//                    config.Providers.Add(x => (ITrxProvider)x.GetRequiredService<Dictionary2<string, TestRecord>>());
//                });
//            })
//            .Build();

//        return host;
//    }

//    [Fact]
//    public async Task StartWithoutErrorsThrows()
//    {
//        var host = BuildService();
//        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");

//        await transaction.Start();
//    }

//    [Fact]
//    public async Task EmptyTransactionWithCommit()
//    {
//        var host = BuildService();
//        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");
//        IListStore<DataChangeRecord> listStore = host.Services.GetRequiredService<IListStore<DataChangeRecord>>();
//        var dict = host.Services.GetRequiredService<Dictionary2<string, TestRecord>>();

//        await transaction.Start();

//        var result = await transaction.Commit();
//        result.BeOk();

//        var records = await listStore.Get("TestJournal");
//        records.BeOk();
//        var data = records.Return();
//        data.Count.Be(1);
//        data[0].Entries.Count.Be(0);
//    }

//    [Fact]
//    public async Task SingleTransactionWithCommit()
//    {
//        var host = BuildService();
//        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");
//        IListStore<DataChangeRecord> listStore = host.Services.GetRequiredService<IListStore<DataChangeRecord>>();
//        var keyStore = host.Services.GetRequiredService<IKeyStore<TestRecord>>();
//        var dict = host.Services.GetRequiredService<Dictionary2<string, TestRecord>>();

//        await transaction.Start();
//        var testData = new TestRecord("Alice", 30);

//        (await keyStore.Set("key1", testData)).BeOk();

//        dict.TryAdd(testData).BeTrue();

//        var result = await transaction.Commit();
//        result.BeOk();

//        var records = await listStore.Get("TestJournal");
//        records.BeOk();
//        var data = records.Return();
//        data.Count.Be(1);
//        data[0].Action(x =>
//        {
//            x.Entries.Count.Be(2);
//            x.Entries[0].Action(y =>
//            {
//                y.TypeName.Be(typeof(DirectoryDetail).Name);
//                y.SourceName.Be("memoryStore");
//                y.ObjectId.Be("transactiontwoprovidertests/singletransactionwithcommit/keystore/key1");
//                y.Action.Be(ChangeOperation.Add);
//                (y.Before == null).BeTrue();

//                var jread = y.After?.ToObject<DirectoryDetail>() ?? throw new ArgumentException();
//                jread.PathDetail.Path.Be("transactiontwoprovidertests/singletransactionwithcommit/keystore/key1");
//                var storedData = jread.Data.ToObject<TestRecord>();
//                storedData.Name.Be("Alice");
//                storedData.Age.Be(30);
//            });
//            x.Entries[1].Action(y =>
//            {
//                y.TypeName.Be(typeof(TestRecord).Name);
//                y.SourceName.Be("dictionary2");
//                y.ObjectId.Be("Alice");
//                y.Action.Be(ChangeOperation.Add);
//                (y.Before == null).BeTrue();

//                var jread = y.After?.ToObject<TestRecord>() ?? throw new ArgumentException();
//                jread.Name.Be("Alice");
//                jread.Age.Be(30);
//            });
//        });
//    }


//    [Fact]
//    public async Task UpdateTransactionWithCommit()
//    {
//        var host = BuildService();
//        var keyStore = host.Services.GetRequiredService<IKeyStore<TestRecord>>();
//        var dict = host.Services.GetRequiredService<Dictionary2<string, TestRecord>>();
//        var initialRecord = new TestRecord("Alice", 30);
//        (await keyStore.Set("key1", initialRecord)).BeOk();
//        dict.TryAdd(initialRecord).BeTrue();

//        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");
//        IListStore<DataChangeRecord> listStore = host.Services.GetRequiredService<IListStore<DataChangeRecord>>();

//        await transaction.Start();

//        var updatedRecord = new TestRecord("Alice-Updated", 31);
//        (await keyStore.Set("key1", updatedRecord)).BeOk();

//        var updatedDictionaryRecord = new TestRecord("Alice", 31);
//        dict.TryUpdate(updatedDictionaryRecord, initialRecord).BeTrue();

//        var result = await transaction.Commit();
//        result.BeOk();

//        var current = await keyStore.Get("key1");
//        current.BeOk();
//        var currentValue = current.Return();
//        currentValue.Name.Be("Alice-Updated");
//        currentValue.Age.Be(31);

//        dict.TryGetValue("Alice", out var currentDictionaryValue).BeTrue();
//        currentDictionaryValue!.Name.Be("Alice");
//        currentDictionaryValue.Age.Be(31);

//        var records = await listStore.Get("TestJournal");
//        records.BeOk();
//        var entries = records.Return()[0].Entries;
//        entries.Count.Be(2);

//        entries[0].Action(y =>
//        {
//            y.Action.Be(ChangeOperation.Update);
//            y.SourceName.Be("memoryStore");
//            y.ObjectId.Be("transactiontwoprovidertests/updatetransactionwithcommit/keystore/key1");

//            var beforeDetail = y.Before?.ToObject<DirectoryDetail>() ?? throw new ArgumentException();
//            var afterDetail = y.After?.ToObject<DirectoryDetail>() ?? throw new ArgumentException();

//            var beforeRecord = beforeDetail.Data.ToObject<TestRecord>();
//            beforeRecord.Name.Be("Alice");
//            beforeRecord.Age.Be(30);

//            var afterRecord = afterDetail.Data.ToObject<TestRecord>();
//            afterRecord.Name.Be("Alice-Updated");
//            afterRecord.Age.Be(31);
//        });

//        entries[1].Action(y =>
//        {
//            y.Action.Be(ChangeOperation.Update);
//            y.SourceName.Be("dictionary2");
//            y.ObjectId.Be("Alice");

//            var beforeRecord = y.Before?.ToObject<TestRecord>() ?? throw new ArgumentException();
//            beforeRecord.Name.Be("Alice");
//            beforeRecord.Age.Be(30);

//            var afterRecord = y.After?.ToObject<TestRecord>() ?? throw new ArgumentException();
//            afterRecord.Name.Be("Alice");
//            afterRecord.Age.Be(31);
//        });
//    }

//    [Fact]
//    public async Task DeleteTransactionWithCommit()
//    {
//        var host = BuildService();
//        var keyStore = host.Services.GetRequiredService<IKeyStore<TestRecord>>();
//        var dict = host.Services.GetRequiredService<Dictionary2<string, TestRecord>>();
//        var initialRecord = new TestRecord("Alice", 30);
//        (await keyStore.Set("key1", initialRecord)).BeOk();
//        dict.TryAdd(initialRecord).BeTrue();

//        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");
//        IListStore<DataChangeRecord> listStore = host.Services.GetRequiredService<IListStore<DataChangeRecord>>();

//        await transaction.Start();
//        (await keyStore.Delete("key1")).BeOk();
//        dict.TryRemove("Alice", out _).BeTrue();

//        var result = await transaction.Commit();
//        result.BeOk();

//        var exists = await keyStore.Exists("key1");
//        exists.StatusCode.Be(StatusCode.NotFound);

//        dict.ContainsKey("Alice").BeFalse();

//        var records = await listStore.Get("TestJournal");
//        records.BeOk();
//        var entries = records.Return()[0].Entries;
//        entries.Count.Be(2);

//        entries[0].Action(y =>
//        {
//            y.Action.Be(ChangeOperation.Delete);
//            y.SourceName.Be("memoryStore");
//            y.ObjectId.Be("transactiontwoprovidertests/deletetransactionwithcommit/keystore/key1");
//            (y.After == null).BeTrue();

//            var beforeDetail = y.Before?.ToObject<DirectoryDetail>() ?? throw new ArgumentException();
//            var beforeRecord = beforeDetail.Data.ToObject<TestRecord>();
//            beforeRecord.Name.Be("Alice");
//            beforeRecord.Age.Be(30);
//        });

//        entries[1].Action(y =>
//        {
//            y.Action.Be(ChangeOperation.Delete);
//            y.SourceName.Be("dictionary2");
//            y.ObjectId.Be("Alice");
//            (y.After == null).BeTrue();

//            var beforeRecord = y.Before?.ToObject<TestRecord>() ?? throw new ArgumentException();
//            beforeRecord.Name.Be("Alice");
//            beforeRecord.Age.Be(30);
//        });
//    }

//    [Fact]
//    public async Task AddTransactionWithRollbackRemovesItem()
//    {
//        var host = BuildService();
//        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");
//        var keyStore = host.Services.GetRequiredService<IKeyStore<TestRecord>>();
//        var dict = host.Services.GetRequiredService<Dictionary2<string, TestRecord>>();

//        await transaction.Start();
//        var record = new TestRecord("Alice", 30);
//        (await keyStore.Set("key1", record)).BeOk();
//        dict.TryAdd(record).BeTrue();

//        var result = await transaction.Rollback();
//        result.BeOk();

//        var exists = await keyStore.Exists("key1");
//        exists.StatusCode.Be(StatusCode.NotFound);

//        dict.ContainsKey("Alice").BeFalse();
//    }

//    [Fact]
//    public async Task UpdateTransactionWithRollbackRestoresPrevious()
//    {
//        var host = BuildService();
//        var keyStore = host.Services.GetRequiredService<IKeyStore<TestRecord>>();
//        var dict = host.Services.GetRequiredService<Dictionary2<string, TestRecord>>();
//        var initialRecord = new TestRecord("Alice", 30);
//        (await keyStore.Set("key1", initialRecord)).BeOk();
//        dict.TryAdd(initialRecord).BeTrue();

//        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");

//        await transaction.Start();
//        (await keyStore.Set("key1", new TestRecord("Bob", 40))).BeOk();

//        var updatedDictionaryRecord = new TestRecord("Alice", 31);
//        dict.TryUpdate(updatedDictionaryRecord, initialRecord).BeTrue();

//        var result = await transaction.Rollback();
//        result.BeOk();

//        var current = await keyStore.Get("key1");
//        current.BeOk();
//        var currentValue = current.Return();
//        currentValue.Name.Be("Alice");
//        currentValue.Age.Be(30);

//        dict.TryGetValue("Alice", out var currentDictionaryValue).BeTrue();
//        currentDictionaryValue!.Name.Be("Alice");
//        currentDictionaryValue.Age.Be(30);
//    }

//    [Fact]
//    public async Task DeleteTransactionWithRollbackRestoresItem()
//    {
//        var host = BuildService();
//        var keyStore = host.Services.GetRequiredService<IKeyStore<TestRecord>>();
//        var dict = host.Services.GetRequiredService<Dictionary2<string, TestRecord>>();
//        var initialRecord = new TestRecord("Alice", 30);
//        (await keyStore.Set("key1", initialRecord)).BeOk();
//        dict.TryAdd(initialRecord).BeTrue();

//        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");

//        await transaction.Start();
//        (await keyStore.Delete("key1")).BeOk();

//        var result = await transaction.Rollback();
//        result.BeOk();

//        var current = await keyStore.Get("key1");
//        current.BeOk();
//        var currentValue = current.Return();
//        currentValue.Name.Be("Alice");
//        currentValue.Age.Be(30);
//    }
//}
