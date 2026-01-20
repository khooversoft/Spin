using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Graph.test.Application;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Graph.test.Command;

public class NodeInstructionsIndexTests
{
    private static GraphMap CreateGraphMap(IHost host)
    {
        ILogger<GraphMap> logger = host.Services.GetRequiredService<ILogger<GraphMap>>();

        return new GraphMap(logger)
        {
            new GraphNode("node1", tags: "name=marko,age=29"),
            new GraphNode("node2", tags: "name=vadas,age=27"),
            new GraphNode("node3", tags: "name=lop,lang=java"),
            new GraphNode("node4", tags: "name=josh,age=32"),
            new GraphNode("node5", tags: "name=ripple,lang=java"),
            new GraphNode("node6", tags: "name=peter,age=35"),
            new GraphNode("node7", tags: "lang=java"),

            new GraphEdge("node1", "node2", edgeType: "et1", tags: "knows,level=1"),
            new GraphEdge("node1", "node3", edgeType: "et1", tags: "knows,level=1"),
            new GraphEdge("node6", "node3", edgeType: "et1", tags: "created"),
            new GraphEdge("node4", "node5", edgeType: "et1", tags: "created"),
            new GraphEdge("node4", "node3", edgeType : "et1", tags: "created"),
        };
    }

    private readonly ITestOutputHelper _logOutput;
    private GraphMap _map = null!;

    public NodeInstructionsIndexTests(ITestOutputHelper logOutput) => _logOutput = logOutput;

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

        _map = CreateGraphMap(host);

        IGraphEngine graphEngine = host.Services.GetRequiredService<IGraphEngine>();
        await graphEngine.DataManager.SetMap(_map);

        return host;
    }

    [Fact]
    public async Task SetNode()
    {
        using var host = await CreateService();
        var graphClient = host.Services.GetRequiredService<IGraphClient>();
        var graphEngine = host.Services.GetRequiredService<IGraphEngine>();

        var newMapOption = await graphClient.ExecuteBatch("set node key=provider:provider1/provider1-key set uniqueIndex;");
        newMapOption.IsOk().BeTrue();

        graphEngine.DataManager.GetMap().Nodes.LookupTag("uniqueIndex").Action(x =>
        {
            x.Count.Be(1);
            Enumerable.SequenceEqual(x, ["provider:provider1/provider1-key"]);
        });

        QueryBatchResult commandResults = newMapOption.Return();
        var compareMap = GraphCommandTools.CompareMap(_map, graphEngine.DataManager.GetMap());

        compareMap.Count.Be(1);
        compareMap[0].Cast<GraphNode>().Action(x =>
        {
            x.Key.Be("provider:provider1/provider1-key");
            x.Tags.ToTagsString().Be("uniqueIndex");
        });

        commandResults.Items.Count.Be(1);
    }

    [Fact]
    public async Task SetNodeWithIndex()
    {
        using var host = await CreateService();
        var graphClient = host.Services.GetRequiredService<IGraphClient>();
        var graphEngine = host.Services.GetRequiredService<IGraphEngine>();

        var cmd = "set node key=user:username1@company.com set loginProvider=userEmail:username1@domain1.com, email=userEmail:username1@domain1.com index loginProvider ;";
        var newMapOption = await graphClient.ExecuteBatch(cmd);
        newMapOption.IsOk().BeTrue();

        var uniqueIndex = new UniqueIndex("loginProvider", "userEmail", "userEmail:username1@domain1.com");
        graphEngine.DataManager.GetMap().Nodes.LookupByNodeKey("user:username1@company.com").Action(x =>
        {
            x.Count.Be(1);
            Enumerable.SequenceEqual(x, [uniqueIndex]);
        });

        graphEngine.DataManager.GetMap().Nodes.LookupTag("email").Action(x =>
        {
            x.Count.Be(1);
            Enumerable.SequenceEqual(x, ["user:username1@company.com"]);
        });

        graphEngine.DataManager.GetMap().Nodes.LookupIndex("loginProvider", "userEmail:username1@domain1.com").Action(x =>
        {
            x.IsOk().BeTrue();
            x.Return().NodeKey.Be("user:username1@company.com");
        });

        QueryBatchResult commandResults = newMapOption.Return();
        var compareMap = GraphCommandTools.CompareMap(_map, graphEngine.DataManager.GetMap());

        compareMap.Count.Be(1);
        compareMap[0].Cast<GraphNode>().Action(x =>
        {
            x.Key.Be("user:username1@company.com");
            x.Tags.ToTagsString().Be("email=userEmail:username1@domain1.com,loginProvider=userEmail:username1@domain1.com");
            x.Indexes.Any(x => x == "loginProvider").BeTrue();
        });

        commandResults.Items.Count.Be(1);
    }

    [Fact]
    public async Task SetNodeWithTwoIndex()
    {
        using var host = await CreateService();
        var graphClient = host.Services.GetRequiredService<IGraphClient>();
        var graphEngine = host.Services.GetRequiredService<IGraphEngine>();

        var cmd = "set node key=user:username1@company.com set loginProvider=provider:provider1/provider1-key, email=userEmail:username1@domain1.com index loginProvider, email ;";
        var newMapOption = await graphClient.ExecuteBatch(cmd);
        newMapOption.IsOk().BeTrue();

        UniqueIndex[] indexes = [
            new UniqueIndex("email", "userEmail:username1@domain1.com", "user:username1@company.com"),
            new UniqueIndex("loginProvider", "provider:provider1/provider1-key", "user:username1@company.com"),
            ];

        graphEngine.DataManager.GetMap().Nodes.LookupByNodeKey("user:username1@company.com").Action(x =>
        {
            x.Count.Be(indexes.Length);
            var source = x.OrderBy(x => x.PrimaryKey).ToArray();
            var target = indexes.OrderBy(x => x.PrimaryKey).ToArray();

            source.SequenceEqual(target).BeTrue();
        });

        graphEngine.DataManager.GetMap().Nodes.LookupTag("email").Action(x =>
        {
            x.Count.Be(1);
            x.SequenceEqual(["user:username1@company.com"]).BeTrue();
        });

        graphEngine.DataManager.GetMap().Nodes.LookupIndex("loginProvider", "provider:provider1/provider1-key").Action(x =>
        {
            x.IsOk().BeTrue();
            x.Return().NodeKey.Be("user:username1@company.com");
        });

        QueryBatchResult commandResults = newMapOption.Return();
        var compareMap = GraphCommandTools.CompareMap(_map, graphEngine.DataManager.GetMap());

        compareMap.Count.Be(1);
        compareMap[0].Cast<GraphNode>().Action(x =>
        {
            x.Key.Be("user:username1@company.com");
            x.Tags.ToTagsString().Be("email=userEmail:username1@domain1.com,loginProvider=provider:provider1/provider1-key");
            x.Indexes.Any(x => x == "loginProvider").BeTrue();
            x.Indexes.Any(x => x == "email").BeTrue();
        });

        commandResults.Items.Count.Be(1);
    }
}
