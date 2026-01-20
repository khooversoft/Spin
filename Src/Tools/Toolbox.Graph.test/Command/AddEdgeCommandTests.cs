using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Graph.test.Application;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Graph.test.Command;

public class AddEdgeCommandTests
{
    private GraphMap _map = null!;
    private readonly ITestOutputHelper _logOutput;

    public AddEdgeCommandTests(ITestOutputHelper logOutput)
    {
        _logOutput = logOutput;
    }

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

    [Fact]
    public async Task MissingEdgeTypeShouldFail()
    {
        using var host = await CreateService();
        var graphClient = host.Services.GetRequiredService<IGraphClient>();
        var graphEngine = host.Services.GetRequiredService<IGraphEngine>();

        var newMapOption = await graphClient.ExecuteBatch("add edge fromKey=node4, toKey=node5;");
        newMapOption.IsError().BeTrue();

        // Verify nothing changed
        graphEngine.DataManager.GetMap().Nodes.Count.Be(7);
        graphEngine.DataManager.GetMap().Edges.Count.Be(5);
    }

    [Fact]
    public async Task SingleAddForEdge()
    {
        using var host = await CreateService();
        var graphClient = host.Services.GetRequiredService<IGraphClient>();
        var graphEngine = host.Services.GetRequiredService<IGraphEngine>();

        var newMapOption = await graphClient.ExecuteBatch("add edge from=node7, to=node1, type=newEdgeType set newTags;");
        newMapOption.IsOk().BeTrue();

        QueryBatchResult commandResults = newMapOption.Return();
        var compareMap = GraphCommandTools.CompareMap(_map, graphEngine.DataManager.GetMap());

        compareMap.Count.Be(1);
        compareMap[0].Cast<GraphEdge>().Action(x =>
        {
            x.FromKey.Be("node7");
            x.ToKey.Be("node1");
            x.EdgeType.Be("newEdgeType");
            x.Tags.ToTagsString().Be("newTags");
        });

        commandResults.Items.Count.Be(1);
        var resultIndex = commandResults.Items.ToCursor();

        resultIndex.NextValue().Return().Action(x =>
        {
            x.Option.IsOk().BeTrue();
            x.Nodes.Count.Be(0);
            x.Edges.Count.Be(0);
            x.DataLinks.Count.Be(0);
        });
    }

    [Fact]
    public async Task SingleAddForEdgeTagsCommand()
    {
        using var host = await CreateService();
        var graphClient = host.Services.GetRequiredService<IGraphClient>();
        var graphEngine = host.Services.GetRequiredService<IGraphEngine>();

        var newMapOption = await graphClient.ExecuteBatch("add edge from=node7, to=node1, type=newEdgeType set -newTags;");
        newMapOption.IsOk().BeTrue();

        QueryBatchResult commandResults = newMapOption.Return();
        var compareMap = GraphCommandTools.CompareMap(_map, graphEngine.DataManager.GetMap());

        compareMap.Count.Be(1);
        compareMap[0].Cast<GraphEdge>().Action(x =>
        {
            x.FromKey.Be("node7");
            x.ToKey.Be("node1");
            x.EdgeType.Be("newEdgeType");
            x.Tags.Count.Be(0);
        });

        commandResults.Items.Count.Be(1);
    }

    [Fact]
    public async Task SingleUniqueAddForEdge()
    {
        using var host = await CreateService();
        var graphClient = host.Services.GetRequiredService<IGraphClient>();
        var graphEngine = host.Services.GetRequiredService<IGraphEngine>();

        var newMapOption = await graphClient.ExecuteBatch("add edge from=node7, to=node1, type=newEdgeType set newTags;");
        newMapOption.IsOk().BeTrue();

        QueryBatchResult commandResults = newMapOption.Return();
        var compareMap = GraphCommandTools.CompareMap(_map, graphEngine.DataManager.GetMap());

        compareMap.Count.Be(1);
        compareMap[0].Cast<GraphEdge>().Action(x =>
        {
            x.FromKey.Be("node7");
            x.ToKey.Be("node1");
            x.EdgeType.Be("newEdgeType");
            x.Tags.ToTagsString().Be("newTags");
        });

        commandResults.Items.Count.Be(1);
    }
}
