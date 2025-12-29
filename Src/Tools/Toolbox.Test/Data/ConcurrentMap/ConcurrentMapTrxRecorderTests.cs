using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Test.Data.ConcurrentMap;

public class ConcurrentMapTrxRecorderTests
{
    private ITestOutputHelper _outputHelper;
    private record MapRecord(string Name, int Age);

    public ConcurrentMapTrxRecorderTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

    private IHost BuildService()
    {
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
                        BasePath = "listBase",
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

        return host;
    }

    [Fact]
    public async Task AttachToConcurrentMap()
    {
        var host = BuildService();
        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");
        IListStore2<DataChangeRecord> listStore = host.Services.GetRequiredService<IListStore2<DataChangeRecord>>();
        int rollbackCount = 0;

        transaction.EnlistLambda("source1", entry =>
        {
            Interlocked.Increment(ref rollbackCount);
            return new Option(StatusCode.OK).ToTaskResult();
        });

        var concurrentMap = new ConcurrentMap<string, string>(x => x);
        concurrentMap.AttachRecorder(transaction.TrxRecorder);
        await transaction.Start();

        var result = await transaction.Commit();
        result.BeOk();
        rollbackCount.Be(0);

        var records = await listStore.Get("TestJournal");
        records.BeOk();
        var data = records.Return();
        data.Count.Be(1);
        data[0].Entries.Count.Be(0);

        concurrentMap.DetachRecorder();
    }

    [Fact]
    public async Task AttachAndDetachRecorder()
    {
        var host = BuildService();
        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");
        IListStore2<DataChangeRecord> listStore = host.Services.GetRequiredService<IListStore2<DataChangeRecord>>();
        int rollbackCount = 0;

        transaction.EnlistLambda("source1", entry =>
        {
            Interlocked.Increment(ref rollbackCount);
            return new Option(StatusCode.OK).ToTaskResult();
        });

        var concurrentMap = new ConcurrentMap<string, string>(x => x);
        concurrentMap.AttachRecorder(transaction.TrxRecorder);
        await transaction.Start();

        concurrentMap.TryAdd("Item1").BeTrue();

        (await transaction.Commit()).BeOk();
        rollbackCount.Be(0);

        (await listStore.Get("TestJournal")).Action(x =>
        {
            x.BeOk();
            var data = x.Return();
            data.Count.Be(1);
            data[0].Entries.Count.Be(1);
        });

        concurrentMap.DetachRecorder();

        concurrentMap.TryAdd("Item2").BeTrue();

        (await listStore.Get("TestJournal")).Action(x =>
        {
            x.BeOk();
            var data = x.Return();
            data.Count.Be(1);
            data[0].Entries.Count.Be(1);
        });
    }

    [Fact]
    public async Task VerifyJournalRecorder()
    {
        var host = BuildService();
        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");
        IListStore2<DataChangeRecord> listStore = host.Services.GetRequiredService<IListStore2<DataChangeRecord>>();
        int rollbackCount = 0;

        transaction.EnlistLambda("source1", entry =>
        {
            Interlocked.Increment(ref rollbackCount);
            return new Option(StatusCode.OK).ToTaskResult();
        });

        var concurrentMap = new ConcurrentMap<string, MapRecord>(x => x.Name);
        concurrentMap.AttachRecorder(transaction.TrxRecorder);
        await transaction.Start();

        var r1 = new MapRecord("Item1", 25);
        concurrentMap.TryAdd(r1).BeTrue();

        (await transaction.Commit()).BeOk();
        rollbackCount.Be(0);

        (await listStore.Get("TestJournal")).Action(x =>
        {
            x.BeOk();
            var data = x.Return();
            var list = data.SelectMany(x => x.Entries).ToList();
            list.Count.Be(1);
            list[0].Action(entry =>
            {
                entry.TypeName.Be(typeof(MapRecord).Name);
                entry.SourceName.Be("concurrentMap");
                entry.ObjectId.Be("Item1");
                entry.Action.Be(ChangeOperation.Add);
                (entry.Before == null).BeTrue();

                var s1 = entry.After?.ToObject<MapRecord>() ?? throw new ArgumentException();
                s1.Name.Be("Item1");
                s1.Age.Be(25);
            });
        });
    }

