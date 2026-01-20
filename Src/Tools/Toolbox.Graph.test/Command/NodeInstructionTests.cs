using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Graph.test.Application;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Graph.test.Command;

public class NodeInstructionTests
{
    private GraphMap _map = null!;

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
    public NodeInstructionTests(ITestOutputHelper logOutput) => _logOutput = logOutput;

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

    [Theory]
    [InlineData("add node NodeKey=node4, toKey=node5;")]
    [InlineData("delete node from=node1, to=node2, type=et1 set newTags, t2=v2")]
    public async Task Failures(string query)
    {
        using var host = await CreateService();
        var graphClient = host.Services.GetRequiredService<IGraphClient>();
        var graphEngine = host.Services.GetRequiredService<IGraphEngine>();

        var newMapOption = await graphClient.Execute(query);
        newMapOption.IsError().BeTrue();

        graphEngine.DataManager.GetMap().Nodes.Count.Be(7);
        graphEngine.DataManager.GetMap().Edges.Count.Be(5);
    }

    [Theory]
    [InlineData("set node key=user:username1@company.com set email=userEmail:username1@domain1.com,loginProvider=userEmail:username1@domain1.com ;")]
    [InlineData("set node key=user:username1@company.com set loginProvider=userEmail:username1@domain1.com, email=userEmail:username1@domain1.com index loginProvider;")]
    [InlineData("set node key=userEmail:username1@domain1.com ;")]
    [InlineData("set edge from=user:username1@company.com, to=userEmail:username1@domain1.com, type=uniqueIndex ;")]
    [InlineData("set node key=logonProvider:loginprovider/loginprovider.key1 ;")]
    [InlineData("set edge from=user:username1@company.com, to=logonProvider:loginprovider/loginprovider.key1, type=uniqueIndex ;")]
    public async Task SyntaxShouldPass(string query)
    {
        using var host = await CreateService();
        var parser = host.Services.GetRequiredService<GraphLanguageParser>();
        var parse = parser.Parse(query);
        parse.Status.IsOk().BeTrue();
    }

    [Fact]
    public async Task AddNode()
    {
        using var host = await CreateService();
        var graphClient = host.Services.GetRequiredService<IGraphClient>();
        var graphEngine = host.Services.GetRequiredService<IGraphEngine>();

        var newMapOption = await graphClient.Execute("add node key=node8;");
        newMapOption.IsOk().BeTrue();

        var compareMap = GraphCommandTools.CompareMap(_map, graphEngine.DataManager.GetMap());

        compareMap.Count.Be(1);
        compareMap[0].Cast<GraphNode>().Action(x =>
        {
            x.Key.Be("node8");
            x.Tags.Count.Be(0);
        });
    }

    [Fact]
    public async Task AddNodeWithTag()
    {
        using var host = await CreateService();
        var graphClient = host.Services.GetRequiredService<IGraphClient>();
        var graphEngine = host.Services.GetRequiredService<IGraphEngine>();

        var newMapOption = await graphClient.ExecuteBatch("add node key=node9 set newTags;");
        newMapOption.IsOk().BeTrue();

        QueryBatchResult commandResults = newMapOption.Return();
        commandResults.Items.Count.Be(1);

        var compareMap = GraphCommandTools.CompareMap(_map, graphEngine.DataManager.GetMap());

        compareMap.Count.Be(1);
        compareMap[0].Cast<GraphNode>().Action(x =>
        {
            x.Key.Be("node9");
            x.Tags.ToTagsString().Be("newTags");
        });
    }

    [Fact]
    public async Task AddNodeWithRemoveTagsThatDoesNotExist()
    {
        using var host = await CreateService();
        var graphClient = host.Services.GetRequiredService<IGraphClient>();
        var graphEngine = host.Services.GetRequiredService<IGraphEngine>();

        var newMapOption = await graphClient.ExecuteBatch("add node key=node10 set -newTags;");
        newMapOption.IsOk().BeTrue();

        QueryBatchResult commandResults = newMapOption.Return();
        commandResults.Items.Count.Be(1);

        var compareMap = GraphCommandTools.CompareMap(_map, graphEngine.DataManager.GetMap());

        compareMap.Count.Be(1);
        compareMap[0].Cast<GraphNode>().Action(x =>
        {
            x.Key.Be("node10");
            x.Tags.Count.Be(0);
        });
    }

    [Fact]
    public async Task AddNodeWithTwoTags()
    {
        using var host = await CreateService();
        var graphClient = host.Services.GetRequiredService<IGraphClient>();
        var graphEngine = host.Services.GetRequiredService<IGraphEngine>();

        var newMapOption = await graphClient.ExecuteBatch("add node key=node8 set newTags, t2=v2;");
        newMapOption.IsOk().BeTrue();

        QueryBatchResult commandResults = newMapOption.Return();
        var compareMap = GraphCommandTools.CompareMap(_map, graphEngine.DataManager.GetMap());

        compareMap.Count.Be(1);
        compareMap[0].Cast<GraphNode>().Action(x =>
        {
            x.Key.Be("node8");
            x.Tags.ToTagsString().Be("newTags,t2=v2");
        });

        commandResults.Items.Count.Be(1);
    }

