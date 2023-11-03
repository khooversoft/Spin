using FluentAssertions;
using Toolbox.Data;
using Toolbox.Extensions;

namespace Toolbox.Test.Data;

public class GraphMapQueryTests
{
    [Fact]
    public void SingleNodeQuery()
    {
        var map = new GraphMap<string>()
        {
            new GraphNode<string>("node1", tags: "name=marko;age=29"),
            new GraphNode<string>("node2", tags: "name=vadas;age=27"),
            new GraphNode<string>("node3", tags: "name=lop;lang=java"),
            new GraphNode<string>("node4", tags: "name=josh;age=32"),
            new GraphNode<string>("node5", tags: "name=ripple;lang=java"),
            new GraphNode<string>("node6", tags: "name=peter;age=35"),
            new GraphNode<string>("node7", tags: "lang=java"),

            new GraphEdge<string>("node1", "node2", tags: "knows;level=1"),
            new GraphEdge<string>("node1", "node3", tags: "knows;level=1"),
            new GraphEdge<string>("node6", "node3", tags: "created"),
            new GraphEdge<string>("node4", "node5", tags: "created"),
            new GraphEdge<string>("node4", "node3", tags: "created"),
        };

        map.Query1().Nodes(x => x.Key == "node1").HasEdge(x => true).Nodes.Action(x =>
        {
            x.Count.Should().Be(1);
            x[0].Key.Should().Be("node1");
            x[0].Tags.Has("name=marko;age=29").Should().BeTrue();
            x[0].Tags.Has("name=marko").Should().BeTrue();
            x[0].Tags.Has("name").Should().BeTrue();
        });

        map.Query1().Nodes(x => x.Tags.Has("name")).Nodes.Action(x =>
        {
            x.Count.Should().Be(6);
            var inSet = map.Nodes.Where(x => x.Key != "node7").Select(x => x.Key).ToArray();
            Enumerable.SequenceEqual(x.Select(x => x.Key).OrderBy(y => y), inSet).Should().BeTrue();
        });

        map.Query1().Nodes(x => x.Tags.Has("name", "marko")).Nodes.Action(x =>
        {
            x.Count.Should().Be(1);
            x.First().Key.Should().Be("node1");
        });

        map.Query1().Nodes().HasEdge(x => x.Tags.Has("knows")).Action(x =>
        {
            x.Nodes.Count.Should().Be(1);
            x.Nodes.First().Key.Should().Be("node1");

            x.Edges.Count.Should().Be(2);
            var shouldMatch = new[] { ("node1", "node2"), ("node1", "node3") };
            var inSet = x.Edges.OrderBy(x => x.ToKey).Select(x => (x.FromKey, x.ToKey)).ToArray();
            Enumerable.SequenceEqual(inSet, shouldMatch).Should().BeTrue();
        });

        map.Query1().Edges().HasNode(x => x.Tags.Has("lang")).Action(x =>
        {
            x.Nodes.Count.Should().Be(2);
            var n1 = new[] { "node3", "node5" };
            var n2 = x.Nodes.Select(x => x.Key).OrderBy(x => x);
            Enumerable.SequenceEqual(n1, n2).Should().BeTrue();

            x.Edges.Count.Should().Be(4);
            var e1 = new[] { "node3", "node3", "node3", "node5" };
            var e2 = x.Edges.Select(x => x.ToKey).OrderBy(x => x);
            Enumerable.SequenceEqual(e1, e2).Should().BeTrue();
        });
    }
}
