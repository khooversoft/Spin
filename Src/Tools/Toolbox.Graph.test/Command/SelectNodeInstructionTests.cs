using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Toolbox.Graph.test.Application;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Graph.test.Command;

public class SelectNodeInstructionTests
{
    private readonly GraphMap _map = new GraphMap()
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

    private readonly ITestOutputHelper _logOutput;
    public SelectNodeInstructionTests(ITestOutputHelper logOutput) => _logOutput = logOutput;

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
        var context = host.Services.GetRequiredService<ILogger<NodeInstructionsIndexTests>>().ToScopeContext();
        await graphEngine.DataManager.SetMap(_map, context);

        return host;
    }

    [Fact]
    public async Task SelectNode()
    {
        using var host = await CreateService();
        var context = host.Services.GetRequiredService<ILogger<AddEdgeCommandTests>>().ToScopeContext();
        var graphClient = host.Services.GetRequiredService<IGraphClient>();
        var collector = host.Services.GetRequiredService<GraphMapCounter>();

        var newMapOption = await graphClient.Execute("select (key=node2) a1 ;", context);
        newMapOption.IsOk().BeTrue(newMapOption.ToString());

        QueryResult result = newMapOption.Return();
        result.Option.IsOk().BeTrue();
        result.Alias.Be("a1");
        result.Nodes.Count.Be(1);
        result.Edges.Count.Be(0);
        result.DataLinks.Count.Be(0);

        result.Nodes[0].Key.Be("node2");
        result.Nodes[0].Tags.ToTagsString().Be("age=27,name=vadas");

        collector.Nodes.IndexHit.Value.Be(1);
        collector.Nodes.IndexMissed.Value.Be(0);
        collector.Nodes.IndexScan.Value.Be(0);
    }

    [Fact]
    public async Task SelectNodeWithTag()
    {
        using var host = await CreateService();
        var context = host.Services.GetRequiredService<ILogger<AddEdgeCommandTests>>().ToScopeContext();
        var graphClient = host.Services.GetRequiredService<IGraphClient>();
        var collector = host.Services.GetRequiredService<GraphMapCounter>();

        var newMapOption = await graphClient.Execute("select (user) ;", context);
        newMapOption.IsOk().BeTrue(newMapOption.ToString());

        QueryResult result = newMapOption.Return();
        result.Option.IsOk().BeTrue();
        result.Alias.NotEmpty();
        result.Nodes.Count.Be(1);
        result.Edges.Count.Be(0);
        result.DataLinks.Count.Be(0);

        result.Nodes[0].Key.Be("node4");
        result.Nodes[0].Tags.ToTagsString().Be("age=32,name=josh,user");

        collector.Nodes.IndexHit.Value.Be(1);
        collector.Nodes.IndexMissed.Value.Be(0);
        collector.Nodes.IndexScan.Value.Be(0);
    }

    [Fact]
    public async Task SelectAllNodes()
    {
        using var host = await CreateService();
        var context = host.Services.GetRequiredService<ILogger<AddEdgeCommandTests>>().ToScopeContext();
        var graphClient = host.Services.GetRequiredService<IGraphClient>();
        var collector = host.Services.GetRequiredService<GraphMapCounter>();

        var newMapOption = await graphClient.Execute("select (*) ;", context);
        newMapOption.IsOk().BeTrue(newMapOption.ToString());

        QueryResult result = newMapOption.Return();
        result.Option.IsOk().BeTrue();
        result.Alias.NotEmpty();
        result.Nodes.Count.Be(7);
        result.Edges.Count.Be(0);
        result.DataLinks.Count.Be(0);

        result.Nodes.Select(x => x.Key).OrderBy(x => x).SequenceEqual(["node1", "node2", "node3", "node4", "node5", "node6", "node7"]).BeTrue();

        collector.Nodes.IndexHit.Value.Be(0);
        collector.Nodes.IndexMissed.Value.Be(0);
        collector.Nodes.IndexScan.Value.Be(1);
    }

    [Fact]
    public async Task SelectAllNodesWithAgeTag()
    {
        using var host = await CreateService();
        var context = host.Services.GetRequiredService<ILogger<AddEdgeCommandTests>>().ToScopeContext();
        var graphClient = host.Services.GetRequiredService<IGraphClient>();
        var collector = host.Services.GetRequiredService<GraphMapCounter>();

        var newMapOption = await graphClient.Execute("select (age) ;", context);
        newMapOption.IsOk().BeTrue(newMapOption.ToString());

        QueryResult result = newMapOption.Return();
        result.Option.IsOk().BeTrue();
        result.Alias.NotEmpty();
        result.Nodes.Count.Be(4);
        result.Edges.Count.Be(0);
        result.DataLinks.Count.Be(0);

        result.Nodes.Select(x => x.Key).OrderBy(x => x).SequenceEqual(["node1", "node2", "node4", "node6"]).BeTrue();

        collector.Nodes.IndexHit.Value.Be(1);
        collector.Nodes.IndexMissed.Value.Be(0);
        collector.Nodes.IndexScan.Value.Be(0);
    }
}
