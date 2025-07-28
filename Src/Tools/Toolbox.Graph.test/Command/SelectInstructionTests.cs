using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Graph.test.Command;

public class SelectInstructionTests
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
    public SelectInstructionTests(ITestOutputHelper logOutput) => _logOutput = logOutput;

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
    public async Task SelectDirectedNodeToEdge()
    {
        using var host = await CreateService();
        var context = host.Services.GetRequiredService<ILogger<AddEdgeCommandTests>>().ToScopeContext();
        var graphClient = host.Services.GetRequiredService<IGraphClient>();
        var collector = host.Services.GetRequiredService<GraphMapCounter>();

        var newMapOption = await graphClient.Execute("select (*) -> [*] ;", context);
        newMapOption.IsOk().BeTrue();

        QueryResult result = newMapOption.Return();
        result.Option.IsOk().BeTrue();
        result.Alias.NotEmpty();
        result.Nodes.Count.Be(0);
        result.Edges.Count.Be(6);
        result.DataLinks.Count.Be(0);

        var expected = new (string FromKey, string ToKey, string EdgeType)[]
        {
            ("node1", "node2", "et1"),
            ("node1", "node3", "et1"),
            ("node6", "node3", "et1"),
            ("node4", "node5", "et1"),
            ("node4", "node3", "et1"),
            ("node5", "node4", "et1"),
        }.OrderBy(x => x).ToArray();

        result.Edges.Select(x => (x.FromKey, x.ToKey, x.EdgeType)).OrderBy(x => x).SequenceEqual(expected).BeTrue();

        collector.Nodes.IndexHit.Value.Be(0);
        collector.Nodes.IndexMissed.Value.Be(0);
        collector.Nodes.IndexScan.Value.Be(1);
        collector.Edges.IndexHit.Value.Be(7);
        collector.Edges.IndexMissed.Value.Be(0);
        collector.Edges.IndexScan.Value.Be(0);
    }

    [Fact]
    public async Task SelectReverseNodeToEdge()
    {
        using var host = await CreateService();
        var context = host.Services.GetRequiredService<ILogger<AddEdgeCommandTests>>().ToScopeContext();
        var graphClient = host.Services.GetRequiredService<IGraphClient>();
        var collector = host.Services.GetRequiredService<GraphMapCounter>();

        var newMapOption = await graphClient.Execute("select (*) <- [*] ;", context);
        newMapOption.IsOk().BeTrue();

        QueryResult result = newMapOption.Return();
        result.Option.IsOk().BeTrue();
        result.Alias.NotEmpty();
        result.Nodes.Count.Be(0);
        result.Edges.Count.Be(6);
        result.DataLinks.Count.Be(0);

        var expected = new (string FromKey, string ToKey, string EdgeType)[]
        {
            ("node1", "node2", "et1"),
            ("node1", "node3", "et1"),
            ("node6", "node3", "et1"),
            ("node4", "node5", "et1"),
            ("node4", "node3", "et1"),
            ("node5", "node4", "et1"),
        }.OrderBy(x => x).ToArray();

        result.Edges.Select(x => (x.FromKey, x.ToKey, x.EdgeType)).OrderBy(x => x).SequenceEqual(expected).BeTrue();

        collector.Nodes.IndexHit.Value.Be(0);
        collector.Nodes.IndexMissed.Value.Be(0);
        collector.Nodes.IndexScan.Value.Be(1);
        collector.Edges.IndexHit.Value.Be(7);
        collector.Edges.IndexMissed.Value.Be(0);
        collector.Edges.IndexScan.Value.Be(0);
    }

    [Fact]
    public async Task SelectDirectedJoinEdge()
    {
        using var host = await CreateService();
        var context = host.Services.GetRequiredService<ILogger<AddEdgeCommandTests>>().ToScopeContext();
        var graphClient = host.Services.GetRequiredService<IGraphClient>();
        var collector = host.Services.GetRequiredService<GraphMapCounter>();

        var newMapOption = await graphClient.Execute("select [*] -> (*) ;", context);
        newMapOption.IsOk().BeTrue();

        QueryResult result = newMapOption.Return();
        result.Option.IsOk().BeTrue();
        result.Alias.NotEmpty();
        result.Nodes.Count.Be(4);
        result.Edges.Count.Be(0);
        result.DataLinks.Count.Be(0);

        result.Nodes.Select(x => x.Key).OrderBy(x => x).SequenceEqual(["node2", "node3", "node4", "node5"]).BeTrue();

        collector.Nodes.IndexHit.Value.Be(6);
        collector.Nodes.IndexMissed.Value.Be(0);
        collector.Nodes.IndexScan.Value.Be(0);
        collector.Edges.IndexHit.Value.Be(0);
        collector.Edges.IndexMissed.Value.Be(0);
        collector.Edges.IndexScan.Value.Be(1);
    }

    [Fact]
    public async Task SelectDirectedNodeToEdgeToNode()
    {
        using var host = await CreateService();
        var context = host.Services.GetRequiredService<ILogger<AddEdgeCommandTests>>().ToScopeContext();
        var graphClient = host.Services.GetRequiredService<IGraphClient>();
        var collector = host.Services.GetRequiredService<GraphMapCounter>();

        var newMapOption = await graphClient.Execute("select (*) -> [*] -> (*) ;", context);
        newMapOption.IsOk().BeTrue();

        QueryResult result = newMapOption.Return();
        result.Option.IsOk().BeTrue();
        result.Alias.NotEmpty();
        result.Nodes.Count.Be(4);
        result.Edges.Count.Be(0);
        result.DataLinks.Count.Be(0);

        result.Nodes.Select(x => x.Key).OrderBy(x => x).SequenceEqual(["node2", "node3", "node4", "node5"]).BeTrue();
    }

    [Fact]
    public async Task SelectDirectedNodeToEdgeToNodeWithAlias()
    {
        using var host = await CreateService();
        var context = host.Services.GetRequiredService<ILogger<AddEdgeCommandTests>>().ToScopeContext();
        var graphClient = host.Services.GetRequiredService<IGraphClient>();
        var collector = host.Services.GetRequiredService<GraphMapCounter>();

        var newMapOption = await graphClient.ExecuteBatch("select (*) a1 -> [*] a2 -> (*) a3 ;", context);
        newMapOption.IsOk().BeTrue();

        QueryBatchResult result = newMapOption.Return();
        result.Option.IsOk().BeTrue();
        result.Items.Count.Be(3);
        result.Items.Select(x => x.Alias).SequenceEqual(["a1", "a2", "a3"]).BeTrue();

        result.Items[0].Action(x =>
        {
            x.Nodes.Select(x => x.Key).OrderBy(x => x).SequenceEqual(["node1", "node2", "node3", "node4", "node5", "node6", "node7"]).BeTrue();
            x.Edges.Count.Be(0);
            x.DataLinks.Count.Be(0);
        });

        result.Items[1].Action(x =>
        {
            x.Nodes.Count.Be(0);
            x.Edges.Count.Be(6);
            x.DataLinks.Count.Be(0);
        });

        result.Items[2].Action(x =>
        {
            x.Nodes.Select(x => x.Key).OrderBy(x => x).SequenceEqual(["node2", "node3", "node4", "node5"]).BeTrue();
            x.Edges.Count.Be(0);
            x.DataLinks.Count.Be(0);
        });
    }

    [Fact]
    public async Task SelectRightNodeToEdge()
    {
        using var host = await CreateService();
        var context = host.Services.GetRequiredService<ILogger<AddEdgeCommandTests>>().ToScopeContext();
        var graphClient = host.Services.GetRequiredService<IGraphClient>();
        var collector = host.Services.GetRequiredService<GraphMapCounter>();

        var newMapOption = await graphClient.ExecuteBatch("select (key=node4) <- [*] ;", context);
        newMapOption.IsOk().BeTrue();

        QueryBatchResult result = newMapOption.Return();
        result.Option.IsOk().BeTrue();
        result.Items.Count.Be(1);

        result.Items[0].Action(x =>
        {
            x.Nodes.Count.Be(0);
            x.Edges.Count.Be(1);
            x.DataLinks.Count.Be(0);

            var expected = new List<(string FromKey, string ToKey)>
            {
                ("node5", "node4"),
            };

            x.Edges.Select(x => (x.FromKey, x.ToKey)).SequenceEqual(expected).BeTrue();
        });

        collector.Nodes.IndexHit.Value.Be(1);
        collector.Nodes.IndexMissed.Value.Be(0);
        collector.Nodes.IndexScan.Value.Be(0);
        collector.Edges.IndexHit.Value.Be(2);
        collector.Edges.IndexMissed.Value.Be(0);
        collector.Edges.IndexScan.Value.Be(0);
    }

    [Fact]
    public async Task SelectFullNodeToEdge()
    {
        using var host = await CreateService();
        var context = host.Services.GetRequiredService<ILogger<AddEdgeCommandTests>>().ToScopeContext();
        var graphClient = host.Services.GetRequiredService<IGraphClient>();
        var collector = host.Services.GetRequiredService<GraphMapCounter>();

        var newMapOption = await graphClient.ExecuteBatch("select (key=node4) <-> [*] ;", context);
        newMapOption.IsOk().BeTrue();

        QueryBatchResult result = newMapOption.Return();
        result.Option.IsOk().BeTrue();
        result.Items.Count.Be(1);

        result.Items[0].Action(x =>
        {
            x.Nodes.Count.Be(0);
            x.Edges.Count.Be(3);
            x.DataLinks.Count.Be(0);

            var expected = new (string FromKey, string ToKey)[]
            {
                ("node4", "node5"),
                ("node4", "node3"),
                ("node5", "node4"),
            }.OrderBy(x => x).ToArray();

            x.Edges.Select(x => (x.FromKey, x.ToKey)).OrderBy(x => x).SequenceEqual(expected).BeTrue();
        });

        collector.Nodes.IndexHit.Value.Be(1);
        collector.Nodes.IndexMissed.Value.Be(0);
        collector.Nodes.IndexScan.Value.Be(0);
        collector.Edges.IndexHit.Value.Be(4);
        collector.Edges.IndexMissed.Value.Be(0);
        collector.Edges.IndexScan.Value.Be(0);
    }

    [Fact]
    public async Task SelectRightJoinEdgeToNode()
    {
        using var host = await CreateService();
        var context = host.Services.GetRequiredService<ILogger<AddEdgeCommandTests>>().ToScopeContext();
        var graphClient = host.Services.GetRequiredService<IGraphClient>();
        var collector = host.Services.GetRequiredService<GraphMapCounter>();

        var newMapOption = await graphClient.ExecuteBatch("select [knows] <- (*) ;", context);
        newMapOption.IsOk().BeTrue();

        QueryBatchResult result = newMapOption.Return();
        result.Option.IsOk().BeTrue();
        result.Items.Count.Be(1);

        result.Items[0].Action(x =>
        {
            x.Nodes.Count.Be(1);
            x.Edges.Count.Be(0);
            x.DataLinks.Count.Be(0);
            x.Nodes.Select(x => x.Key).SequenceEqual(["node1"]).BeTrue();
        });

        collector.Nodes.IndexHit.Value.Be(2);
        collector.Nodes.IndexMissed.Value.Be(0);
        collector.Nodes.IndexScan.Value.Be(0);
        collector.Edges.IndexHit.Value.Be(3);
        collector.Edges.IndexMissed.Value.Be(0);
        collector.Edges.IndexScan.Value.Be(0);
    }

    [Fact]
    public async Task SelectFullEdgeToNode()
    {
        using var host = await CreateService();
        var context = host.Services.GetRequiredService<ILogger<AddEdgeCommandTests>>().ToScopeContext();
        var graphClient = host.Services.GetRequiredService<IGraphClient>();
        var collector = host.Services.GetRequiredService<GraphMapCounter>();

        var newMapOption = await graphClient.ExecuteBatch("select [knows] <-> (*) ;", context);
        newMapOption.IsOk().BeTrue();

        QueryBatchResult result = newMapOption.Return();
        result.Option.IsOk().BeTrue();
        result.Items.Count.Be(1);

        result.Items[0].Action(x =>
        {
            x.Nodes.Count.Be(3);
            x.Edges.Count.Be(0);
            x.DataLinks.Count.Be(0);
            x.Nodes.Select(x => x.Key).OrderBy(x => x).SequenceEqual(["node1", "node2", "node3"]).BeTrue();
        });

        collector.Nodes.IndexHit.Value.Be(4);
        collector.Nodes.IndexMissed.Value.Be(0);
        collector.Nodes.IndexScan.Value.Be(0);
        collector.Edges.IndexHit.Value.Be(3);
        collector.Edges.IndexMissed.Value.Be(0);
        collector.Edges.IndexScan.Value.Be(0);
    }
}
