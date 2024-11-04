using FluentAssertions;
using Toolbox.Types;

namespace Toolbox.Graph.test.Command;

public class SelectEdgeInstructionTests
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
        new GraphEdge("node6", "node3", edgeType: "et3", tags: "created"),
        new GraphEdge("node4", "node5", edgeType: "et3", tags: "created"),
        new GraphEdge("node4", "node3", edgeType : "et3", tags: "created"),
        new GraphEdge("node5", "node4", edgeType : "et2", tags: "created"),
    };

    [Fact]
    public async Task SelecteAllEdges()
    {
        var copyMap = _map.Clone();
        var testClient = GraphTestStartup.CreateGraphTestHost(copyMap);
        var newMapOption = await testClient.Execute("select [*] ;", NullScopeContext.Default);
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
            ("node6", "node3", "et3"),
            ("node4", "node5", "et3"),
            ("node4", "node3", "et3"),
            ("node5", "node4", "et2"),
        };

        result.Edges.Select(x => (x.FromKey, x.ToKey, x.EdgeType)).Should().BeEquivalentTo(expected);

        copyMap.Meter.Edge.GetIndexHit().Should().Be(0);
        copyMap.Meter.Edge.GetIndexMissed().Should().Be(0);
        copyMap.Meter.Edge.GetIndexScan().Should().Be(1);
    }

    [Fact]
    public async Task SelecteAllEdgesWithTag()
    {
        var copyMap = _map.Clone();
        var testClient = GraphTestStartup.CreateGraphTestHost(copyMap);
        var newMapOption = await testClient.Execute("select [*, level=1] ;", NullScopeContext.Default);
        newMapOption.IsOk().Should().BeTrue(newMapOption.ToString());

        QueryResult result = newMapOption.Return();
        result.Option.IsOk().Should().BeTrue();
        result.Alias.Should().NotBeNullOrWhiteSpace();
        result.Nodes.Count.Should().Be(0);
        result.Edges.Count.Should().Be(2);
        result.DataLinks.Count.Should().Be(0);

        var expected = new List<(string FromKey, string ToKey, string EdgeType)>
        {
            ("node1", "node2", "et1"),
            ("node1", "node3", "et1"),
        };

        result.Edges.Select(x => (x.FromKey, x.ToKey, x.EdgeType)).Should().BeEquivalentTo(expected);

        copyMap.Meter.Edge.GetIndexHit().Should().Be(0);
        copyMap.Meter.Edge.GetIndexMissed().Should().Be(0);
        copyMap.Meter.Edge.GetIndexScan().Should().Be(1);
    }

    [Fact]
    public async Task SelectEdgesWithTag()
    {
        var copyMap = _map.Clone();
        var testClient = GraphTestStartup.CreateGraphTestHost(copyMap);
        var newMapOption = await testClient.Execute("select [level=1] ;", NullScopeContext.Default);
        newMapOption.IsOk().Should().BeTrue(newMapOption.ToString());

        QueryResult result = newMapOption.Return();
        result.Option.IsOk().Should().BeTrue();
        result.Alias.Should().NotBeNullOrWhiteSpace();
        result.Nodes.Count.Should().Be(0);
        result.Edges.Count.Should().Be(2);
        result.DataLinks.Count.Should().Be(0);

        var expected = new List<(string FromKey, string ToKey, string EdgeType)>
        {
            ("node1", "node2", "et1"),
            ("node1", "node3", "et1"),
        };

        result.Edges.Select(x => (x.FromKey, x.ToKey, x.EdgeType)).Should().BeEquivalentTo(expected);

        copyMap.Meter.Edge.GetIndexHit().Should().Be(3);
        copyMap.Meter.Edge.GetIndexMissed().Should().Be(0);
        copyMap.Meter.Edge.GetIndexScan().Should().Be(0);
    }

    [Fact]
    public async Task SelectEdgesByFrom()
    {
        var copyMap = _map.Clone();
        var testClient = GraphTestStartup.CreateGraphTestHost(copyMap);
        var newMapOption = await testClient.Execute("select [from=node1] ;", NullScopeContext.Default);
        newMapOption.IsOk().Should().BeTrue(newMapOption.ToString());

        QueryResult result = newMapOption.Return();
        result.Option.IsOk().Should().BeTrue();
        result.Alias.Should().NotBeNullOrWhiteSpace();
        result.Nodes.Count.Should().Be(0);
        result.Edges.Count.Should().Be(2);
        result.DataLinks.Count.Should().Be(0);

        var expected = new List<(string FromKey, string ToKey, string EdgeType)>
        {
            ("node1", "node2", "et1"),
            ("node1", "node3", "et1"),
        };

        result.Edges.Select(x => (x.FromKey, x.ToKey, x.EdgeType)).Should().BeEquivalentTo(expected);

        copyMap.Meter.Edge.GetIndexHit().Should().Be(3);
        copyMap.Meter.Edge.GetIndexMissed().Should().Be(0);
        copyMap.Meter.Edge.GetIndexScan().Should().Be(0);
    }

    [Fact]
    public async Task SelectEdgesByTo()
    {
        var copyMap = _map.Clone();
        var testClient = GraphTestStartup.CreateGraphTestHost(copyMap);
        var newMapOption = await testClient.Execute("select [to=node3] ;", NullScopeContext.Default);
        newMapOption.IsOk().Should().BeTrue(newMapOption.ToString());

        QueryResult result = newMapOption.Return();
        result.Option.IsOk().Should().BeTrue();
        result.Alias.Should().NotBeNullOrWhiteSpace();
        result.Nodes.Count.Should().Be(0);
        result.Edges.Count.Should().Be(3);
        result.DataLinks.Count.Should().Be(0);

        var expected = new List<(string FromKey, string ToKey, string EdgeType)>
        {
            ("node1", "node3", "et1"),
            ("node6", "node3", "et3"),
            ("node4", "node3", "et3"),
        };

        result.Edges.Select(x => (x.FromKey, x.ToKey, x.EdgeType)).Should().BeEquivalentTo(expected);

        copyMap.Meter.Edge.GetIndexHit().Should().Be(4);
        copyMap.Meter.Edge.GetIndexMissed().Should().Be(0);
        copyMap.Meter.Edge.GetIndexScan().Should().Be(0);
    }

    [Fact]
    public async Task SelectEdgesByEdgeType()
    {
        var copyMap = _map.Clone();
        var testClient = GraphTestStartup.CreateGraphTestHost(copyMap);
        var newMapOption = await testClient.Execute("select [type=et3] ;", NullScopeContext.Default);
        newMapOption.IsOk().Should().BeTrue(newMapOption.ToString());

        QueryResult result = newMapOption.Return();
        result.Option.IsOk().Should().BeTrue();
        result.Alias.Should().NotBeNullOrWhiteSpace();
        result.Nodes.Count.Should().Be(0);
        result.Edges.Count.Should().Be(3);
        result.DataLinks.Count.Should().Be(0);

        var expected = new List<(string FromKey, string ToKey, string EdgeType)>
        {
            ("node6", "node3", "et3"),
            ("node4", "node5", "et3"),
            ("node4", "node3", "et3"),
        };

        result.Edges.Select(x => (x.FromKey, x.ToKey, x.EdgeType)).Should().BeEquivalentTo(expected);

        copyMap.Meter.Edge.GetIndexHit().Should().Be(4);
        copyMap.Meter.Edge.GetIndexMissed().Should().Be(0);
        copyMap.Meter.Edge.GetIndexScan().Should().Be(0);
    }
}
