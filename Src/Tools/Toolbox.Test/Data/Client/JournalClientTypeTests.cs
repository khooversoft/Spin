using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Toolbox.Data;
using Toolbox.Test.Data.Client.Common;
using Toolbox.Tools;
using Xunit.Abstractions;

namespace Toolbox.Test.Data.Client;

public class JournalClientTypeTests
{
    private readonly ITestOutputHelper _outputHelper;
    private const string _pipelineName = nameof(JournalClientTypeTests) + ".pipeline";
    public JournalClientTypeTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

    [Fact]
    public async Task SingleAppend()
    {
        using var host = BuildService();
        await JournalCommonTests.SingleAppend(host, _pipelineName);
    }

    [Fact]
    public async Task TwoAppend()
    {
        using var host = BuildService();
        await JournalCommonTests.TwoAppend(host, _pipelineName);
    }

    [Fact]
    public async Task ScaleAppend()
    {
        using var host = BuildService();
        await JournalCommonTests.ScaleAppend(host, _pipelineName);
    }

    private IHost BuildService()
    {
        var host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddLogging(config => config.AddLambda(_outputHelper.WriteLine).AddDebug().AddFilter(x => true));
                services.AddInMemoryFileStore();

                services.AddJournalPipeline<JournalCommonTests.EntityModel>(builder =>
                {
                    builder.MemoryCacheDuration = TimeSpan.FromMinutes(1);
                    builder.BasePath = nameof(JournalClientTypeTests);
                    builder.AddMemory();
                    builder.AddJournalStore();
                }, _pipelineName);
            })
            .Build();

        return host;
    }
}
