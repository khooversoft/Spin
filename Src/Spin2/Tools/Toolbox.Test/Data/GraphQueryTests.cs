using FluentAssertions;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.Test.Data;

public class GraphQueryTests
{
    private readonly GraphMap _map = new GraphMap()
    {
        new GraphNode("node1", tags: "name=marko;age=29;target"),
        new GraphNode("node2", tags: "name=vadas;age=27"),
        new GraphNode("node3", tags: "name=lop;lang=java"),
        new GraphNode("node4", tags: "name=josh;age=32"),
        new GraphNode("node5", tags: "name=ripple;lang=java;target"),
        new GraphNode("node6", tags: "name=peter;age=35"),
        new GraphNode("node7", tags: "lang=java"),

        new GraphEdge("node1", "node2", tags: "knows;level=1"),
        new GraphEdge("node1", "node3", tags: "knows;level=2"),
        new GraphEdge("node6", "node3", tags: "created"),
        new GraphEdge("node4", "node5", tags: "created"),
        new GraphEdge("node4", "node3", tags: "created"),
    };

    [Fact]
    public void NodeToEdge()
    {
        QueryResult result = _map.Query().Search("(name) -> [knows]");

        result.StatusCode.IsOk().Should().BeTrue();
        result.Result.Count.Should().Be(2);
        result.Alias.Count.Should().Be(0);

        var edges = result.Result.Select(x => x.Cast<GraphEdge>()).ToArray();
        edges.Length.Should().Be(2);

        var inSet = _map.Edges.Where(x => x.Tags.Has("knows")).Select(x => x.Key).OrderBy(x => x).ToArray();
        Enumerable.SequenceEqual(edges.Select(x => x.Key).OrderBy(y => y), inSet).Should().BeTrue();
    }

    [Fact]
    public void NodeToEdgeWithAlias()
    {
        QueryResult result = _map.Query().Search("(name) -> [knows] a1");

        result.StatusCode.IsOk().Should().BeTrue();
        result.Result.Count.Should().Be(2);
        result.Alias.Count.Should().Be(1);

        var edges = result.Result.Select(x => x.Cast<GraphEdge>()).ToArray();
        edges.Length.Should().Be(2);

        var inSet = _map.Edges.Where(x => x.Tags.Has("knows")).Select(x => x.Key).OrderBy(x => x).ToArray();
        Enumerable.SequenceEqual(edges.Select(x => x.Key).OrderBy(y => y), inSet).Should().BeTrue();

        result.Alias.Values.First().Count.Should().Be(2);
        edges = result.Alias.Values.First().Select(x => x.Cast<GraphEdge>()).OrderBy(x => x.Key).ToArray();
        Enumerable.SequenceEqual(edges.Select(x => x.Key).OrderBy(y => y), inSet).Should().BeTrue();
    }

    [Fact]
    public void NodeToEdgeToNode()
    {
        QueryResult result = _map.Query().Search("(name) -> [knows] -> ('age=29')");

        result.StatusCode.IsOk().Should().BeTrue();
        result.Result.Count.Should().Be(1);
        result.Alias.Count.Should().Be(0);

        var nodes = result.Result.Select(x => x.Cast<GraphNode>()).ToArray();
        nodes.Length.Should().Be(1);

        var inSet = _map.Nodes.Where(x => x.Tags.Has("age=29")).Select(x => x.Key).ToArray();
        Enumerable.SequenceEqual(nodes.Select(x => x.Key).OrderBy(y => y), inSet).Should().BeTrue();
    }

