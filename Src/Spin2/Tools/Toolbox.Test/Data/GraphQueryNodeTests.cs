using FluentAssertions;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.Test.Data;

public class GraphQueryNodeTests
{
    private readonly GraphMap _map = new GraphMap()
    {
        new GraphNode("node1", tags: "name=marko;age=29"),
        new GraphNode("node2", tags: "name=vadas;age=27"),
        new GraphNode("node3", tags: "name=lop;lang=java"),
        new GraphNode("node4", tags: "name=josh;age=32"),
        new GraphNode("node5", tags: "name=ripple;lang=java"),
        new GraphNode("node6", tags: "name=peter;age=35"),
        new GraphNode("node7", tags: "lang=java"),

        new GraphEdge("node1", "node2", edgeType: "owns-1", tags: "knows;level=1"),
        new GraphEdge("node1", "node3", edgeType: "friends-2", tags: "knows;level=1"),
        new GraphEdge("node6", "node3", edgeType: "owns-3", tags: "created"),
        new GraphEdge("node4", "node5", edgeType: "owns-1", tags: "created"),
        new GraphEdge("node4", "node3", tags: "created"),
    };


    [Fact]
    public void NodeQuery()
    {
        GraphQueryResult result = _map.Query().Execute("(key=node1)");

        result.StatusCode.IsOk().Should().BeTrue();
        result.Alias.Count.Should().Be(0);
        result.Items.Count.Should().Be(1);

        GraphNode node = result.Items[0].Cast<GraphNode>();

        node.Key.Should().Be("node1");
        node.Tags.Has("name=marko;age=29").Should().BeTrue();
        node.Tags.Has("name=marko").Should().BeTrue();
        node.Tags.Has("name").Should().BeTrue();
    }

    [Fact]
    public void QueryWithAlias()
    {
        GraphQueryResult result = _map.Query().Execute("(key=node1) a1");

        result.StatusCode.IsOk().Should().BeTrue();
        result.Items.Count.Should().Be(1);
        result.Alias.Count.Should().Be(1);

        GraphNode node = result.Items[0].Cast<GraphNode>();

        node.Key.Should().Be("node1");
        node.Tags.Has("name=marko;age=29").Should().BeTrue();
        node.Tags.Has("name=marko").Should().BeTrue();
        node.Tags.Has("name").Should().BeTrue();

        result.Alias.ContainsKey("a1").Should().BeTrue();
        node = result.Alias["a1"].Select(x => x.Cast<GraphNode>()).First();
        node.Key.Should().Be("node1");
        node.Tags.Has("name=marko;age=29").Should().BeTrue();
        node.Tags.Has("name=marko").Should().BeTrue();
        node.Tags.Has("name").Should().BeTrue();
    }

    [Fact]
    public void TagDefaultQuery1()
    {
        GraphQueryResult result = _map.Query().Execute("(name) a1");

        result.StatusCode.IsOk().Should().BeTrue();
        result.Items.Count.Should().Be(6);
        result.Alias.Count.Should().Be(1);

        var nodes = result.Items.Select(x => x.Cast<GraphNode>()).ToArray();
        nodes.Length.Should().Be(6);

        var inSet = _map.Nodes.Where(x => x.Key != "node7").Select(x => x.Key).OrderBy(x => x).ToArray();
        Enumerable.SequenceEqual(nodes.Select(x => x.Key).OrderBy(y => y), inSet).Should().BeTrue();
    }

    [Fact]
    public void SpecificTagDefaultQuery()
    {
        GraphQueryResult result = _map.Query().Execute("('name=marko')");

        result.StatusCode.IsOk().Should().BeTrue(result.Error);
        result.Items.Count.Should().Be(1);
        result.Alias.Count.Should().Be(0);

        var nodes = result.Items.Select(x => x.Cast<GraphNode>()).ToArray();
        nodes.Length.Should().Be(1);

        var inSet = _map.Nodes.Where(x => x.Key == "node1" && x.Tags.Has("name=marko")).Select(x => x.Key).ToArray();
        Enumerable.SequenceEqual(nodes.Select(x => x.Key).OrderBy(y => y), inSet).Should().BeTrue();
    }

    [Fact]
    public void TagWithTagKeywordQuery()
    {
        GraphQueryResult result = _map.Query().Execute("(tags='name=marko')");

        result.StatusCode.IsOk().Should().BeTrue(result.Error);
        result.Items.Count.Should().Be(1);
        result.Alias.Count.Should().Be(0);

        var nodes = result.Items.Select(x => x.Cast<GraphNode>()).ToArray();
        nodes.Length.Should().Be(1);

        var inSet = _map.Nodes.Where(x => x.Key == "node1" && x.Tags.Has("name=marko")).Select(x => x.Key).ToArray();
        Enumerable.SequenceEqual(nodes.Select(x => x.Key).OrderBy(y => y), inSet).Should().BeTrue();
    }

    [Fact]
    public void StarNodeSearch()
    {
        var cmds = new (string query, Func<GraphNode, bool> filter, int count)[]
        {
            ("(key=*)", x => true, 7),
            ("(key=*5)", x => x.Key == "node5", 1),
        };

        foreach (var cmd in cmds)
        {
            GraphQueryResult result = _map.Query().Execute(cmd.query);

            result.StatusCode.IsOk().Should().BeTrue(result.Error);
            result.Items.Count.Should().Be(cmd.count);
            result.Alias.Count.Should().Be(0);

            var nodes = result.Items.Select(x => x.Cast<GraphNode>()).ToArray();
            nodes.Length.Should().Be(cmd.count);

            var inSet = _map.Nodes.Where(x => cmd.filter(x)).Select(x => x.Key).ToArray();
            Enumerable.SequenceEqual(nodes.Select(x => x.Key).OrderBy(y => y), inSet).Should().BeTrue();
        }
    }

    [Fact]
    public void StarEdgeSearch()
    {
        var cmds = new (string query, Func<GraphEdge, bool> filter, int count)[]
        {
            ("[fromKey=*]", x => true, 5),
            ("[fromKey=node4]", x => x.FromKey == "node4", 2),
            ("[fromKey=*1]", x => x.FromKey.EndsWith("1"), 2),

            ("[toKey=*]", x => true, 5),
            ("[toKey=node5]", x => x.ToKey == "node5", 1),
            ("[toKey=*3]", x => x.ToKey.EndsWith("3"), 3),

            ("[EdgeType=*]", x => true, 5),
            ("[edgeType=owns-3]", x => x.EdgeType == "owns-3", 1),
            ("[edgeType=owns*]", x => x.EdgeType.StartsWith("own"), 3),
        };

        foreach (var cmd in cmds)
        {
            GraphQueryResult result = _map.Query().Execute(cmd.query);

            result.StatusCode.IsOk().Should().BeTrue(result.Error);
            result.Items.Count.Should().Be(cmd.count);
            result.Alias.Count.Should().Be(0);

            var nodes = result.Items.Select(x => x.Cast<GraphEdge>()).ToArray();
            nodes.Length.Should().Be(cmd.count);

            var inSet = _map.Edges.Where(x => cmd.filter(x)).Select(x => x.Key).OrderBy(x => x).ToArray();
            inSet.Length.Should().Be(nodes.Length, cmd.query);
            Enumerable.SequenceEqual(nodes.Select(x => x.Key).OrderBy(y => y), inSet).Should().BeTrue(cmd.query);
        }
    }
}
