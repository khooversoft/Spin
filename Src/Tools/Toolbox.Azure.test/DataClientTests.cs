using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Toolbox.Azure.test.Application;
using Toolbox.Store;
using Toolbox.Test.Data.Client;
using Toolbox.Tools;
using Xunit.Abstractions;

namespace Toolbox.Azure.test;

public class DataClientTests
{
    private readonly ITestOutputHelper _outputHelper;
    public DataClientTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

    [Fact]
    public async Task NoCache()
    {
        using var host = BuildService(false, false, null);
        await DataClientCommonTests.NoCache(host);
    }

    [Fact]
    public async Task ProviderCreatedCache()
    {
        int command = -1;
        var custom = new DataClientCommonTests.CustomProvider(x => command = x);

        using var host = BuildService(false, false, custom);
        await DataClientCommonTests.ProviderCreatedCache(host);
        command.Be(1);
    }

    [Fact]
    public async Task OnlyMemoryCache()
    {
        using var host = BuildService(true, false, null, memoryCacheDuration: TimeSpan.FromMilliseconds(100));
        await DataClientCommonTests.OnlyMemoryCache(host);
    }

    [Fact]
    public async Task OnlyFileCache()
    {
        using var host = BuildService(false, true, null, fileCacheDuration: TimeSpan.FromSeconds(1));
        await DataClientCommonTests.OnlyFileCache(host);
    }

    [Fact]
    public async Task MemoryAndFileCache()
    {
        using var host = BuildService(
            true,
            true,
            null,
            memoryCacheDuration: TimeSpan.FromMilliseconds(500),
            fileCacheDuration: TimeSpan.FromSeconds(2)
            );

        await DataClientCommonTests.MemoryAndFileCache(host, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(3));
    }

    [Fact]
    public async Task MemoryAndFileCacheWithProviderAsSource()
    {
        int command = -1;
        var custom = new DataClientCommonTests.CustomProvider(x => command = x);

        using var host = BuildService(
            true,
            true,
            custom,
            memoryCacheDuration: TimeSpan.FromMilliseconds(500),
            fileCacheDuration: TimeSpan.FromSeconds(2)
            );

        await DataClientCommonTests.MemoryAndFileCacheWithProviderAsSource(host, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(3));
        command.Be(1);
    }


    private IHost BuildService(
        bool addMemory,
        bool addFileStore,
        IDataProvider? custom,
        TimeSpan? memoryCacheDuration = null,
        TimeSpan? fileCacheDuration = null
        )
    {
        var datalakeOption = TestApplication.ReadOption("datastore-hybridCache-tests");

        var host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddLogging(config => config.AddLambda(_outputHelper.WriteLine).AddDebug().AddFilter(x => true));

                if (addFileStore) services.AddDatalakeFileStore(datalakeOption);

                var option = (memoryCacheDuration, fileCacheDuration) switch
                {
                    (TimeSpan v1, null) => new DataClientOption { MemoryCacheDuration = v1, },
                    (null, TimeSpan v2) => new DataClientOption { FileCacheDuration = v2 },
                    (TimeSpan v1, TimeSpan v2) => new DataClientOption { MemoryCacheDuration = v1, FileCacheDuration = v2 },
                    _ => new DataClientOption()
                };

                services.Configure<DataClientOption>(x =>
                {
                    x.MemoryCacheDuration = option.MemoryCacheDuration;
                    x.FileCacheDuration = option.FileCacheDuration;
                });

                services.AddHybridCache(builder =>
                {
                    if (addMemory) builder.AddMemoryCache();
                    if (addFileStore) builder.AddFileStoreCache();
                    if (custom != null) builder.AddProvider(custom);
                });
            })
            .Build();

        return host;
    }

}
