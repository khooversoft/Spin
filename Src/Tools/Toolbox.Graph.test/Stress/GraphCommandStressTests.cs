using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Toolbox.Types;
using Toolbox.Tools;
using Xunit.Abstractions;
using Toolbox.Graph.test.Application;
using Toolbox.Azure;
using Toolbox.Store;
using Toolbox.Extensions;

namespace Toolbox.Graph.test.Stress;

public class GraphCommandStressTests
{
    private readonly ITestOutputHelper _logOutput;
    public GraphCommandStressTests(ITestOutputHelper logOutput) => _logOutput = logOutput;

    private async Task<IHost> CreateService(bool useDatalake)
    {
        DatalakeOption datalakeOption = TestApplication.ReadDatalakeOption("test-GraphCommandStressTests");

        var host = Host.CreateDefaultBuilder()
            .ConfigureLogging(config => config.AddFilter(x => true).AddLambda(x => _logOutput.WriteLine(x)))
            .ConfigureServices((context, services) =>
            {
                _ = useDatalake switch
                {
                    true => services.AddDatalakeFileStore(datalakeOption),
                    false => services.AddInMemoryFileStore(),
                };

                services.AddGraphEngine(config => config.BasePath = "basePath");
            })
            .Build();

        var context = host.Services.GetRequiredService<ILogger<GraphCommandStressTests>>().ToScopeContext();

        IFileStore fileStore = host.Services.GetRequiredService<IFileStore>();
        var list = await fileStore.Search("**/*", context);
        await list.ForEachAsync(async x => await fileStore.File(x.Path).Delete(context));

        IGraphEngine graphEngine = host.Services.GetRequiredService<IGraphEngine>();
        await graphEngine.DataManager.LoadDatabase(context);

        return host;
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task StressTest(bool useDataLake)
    {
        using var host = await CreateService(useDataLake);
        var context = host.Services.GetRequiredService<ILogger<GraphCommandStressTests>>().ToScopeContext();
        var graphClient = host.Services.GetRequiredService<IGraphClient>();
        var graphEngine = host.Services.GetRequiredService<IGraphEngine>();
        const int taskCount = 10;

        var token = new CancellationTokenSource(TimeSpan.FromSeconds(20));

        Task<bool>[] list = Enumerable.Range(0, taskCount)
            .SelectMany(x => new IWorker[] {
                new CommandTwoNodesAndEdges(graphClient, context, x),
                new CommandThreeNodesAndEdges(graphClient, context, x) },
                (o, i) => i)
            .Select(x => x.Run(token, context))
            .ToArray();

        context.LogInformation("Starting tasks");
        bool[] result = await Task.WhenAll(list);
        context.LogInformation("Finished tasks");
    }
}
