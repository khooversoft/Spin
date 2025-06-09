using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Toolbox.Store;
using Toolbox.Tools;
using Xunit.Abstractions;

namespace Toolbox.Test.Store;

public class HybridCacheTypedTests
{
    private readonly ITestOutputHelper _outputHelper;
    public HybridCacheTypedTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

    [Fact]
    public async Task NoCache()
    {
        using var host = BuildService(false, false, null);
        await HybridCacheTypedCommonTests.NoCache(host);
    }

    [Fact]
    public async Task ProviderCreatedCache()
    {
        int command = -1;
        var custom = new HybridCacheTypedCommonTests.CustomProvider(x => command = x);

        using var host = BuildService(false, false, custom);
        await HybridCacheTypedCommonTests.ProviderCreatedCache(host);
        command.Be(1);
    }

    [Fact]
    public async Task OnlyMemoryCache()
    {
        using var host = BuildService(true, false, null, memoryCacheDuration: TimeSpan.FromMilliseconds(100));
        await HybridCacheTypedCommonTests.OnlyMemoryCache(host);
    }

    [Fact]
    public async Task OnlyFileCache()
    {
        using var host = BuildService(false, true, null, fileCacheDuration: TimeSpan.FromMilliseconds(100));
        await HybridCacheTypedCommonTests.OnlyFileCache(host);
    }

    [Fact]
    public async Task MemoryAndFileCache()
    {
        using var host = BuildService(true, true, null, memoryCacheDuration: TimeSpan.FromMilliseconds(100), fileCacheDuration: TimeSpan.FromMilliseconds(500));
        await HybridCacheTypedCommonTests.MemoryAndFileCache(host);
    }

    [Fact]
    public async Task MemoryAndFileCacheWithProviderAsSource()
    {
        int command = -1;
        var custom = new HybridCacheTypedCommonTests.CustomProvider(x => command = x);

        using var host = BuildService(true, true, custom, TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(500));
        await HybridCacheTypedCommonTests.MemoryAndFileCacheWithProviderAsSource(host);
        command.Be(1);
    }


    private IHost BuildService(
        bool addMemory,
        bool addFileStore,
        IHybridCacheProvider? custom,
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
                    (TimeSpan v1, null) => new HybridCacheOption { MemoryCacheDuration = v1, },
                    (null, TimeSpan v2) => new HybridCacheOption { FileCacheDuration = v2 },
                    (TimeSpan v1, TimeSpan v2) => new HybridCacheOption { MemoryCacheDuration = v1, FileCacheDuration = v2 },
                    _ => new HybridCacheOption()
                };

                services.Configure<HybridCacheOption>(x =>
                {
                    x.MemoryCacheDuration = option.MemoryCacheDuration;
                    x.FileCacheDuration = option.FileCacheDuration;
                });

                services.AddHybridCache<HybridCacheTypedCommonTests.EntityModel>(builder =>
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
