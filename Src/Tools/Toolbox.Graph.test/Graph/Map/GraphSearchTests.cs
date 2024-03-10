using FluentAssertions;

namespace Toolbox.Graph.test.Graph.Map;

public class GraphSearchTests
{
    private readonly GraphMap _map = new GraphMap()
    {
        new GraphNode("node1", tags: "name=marko,age=29"),
        new GraphNode("node2", tags: "name=vadas,age=27"),
        new GraphNode("node3", tags: "name=lop,lang=java"),
        new GraphNode("node4", tags: "name=josh,age=32"),
        new GraphNode("node5", tags: "name=ripple,lang=java"),
        new GraphNode("node6", tags: "name=peter,age=35"),
        new GraphNode("node7", tags: "lang=java"),

        new GraphEdge("node1", "node2", tags: "knows,level=1"),
        new GraphEdge("node1", "node3", tags: "knows,level=1"),
        new GraphEdge("node6", "node3", tags: "created"),
        new GraphEdge("node4", "node5", tags: "created"),
        new GraphEdge("node4", "node3", tags: "created"),
    };

    [Fact]
    public void SingleNodeQuery()
    {
        var nodes = _map.Search().Nodes(x => x.Key == "node1").HasEdge(x => true).Nodes;

        nodes.Count.Should().Be(1);
        nodes[0].Key.Should().Be("node1");
        nodes[0].Tags.Has("name=marko,age=29").Should().BeTrue();
        nodes[0].Tags.Has("name=marko").Should().BeTrue();
        nodes[0].Tags.Has("name").Should().BeTrue();
    }

    [Fact]
    public void NodesWithTag()
    {
        var nodes = _map.Search().Nodes(x => x.Tags.Has("name")).Nodes;

        nodes.Count.Should().Be(6);
        var inSet = _map.Nodes.Where(x => x.Key != "node7").Select(x => x.Key).ToArray();
        nodes.Select(x => x.Key).OrderBy(y => y).SequenceEqual(inSet).Should().BeTrue();
    }

    [Fact]
    public void NodesBySpecificTag()
    {
        var nodes = _map.Search().Nodes(x => x.Tags.Has("name", "marko")).Nodes;

        nodes.Count.Should().Be(1);
        nodes.First().Key.Should().Be("node1");
    }

    [Fact]
    public void EdgesByTag()
    {
        var result = _map.Search().Nodes().HasEdge(x => x.Tags.Has("knows"));

        result.Nodes.Count.Should().Be(1);
        result.Nodes.First().Key.Should().Be("node1");

        result.Edges.Count.Should().Be(2);
        var shouldMatch = new[] { ("node1", "node2"), ("node1", "node3") };
        var inSet = result.Edges.OrderBy(x => x.ToKey).Select(x => (x.FromKey, x.ToKey)).ToArray();
        inSet.SequenceEqual(shouldMatch).Should().BeTrue();
    }

    [Fact]
    public void EdgesToNodesForTag()
    {
        var result = _map.Search().Edges().HasNode(x => x.Tags.Has("lang"));

        result.Nodes.Count.Should().Be(2);
        var n1 = new[] { "node3", "node5" };
        var n2 = result.Nodes.Select(x => x.Key).OrderBy(x => x);
        n1.SequenceEqual(n2).Should().BeTrue();

        result.Edges.Count.Should().Be(4);
        var e1 = new[] { "node3", "node3", "node3", "node5" };
        var e2 = result.Edges.Select(x => x.ToKey).OrderBy(x => x);
        e1.SequenceEqual(e2).Should().BeTrue();
    }
}
