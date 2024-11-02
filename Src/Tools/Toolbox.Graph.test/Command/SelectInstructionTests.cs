using FluentAssertions;
using Toolbox.Extensions;
using Toolbox.Types;

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


    [Fact]
    public async Task SelectDirectedNodeToEdge()
    {
        var copyMap = _map.Clone();
        var testClient = GraphTestStartup.CreateGraphTestHost(copyMap);
        var newMapOption = await testClient.Execute("select (*) -> [*] ;", NullScopeContext.Default);
        newMapOption.IsOk().Should().BeTrue(newMapOption.ToString());

        QueryResult result = newMapOption.Return();
        result.Option.IsOk().Should().BeTrue();
        result.Alias.Should().NotBeNullOrWhiteSpace();
        result.Nodes.Count.Should().Be(0);
        result.Edges.Count.Should().Be(6);
        result.DataLinks.Count.Should().Be(0);

        var expected = new List<(string FromKey, string ToKey, string EdgeType)>
        {
            ("node1", "node2", "et1"),
            ("node1", "node3", "et1"),
            ("node6", "node3", "et1"),
            ("node4", "node5", "et1"),
            ("node4", "node3", "et1"),
            ("node5", "node4", "et1"),
        };

        result.Edges.Select(x => (x.FromKey, x.ToKey, x.EdgeType)).Should().BeEquivalentTo(expected);

        copyMap.Meter.Node.GetIndexHit().Should().Be(0);
        copyMap.Meter.Node.GetIndexMissed().Should().Be(0);
        copyMap.Meter.Node.GetIndexScan().Should().Be(1);
        copyMap.Meter.Edge.GetIndexHit().Should().Be(6);
        copyMap.Meter.Edge.GetIndexMissed().Should().Be(0);
        copyMap.Meter.Edge.GetIndexScan().Should().Be(0);
    }

    [Fact]
    public async Task SelectDirectedJoinEdge()
    {
        var copyMap = _map.Clone();
        var testClient = GraphTestStartup.CreateGraphTestHost(copyMap);
        var newMapOption = await testClient.Execute("select [*] -> (*) ;", NullScopeContext.Default);
        newMapOption.IsOk().Should().BeTrue(newMapOption.ToString());

        QueryResult result = newMapOption.Return();
        result.Option.IsOk().Should().BeTrue();
        result.Alias.Should().NotBeNullOrWhiteSpace();
        result.Nodes.Count.Should().Be(4);
        result.Edges.Count.Should().Be(0);
        result.DataLinks.Count.Should().Be(0);

        result.Nodes.Select(x => x.Key).Should().BeEquivalentTo("node2", "node3", "node4", "node5");

        copyMap.Meter.Node.GetIndexHit().Should().Be(6);
        copyMap.Meter.Node.GetIndexMissed().Should().Be(0);
        copyMap.Meter.Node.GetIndexScan().Should().Be(0);
        copyMap.Meter.Edge.GetIndexHit().Should().Be(0);
        copyMap.Meter.Edge.GetIndexMissed().Should().Be(0);
        copyMap.Meter.Edge.GetIndexScan().Should().Be(1);
    }

    [Fact]
    public async Task SelectDirectedNodeToEdgeToNode()
    {
        var copyMap = _map.Clone();
        var testClient = GraphTestStartup.CreateGraphTestHost(copyMap);
        var newMapOption = await testClient.Execute("select (*) -> [*] -> (*) ;", NullScopeContext.Default);
        newMapOption.IsOk().Should().BeTrue(newMapOption.ToString());

        QueryResult result = newMapOption.Return();
        result.Option.IsOk().Should().BeTrue();
        result.Alias.Should().NotBeNullOrWhiteSpace();
        result.Nodes.Count.Should().Be(4);
        result.Edges.Count.Should().Be(0);
        result.DataLinks.Count.Should().Be(0);

        result.Nodes.Select(x => x.Key).Should().BeEquivalentTo("node2", "node3", "node4", "node5");
    }

    [Fact]
    public async Task SelectDirectedNodeToEdgeToNodeWithAlias()
    {
        var copyMap = _map.Clone();
        var testClient = GraphTestStartup.CreateGraphTestHost(copyMap);
        var newMapOption = await testClient.ExecuteBatch("select (*) a1 -> [*] a2 -> (*) a3 ;", NullScopeContext.Default);
        newMapOption.IsOk().Should().BeTrue(newMapOption.ToString());

        QueryBatchResult result = newMapOption.Return();
        result.Option.IsOk().Should().BeTrue();
        result.Items.Count.Should().Be(3);
        result.Items.Select(x => x.Alias).Should().BeEquivalentTo("a1", "a2", "a3");

        result.Items[0].Action(x =>
        {
            x.Nodes.Select(x => x.Key).Should().BeEquivalentTo("node1", "node2", "node3", "node4", "node5", "node6", "node7");
            x.Edges.Count.Should().Be(0);
            x.DataLinks.Count.Should().Be(0);
        });

        result.Items[1].Action(x =>
        {
            x.Nodes.Count.Should().Be(0);
            x.Edges.Count.Should().Be(6);
            x.DataLinks.Count.Should().Be(0);
        });

        result.Items[2].Action(x =>
        {
            x.Nodes.Select(x => x.Key).Should().BeEquivalentTo("node2", "node3", "node4", "node5");
            x.Edges.Count.Should().Be(0);
            x.DataLinks.Count.Should().Be(0);
        });
    }

    [Fact]
    public async Task SelectFullNodeToEdge()
    {
        var copyMap = _map.Clone();
        var testClient = GraphTestStartup.CreateGraphTestHost(copyMap);
        var newMapOption = await testClient.ExecuteBatch("select (key=node4) <-> [*] ;", NullScopeContext.Default);
        newMapOption.IsOk().Should().BeTrue(newMapOption.ToString());

        QueryBatchResult result = newMapOption.Return();
        result.Option.IsOk().Should().BeTrue();
        result.Items.Count.Should().Be(1);

        result.Items[0].Action(x =>
        {
            x.Nodes.Count.Should().Be(0);
            x.Edges.Count.Should().Be(3);
            x.DataLinks.Count.Should().Be(0);

            var expected = new List<(string FromKey, string ToKey)>
            {
                ("node4", "node5"),
                ("node4", "node3"),
                ("node5", "node4"),
            };

            x.Edges.Select(x => (x.FromKey, x.ToKey)).Should().BeEquivalentTo(expected);
        });

        copyMap.Meter.Node.GetIndexHit().Should().Be(1);
        copyMap.Meter.Node.GetIndexMissed().Should().Be(0);
        copyMap.Meter.Node.GetIndexScan().Should().Be(0);
        copyMap.Meter.Edge.GetIndexHit().Should().Be(3);
        copyMap.Meter.Edge.GetIndexMissed().Should().Be(0);
        copyMap.Meter.Edge.GetIndexScan().Should().Be(0);
    }

    [Fact]
    public async Task SelectFullEdgeToNode()
    {
        var copyMap = _map.Clone();
        var testClient = GraphTestStartup.CreateGraphTestHost(copyMap);
        var newMapOption = await testClient.ExecuteBatch("select [knows] <-> (*) ;", NullScopeContext.Default);
        newMapOption.IsOk().Should().BeTrue(newMapOption.ToString());

        QueryBatchResult result = newMapOption.Return();
        result.Option.IsOk().Should().BeTrue();
        result.Items.Count.Should().Be(1);

        result.Items[0].Action(x =>
        {
            x.Nodes.Count.Should().Be(3);
            x.Edges.Count.Should().Be(0);
            x.DataLinks.Count.Should().Be(0);
            x.Nodes.Select(x => x.Key).Should().BeEquivalentTo("node1", "node2", "node3");
        });

        copyMap.Meter.Node.GetIndexHit().Should().Be(4);
        copyMap.Meter.Node.GetIndexMissed().Should().Be(0);
        copyMap.Meter.Node.GetIndexScan().Should().Be(0);
        copyMap.Meter.Edge.GetIndexHit().Should().Be(3);
        copyMap.Meter.Edge.GetIndexMissed().Should().Be(0);
        copyMap.Meter.Edge.GetIndexScan().Should().Be(0);
    }
}
