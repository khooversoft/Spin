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

namespace Toolbox.Azure.test.KeyStore;

public class KeySpace_TrxRecoveryTests
{
    private ITestOutputHelper _outputHelper;
    private record MapRecord(string Name, int Age);
    public KeySpace_TrxRecoveryTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

    private async Task<IHost> BuildService(bool useHash, bool useCache, [CallerMemberName] string function = "")
    {
        string basePath = nameof(KeySpace_TrxRecoveryTests) + "/" + function;
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

                services.AddKeyStore("file");
                services.AddListStore<DataChangeRecord>("list");
                services.AddSingleton(new TestProvider());

                services.AddTransaction("default", config =>
                {
                    config.ListSpaceName = "list";
                    config.JournalKey = "TestJournal";
                    config.TrxProviders.Add(x => (ITrxProvider)x.GetRequiredKeyedService<IKeyStore>("file"));
                    config.TrxProviders.Add(x => x.GetRequiredService<TestProvider>());
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
    public async Task CommitNoTransactions(bool useHash, bool useCache)
    {
        var host = await BuildService(useHash, useCache);
        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");
        IListStore<DataChangeRecord> listStore = host.Services.GetRequiredService<IListStore<DataChangeRecord>>();
        IKeyStore keyStore = host.Services.GetRequiredKeyedService<IKeyStore>("file");
        var testProvider = host.Services.GetRequiredService<TestProvider>();
        testProvider._logSequenceNumber.BeEmpty();

        await transaction.Start();

        var result = (await transaction.Commit()).BeOk();
        testProvider._logSequenceNumber.BeEmpty();

        var data = (await listStore.Get("TestJournal")).BeOk().Return();
        data.Count.Be(1);
        data[0].Entries.Count.Be(0);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task SingleNoRecovery(bool useHash, bool useCache)
    {
        var host = await BuildService(useHash, useCache);
        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");
        IListStore<DataChangeRecord> listStore = host.Services.GetRequiredService<IListStore<DataChangeRecord>>();
        IKeyStore keyStore = host.Services.GetRequiredKeyedService<IKeyStore>("file");
        var testProvider = host.Services.GetRequiredService<TestProvider>();
        testProvider._logSequenceNumber.BeEmpty();

        await transaction.Start();

        var data = new MapRecord("Item1", 25);
        (await keyStore.Add("Item1", data)).BeOk();

        (await transaction.Commit()).BeOk();
        testProvider._logSequenceNumber.NotEmpty();

        var journal = (await listStore.Get("TestJournal")).BeOk().Return();
        journal.Count.Be(1);
        journal[0].Entries.Count.Be(1);

        (await transaction.Recovery()).BeOk();
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task RecoveryOneOutstanding(bool useHash, bool useCache)
    {
        var host = await BuildService(useHash, useCache);
        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");
        IListStore<DataChangeRecord> listStore = host.Services.GetRequiredService<IListStore<DataChangeRecord>>();
        IKeyStore keyStore = host.Services.GetRequiredKeyedService<IKeyStore>("file");
        var testProvider = host.Services.GetRequiredService<TestProvider>();
        IKeyStore lowerKeyStore = host.Services.GetRequiredService<IKeyStore>();
        testProvider._logSequenceNumber.BeEmpty();

        var r1 = new MapRecord("Item1", 10);
        var r2 = new MapRecord("Item2", 20);
        var r3 = new MapRecord("Item3", 30);

        await using (var s2 = await transaction.Start())
        {
            (await keyStore.Add(r1.Name, r1)).BeOk();
        }

        // Get current LSN to rest to
        string currentLsn = testProvider._logSequenceNumber.NotNull();

        await using (var s2 = await transaction.Start())
        {
            (await keyStore.Add(r2.Name, r2)).BeOk();
        }

        testProvider._logSequenceNumber.NotEmpty().NotBe(currentLsn);

        // Get lower store access, not recorded
        var lowerR1Path = ((KeySpace)keyStore).KeyPathStrategy.BuildPath("item1");
        var lowerR2Path = ((KeySpace)keyStore).KeyPathStrategy.BuildPath("item2");

        (await keyStore.Exists("item1")).BeOk();
        (await keyStore.Exists("item2")).BeOk();
        (await lowerKeyStore.Exists(lowerR1Path)).BeOk();
        (await lowerKeyStore.Exists(lowerR2Path)).BeOk();

        (await lowerKeyStore.Delete(lowerR2Path)).BeOk();
        (await lowerKeyStore.Exists(lowerR2Path)).BeNotFound();

        // Recover by journal
        testProvider._logSequenceNumber = currentLsn.NotEmpty();
        (await transaction.Recovery()).BeOk();

        (await lowerKeyStore.Exists(lowerR1Path)).BeOk();
        (await lowerKeyStore.Exists(lowerR2Path)).BeOk();
        (await keyStore.Get("item1")).BeOk().Return().ToObject<MapRecord>().Be(r1);
        (await keyStore.Get("item2")).BeOk().Return().ToObject<MapRecord>().Be(r2);

        var journal = (await listStore.Get("TestJournal")).BeOk().Return();
        journal.Count.Be(2);
        journal[0].Entries.Count.Be(1);
        journal[1].Entries.Count.Be(1);

        await using (var s3 = await transaction.Start())
        {
            (await keyStore.Add(r3.Name, r3)).BeOk();
        }

        (await keyStore.Get("item1")).BeOk().Return().ToObject<MapRecord>().Be(r1);
        (await keyStore.Get("item2")).BeOk().Return().ToObject<MapRecord>().Be(r2);
        (await keyStore.Get("item3")).BeOk().Return().ToObject<MapRecord>().Be(r3);
        var list = await keyStore.Search("**");
        list.Count.Be(4);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task RecoveryMultipleOutstanding(bool useHash, bool useCache)
    {
        var host = await BuildService(useHash, useCache);
        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");
        IListStore<DataChangeRecord> listStore = host.Services.GetRequiredService<IListStore<DataChangeRecord>>();
        IKeyStore keyStore = host.Services.GetRequiredKeyedService<IKeyStore>("file");
        var testProvider = host.Services.GetRequiredService<TestProvider>();
        IKeyStore lowerKeyStore = host.Services.GetRequiredService<IKeyStore>();
        testProvider._logSequenceNumber.BeEmpty();

        const int firstCount = 6;

        var records = Enumerable.Range(0, 10)
            .Select(x => new MapRecord($"Item{x}", (x * 10) + 10))
            .Select(x => (r: x, p: ((KeySpace)keyStore).KeyPathStrategy.BuildPath(x.Name)))
            .ToArray();

        var firstSet = records.Take(firstCount).ToArray();
        var deleteSet = records.Skip(firstCount).ToArray();

        await using (var s1 = await transaction.Start())
        {
            await firstSet.ForEachAsync(async x => (await keyStore.Add(x.r.Name, x.r)).BeOk());
        }

        string currentLsn = testProvider._logSequenceNumber.NotNull();

        await using (var s2 = await transaction.Start())
        {
            await deleteSet.ForEachAsync(async x => (await keyStore.Add(x.r.Name, x.r)).BeOk());
        }

        // verify all files
        foreach (var record in records)
        {
            (await keyStore.Get(record.r.Name)).BeOk().Return().ToObject<MapRecord>().Be(record.r);
            (await lowerKeyStore.Exists(record.p)).BeOk();
        }

        foreach (var record in deleteSet)
        {
            (await lowerKeyStore.Delete(record.p)).BeOk();
            (await lowerKeyStore.Exists(record.p)).BeNotFound();
        }

        // Recover by journal
        testProvider._logSequenceNumber = currentLsn.NotEmpty();
        (await transaction.Recovery()).BeOk();

        // verify all files
        foreach (var record in records)
        {
            (await keyStore.Get(record.r.Name)).BeOk().Return().ToObject<MapRecord>().Be(record.r);
            (await lowerKeyStore.Exists(record.p)).BeOk();
        }

        var journal = (await listStore.Get("TestJournal")).BeOk().Return();
        journal.Count.Be(2);
        journal[0].Entries.Count.Be(6);
        journal[1].Entries.Count.Be(4);
    }

    private class TestProvider : ITrxProvider
    {
        private TrxRecorder? _recorder;
        public string? _logSequenceNumber;

        public string SourceName => nameof(TestProvider);
        public void AttachRecorder(TrxRecorder trxRecorder) => _recorder = trxRecorder;
        public Task<Option> Checkpoint() => new Option(StatusCode.ServiceUnavailable).ToTaskResult();

        public Task<Option> Commit(DataChangeRecord dcr)
        {
            _logSequenceNumber = dcr.GetLastLogSequenceNumber();
            return new Option(StatusCode.OK).ToTaskResult();
        }

        public void DetachRecorder() => _recorder = null;
        public Option<string> GetLogSequenceNumber() => _logSequenceNumber switch { null => StatusCode.NotFound, _ => _logSequenceNumber };
        public Task<Option> Restore(string json) => new Option(StatusCode.OK).ToTaskResult();
        public Task<Option> Rollback(DataChangeEntry dataChangeRecord) => new Option(StatusCode.OK).ToTaskResult();
        public void SetLogSequenceNumber(string lsn) => _logSequenceNumber = lsn;
        public Task<Option> Start() => new Option(StatusCode.OK).ToTaskResult();
        public Task<Option> Recovery(TrxRecoveryScope trxRecoveryScope) => new Option(StatusCode.OK).ToTaskResult();
    }
}
