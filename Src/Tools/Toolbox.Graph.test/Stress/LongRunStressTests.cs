using System.Security.Cryptography;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Toolbox.Azure;
using Toolbox.Extensions;
using Toolbox.Graph.test.Application;
using Toolbox.Store;
using Toolbox.Tools;

using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Graph.test.Stress;

public class LongRunStressTests
{
    private readonly ITestOutputHelper _logOutput;
    private record Payload(string Name, int Count);

    public LongRunStressTests(ITestOutputHelper logOutput) => _logOutput = logOutput;

    private async Task<IHost> CreateService(bool useDatalake)
    {
        DatalakeOption datalakeOption = TestApplication.ReadDatalakeOption("test-GraphTransactionTests");

        var host = Host.CreateDefaultBuilder()
            .ConfigureLogging(config =>
            {
                //config.AddFilter(x => true);
                config.AddLambda(x => _logOutput.WriteLine(x));
            })
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

        var context = host.Services.GetRequiredService<ILogger<LongRunStressTests>>().ToScopeContext();

        IFileStore fileStore = host.Services.GetRequiredService<IFileStore>();
        var list = await fileStore.Search("**/*", context);
        await list.ForEachAsync(async x => await fileStore.File(x.Path).Delete(context));

        IGraphEngine graphEngine = host.Services.GetRequiredService<IGraphEngine>();
        await graphEngine.DataManager.LoadDatabase(context);

        return host;
    }

    [Theory]
    [InlineData("AddNodes", false)]
    [InlineData("AddNodesInBatch", false)]
    [InlineData("AddEdges", false)]
    [InlineData("AddNodesWithTags", false)]
    [InlineData("AddEdgesBatch", false)]
    [InlineData("AddEdgesWithTagsInBatch", false)]
    [InlineData("AddNodesWithData", false)]

