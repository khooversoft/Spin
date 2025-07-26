using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
            .ConfigureLogging(config => config.AddFilter(x => true).AddLambda(x => _logOutput.WriteLine(x)))
            .ConfigureServices((context, services) =>
            {
                services.AddInMemoryFileStore();
                services.AddGraphEngine(config => config.BasePath = "basePath");
            })
            .Build();

        IGraphEngine graphEngine = host.Services.GetRequiredService<IGraphEngine>();
        var context = host.Services.GetRequiredService<ILogger<ParallelStressTests>>().ToScopeContext();
        await graphEngine.DataManager.LoadDatabase(context);

        return host;
    }

    [Fact]
    public async Task ParallelAddTasks()
    {
        const int count = 1000;
        using var host = await CreateService();
        var context = host.Services.GetRequiredService<ILogger<AddEdgeCommandTests>>().ToScopeContext();
        var graphClient = host.Services.GetRequiredService<IGraphClient>();
        var graphEngine = host.Services.GetRequiredService<IGraphEngine>();

        await ActionParallel.Run(x => AddNodes(graphClient, x, context), Enumerable.Range(0, count), 5);
        await ActionParallel.Run(x => AddEdges(graphClient, x, context), Enumerable.Range(0, count - 1), 5);

        graphEngine.DataManager.GetMap().Nodes.Count.Be(count);
        graphEngine.DataManager.GetMap().Edges.Count.Be(count - 1);
    }

    private async Task AddNodes(IGraphClient graphClient, int index, ScopeContext context)
    {
        string key = $"node{index}";
        string tags = $"t1,t2=v{index}";

        string cmd = $"set node key={key} set {tags};";
        var option = await graphClient.ExecuteBatch(cmd, context);
        option.IsOk().BeTrue();
    }

    private async Task<Option> AddEdges(IGraphClient graphClient, int index, ScopeContext context)
    {
        string fromKey = $"node{index}";
        string toKey = $"node{index + 1}";
        string tags = $"t1,t2=v{index}";

        string cmd = $"set edge from={fromKey}, to={toKey}, type=et set {tags};";
        var option = await graphClient.ExecuteBatch(cmd, context);
        option.IsOk().BeTrue();
        return option.ToOptionStatus();
    }

    //private async Task<Option> AddBatch(GraphTestClient testClient, int index, Func<int, bool> addEdge)
    //{
    //    var cmds = new Sequence<string>();

    //    string key = $"node{index}";
    //    string tags = $"t1,t2=v{index}";
    //    string cmd = $"set node key={key}, {tags};";
    //    cmds += cmd;

    //    if (addEdge(index))
    //    {
    //        string fromKey = $"node{index - 1}";
    //        string toKey = $"node{index}";
    //        string cmd2 = $"set edge Key={fromKey}, toKey={toKey}, type=et set {tags};";
    //        cmds += cmd2;
    //    }

    //    string command = cmds.Join();
    //    var option = await testClient.ExecuteBatch(command, NullScopeContext.Default);
    //    option.IsOk().BeTrue();
    //    return option.ToOptionStatus();
    //}
}
