//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Hosting;
//using Microsoft.Extensions.Logging;
//using Toolbox.Azure;
//using Toolbox.Extensions;
//using Toolbox.Graph.test.Application;
//using Toolbox.Store;
//using Toolbox.Tools;
//using Toolbox.Types;
//using Xunit.Abstractions;

//namespace Toolbox.Graph.test.Stress;

//public class GraphCommandStressTests
//{
//    private readonly ITestOutputHelper _logOutput;
//    private ILogger _logger = null!;

//    public GraphCommandStressTests(ITestOutputHelper logOutput) => _logOutput = logOutput;

//    private async Task<IHost> CreateService(bool useDatalake)
//    {
//        var host = Host.CreateDefaultBuilder()
//            .AddDebugLogging(x => _logOutput.WriteLine(x))
//            .ConfigureServices((context, services) =>
//            {
//                services.AddInMemoryKeyStore();
//                services.AddGraphEngine(config => config.BasePath = "basePath");
//            })
//            .Build();

//        _logger = host.Services.GetRequiredService<ILogger<GraphCommandStressTests>>();

//        IGraphEngine graphEngine = host.Services.GetRequiredService<IGraphEngine>();
//        await graphEngine.DataManager.LoadDatabase();
//        return host;
//    }

//    [Theory]
//    [InlineData(false)]
//    [InlineData(true)]
//    public async Task StressTest(bool useDataLake)
//    {
//        using var host = await CreateService(useDataLake);
//        var graphClient = host.Services.GetRequiredService<IGraphClient>();
//        var graphEngine = host.Services.GetRequiredService<IGraphEngine>();
//        const int taskCount = 10;

//        var token = new CancellationTokenSource(TimeSpan.FromSeconds(20));

//        Task<bool>[] list = Enumerable.Range(0, taskCount)
//            .SelectMany(x => new IWorker[] {
//                new CommandTwoNodesAndEdges(graphClient, _logger, x),
//                new CommandThreeNodesAndEdges(graphClient, _logger, x) },
//                (o, i) => i)
//            .Select(x => x.Run(token))
//            .ToArray();

//        _logger.LogInformation("Starting tasks");
//        bool[] result = await Task.WhenAll(list);
//        _logger.LogInformation("Finished tasks");
//    }
//}
