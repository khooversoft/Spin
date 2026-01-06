using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Test.Store.KeyStore;

public class KeyStore_TypedTests
{
    private ITestOutputHelper _outputHelper;
    private record TestRecord(string Name, int Age);

    public KeyStore_TypedTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

    protected virtual void AddStore(IServiceCollection services, string basePath) => services.AddInMemoryKeyStore();

    private async Task<IHost> BuildService(bool useHash, bool useCache, [CallerMemberName] string function = "")
    {
        string basePath = nameof(KeyStore_ReadWriteTests) + "/" + function;

        var host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddLogging(c => c.AddLambda(_outputHelper.WriteLine).AddDebug().AddFilter(_ => true));
                AddStore(services, basePath);
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
    public async Task SingleFileReadWriteSearchDelete(bool useHash, bool useCache)
    {
        var host = await BuildService(useHash, useCache);
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

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task SetTyped_CreateNewFile_ShouldSucceed(bool useHash, bool useCache)
    {
        var host = await BuildService(useHash, useCache);
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

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task SetTyped_UpdateExistingFile_ShouldOverwriteAndReturnNewETag(bool useHash, bool useCache)
    {
        var host = await BuildService(useHash, useCache);
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