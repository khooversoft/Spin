using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Toolbox.Data;
using Toolbox.Tools;
using Xunit.Abstractions;

namespace Toolbox.Test.Data.Client;

public class DataClientTypedTests
{
    private readonly ITestOutputHelper _outputHelper;
    public DataClientTypedTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

    [Fact]
    public async Task NoCache()
    {
        using var host = BuildService(false, false, null);
        await DataClientTypedCommonTests.NoCache(host);
    }

    [Fact]
    public async Task ProviderCreatedCache()
    {
        int command = -1;
        var custom = new DataClientTypedCommonTests.CustomProvider(x => command = x);

        using var host = BuildService(false, false, custom);
        await DataClientTypedCommonTests.ProviderCreatedCache(host);
        command.Be(1);
    }

    [Fact]
    public async Task OnlyMemoryCache()
    {
        using var host = BuildService(true, false, null, memoryCacheDuration: TimeSpan.FromMilliseconds(100));
        await DataClientTypedCommonTests.OnlyMemoryCache(host);
    }

    [Fact]
    public async Task OnlyFileCache()
    {
        using var host = BuildService(false, true, null, fileCacheDuration: TimeSpan.FromMilliseconds(100));
        await DataClientTypedCommonTests.OnlyFileCache(host);
    }

    [Fact]
    public async Task MemoryAndFileCache()
    {
        using var host = BuildService(true, true, null, memoryCacheDuration: TimeSpan.FromMilliseconds(100), fileCacheDuration: TimeSpan.FromMilliseconds(500));
        await DataClientTypedCommonTests.MemoryAndFileCache(host);
    }

    [Fact]
    public async Task MemoryAndFileCacheWithProviderAsSource()
    {
        int command = -1;
        var custom = new DataClientTypedCommonTests.CustomProvider(x => command = x);

        using var host = BuildService(true, true, custom, TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(500));
        await DataClientTypedCommonTests.MemoryAndFileCacheWithProviderAsSource(host);
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
        var host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddLogging(config => config.AddLambda(_outputHelper.WriteLine).AddDebug().AddFilter(x => true));

                if (addFileStore) services.AddInMemoryFileStore();

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

                services.AddDataClient<DataClientTypedCommonTests.EntityModel>(builder =>
                {
                    if (addMemory) builder.AddMemoryCache();
                    if (addFileStore) builder.AddFileStoreCache();
                    if (custom != null) builder.AddProvider(_ => custom);
                });
            })
            .Build();

        return host;
    }
}
