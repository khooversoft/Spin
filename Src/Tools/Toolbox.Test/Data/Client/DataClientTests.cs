using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Toolbox.Data;
using Toolbox.Test.Data.Client.Common;
using Toolbox.Tools;
using Xunit.Abstractions;

namespace Toolbox.Test.Data.Client;

public class DataClientTests
{
    private readonly ITestOutputHelper _outputHelper;
    private const string _pipelineName = nameof(DataClientTests) + ".pipeline";
    public DataClientTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

    [Fact]
    public void NoCache()
    {
        using var host = BuildService(false, false, null);
        DataClientCommonTests.NoHandler(host, _pipelineName);
    }

    [Fact]
    public async Task ProviderCreatedCache()
    {
        int command = -1;
        var custom = new DataClientCommonTests.CustomProvider(x => command = x);

        using var host = BuildService(false, false, custom);
        await DataClientCommonTests.ProviderCreatedCache(host, _pipelineName);
        command.Be(1);
    }

    [Fact]
    public async Task OnlyMemoryCache()
    {
        using var host = BuildService(true, false, null, memoryCacheDuration: TimeSpan.FromMilliseconds(100));
        await DataClientCommonTests.OnlyMemoryCache(host, _pipelineName);
    }

    [Fact]
    public async Task OnlyFileCache()
    {
        using var host = BuildService(false, true, null, fileCacheDuration: TimeSpan.FromMilliseconds(100));
        await DataClientCommonTests.OnlyFileCache(host, _pipelineName);
    }

    [Fact]
    public async Task MemoryAndFileCache()
    {
        using var host = BuildService(true, true, null, memoryCacheDuration: TimeSpan.FromMilliseconds(100), fileCacheDuration: TimeSpan.FromMilliseconds(500));
        await DataClientCommonTests.MemoryAndFileCache(host, _pipelineName);
    }

    [Fact]
    public async Task MemoryAndFileCacheWithProviderAsSource()
    {
        int command = -1;
        var custom = new DataClientCommonTests.CustomProvider(x => command = x);

        using var host = BuildService(true, true, custom, TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(500));
        await DataClientCommonTests.MemoryAndFileCacheWithProviderAsSource(host, _pipelineName);
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

                services.AddDataPipeline<DataClientCommonTests.EntityModel>(_pipelineName, builder =>
                {
                    builder.MemoryCacheDuration = memoryCacheDuration;
                    builder.FileCacheDuration = fileCacheDuration;
                    builder.BasePath = nameof(DataClientTests);

                    if (addMemory) builder.AddCacheMemory();
                    if (addFileStore) builder.AddFileStore();
                    if (custom != null) builder.AddProvider(_ => custom);
                });
            })
            .Build();

        return host;
    }
}
