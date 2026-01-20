using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Graph.test.Command;

public class SelectNodeInstructionTests
{
    private static GraphMap CreateGraphMap(IHost host)
    {
        ILogger<GraphMap> logger = host.Services.GetRequiredService<ILogger<GraphMap>>();

        return new GraphMap(logger)
        {
            new GraphNode("node1", tags: "name=marko,age=29"),
            new GraphNode("node2", tags: "name=vadas,age=27"),
            new GraphNode("node3", tags: "name=lop,lang=java;"),
            new GraphNode("node4", tags: "name=josh,age=32,user"),
            new GraphNode("node5", tags: "name=ripple,lang=java"),
            new GraphNode("node6", tags: "name=peter,age=35"),
            new GraphNode("node7", tags: "lang=java"),

            new GraphEdge("node1", "node2", edgeType: "et1", tags: "knows,level=1"),
            new GraphEdge("node1", "node3", edgeType: "et1", tags: "knows,level=1"),
            new GraphEdge("node6", "node3", edgeType: "et1", tags: "created"),
            new GraphEdge("node4", "node5", edgeType: "et1", tags: "created"),
            new GraphEdge("node4", "node3", edgeType : "et1", tags: "created"),
            new GraphEdge("node5", "node4", edgeType : "et1", tags: "created"),
        };
    }

    private readonly ITestOutputHelper _logOutput;
    public SelectNodeInstructionTests(ITestOutputHelper logOutput) => _logOutput = logOutput;

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

        var map = CreateGraphMap(host);

        IGraphEngine graphEngine = host.Services.GetRequiredService<IGraphEngine>();
        await graphEngine.DataManager.SetMap(map);

        return host;
    }

    [Fact]
    public async Task SelectNode()
    {
        using var host = await CreateService();
        var graphClient = host.Services.GetRequiredService<IGraphClient>();

        var newMapOption = await graphClient.Execute("select (key=node2) a1 ;");
        newMapOption.IsOk().BeTrue(newMapOption.ToString());

        QueryResult result = newMapOption.Return();
        result.Option.IsOk().BeTrue();
        result.Alias.Be("a1");
        result.Nodes.Count.Be(1);
        result.Edges.Count.Be(0);
        result.DataLinks.Count.Be(0);

        result.Nodes[0].Key.Be("node2");
        result.Nodes[0].Tags.ToTagsString().Be("age=27,name=vadas");
    }

    [Fact]
    public async Task SelectNodeWithTag()
    {
        using var host = await CreateService();
        var graphClient = host.Services.GetRequiredService<IGraphClient>();

        var newMapOption = await graphClient.Execute("select (user) ;");
        newMapOption.IsOk().BeTrue(newMapOption.ToString());

        QueryResult result = newMapOption.Return();
        result.Option.IsOk().BeTrue();
        result.Alias.NotEmpty();
        result.Nodes.Count.Be(1);
        result.Edges.Count.Be(0);
        result.DataLinks.Count.Be(0);

        result.Nodes[0].Key.Be("node4");
        result.Nodes[0].Tags.ToTagsString().Be("age=32,name=josh,user");
    }

    [Fact]
    public async Task SelectAllNodes()
    {
        using var host = await CreateService();
        var graphClient = host.Services.GetRequiredService<IGraphClient>();

        var newMapOption = await graphClient.Execute("select (*) ;");
        newMapOption.IsOk().BeTrue(newMapOption.ToString());

        QueryResult result = newMapOption.Return();
        result.Option.IsOk().BeTrue();
        result.Alias.NotEmpty();
        result.Nodes.Count.Be(7);
        result.Edges.Count.Be(0);
        result.DataLinks.Count.Be(0);

        result.Nodes.Select(x => x.Key).OrderBy(x => x).SequenceEqual(["node1", "node2", "node3", "node4", "node5", "node6", "node7"]).BeTrue();
    }

    [Fact]
    public async Task SelectAllNodesWithAgeTag()
    {
        using var host = await CreateService();
        var graphClient = host.Services.GetRequiredService<IGraphClient>();

        var newMapOption = await graphClient.Execute("select (age) ;");
        newMapOption.IsOk().BeTrue(newMapOption.ToString());

        QueryResult result = newMapOption.Return();
        result.Option.IsOk().BeTrue();
        result.Alias.NotEmpty();
        result.Nodes.Count.Be(4);
        result.Edges.Count.Be(0);
        result.DataLinks.Count.Be(0);

        result.Nodes.Select(x => x.Key).OrderBy(x => x).SequenceEqual(["node1", "node2", "node4", "node6"]).BeTrue();
    }
}