    [Fact]
    public async Task VerifyTryRemoveRecording()
    {
        var host = BuildService();
        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");
        IListStore2<DataChangeRecord> listStore = host.Services.GetRequiredService<IListStore2<DataChangeRecord>>();

        var concurrentMap = new ConcurrentMap<string, MapRecord>(x => x.Name);
        var r1 = new MapRecord("Item1", 25);
        concurrentMap.TryAdd(r1).BeTrue();

        transaction.EnlistLambda("source1", _ => new Option(StatusCode.OK).ToTaskResult());
        concurrentMap.AttachRecorder(transaction.TrxRecorder);
        await transaction.Start();

        concurrentMap.TryRemove("Item1", out var removed).BeTrue();

        (await transaction.Commit()).BeOk();

        (await listStore.Get("TestJournal")).Action(x =>
        {
            x.BeOk();
            var list = x.Return().SelectMany(r => r.Entries).ToList();
            list.Count.Be(1);
            list[0].Action.Be(ChangeOperation.Delete);
            list[0].ObjectId.Be("Item1");
            (list[0].Before != null).BeTrue();
            (list[0].After == null).BeTrue();
        });

        concurrentMap.DetachRecorder();
    }

    [Fact]
    public async Task VerifyTryUpdateRecording()
    {
        var host = BuildService();
        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");
        IListStore2<DataChangeRecord> listStore = host.Services.GetRequiredService<IListStore2<DataChangeRecord>>();

        var concurrentMap = new ConcurrentMap<string, MapRecord>(x => x.Name);
        var original = new MapRecord("Item1", 25);
        concurrentMap.TryAdd(original).BeTrue();

        transaction.EnlistLambda("source1", _ => new Option(StatusCode.OK).ToTaskResult());
        concurrentMap.AttachRecorder(transaction.TrxRecorder);
        await transaction.Start();

        var updated = new MapRecord("Item1", 30);
        concurrentMap.TryUpdate(updated, original).BeTrue();

        (await transaction.Commit()).BeOk();

        (await listStore.Get("TestJournal")).Action(x =>
        {
            x.BeOk();
            var list = x.Return().SelectMany(r => r.Entries).ToList();
            list.Count.Be(1);
            list[0].Action.Be(ChangeOperation.Update);
            list[0].ObjectId.Be("Item1");

            var before = list[0].Before?.ToObject<MapRecord>();
            var after = list[0].After?.ToObject<MapRecord>();
            before.NotNull().Age.Be(25);
            after.NotNull().Age.Be(30);
        });

        concurrentMap.DetachRecorder();
    }

    [Fact]
    public async Task VerifySetNewItemRecording()
    {
        var host = BuildService();
        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");
        IListStore2<DataChangeRecord> listStore = host.Services.GetRequiredService<IListStore2<DataChangeRecord>>();

        var concurrentMap = new ConcurrentMap<string, MapRecord>(x => x.Name);

        transaction.EnlistLambda("source1", _ => new Option(StatusCode.OK).ToTaskResult());
        concurrentMap.AttachRecorder(transaction.TrxRecorder);
        await transaction.Start();

        concurrentMap.Set(new MapRecord("Item1", 25));

        (await transaction.Commit()).BeOk();

        (await listStore.Get("TestJournal")).Action(x =>
        {
            x.BeOk();
            var list = x.Return().SelectMany(r => r.Entries).ToList();
            list.Count.Be(1);
            list[0].Action.Be(ChangeOperation.Add);
        });

        concurrentMap.DetachRecorder();
    }

    [Fact]
    public async Task VerifySetExistingItemRecording()
    {
        var host = BuildService();
        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");
        IListStore2<DataChangeRecord> listStore = host.Services.GetRequiredService<IListStore2<DataChangeRecord>>();

        var concurrentMap = new ConcurrentMap<string, MapRecord>(x => x.Name);
        concurrentMap.TryAdd(new MapRecord("Item1", 25)).BeTrue();

        transaction.EnlistLambda("source1", _ => new Option(StatusCode.OK).ToTaskResult());
        concurrentMap.AttachRecorder(transaction.TrxRecorder);
        await transaction.Start();

        concurrentMap.Set(new MapRecord("Item1", 30));

        (await transaction.Commit()).BeOk();

        (await listStore.Get("TestJournal")).Action(x =>
        {
            x.BeOk();
            var list = x.Return().SelectMany(r => r.Entries).ToList();
            list.Count.Be(1);
            list[0].Action.Be(ChangeOperation.Update);
        });

        concurrentMap.DetachRecorder();
    }

