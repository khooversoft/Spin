using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            new GraphNode<string>("node1", "name=marko;age=29"),
            new GraphNode<string>("node2", "name=vadas;age=27"),
            new GraphNode<string>("node3", "name=lop;lang=java"),
            new GraphNode<string>("node4", "name=josh;age=32"),
            new GraphNode<string>("node5", "name=ripple;lang=java"),
            new GraphNode<string>("node6", "name=peter;age=35"),
            new GraphNode<string>("node7", "lang=java"),

            new GraphEdge<string>("node1", "node2", "knows;level=1"),
            new GraphEdge<string>("node1", "node3", "knows;level=1"),
            new GraphEdge<string>("node6", "node3", "created"),
            new GraphEdge<string>("node4", "node5", "created"),
            new GraphEdge<string>("node4", "node3", "created"),
        };

        map.Query().Nodes(x => x.Tags.Has("name")).Nodes.Action(x =>
        {
            x.Count.Should().Be(6);
            var inSet = map.Nodes.Where(x => x.Key != "node7").Select(x => x.Key).ToArray();
            Enumerable.SequenceEqual(x.Select(x => x.Key).OrderBy(y => y), inSet).Should().BeTrue();
        });

        map.Query().Nodes(x => x.Tags.Has("name", "marko")).Nodes.Action(x =>
        {
            x.Count.Should().Be(1);
            x.First().Key.Should().Be("node1");
        });

        map.Query().Nodes().HasEdge(x => x.Tags.Has("knows")).Action(x =>
        {
            x.Nodes.Count.Should().Be(1);
            x.Nodes.First().Key.Should().Be("node1");

            x.Edges.Count.Should().Be(2);
            var shouldMatch = new[] { ("node1", "node2"), ("node1", "node3") };
            var inSet = x.Edges.OrderBy(x => x.ToNodeKey).Select(x => (x.FromNodeKey, x.ToNodeKey)).ToArray();
            Enumerable.SequenceEqual(inSet, shouldMatch).Should().BeTrue();
        });

        map.Query().Edges().HasNode(x => x.Tags.Has("lang")).Action(x =>
        {
            x.Nodes.Count.Should().Be(2);
            var n1 = new[] { "node3", "node5" };
            var n2 = x.Nodes.Select(x => x.Key).OrderBy(x => x);
            Enumerable.SequenceEqual(n1, n2).Should().BeTrue();

            x.Edges.Count.Should().Be(4);
            var e1 = new[] { "node3", "node3", "node3", "node5" };
            var e2 = x.Edges.Select(x => x.ToNodeKey).OrderBy(x => x);
            Enumerable.SequenceEqual(e1, e2).Should().BeTrue();
        });
    }
}
