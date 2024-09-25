using FluentAssertions;
using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.Graph.test.Command;

public class DeleteInstructionTests
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
    };

    [Fact]
    public async Task DeleteAllNode()
    {
        var copyMap = _map.Clone();
        var testClient = GraphTestStartup.CreateGraphTestHost(copyMap);
        var newMapOption = await testClient.Execute("delete (*) ;", NullScopeContext.Instance);
        newMapOption.IsOk().Should().BeTrue(newMapOption.ToString());

        QueryResult result = newMapOption.Return();
        result.Option.IsOk().Should().BeTrue();
        result.Nodes.Count.Should().Be(7);
        result.Edges.Count.Should().Be(0);
        result.DataLinks.Count.Should().Be(0);

        copyMap.Nodes.Count.Should().Be(0);
        copyMap.Edges.Count.Should().Be(0);
    }

    [Fact]
    public async Task DeleteAllEdges()
    {
        var copyMap = _map.Clone();
        var testClient = GraphTestStartup.CreateGraphTestHost(copyMap);
        var newMapOption = await testClient.Execute("delete [*] ;", NullScopeContext.Instance);
        newMapOption.IsOk().Should().BeTrue(newMapOption.ToString());

        QueryResult result = newMapOption.Return();
        result.Option.IsOk().Should().BeTrue();
        result.Nodes.Count.Should().Be(0);
        result.Edges.Count.Should().Be(5);
        result.DataLinks.Count.Should().Be(0);

        copyMap.Nodes.Count.Should().Be(7);
        copyMap.Edges.Count.Should().Be(0);
    }

    [Fact]
    public async Task DeleteNode()
    {
        var copyMap = _map.Clone();
        var testClient = GraphTestStartup.CreateGraphTestHost(copyMap);
        var newMapOption = await testClient.Execute("delete (key=node2) a1 ;", NullScopeContext.Instance);
        newMapOption.IsOk().Should().BeTrue(newMapOption.ToString());

        QueryResult result = newMapOption.Return();
        result.Option.IsOk().Should().BeTrue();
        result.Alias.Should().Be("a1");
        result.Nodes.Count.Should().Be(1);
        result.Edges.Count.Should().Be(0);
        result.DataLinks.Count.Should().Be(0);

        var compareMap = GraphCommandTools.CompareMap(_map, copyMap);

        compareMap.Count.Should().Be(2);
        compareMap.OfType<GraphNode>().Single().Action(x =>
        {
            x.Key.Should().Be("node2");
        });
        compareMap.OfType<GraphEdge>().Single().Action(x =>
        {
            x.FromKey.Should().Be("node1");
            x.ToKey.Should().Be("node2");
        });
    }

    [Fact]
    public async Task DeleteEdgeFromNode()
    {
        var copyMap = _map.Clone();
        var testClient = GraphTestStartup.CreateGraphTestHost(copyMap);
        var newMapOption = await testClient.Execute("delete (key=node6) -> [*] ;", NullScopeContext.Instance);
        newMapOption.IsOk().Should().BeTrue(newMapOption.ToString());

        QueryResult result = newMapOption.Return();
        result.Option.IsOk().Should().BeTrue();
        result.Nodes.Count.Should().Be(0);
        result.Edges.Count.Should().Be(1);
        result.DataLinks.Count.Should().Be(0);

        var compareMap = GraphCommandTools.CompareMap(_map, copyMap);

        compareMap.Count.Should().Be(1);
        compareMap.OfType<GraphEdge>().Single().Action(x =>
        {
            x.FromKey.Should().Be("node6");
            x.ToKey.Should().Be("node3");
        });
    }

    [Fact]
    public async Task DeleteNodeFromEdgeFromNode()
    {
        var copyMap = _map.Clone();
        var testClient = GraphTestStartup.CreateGraphTestHost(copyMap);
        var newMapOption = await testClient.Execute("delete (key=node6) -> [*] -> (*) ;", NullScopeContext.Instance);
        newMapOption.IsOk().Should().BeTrue(newMapOption.ToString());

        QueryResult result = newMapOption.Return();
        result.Option.IsOk().Should().BeTrue();
        result.Nodes.Count.Should().Be(1);
        result.Edges.Count.Should().Be(0);
        result.DataLinks.Count.Should().Be(0);

        var compareMap = GraphCommandTools.CompareMap(_map, copyMap);

        compareMap.Count.Should().Be(4);
        compareMap.OfType<GraphNode>().Single().Action(x =>
        {
            x.Key.Should().Be("node3");
        });

        var expect = new (string from, string to)[]
        {
            ("node1", "node3"),
            ("node4", "node3"),
            ("node6", "node3"),
        };

        compareMap.OfType<GraphEdge>().Select(x => (x.FromKey, x.ToKey)).Should().BeEquivalentTo(expect);
    }
}