    [Fact]
    public void NodeToEdgeToNodeWithAliases()
    {
        QueryResult result = _map.Query().Search("(target) a1 -> [knows] a2 -> ('age=29') a3");

        result.StatusCode.IsOk().Should().BeTrue();
        result.Result.Count.Should().Be(1);
        result.Alias.Count.Should().Be(3);

        var nodes = result.Result.Select(x => x.Cast<GraphNode>()).ToArray();
        nodes.Length.Should().Be(1);

        var inSet = _map.Nodes.Where(x => x.Tags.Has("age=29")).Select(x => x.Key).ToArray();
        Enumerable.SequenceEqual(nodes.Select(x => x.Key).OrderBy(y => y), inSet).Should().BeTrue();

        result.Alias.ContainsKey("a1").Should().BeTrue();
        result.Alias["a1"].Count.Should().Be(2);
        nodes = result.Alias["a1"].Select(x => x.Cast<GraphNode>()).OrderBy(x => x.Key).ToArray();
        var inSetNode = _map.Nodes.Where(x => x.Tags.Has("target")).OrderBy(x => x.Key).ToArray();
        Enumerable.SequenceEqual(nodes, inSetNode).Should().BeTrue();

        result.Alias.ContainsKey("a2").Should().BeTrue();
        result.Alias["a2"].Count.Should().Be(2);
        var edges = result.Alias["a2"].Select(x => x.Cast<GraphEdge>()).OrderBy(x => x.Key).ToArray();
        var inSetEdge = _map.Nodes.Where(x => x.Tags.Has("knows")).OrderBy(x => x.Key).ToArray();
        Enumerable.SequenceEqual(nodes, inSetNode).Should().BeTrue();

        result.Alias.ContainsKey("a3").Should().BeTrue();
        result.Alias["a3"].Count.Should().Be(1);
        nodes = result.Alias["a3"].Select(x => x.Cast<GraphNode>()).OrderBy(x => x.Key).ToArray();
        inSetNode = _map.Nodes.Where(x => x.Tags.Has("age=29")).OrderBy(x => x.Key).ToArray();
        Enumerable.SequenceEqual(nodes, inSetNode).Should().BeTrue();
    }

    [Fact]
    public void KeyNodeToEdgeToNodeWithAliases()
    {
        QueryResult result = _map.Query().Search("(key=Node1) a1 -> [knows] a2 -> ('age=29') a3");

        result.StatusCode.IsOk().Should().BeTrue();
        result.Result.Count.Should().Be(1);
        result.Alias.Count.Should().Be(3);

        var nodes = result.Result.Select(x => x.Cast<GraphNode>()).ToArray();
        nodes.Length.Should().Be(1);

        var inSet = _map.Nodes.Where(x => x.Tags.Has("age=29")).Select(x => x.Key).ToArray();
        Enumerable.SequenceEqual(nodes.Select(x => x.Key).OrderBy(y => y), inSet).Should().BeTrue();

        result.Alias.ContainsKey("a1").Should().BeTrue();
        result.Alias["a1"].Count.Should().Be(1);
        nodes = result.Alias["a1"].Select(x => x.Cast<GraphNode>()).OrderBy(x => x.Key).ToArray();
        var inSetNode = _map.Nodes.Where(x => x.Key == "node1").OrderBy(x => x.Key).ToArray();
        Enumerable.SequenceEqual(nodes, inSetNode).Should().BeTrue();

        result.Alias.ContainsKey("a2").Should().BeTrue();
        result.Alias["a2"].Count.Should().Be(2);
        var edges = result.Alias["a2"].Select(x => x.Cast<GraphEdge>()).OrderBy(x => x.Key).ToArray();
        var inSetEdge = _map.Nodes.Where(x => x.Tags.Has("knows")).OrderBy(x => x.Key).ToArray();
        Enumerable.SequenceEqual(nodes, inSetNode).Should().BeTrue();

        result.Alias.ContainsKey("a3").Should().BeTrue();
        result.Alias["a3"].Count.Should().Be(1);
        nodes = result.Alias["a3"].Select(x => x.Cast<GraphNode>()).OrderBy(x => x.Key).ToArray();
        inSetNode = _map.Nodes.Where(x => x.Tags.Has("age=29")).OrderBy(x => x.Key).ToArray();
        Enumerable.SequenceEqual(nodes, inSetNode).Should().BeTrue();
    }
}
