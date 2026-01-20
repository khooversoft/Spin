using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Graph.test.Command;

public class ParallelStressTests
{
    private readonly ITestOutputHelper _logOutput;
    public ParallelStressTests(ITestOutputHelper logOutput) => _logOutput = logOutput;

    private async Task<IHost> CreateService()
    {
        var host = Host.CreateDefaultBuilder()
            .AddDebugLogging(x => _logOutput.WriteLine(x))
            .ConfigureServices((context, services) =>
            {
                services.AddInMemoryKeyStore();
                services.AddGraphEngine(config => config.BasePath = "basePath");
            })
            .Build();

        IGraphEngine graphEngine = host.Services.GetRequiredService<IGraphEngine>();
        await graphEngine.DataManager.LoadDatabase();

        return host;
    }

    [Fact]
    public async Task ParallelAddTasks()
    {
        const int count = 1000;
        using var host = await CreateService();
        var graphClient = host.Services.GetRequiredService<IGraphClient>();
        var graphEngine = host.Services.GetRequiredService<IGraphEngine>();

        await ActionParallel.Run(x => AddNodes(graphClient, x), Enumerable.Range(0, count), 5);
        await ActionParallel.Run(x => AddEdges(graphClient, x), Enumerable.Range(0, count - 1), 5);

        graphEngine.DataManager.GetMap().Nodes.Count.Be(count);
        graphEngine.DataManager.GetMap().Edges.Count.Be(count - 1);
    }

    private async Task AddNodes(IGraphClient graphClient, int index)
    {
        string key = $"node{index}";
        string tags = $"t1,t2=v{index}";

        string cmd = $"set node key={key} set {tags};";
        var option = await graphClient.ExecuteBatch(cmd);
        option.IsOk().BeTrue();
    }

    private async Task<Option> AddEdges(IGraphClient graphClient, int index)
    {
        string fromKey = $"node{index}";
        string toKey = $"node{index + 1}";
        string tags = $"t1,t2=v{index}";

        string cmd = $"set edge from={fromKey}, to={toKey}, type=et set {tags};";
        var option = await graphClient.ExecuteBatch(cmd);
        option.IsOk().BeTrue();
        return option.ToOptionStatus();
    }
}