    [Fact]
    public async Task VerifyClearRecording()
    {
        var host = BuildService();
        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");
        IListStore2<DataChangeRecord> listStore = host.Services.GetRequiredService<IListStore2<DataChangeRecord>>();

        var concurrentMap = new ConcurrentMap<string, MapRecord>(x => x.Name);
        concurrentMap.TryAdd(new MapRecord("Item1", 25)).BeTrue();
        concurrentMap.TryAdd(new MapRecord("Item2", 30)).BeTrue();

        transaction.EnlistLambda("source1", _ => new Option(StatusCode.OK).ToTaskResult());
        concurrentMap.AttachRecorder(transaction.TrxRecorder);
        await transaction.Start();

        concurrentMap.Clear();

        (await transaction.Commit()).BeOk();

        (await listStore.Get("TestJournal")).Action(x =>
        {
            x.BeOk();
            var list = x.Return().SelectMany(r => r.Entries).ToList();
            list.Count.Be(2);
            list.All(e => e.Action == ChangeOperation.Delete).BeTrue();
        });

        concurrentMap.DetachRecorder();
    }

    [Fact]
    public async Task VerifyMultipleOperationsInSingleTransaction()
    {
        var host = BuildService();
        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");
        IListStore2<DataChangeRecord> listStore = host.Services.GetRequiredService<IListStore2<DataChangeRecord>>();

        var concurrentMap = new ConcurrentMap<string, MapRecord>(x => x.Name);

        transaction.EnlistLambda("source1", _ => new Option(StatusCode.OK).ToTaskResult());
        concurrentMap.AttachRecorder(transaction.TrxRecorder);
        await transaction.Start();

        var item1 = new MapRecord("Item1", 25);
        var item2 = new MapRecord("Item2", 30);
        concurrentMap.TryAdd(item1).BeTrue();
        concurrentMap.TryAdd(item2).BeTrue();
        concurrentMap.Set(new MapRecord("Item1", 26));
        concurrentMap.TryRemove("Item2", out _).BeTrue();

        (await transaction.Commit()).BeOk();

        (await listStore.Get("TestJournal")).Action(x =>
        {
            x.BeOk();
            var list = x.Return().SelectMany(r => r.Entries).ToList();
            list.Count.Be(4);
            list[0].Action.Be(ChangeOperation.Add);
            list[1].Action.Be(ChangeOperation.Add);
            list[2].Action.Be(ChangeOperation.Update);
            list[3].Action.Be(ChangeOperation.Delete);
        });

        concurrentMap.DetachRecorder();
    }

    [Fact]
    public async Task VerifyGetOrAddNewItemRecording()
    {
        var host = BuildService();
        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");
        IListStore2<DataChangeRecord> listStore = host.Services.GetRequiredService<IListStore2<DataChangeRecord>>();

        var concurrentMap = new ConcurrentMap<string, MapRecord>(x => x.Name);

        transaction.EnlistLambda("source1", _ => new Option(StatusCode.OK).ToTaskResult());
        concurrentMap.AttachRecorder(transaction.TrxRecorder);
        await transaction.Start();

        var result = concurrentMap.GetOrAdd(new MapRecord("Item1", 25));
        result.Name.Be("Item1");

        (await transaction.Commit()).BeOk();

        (await listStore.Get("TestJournal")).Action(x =>
        {
            x.BeOk();
            var list = x.Return().SelectMany(r => r.Entries).ToList();
            list.Count.Be(1);
            list[0].Action.Be(ChangeOperation.Add);
        });

        concurrentMap.DetachRecorder();
    }

    [Fact]
    public async Task VerifyGetOrAddExistingItemNoRecording()
    {
        var host = BuildService();
        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");
        IListStore2<DataChangeRecord> listStore = host.Services.GetRequiredService<IListStore2<DataChangeRecord>>();

        var concurrentMap = new ConcurrentMap<string, MapRecord>(x => x.Name);
        concurrentMap.TryAdd(new MapRecord("Item1", 25)).BeTrue();

        transaction.EnlistLambda("source1", _ => new Option(StatusCode.OK).ToTaskResult());
        concurrentMap.AttachRecorder(transaction.TrxRecorder);
        await transaction.Start();

        var result = concurrentMap.GetOrAdd(new MapRecord("Item1", 30));
        result.Age.Be(25); // Returns existing, not new

        (await transaction.Commit()).BeOk();

        (await listStore.Get("TestJournal")).Action(x =>
        {
            x.BeOk();
            var list = x.Return().SelectMany(r => r.Entries).ToList();
            list.Count.Be(0); // No recording for existing item
        });

        concurrentMap.DetachRecorder();
    }
}
