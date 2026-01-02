using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Toolbox.Azure.test.Application;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Azure.test.Datalake;

public class DatalakeStore_ETagTests
{
    private ITestOutputHelper _outputHelper;
    private record TestRecord(string Name, int Age);

    public DatalakeStore_ETagTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

    private async Task<IHost> BuildService([CallerMemberName] string function = "")
    {
        var option = TestApplication.ReadOption(nameof(DatalakeStore_ETagTests) + "/" + function);

        var host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddLogging(c => c.AddLambda(_outputHelper.WriteLine).AddDebug().AddFilter(_ => true));
                services.AddDatalakeFileStore(option);
            })
            .Build();

        // Clear the store before running tests, this includes any locked files
        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();
        await keyStore.ClearStore();
        (await keyStore.Search("**.*")).Count().Be(0);
        return host;
    }


    [Fact]
    public async Task Set_WithETag_OptimisticConcurrency_ShouldSucceed()
    {
        var host = await BuildService();
        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();

        string path = "etag-test.json";
        var record1 = new TestRecord("First", 10);
        var record2 = new TestRecord("Second", 20);

        (await keyStore.Add(path, record1)).BeOk();

        var getResult = await keyStore.Get(path);
        getResult.BeOk();
        DataETag dataWithETag = getResult.Return();

        var updateData = record2.ToDataETag(dataWithETag.ETag);
        (await keyStore.Set(path, updateData)).BeOk();

        (await keyStore.Get<TestRecord>(path)).BeOk().Return().Be(record2);

        (await keyStore.Delete(path)).BeOk();
    }


    [Fact]
    public async Task Set_WithStaleETag_ShouldReturnLocked()
    {
        var host = await BuildService();
        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();

        string path = "stale-etag-test.json";
        var record1 = new TestRecord("First", 10);
        var record2 = new TestRecord("Second", 20);
        var record3 = new TestRecord("Third", 30);

        (await keyStore.Add(path, record1)).BeOk();

        var getResult1 = await keyStore.Get(path);
        getResult1.BeOk();
        DataETag dataWithETag1 = getResult1.Return();

        (await keyStore.Set(path, record2.ToDataETag())).BeOk();

        var staleUpdate = record3.ToDataETag(dataWithETag1.ETag);
        var setResult = await keyStore.Set(path, staleUpdate);
        setResult.BeConflict();

        (await keyStore.Delete(path)).BeOk();
    }


    [Fact]
    public async Task Get_ReturnsValidETag()
    {
        var host = await BuildService();
        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();

        string path = "etag-return-test.json";
        var testRecord = new TestRecord("ETag Test", 30);

        (await keyStore.Add(path, testRecord)).BeOk();

        var getResult = await keyStore.Get(path);
        getResult.BeOk();
        DataETag data = getResult.Return();

        data.ETag.NotEmpty();
        data.Data.Length.Assert(x => x > 0);

        (await keyStore.Delete(path)).BeOk();
    }

    [Fact]
    public async Task Add_ReturnsETag()
    {
        var host = await BuildService();
        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();

        string path = "add-etag-test.json";
        var testRecord = new TestRecord("Add ETag Test", 35);

        var addResult = await keyStore.Add(path, testRecord.ToDataETag());
        addResult.BeOk();
        string etag = addResult.Return();
        etag.NotEmpty();

        (await keyStore.Delete(path)).BeOk();
    }

    [Fact]
    public async Task Set_WithETag_ShouldReturnNewETag_AndAllowChainedUpdates()
    {
        var host = await BuildService();
        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();

        string path = "etag-chain-test.json";
        var record1 = new TestRecord("One", 1);
        var record2 = new TestRecord("Two", 2);
        var record3 = new TestRecord("Three", 3);

        var addResult = await keyStore.Add(path, record1.ToDataETag());
        addResult.BeOk();
        string initialETag = addResult.Return();
        initialETag.NotEmpty();

        var setResult1 = await keyStore.Set(path, record2.ToDataETag(initialETag));
        setResult1.BeOk();
        string etag1 = setResult1.Return();
        etag1.NotEmpty().NotBe(initialETag);

        var setResult2 = await keyStore.Set(path, record3.ToDataETag(etag1));
        setResult2.BeOk();
        string etag2 = setResult2.Return();
        etag2.NotEmpty().NotBe(etag1);

        (await keyStore.Get<TestRecord>(path)).BeOk().Return().Be(record3);

        (await keyStore.Delete(path)).BeOk();
    }

    [Fact]
    public async Task Set_WithoutETag_ShouldOverwriteAndChangeETag()
    {
        var host = await BuildService();
        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();

        string path = "etag-overwrite-no-match.json";
        var original = new TestRecord("Original", 5);
        var updated = new TestRecord("Updated", 6);

        var addResult = await keyStore.Add(path, original.ToDataETag());
        addResult.BeOk();
        string originalETag = addResult.Return();
        originalETag.NotEmpty();

        var setResult = await keyStore.Set(path, updated.ToDataETag());
        setResult.BeOk();
        string newETag = setResult.Return();
        newETag.NotEmpty().NotBe(originalETag);

        (await keyStore.Get<TestRecord>(path)).BeOk().Return().Be(updated);

        (await keyStore.Delete(path)).BeOk();
    }
}
