using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Toolbox.Azure.test.Application;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit;
using Xunit.Abstractions;

namespace Toolbox.Azure.test.Datalake;

public class DatalakeTrxRecorderTests
{
    private ITestOutputHelper _outputHelper;
    private record MapRecord(string Name, int Age);
    public DatalakeTrxRecorderTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

    private async Task<IHost> BuildService(bool useHash, bool useCache, [CallerMemberName] string function = "")
    {
        string basePath = nameof(DatalakeTrxRecorderTests) + "/" + function;
        var option = TestApplication.ReadOption(basePath);

        var host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddLogging(c => c.AddLambda(_outputHelper.WriteLine).AddDebug().AddFilter(_ => true));
                services.AddDatalakeFileStore(option);
                services.AddMemoryCache();

                services.AddDataSpace(cnfg =>
                {
                    cnfg.Spaces.Add(new SpaceDefinition
                    {
                        Name = "file",
                        ProviderName = "fileStore",
                        BasePath = basePath,
                        SpaceFormat = useHash ? SpaceFormat.Hash : SpaceFormat.Key,
                        UseCache = useCache
                    });

                    cnfg.Add<KeyStoreProvider>("fileStore");

                    cnfg.Spaces.Add(new SpaceDefinition
                    {
                        Name = "list",
                        ProviderName = "listStore",
                        BasePath = "journals",
                        SpaceFormat = SpaceFormat.List,
                    });
                    cnfg.Add<ListStoreProvider>("listStore");
                });

                services.AddListStore<DataChangeRecord>("list");
                services.AddTransaction("default", config =>
                {
                    config.ListSpaceName = "list";
                    config.JournalKey = "TestJournal";
                });
            })
            .Build();

        // Clear the store before running tests, this includes any locked files
        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();
        await keyStore.DeleteFolder(basePath);
        (await keyStore.Search($"{basePath}/***")).Count().Be(0);
        return host;
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task AttachToConcurrentMap(bool useHash, bool useCache)
    {
        var host = await BuildService(useHash, useCache);
        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");
        IListStore<DataChangeRecord> listStore = host.Services.GetRequiredService<IListStore<DataChangeRecord>>();
        var keyStore = host.Services.GetRequiredService<IKeyStore>();
        int rollbackCount = 0;

        transaction.EnlistLambda("source1", entry =>
        {
            Interlocked.Increment(ref rollbackCount);
            return new Option(StatusCode.OK).ToTaskResult();
        });

        keyStore.AttachRecorder(transaction.TrxRecorder);
        await transaction.Start();

        (await transaction.Commit()).BeOk();
        rollbackCount.Be(0);

        var data = (await listStore.Get("TestJournal")).BeOk().Return();
        data.Count.Be(1);
        data[0].Entries.Count.Be(0);

        keyStore.DetachRecorder();
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task AttachAndDetachRecorder(bool useHash, bool useCache)
    {
        var host = await BuildService(useHash, useCache);
        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");
        IListStore<DataChangeRecord> listStore = host.Services.GetRequiredService<IListStore<DataChangeRecord>>();
        var keyStore = host.Services.GetRequiredService<IKeyStore>();
        int rollbackCount = 0;

        transaction.EnlistLambda("source1", entry =>
        {
            Interlocked.Increment(ref rollbackCount);
            return new Option(StatusCode.OK).ToTaskResult();
        });

        keyStore.AttachRecorder(transaction.TrxRecorder);
        await transaction.Start();

        var data = new MapRecord("Item1", 25);
        (await keyStore.Add("Item1", data)).BeOk();

        (await transaction.Commit()).BeOk();
        rollbackCount.Be(0);

        (await listStore.Get("TestJournal")).BeOk().Return().Action(data =>
        {
            data.Count.Be(1);
            data[0].Entries.Count.Be(1);
        });

        keyStore.DetachRecorder();

        (await keyStore.Add("Item2", data)).BeOk();

        (await listStore.Get("TestJournal")).BeOk().Return().Action(data =>
        {
            data.Count.Be(1);
            data[0].Entries.Count.Be(1);
        });
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task VerifyJournalRecorder(bool useHash, bool useCache)
    {
        var host = await BuildService(useHash, useCache);
        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");
        IListStore<DataChangeRecord> listStore = host.Services.GetRequiredService<IListStore<DataChangeRecord>>();
        var keyStore = host.Services.GetRequiredService<IKeyStore>();
        int rollbackCount = 0;

        transaction.EnlistLambda("source1", entry =>
        {
            Interlocked.Increment(ref rollbackCount);
            return new Option(StatusCode.OK).ToTaskResult();
        });

        keyStore.AttachRecorder(transaction.TrxRecorder);
        await transaction.Start();

        MapRecord r1 = new MapRecord("Item1", 25);
        (await keyStore.Add("Item1", r1)).BeOk();

        (await transaction.Commit()).BeOk();
        rollbackCount.Be(0);

        (await listStore.Get("TestJournal")).Action(result =>
        {
            result.BeOk();
            List<DataChangeEntry> entries = result.Return().SelectMany(x => x.Entries).ToList();
            entries.Count.Be(1);
            entries[0].Action(entry =>
            {
                entry.SourceName.Be(DatalakeStore.SourceNameText);
                entry.ObjectId.Contains("Item1").BeTrue();
                entry.Action.Be(ChangeOperation.Add);
                (entry.Before == null).BeTrue();

                MapRecord record = entry.After?.ToObject<MapRecord>() ?? throw new ArgumentException();
                record.Name.Be("Item1");
                record.Age.Be(25);
            });
        });

        keyStore.DetachRecorder();
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task VerifyTryRemoveRecording(bool useHash, bool useCache)
    {
        var host = await BuildService(useHash, useCache);
        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");
        IListStore<DataChangeRecord> listStore = host.Services.GetRequiredService<IListStore<DataChangeRecord>>();
        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();

        MapRecord r1 = new MapRecord("Item1", 25);
        (await keyStore.Add("Item1", r1)).BeOk();

        transaction.EnlistLambda("source1", _ => new Option(StatusCode.OK).ToTaskResult());
        keyStore.AttachRecorder(transaction.TrxRecorder);
        await transaction.Start();

        (await keyStore.Delete("Item1")).BeOk();

        (await transaction.Commit()).BeOk();

        (await listStore.Get("TestJournal")).Action(result =>
        {
            result.BeOk();
            List<DataChangeEntry> entries = result.Return().SelectMany(r => r.Entries).ToList();
            entries.Count.Be(1);
            entries[0].Action.Be(ChangeOperation.Delete);
            entries[0].ObjectId.Contains("Item1").BeTrue();
            (entries[0].Before != null).BeTrue();
            (entries[0].After == null).BeTrue();
        });

        keyStore.DetachRecorder();
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task VerifyTryUpdateRecording(bool useHash, bool useCache)
    {
        var host = await BuildService(useHash, useCache);
        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");
        IListStore<DataChangeRecord> listStore = host.Services.GetRequiredService<IListStore<DataChangeRecord>>();
        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();

        MapRecord original = new MapRecord("Item1", 25);
        (await keyStore.Add("Item1", original)).BeOk();

        transaction.EnlistLambda("source1", _ => new Option(StatusCode.OK).ToTaskResult());
        keyStore.AttachRecorder(transaction.TrxRecorder);
        await transaction.Start();

        MapRecord updated = new MapRecord("Item1", 30);
        (await keyStore.Set("Item1", updated)).BeOk();

        (await transaction.Commit()).BeOk();

        (await listStore.Get("TestJournal")).Action(result =>
        {
            result.BeOk();
            List<DataChangeEntry> entries = result.Return().SelectMany(r => r.Entries).ToList();
            entries.Count.Be(1);
            entries[0].Action.Be(ChangeOperation.Update);
            entries[0].ObjectId.Contains("Item1").BeTrue();

            MapRecord before = entries[0].Before?.ToObject<MapRecord>() ?? throw new ArgumentException();
            MapRecord after = entries[0].After?.ToObject<MapRecord>() ?? throw new ArgumentException();
            before.Age.Be(25);
            after.Age.Be(30);
        });

        keyStore.DetachRecorder();
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task VerifySetNewItemRecording(bool useHash, bool useCache)
    {
        var host = await BuildService(useHash, useCache);
        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");
        IListStore<DataChangeRecord> listStore = host.Services.GetRequiredService<IListStore<DataChangeRecord>>();
        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();

        transaction.EnlistLambda("source1", _ => new Option(StatusCode.OK).ToTaskResult());
        keyStore.AttachRecorder(transaction.TrxRecorder);
        await transaction.Start();

        (await keyStore.Set("Item1", new MapRecord("Item1", 25))).BeOk();

        (await transaction.Commit()).BeOk();

        (await listStore.Get("TestJournal")).Action(result =>
        {
            result.BeOk();
            List<DataChangeEntry> entries = result.Return().SelectMany(r => r.Entries).ToList();
            entries.Count.Be(1);
            entries[0].Action.Be(ChangeOperation.Add);
        });

        keyStore.DetachRecorder();
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task VerifySetExistingItemRecording(bool useHash, bool useCache)
    {
        var host = await BuildService(useHash, useCache);
        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");
        IListStore<DataChangeRecord> listStore = host.Services.GetRequiredService<IListStore<DataChangeRecord>>();
        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();

        (await keyStore.Add("Item1", new MapRecord("Item1", 25))).BeOk();

        transaction.EnlistLambda("source1", _ => new Option(StatusCode.OK).ToTaskResult());
        keyStore.AttachRecorder(transaction.TrxRecorder);
        await transaction.Start();

        (await keyStore.Set("Item1", new MapRecord("Item1", 30))).BeOk();

        (await transaction.Commit()).BeOk();

        (await listStore.Get("TestJournal")).Action(result =>
        {
            result.BeOk();
            List<DataChangeEntry> entries = result.Return().SelectMany(r => r.Entries).ToList();
            entries.Count.Be(1);
            entries[0].Action.Be(ChangeOperation.Update);
        });

        keyStore.DetachRecorder();
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task VerifyClearRecording(bool useHash, bool useCache)
    {
        var host = await BuildService(useHash, useCache);
        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");
        IListStore<DataChangeRecord> listStore = host.Services.GetRequiredService<IListStore<DataChangeRecord>>();
        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();

        (await keyStore.Add("Item1", new MapRecord("Item1", 25))).BeOk();
        (await keyStore.Add("Item2", new MapRecord("Item2", 30))).BeOk();

        transaction.EnlistLambda("source1", _ => new Option(StatusCode.OK).ToTaskResult());
        keyStore.AttachRecorder(transaction.TrxRecorder);
        await transaction.Start();

        (await keyStore.Delete("Item1")).BeOk();
        (await keyStore.Delete("Item2")).BeOk();

        (await transaction.Commit()).BeOk();

        (await listStore.Get("TestJournal")).Action(result =>
        {
            result.BeOk();
            List<DataChangeEntry> entries = result.Return().SelectMany(r => r.Entries).ToList();
            entries.Count.Be(2);
            entries.All(e => e.Action == ChangeOperation.Delete).BeTrue();
        });

        keyStore.DetachRecorder();
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task VerifyMultipleOperationsInSingleTransaction(bool useHash, bool useCache)
    {
        var host = await BuildService(useHash, useCache);
        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");
        IListStore<DataChangeRecord> listStore = host.Services.GetRequiredService<IListStore<DataChangeRecord>>();
        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();

        transaction.EnlistLambda("source1", _ => new Option(StatusCode.OK).ToTaskResult());
        keyStore.AttachRecorder(transaction.TrxRecorder);
        await transaction.Start();

        MapRecord item1 = new MapRecord("Item1", 25);
        MapRecord item2 = new MapRecord("Item2", 30);
        (await keyStore.Add("Item1", item1)).BeOk();
        (await keyStore.Add("Item2", item2)).BeOk();
        (await keyStore.Set("Item1", new MapRecord("Item1", 26))).BeOk();
        (await keyStore.Delete("Item2")).BeOk();

        (await transaction.Commit()).BeOk();

        (await listStore.Get("TestJournal")).Action(result =>
        {
            result.BeOk();
            List<DataChangeEntry> entries = result.Return().SelectMany(r => r.Entries).ToList();
            entries.Count.Be(4);
            entries[0].Action.Be(ChangeOperation.Add);
            entries[1].Action.Be(ChangeOperation.Add);
            entries[2].Action.Be(ChangeOperation.Update);
            entries[3].Action.Be(ChangeOperation.Delete);
        });

        keyStore.DetachRecorder();
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task VerifyGetOrAddNewItemRecording(bool useHash, bool useCache)
    {
        var host = await BuildService(useHash, useCache);
        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");
        IListStore<DataChangeRecord> listStore = host.Services.GetRequiredService<IListStore<DataChangeRecord>>();
        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();

        transaction.EnlistLambda("source1", _ => new Option(StatusCode.OK).ToTaskResult());
        keyStore.AttachRecorder(transaction.TrxRecorder);
        await transaction.Start();

        (await keyStore.Add("Item1", new MapRecord("Item1", 25))).BeOk();

        (await transaction.Commit()).BeOk();

        (await listStore.Get("TestJournal")).Action(result =>
        {
            result.BeOk();
            List<DataChangeEntry> entries = result.Return().SelectMany(r => r.Entries).ToList();
            entries.Count.Be(1);
            entries[0].Action.Be(ChangeOperation.Add);
        });

        keyStore.DetachRecorder();
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task VerifyGetOrAddExistingItemNoRecording(bool useHash, bool useCache)
    {
        var host = await BuildService(useHash, useCache);
        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");
        IListStore<DataChangeRecord> listStore = host.Services.GetRequiredService<IListStore<DataChangeRecord>>();
        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();

        (await keyStore.Add("Item1", new MapRecord("Item1", 25))).BeOk();

        transaction.EnlistLambda("source1", _ => new Option(StatusCode.OK).ToTaskResult());
        keyStore.AttachRecorder(transaction.TrxRecorder);
        await transaction.Start();

        // Read-only operation should not be recorded
        (await keyStore.Get("Item1")).BeOk();

        (await transaction.Commit()).BeOk();

        (await listStore.Get("TestJournal")).Action(result =>
        {
            result.BeOk();
            List<DataChangeEntry> entries = result.Return().SelectMany(r => r.Entries).ToList();
            entries.Count.Be(0);
        });

        keyStore.DetachRecorder();
    }
}