    [Fact]
    public async Task DeleteNode()
    {
        using var host = await CreateService();
        var graphClient = host.Services.GetRequiredService<IGraphClient>();
        var graphEngine = host.Services.GetRequiredService<IGraphEngine>();

        var newMapOption = await graphClient.ExecuteBatch("delete node key=node1 ;");
        newMapOption.IsOk().BeTrue();

        QueryBatchResult commandResults = newMapOption.Return();
        var compareMap = GraphCommandTools.CompareMap(_map, graphEngine.DataManager.GetMap());

        compareMap.Count.Be(3);
        compareMap.OfType<GraphNode>().First().Action(x =>
        {
            x.Key.Be("node1");
            x.Tags.ToTagsString().Be("age=29,name=marko");
        });
        compareMap.OfType<GraphEdge>().OrderBy(x => x.ToKey).ToArray().Action(x =>
        {
            x.Length.Be(2);
            x[0].Action(y =>
            {
                y.FromKey.Be("node1");
                y.ToKey.Be("node2");
                y.Tags.ToTagsString().Be("knows,level=1");
            });
            x[1].Action(y =>
            {
                y.FromKey.Be("node1");
                y.ToKey.Be("node3");
                y.Tags.ToTagsString().Be("knows,level=1");
            });
        });

        commandResults.Items.Count.Be(1);
    }

    [Fact]
    public async Task DeleteNodeIfExist()
    {
        using var host = await CreateService();
        var graphClient = host.Services.GetRequiredService<IGraphClient>();
        var graphEngine = host.Services.GetRequiredService<IGraphEngine>();

        // Verify delete will fail
        var newMapOption = await graphClient.ExecuteBatch("delete node key=node11 ;");
        newMapOption.IsError().BeTrue();

        // Delet should not fail because of 'ifexist'
        newMapOption = await graphClient.ExecuteBatch("delete node ifexist key=node111 ;");
        newMapOption.IsOk().BeTrue();

        QueryBatchResult commandResults = newMapOption.Return();
        commandResults.Items.Count.Be(1);

        var compareMap = GraphCommandTools.CompareMap(_map, graphEngine.DataManager.GetMap());
        compareMap.Count.Be(0);
    }

    [Fact]
    public async Task SetNode()
    {
        using var host = await CreateService();
        var graphClient = host.Services.GetRequiredService<IGraphClient>();
        var graphEngine = host.Services.GetRequiredService<IGraphEngine>();

        var newMapOption = await graphClient.ExecuteBatch("set node key=node8 ;");
        newMapOption.IsOk().BeTrue();

        QueryBatchResult commandResults = newMapOption.Return();
        var compareMap = GraphCommandTools.CompareMap(_map, graphEngine.DataManager.GetMap());

        compareMap.Count.Be(1);
        compareMap[0].Cast<GraphNode>().Action(x =>
        {
            x.Key.Be("node8");
            x.Tags.Count.Be(0);
        });

        commandResults.Items.Count.Be(1);
    }

    [Fact]
    public async Task SetNodeWithNamespace()
    {
        using var host = await CreateService();
        var graphClient = host.Services.GetRequiredService<IGraphClient>();
        var graphEngine = host.Services.GetRequiredService<IGraphEngine>();

        var newMapOption = await graphClient.ExecuteBatch("set node key=role:owner@domain.com;");
        newMapOption.IsOk().BeTrue();

        QueryBatchResult commandResults = newMapOption.Return();
        var compareMap = GraphCommandTools.CompareMap(_map, graphEngine.DataManager.GetMap());

        compareMap.Count.Be(1);
        compareMap[0].Cast<GraphNode>().Action(x =>
        {
            x.Key.Be("role:owner@domain.com");
            x.Tags.Count.Be(0);
        });

        commandResults.Items.Count.Be(1);
    }

    [Fact]
    public async Task UpdateNodeTags()
    {
        using var host = await CreateService();
        var graphClient = host.Services.GetRequiredService<IGraphClient>();
        var graphEngine = host.Services.GetRequiredService<IGraphEngine>();

        var newMapOption = await graphClient.ExecuteBatch("set node key=node11 set t1, t2=v2 ;");
        newMapOption.IsOk().BeTrue();

        QueryBatchResult commandResults = newMapOption.Return();
        var compareMap = GraphCommandTools.CompareMap(_map, graphEngine.DataManager.GetMap());

        compareMap.Count.Be(1);
        compareMap[0].Cast<GraphNode>().Action(x =>
        {
            x.Key.Be("node11");
            x.Tags.ToTagsString().Be("t1,t2=v2");
        });

        commandResults.Items.Count.Be(1);
    }

    [Fact]
    public async Task SetNodeWithTags()
    {
        using var host = await CreateService();
        var graphClient = host.Services.GetRequiredService<IGraphClient>();
        var graphEngine = host.Services.GetRequiredService<IGraphEngine>();

        var newMapOption = await graphClient.ExecuteBatch("set node key=node4 set t1, t2=v2 ;");
        newMapOption.IsOk().BeTrue();

        QueryBatchResult commandResults = newMapOption.Return();
        var compareMap = GraphCommandTools.CompareMap(_map, graphEngine.DataManager.GetMap());

        compareMap.Count.Be(1);
        compareMap[0].Cast<GraphNode>().Action(x =>
        {
            x.Key.Be("node4");
            x.Tags.ToTagsString().Be("age=32,name=josh,t1,t2=v2");
        });

        commandResults.Items.Count.Be(1);
    }

    [Fact]
    public async Task SetNodeWithRemoveTag()
    {
        using var host = await CreateService();
        var graphClient = host.Services.GetRequiredService<IGraphClient>();
        var graphEngine = host.Services.GetRequiredService<IGraphEngine>();

        var newMapOption = await graphClient.ExecuteBatch("set node key=node4 set -age ;");
        newMapOption.IsOk().BeTrue();

        QueryBatchResult commandResults = newMapOption.Return();
        var compareMap = GraphCommandTools.CompareMap(_map, graphEngine.DataManager.GetMap());

        compareMap.Count.Be(1);
        compareMap[0].Cast<GraphNode>().Action(x =>
        {
            x.Key.Be("node4");
            x.Tags.ToTagsString().Be("name=josh");
        });

        commandResults.Items.Count.Be(1);
    }
}
