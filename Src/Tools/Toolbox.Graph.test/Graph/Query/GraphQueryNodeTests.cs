using FluentAssertions;
using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.Graph.test.Graph.Query;

public class GraphQueryNodeTests
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

        new GraphEdge("node1", "node2", edgeType: "owns-1", tags: "knows,level=1"),
        new GraphEdge("node1", "node3", edgeType: "friends-2", tags: "knows,level=1"),
        new GraphEdge("node6", "node3", edgeType: "owns-3", tags: "created"),
        new GraphEdge("node4", "node5", edgeType: "owns-1", tags: "created"),
        new GraphEdge("node4", "node3", tags: "created"),
    };


    [Fact]
    public void AllNodesQuery()
    {
        GraphQueryResult result = _map.ExecuteScalar("select (*);", NullScopeContext.Instance);

        result.Status.IsOk().Should().BeTrue();
        result.Alias.Count.Should().Be(0);
        result.Items.Length.Should().Be(7);
    }

    [Fact]
    public void NodeQuery()
    {
        GraphQueryResult result = _map.ExecuteScalar("select (key=node1);", NullScopeContext.Instance);

        result.Status.IsOk().Should().BeTrue();
        result.Alias.Count.Should().Be(0);
        result.Items.Length.Should().Be(1);

        GraphNode node = result.Items[0].Cast<GraphNode>();

        node.Key.Should().Be("node1");
        node.Tags.Has("name=marko,age=29").Should().BeTrue();
        node.Tags.Has("name=marko").Should().BeTrue();
        node.Tags.Has("name").Should().BeTrue();
    }

    [Fact]
    public void QueryWithAlias()
    {
        GraphQueryResult result = _map.ExecuteScalar("select (key=node1) a1;", NullScopeContext.Instance);

        result.Status.IsOk().Should().BeTrue();
        result.Items.Length.Should().Be(1);
        result.Alias.Count.Should().Be(1);

        GraphNode node = result.Items[0].Cast<GraphNode>();

        node.Key.Should().Be("node1");
        node.Tags.Has("name=marko,age=29").Should().BeTrue();
        node.Tags.Has("name=marko").Should().BeTrue();
        node.Tags.Has("name").Should().BeTrue();

        result.Alias.ContainsKey("a1").Should().BeTrue();
        node = result.Alias["a1"].Select(x => x.Cast<GraphNode>()).First();
        node.Key.Should().Be("node1");
        node.Tags.Has("name=marko,age=29").Should().BeTrue();
        node.Tags.Has("name=marko").Should().BeTrue();
        node.Tags.Has("name").Should().BeTrue();
    }

    [Fact]
    public void TagDefaultQuery1()
    {
        GraphQueryResult result = _map.ExecuteScalar("select (name) a1;", NullScopeContext.Instance);

        result.Status.IsOk().Should().BeTrue();
        result.Items.Length.Should().Be(6);
        result.Alias.Count.Should().Be(1);

        var nodes = result.Items.Select(x => x.Cast<GraphNode>()).ToArray();
        nodes.Length.Should().Be(6);

        var inSet = _map.Nodes.Where(x => x.Key != "node7").Select(x => x.Key).OrderBy(x => x).ToArray();
        nodes.Select(x => x.Key).OrderBy(y => y).SequenceEqual(inSet).Should().BeTrue();
    }

    [Fact]
    public void SpecificTagDefaultQuery()
    {
        GraphQueryResult result = _map.ExecuteScalar("select (name=marko);", NullScopeContext.Instance);

        result.Status.IsOk().Should().BeTrue(result.Status.Error);
        result.Items.Length.Should().Be(1);
        result.Alias.Count.Should().Be(0);

        var nodes = result.Items.Select(x => x.Cast<GraphNode>()).ToArray();
        nodes.Length.Should().Be(1);

        var inSet = _map.Nodes.Where(x => x.Key == "node1" && x.Tags.Has("name=marko")).Select(x => x.Key).ToArray();
        nodes.Select(x => x.Key).OrderBy(y => y).SequenceEqual(inSet).Should().BeTrue();
    }

    [Fact]
    public void TagWithTagKeywordQuery()
    {
        GraphQueryResult result = _map.ExecuteScalar("select (name=marko);", NullScopeContext.Instance);

        result.Status.IsOk().Should().BeTrue(result.Status.Error);
        result.Items.Length.Should().Be(1);
        result.Alias.Count.Should().Be(0);

        var nodes = result.Items.Select(x => x.Cast<GraphNode>()).ToArray();
        nodes.Length.Should().Be(1);

        var inSet = _map.Nodes.Where(x => x.Key == "node1" && x.Tags.Has("name=marko")).Select(x => x.Key).ToArray();
        nodes.Select(x => x.Key).OrderBy(y => y).SequenceEqual(inSet).Should().BeTrue();
    }

    [Fact]
    public void StarNodeSearch()
    {
        var cmds = new (string query, Func<GraphNode, bool> filter, int count)[]
        {
            ("select (key=*);", x => true, 7),
            ("select (key=*5);", x => x.Key == "node5", 1),
        };

        foreach (var cmd in cmds)
        {
            GraphQueryResult result = _map.ExecuteScalar(cmd.query, NullScopeContext.Instance);

            result.Status.IsOk().Should().BeTrue(result.Status.Error);
            result.Items.Length.Should().Be(cmd.count);
            result.Alias.Count.Should().Be(0);

            var nodes = result.Items.Select(x => x.Cast<GraphNode>()).ToArray();
            nodes.Length.Should().Be(cmd.count);

            var inSet = _map.Nodes.Where(x => cmd.filter(x)).Select(x => x.Key).ToArray();
            nodes.Select(x => x.Key).OrderBy(y => y).SequenceEqual(inSet).Should().BeTrue();
        }
    }

    [Fact]
    public void StarEdgeSearch()
    {
        var cmds = new (string query, Func<GraphEdge, bool> filter, int count)[]
        {
            ("select [fromKey=*];", x => true, 5),
            ("select [fromKey=node4];", x => x.FromKey == "node4", 2),
            ("select [fromKey=*1];", x => x.FromKey.EndsWith("1"), 2),

            ("select [toKey=*];", x => true, 5),
            ("select [toKey=node5];", x => x.ToKey == "node5", 1),
            ("select [toKey=*3];", x => x.ToKey.EndsWith("3"), 3),

            ("select [EdgeType=*];", x => true, 5),
            ("select [edgeType=owns-3];", x => x.EdgeType == "owns-3", 1),
            ("select [edgeType=owns*];", x => x.EdgeType.StartsWith("own"), 3),
        };

        foreach (var cmd in cmds)
        {
            GraphQueryResult result = _map.ExecuteScalar(cmd.query, NullScopeContext.Instance);

            result.Status.IsOk().Should().BeTrue(result.Status.Error);
            result.Items.Length.Should().Be(cmd.count);
            result.Alias.Count.Should().Be(0);

            var nodes = result.Items.Select(x => x.Cast<GraphEdge>()).ToArray();
            nodes.Length.Should().Be(cmd.count);

            var inSet = _map.Edges.Where(x => cmd.filter(x)).Select(x => x.Key).OrderBy(x => x).ToArray();
            inSet.Length.Should().Be(nodes.Length, cmd.query);
            nodes.Select(x => x.Key).OrderBy(y => y).SequenceEqual(inSet).Should().BeTrue(cmd.query);
        }
    }
}
