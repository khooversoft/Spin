using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Toolbox.Azure.test.Application;
using Toolbox.Data;
using Toolbox.Test.Data.Client.Common;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Azure.test.Data;

public class FileUpdateTests
{
    private readonly ITestOutputHelper _outputHelper;
    private const string _pipelineName = nameof(FileUpdateTests) + ".pipeline";
    private const string _basePath = "data/single-file-updates";
    private const int _count = 100;

    public FileUpdateTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

    [Fact]
    public async Task Direct()
    {
        using var host = BuildService(false, false);
        var context = host.Services.CreateContext<FileUpdateTests>();

        using (var scope = context.LogDuration((t, d) => context.LogInformation("Duration={duration}", d)))
        {
            await DataClientSingleFileUpdate.SingleThreadFileUpdate(host, _pipelineName, _count);
        }
    }

    [Fact]
    public async Task WithCache()
    {
        using var host = BuildService(true, false);
        var context = host.Services.CreateContext<FileUpdateTests>();

        using (var scope = context.LogDuration((t, d) => context.LogInformation("Duration={duration}", d)))
        {
            await DataClientSingleFileUpdate.SingleThreadFileUpdate(host, _pipelineName, _count);
        }
    }


    [Fact]
    public async Task WithQueue()
    {
        using var host = BuildService(false, true);
        var context = host.Services.CreateContext<FileUpdateTests>();

        using (var scope = context.LogDuration((t, d) => context.LogInformation("Duration={duration}", d)))
        {
            await DataClientSingleFileUpdate.SingleThreadFileUpdate(host, _pipelineName, _count);
        }
    }

    [Fact]
    public async Task WithCacheQueue()
    {
        using var host = BuildService(true, true);
        var context = host.Services.CreateContext<FileUpdateTests>();

        using (var scope = context.LogDuration((t, d) => context.LogInformation("Duration={duration}", d)))
        {
            await DataClientSingleFileUpdate.SingleThreadFileUpdate(host, _pipelineName, _count);
        }
    }

    private IHost BuildService(bool addCache, bool addQueue)
    {
        var datalakeOption = TestApplication.ReadOption("datastore-hybridCache-tests");

        var host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddLogging(config => config.AddLambda(_outputHelper.WriteLine).AddDebug()/*.AddFilter(x => true)*/);
                services.AddDatalakeFileStore(datalakeOption);

                services.AddDataPipeline<DataClientSingleFileUpdate.EntityMaster>(_pipelineName, builder =>
                {
                    builder.MemoryCacheDuration = TimeSpan.FromSeconds(100);
                    builder.BasePath = _basePath;

                    if (addCache) builder.AddCacheMemory();
                    if (addQueue) builder.AddQueueStore();
                    builder.AddFileStore();
                });
            })
            .Build();

        return host;
    }
}
