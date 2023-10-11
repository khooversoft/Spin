using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Toolbox.Data;

namespace Toolbox.Test.Data;

public class GraphMapEdgeTests
{
    [Fact]
    public void EdgeTest()
    {
        var e1 = new GraphEdge<string>("fk", "tk");
        e1.Should().NotBeNull();
        e1.ToNodeKey.Should().Be("tk");
        e1.FromNodeKey.Should().Be("fk");
    }

    [Fact]
    public void EdgeWithTagsTest()
    {
        var e1 = new GraphEdge<string>("fk", "tk", "t1;t2=v2");
        e1.Should().NotBeNull();
        e1.ToNodeKey.Should().Be("tk");
        e1.FromNodeKey.Should().Be("fk");
        e1.Tags.Should().NotBeNull();
        e1.Tags.ContainsKey("t1").Should().BeTrue();
        e1.Tags["t1"].Should().BeNull();
        e1.Tags.ContainsKey("t2").Should().BeTrue();
        e1.Tags["t2"].Should().Be("v2");
    }

    [Fact]
    public void NodeWithTagsTest()
    {
        var e1 = new GraphNode<string>("n1", "t1;t2=v2");
        e1.Should().NotBeNull();
        e1.Key.Should().Be("n1");
        e1.Tags.Should().NotBeNull();
        e1.Tags.ContainsKey("t1").Should().BeTrue();
        e1.Tags["t1"].Should().BeNull();
        e1.Tags.ContainsKey("t2").Should().BeTrue();
        e1.Tags["t2"].Should().Be("v2");
    }

    [Fact]
    public void TwoNodesCountTest()
    {
        var map = new GraphMap<string, IGraphNode<string>, IGraphEdge<string>>()
            {
                new GraphNode<string>("Node1"),
                new GraphNode<string>("Node2"),
                new GraphEdge<string>("Node1", "Node2"),
            };

        map.Nodes.Count.Should().Be(2);
        map.Nodes.ContainsKey("Node1").Should().BeTrue();
        map.Nodes.ContainsKey("Node2").Should().BeTrue();
        map.Edges.Count.Should().Be(1);
        map.Edges.First().FromNodeKey.Should().Be("Node1");
        map.Edges.First().ToNodeKey.Should().Be("Node2");
    }

    [Fact]
    public void TwoNodesTwoEdgesCountTest()
    {
        var map = new GraphMap<string, IGraphNode<string>, IGraphEdge<string>>()
        {
            new GraphNode<string>("Node1"),
            new GraphNode<string>("Node2"),
            new GraphEdge<string>("Node1", "Node2"),
            new GraphEdge<string>("Node2", "Node1"),
        };

        map.Nodes.Count.Should().Be(2);
        map.Nodes.ContainsKey("Node1").Should().BeTrue();
        map.Nodes.ContainsKey("Node2").Should().BeTrue();
        map.Edges.Count.Should().Be(2);

        var shouldBe = new[]
        {
            ("Node1", "Node2"),
            ("Node2", "Node1"),
        }.OrderBy(x => x.Item1)
        .ToArray();

        var inSet = map.Edges.Select(x => (x.FromNodeKey, x.ToNodeKey)).OrderBy(x => x.FromNodeKey).ToArray();
        inSet.SequenceEqual(shouldBe).Should().BeTrue();
    }

    [Fact]
    public void EdgeSameNodeTestShouldFail()
    {
        GraphMap<string, IGraphNode<string>, IGraphEdge<string>> map = null!;

        Action test1 = () =>
        {
            map = new GraphMap<string, IGraphNode<string>, IGraphEdge<string>>()
            {
                new GraphNode<string>("Node1"),
                new GraphNode<string>("Node2"),
                new GraphEdge<string>("Node1", "Node1"),
            };
        };

        map.Should().BeNull();
        test1.Should().Throw<ArgumentException>();
    }
}
