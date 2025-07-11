using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Toolbox.Data;
using Toolbox.Test.Data.Client.Common;
using Toolbox.Tools;
using Xunit.Abstractions;

namespace Toolbox.Test.Data.Client;

public class DataExclusiveLockTests
{
    private readonly ITestOutputHelper _outputHelper;
    private const string _pipelineName = nameof(DataClientTests) + ".pipeline";
    private const string _basePath = "data/DataExclusiveTests";
    private const string _key = "exclusive-lock-key";

    public DataExclusiveLockTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

    [Fact]
    public async Task ExclusiveLock()
    {
        using var host = BuildService();
        await DataExclusiveLockCommonTests.ExclusiveLock(host, _pipelineName, _key);
    }

    private IHost BuildService()
    {
        var host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddLogging(config => config.AddLambda(_outputHelper.WriteLine).AddDebug().AddFilter(x => true));
                services.AddInMemoryFileStore();

                services.AddDataPipeline<DataExclusiveLockCommonTests.EntityModel>(_pipelineName, builder =>
                {
                    builder.BasePath = _basePath;

                    builder.AddFileLocking(config =>
                    {
                        config.Add<DataExclusiveLockCommonTests.EntityModel>(LockMode.Exclusive, _pipelineName, _key);
                    });

                    builder.AddFileStore();
                });
            })
            .Build();

        return host;
    }
}
