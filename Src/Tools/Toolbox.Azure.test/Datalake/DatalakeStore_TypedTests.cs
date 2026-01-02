using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Toolbox.Azure.test.Application;
using Toolbox.Extensions;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Azure.test.Datalake;

public class DatalakeStore_TypedTests
{
    private ITestOutputHelper _outputHelper;
    private record TestRecord(string Name, int Age);

    public DatalakeStore_TypedTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

    private async Task<IHost> BuildService([CallerMemberName] string function = "")
    {
        var option = TestApplication.ReadOption(nameof(DatalakeStore_TypedTests) + "/" + function);

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
    public async Task SingleFileReadWriteSearchDelete()
    {
        var host = await BuildService();
        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();

        string path = "testrecord.json";
        var testRecord = new TestRecord("John Doe", 30);

        (await keyStore.Exists(path)).BeNotFound();
        (await keyStore.Get<TestRecord>(path)).BeNotFound();
        (await keyStore.Delete(path)).BeNotFound();

        (await keyStore.Add(path, testRecord)).BeOk();
        (await keyStore.Add(path, testRecord)).BeConflict();

        (await keyStore.Exists(path)).BeOk();
        (await keyStore.Get<TestRecord>(path)).BeOk().Return().Action(x =>
        {
            x.Be(testRecord);
        });

        (await keyStore.Search("**.*")).Count().Be(1);
        (await keyStore.Delete(path)).BeOk();
        (await keyStore.Search("**.*")).Count().Be(0);
    }

    [Fact]
    public async Task SetTyped_CreateNewFile_ShouldSucceed()
    {
        var host = await BuildService();
        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();

        string path = "typed-set-new.json";
        var record = new TestRecord("New User", 18);

        (await keyStore.Exists(path)).BeNotFound();

        var setResult = await keyStore.Set(path, record);
        setResult.BeOk();
        setResult.Return().NotEmpty();

        (await keyStore.Get<TestRecord>(path)).BeOk().Return().Be(record);

        (await keyStore.Delete(path)).BeOk();
    }

    [Fact]
    public async Task SetTyped_UpdateExistingFile_ShouldOverwriteAndReturnNewETag()
    {
        var host = await BuildService();
        IKeyStore keyStore = host.Services.GetRequiredService<IKeyStore>();

        string path = "typed-set-update.json";
        var original = new TestRecord("Original", 20);
        var updated = new TestRecord("Updated", 25);

        var addResult = await keyStore.Add(path, original);
        addResult.BeOk();
        string originalETag = addResult.Return();

        var setResult = await keyStore.Set(path, updated);
        setResult.BeOk();
        string newETag = setResult.Return();
        newETag.NotEmpty().NotBe(originalETag);

        (await keyStore.Get<TestRecord>(path)).BeOk().Return().Be(updated);

        (await keyStore.Delete(path)).BeOk();
    }
}
