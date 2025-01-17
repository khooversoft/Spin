using Toolbox.Tools.Should;
using Toolbox.Types;

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

    [Fact]
    public async Task SelectNode()
    {
        var copyMap = _map.Clone();
        var testClient = GraphTestStartup.CreateGraphTestHost(copyMap);
        var newMapOption = await testClient.Execute("select (key=node2) a1 ;", NullScopeContext.Default);
        newMapOption.IsOk().Should().BeTrue(newMapOption.ToString());

        QueryResult result = newMapOption.Return();
        result.Option.IsOk().Should().BeTrue();
        result.Alias.Should().Be("a1");
        result.Nodes.Count.Should().Be(1);
        result.Edges.Count.Should().Be(0);
        result.DataLinks.Count.Should().Be(0);

        result.Nodes[0].Key.Should().Be("node2");
        result.Nodes[0].Tags.ToTagsString().Should().Be("age=27,name=vadas");

        copyMap.Meter.Node.GetIndexHit().Should().Be(1);
        copyMap.Meter.Node.GetIndexMissed().Should().Be(0);
        copyMap.Meter.Node.GetIndexScan().Should().Be(0);
    }

    [Fact]
    public async Task SelectNodeWithTag()
    {
        var copyMap = _map.Clone();
        var testClient = GraphTestStartup.CreateGraphTestHost(copyMap);
        var newMapOption = await testClient.Execute("select (user) ;", NullScopeContext.Default);
        newMapOption.IsOk().Should().BeTrue(newMapOption.ToString());

        QueryResult result = newMapOption.Return();
        result.Option.IsOk().Should().BeTrue();
        result.Alias.Should().NotBeEmpty();
        result.Nodes.Count.Should().Be(1);
        result.Edges.Count.Should().Be(0);
        result.DataLinks.Count.Should().Be(0);

        result.Nodes[0].Key.Should().Be("node4");
        result.Nodes[0].Tags.ToTagsString().Should().Be("age=32,name=josh,user");

        copyMap.Meter.Node.GetIndexHit().Should().Be(1);
        copyMap.Meter.Node.GetIndexMissed().Should().Be(0);
        copyMap.Meter.Node.GetIndexScan().Should().Be(0);
    }

    [Fact]
    public async Task SelectAllNodes()
    {
        var copyMap = _map.Clone();
        var testClient = GraphTestStartup.CreateGraphTestHost(copyMap);
        var newMapOption = await testClient.Execute("select (*) ;", NullScopeContext.Default);
        newMapOption.IsOk().Should().BeTrue(newMapOption.ToString());

        QueryResult result = newMapOption.Return();
        result.Option.IsOk().Should().BeTrue();
        result.Alias.Should().NotBeEmpty();
        result.Nodes.Count.Should().Be(7);
        result.Edges.Count.Should().Be(0);
        result.DataLinks.Count.Should().Be(0);

        result.Nodes.Select(x => x.Key).OrderBy(x => x).SequenceEqual(["node1", "node2", "node3", "node4", "node5", "node6", "node7"]).Should().BeTrue();

        copyMap.Meter.Node.GetIndexHit().Should().Be(0);
        copyMap.Meter.Node.GetIndexMissed().Should().Be(0);
        copyMap.Meter.Node.GetIndexScan().Should().Be(1);
    }

    [Fact]
    public async Task SelectAllNodesWithAgeTag()
    {
        var copyMap = _map.Clone();
        var testClient = GraphTestStartup.CreateGraphTestHost(copyMap);
        var newMapOption = await testClient.Execute("select (age) ;", NullScopeContext.Default);
        newMapOption.IsOk().Should().BeTrue(newMapOption.ToString());

        QueryResult result = newMapOption.Return();
        result.Option.IsOk().Should().BeTrue();
        result.Alias.Should().NotBeEmpty();
        result.Nodes.Count.Should().Be(4);
        result.Edges.Count.Should().Be(0);
        result.DataLinks.Count.Should().Be(0);

        result.Nodes.Select(x => x.Key).OrderBy(x => x).SequenceEqual(["node1", "node2", "node4", "node6"]).Should().BeTrue();

        copyMap.Meter.Node.GetIndexHit().Should().Be(1);
        copyMap.Meter.Node.GetIndexMissed().Should().Be(0);
        copyMap.Meter.Node.GetIndexScan().Should().Be(0);
    }

}
