using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Toolbox.Azure.test.Application;
using Toolbox.Data;
using Toolbox.Test.Data.Client.Common;
using Toolbox.Tools;
using Xunit.Abstractions;

namespace Toolbox.Azure.test;

public class DataClientQueueTests
{
    private const string _pipelineName = nameof(DataClientQueueTests) + ".pipeline";
    private readonly ITestOutputHelper _outputHelper;

    public DataClientQueueTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

    [Fact]
    public async Task OnlyFileCache()
    {
        using var host = BuildService(false, true, null, fileCacheDuration: TimeSpan.FromSeconds(1));
        await DataQueueClientCommonTests.OnlyFileCache(host, _pipelineName);
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

        await DataQueueClientCommonTests.MemoryAndFileCache(host, _pipelineName);
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

        await DataQueueClientCommonTests.MemoryAndFileCacheWithProviderAsSource(host, _pipelineName);
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

                services.AddDataPipeline<DataClientCommonTests.EntityModel>(_pipelineName, builder =>
                {
                    builder.MemoryCacheDuration = memoryCacheDuration;
                    builder.FileCacheDuration = fileCacheDuration;
                    builder.BasePath = nameof(DataClientQueueTests);

                    if (addMemory) builder.AddMemory();
                    builder.AddQueueStore();
                    if (addFileStore) builder.AddFileStore();
                    if (custom != null) builder.AddProvider(_ => custom);
                });
            })
            .Build();

        return host;
    }

}