    [InlineData("AddNodes", true)]
    [InlineData("AddNodesInBatch", true)]
    [InlineData("AddEdges", true)]
    [InlineData("AddNodesWithTags", true)]
    [InlineData("AddEdgesBatch", true)]
    [InlineData("AddEdgesWithTagsInBatch", true)]
    [InlineData("AddNodesWithData", true)]
    public async Task ScenarioStressTest(string testName, bool useDataLake)
    {
        using var host = await CreateService(useDataLake);
        var graphClient = host.Services.GetRequiredService<IGraphClient>();
        var graphEngine = host.Services.GetRequiredService<IGraphEngine>();

        var token = new CancellationTokenSource(TimeSpan.FromSeconds(5)).Token;
        var context = host.Services.GetRequiredService<ILogger<LongRunStressTests>>().ToScopeContext();

        var start = DateTime.Now;
        Task<int> scenarioTest = testName switch
        {
            "AddNodes" => AddNodes(graphClient, "n1", token, context),
            "AddNodesInBatch" => AddNodesInBatch(graphClient, "nb1", token, context),
            "AddEdges" => AddEdges(graphClient, "e1", token, context),
            "AddNodesWithTags" => AddNodesWithTags(graphClient, "n2", token, context),
            "AddEdgesBatch" => AddEdgesBatch(graphClient, "e2", token, context),
            "AddEdgesWithTagsInBatch" => AddEdgesWithTagsInBatch(graphClient, "e3", token, context),
            "AddNodesWithData" => AddNodesWithData(graphClient, "n3", token, context),

            _ => throw new ArgumentException($"Unknown test name: {testName}")
        };

        var total = await scenarioTest;
        var tps = total / (DateTime.Now - start).TotalSeconds;
        context.LogInformation("Total nodes and edges added: {total}, tps={tps}", total, tps);

        var map = graphEngine.DataManager.GetMap();
        var mapTotal = map.Nodes.Count + map.Edges.Count;

        context.LogInformation("GraphMap: nodes={nodes}, edges={edges}, total={total}", map.Nodes.Count, map.Edges.Count, mapTotal);
        total.Be(mapTotal);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task AllLongRunningTest(bool useDataLake)
    {
        using var host = await CreateService(useDataLake);
        var graphClient = host.Services.GetRequiredService<IGraphClient>();
        var graphEngine = host.Services.GetRequiredService<IGraphEngine>();

        var token = new CancellationTokenSource(TimeSpan.FromSeconds(5)).Token;
        var context = host.Services.GetRequiredService<ILogger<LongRunStressTests>>().ToScopeContext();

        var start = DateTime.Now;
        Task<int>[] taskList = [
            AddNodes(graphClient, "n1", token, context),
            AddEdges(graphClient, "e1", token, context),
            AddNodesWithTags(graphClient, "n2", token, context),
            AddNodesWithData(graphClient, "n3", token, context),

            AddNodesInBatch(graphClient, "nb1", token, context),
            AddEdgesBatch(graphClient, "e2", token, context),
            AddEdgesWithTagsInBatch(graphClient, "e3", token, context),
            ];

        var counts = await Task.WhenAll(taskList);
        var total = counts.Sum();
        var tps = total / (DateTime.Now - start).TotalSeconds;
        context.LogInformation("Total nodes and edges added: {total}, tps={tps}", total, tps);

        var map = graphEngine.DataManager.GetMap();
        var mapTotal = map.Nodes.Count + map.Edges.Count;

        context.LogInformation("GraphMap: nodes={nodes}, edges={edges}, total={total}", map.Nodes.Count, map.Edges.Count, mapTotal);
        total.Be(mapTotal);
    }


    private async Task RandomDelay() => await Task.Delay(RandomNumberGenerator.GetInt32(10, 50));

    private async Task<int> AddNodes(IGraphClient graphClient, string prefix, CancellationToken token, ScopeContext context)
    {
        int count = 0;
        var start = DateTime.Now;

        while (!token.IsCancellationRequested)
        {
            var nodeKey = $"{prefix}-Node-{count++}";

            var cmd = $"add node key={nodeKey} ;";
            (await graphClient.Execute(cmd, context)).BeOk();

            await RandomDelay();
        }

        var tps = count / (DateTime.Now - start).TotalSeconds;
        context.LogInformation("AddNodes: nodeCount={nodeCount}, tps={tps}", count, tps);
        return count;
    }

    private async Task<int> AddNodesInBatch(IGraphClient graphClient, string prefix, CancellationToken token, ScopeContext context)
    {
        int count = 0;
        var start = DateTime.Now;

        while (!token.IsCancellationRequested)
        {
            var cmd = Enumerable.Range(0, 10)
                .Select(x => $"add node key={prefix}-{count++}-node ;")
                .Join(Environment.NewLine);

            (await graphClient.Execute(cmd, context)).BeOk();

            await RandomDelay();
        }

        var tps = count / (DateTime.Now - start).TotalSeconds;
        context.LogInformation("AddNodesInBatch: nodeCount={nodeCount}, tps={tps}", count, tps);
        return count;
    }

    private async Task<int> AddEdges(IGraphClient graphClient, string prefix, CancellationToken token, ScopeContext context)
    {
        int count = 0;
        var start = DateTime.Now;

        while (!token.IsCancellationRequested)
        {
            count++;
            var nodeKey = $"{prefix}-Node-{count}";
            var nodeKey2 = $"{prefix}-Node2-{count}";

            var cmd = $"add node key={nodeKey} ;";
            (await graphClient.Execute(cmd, context)).BeOk();

            cmd = $"add node key={nodeKey2} ;";
            (await graphClient.Execute(cmd, context)).BeOk();

            cmd = $"add edge from={nodeKey}, to={nodeKey2}, type=default set t1, t2=v2 ;";
            (await graphClient.Execute(cmd, context)).BeOk();

            await RandomDelay();
        }

        var tps = count / (DateTime.Now - start).TotalSeconds;
        context.LogInformation("AddEdges: edgeCount={edgeCount}, nodeCount={nodeCount}, tps={tps}", count, count * 2, tps);
        return count * 3;
    }

    private async Task<int> AddNodesWithTags(IGraphClient graphClient, string prefix, CancellationToken token, ScopeContext context)
    {
        int count = 0;
        var start = DateTime.Now;

        while (!token.IsCancellationRequested)
        {
            var nodeKey = $"{prefix}-Node-{count++}";

            var cmd = $"add node key={nodeKey} set t1, t2=v2;";
            (await graphClient.Execute(cmd, context)).BeOk();

            await RandomDelay();
        }

        var tps = count / (DateTime.Now - start).TotalSeconds;
        context.LogInformation("AddNodesWithTags: nodeCount={nodeCount}, tps={tps}", count, tps);
        return count;
    }


    private async Task<int> AddEdgesBatch(IGraphClient graphClient, string prefix, CancellationToken token, ScopeContext context)
    {
        int count = 0;
        var start = DateTime.Now;

        while (!token.IsCancellationRequested)
        {
            count++;
            var nodeKey = $"{prefix}-Node-{count}";
            var nodeKey2 = $"{prefix}-Node2-{count}";

            string[] cmds = [
                $"add node key={nodeKey} ;",
                $"add node key={nodeKey2} ;",
                $"add edge from={nodeKey}, to={nodeKey2}, type=default ;",
                ];

            var cmd = cmds.Join(Environment.NewLine);
            (await graphClient.Execute(cmd, context)).BeOk();

            await RandomDelay();
        }

        var tps = count / (DateTime.Now - start).TotalSeconds;
        context.LogInformation("AddEdgesBatch: edgeCount={edgeCount}, nodeCount={nodeCount}, tps={tps}", count, count * 2, tps);
        return count * 3;
    }

    private async Task<int> AddEdgesWithTagsInBatch(IGraphClient graphClient, string prefix, CancellationToken token, ScopeContext context)
    {
        int count = 0;
        var start = DateTime.Now;

        while (!token.IsCancellationRequested)
        {
            count++;
            var nodeKey = $"{prefix}-Node-{count}";
            var nodeKey2 = $"{prefix}-Node2-{count}";

            string[] cmds = [
                $"add node key={nodeKey} ;",
                $"add node key={nodeKey2} ;",
                $"add edge from={nodeKey}, to={nodeKey2}, type=default set t2=v2, t10 ;",
                ];

            var cmd = cmds.Join(Environment.NewLine);
            (await graphClient.Execute(cmd, context)).BeOk();

            await RandomDelay();
        }

        var tps = count / (DateTime.Now - start).TotalSeconds;
        context.LogInformation("AddEdgesWithTagsInBatch: edgeCount={edgeCount}, nodeCount={nodeCount}, tps={tps}", count, count * 2, tps);
        return count * 3;
    }

    private async Task<int> AddNodesWithData(IGraphClient graphClient, string prefix, CancellationToken token, ScopeContext context)
    {
        int count = 0;
        var start = DateTime.Now;

        while (!token.IsCancellationRequested)
        {
            var nodeKey = $"{prefix}-Node-{count++}";
            var payload = new Payload(nodeKey, count);
            var payloadBase64 = payload.ToJson().ToBase64();

            var cmd = $"add node key={nodeKey} set data {{ '{payloadBase64}' }} ;";
            (await graphClient.Execute(cmd, context)).BeOk();

            await RandomDelay();
        }

        var tps = count / (DateTime.Now - start).TotalSeconds;
        context.LogInformation("AddNodesWithData: nodeCount={nodeCount}, tps={tps}", count, tps);
        return count;
    }
}
