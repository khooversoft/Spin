using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Toolbox.Data;

namespace Toolbox.Test.Data;

public class GraphMapNodeTests
{
    [Fact]
    public void EmptyNodeTest()
    {
        var map = new GraphMap<string, IGraphNode<string>, IGraphEdge<string>>();
    }

    [Fact]
    public void NodeTest()
    {
        var e1 = new GraphNode<string>("n1");
        e1.Should().NotBeNull();
        e1.Key.Should().Be("n1");
    }

    [Fact]
    public void OneNodeTest()
    {
        var map = new GraphMap<string, IGraphNode<string>, IGraphEdge<string>>()
        {
            new GraphNode<string>("Node1"),
        };

        map.Nodes.Count.Should().Be(1);
        map.Nodes.ContainsKey("Node1").Should().BeTrue();
        map.Edges.Count.Should().Be(0);
    }

    [Fact]
    public void TwoNodesTest()
    {
        var map = new GraphMap<string, GraphNode<string>, GraphEdge<string>>()
        {
            new GraphNode<string>("Node1"),
            new GraphNode<string>("Node2"),
        };

        map.Nodes.Count.Should().Be(2);
        map.Nodes.ContainsKey("Node1").Should().BeTrue();
        map.Nodes.ContainsKey("Node2").Should().BeTrue();
        map.Edges.Count.Should().Be(0);
    }

    [Fact]
    public void TwoNodesSameKeyFailureTest()
    {
        GraphMap<string, IGraphNode<string>, IGraphEdge<string>> map = null!;

        var test1 = () => map = new GraphMap<string, IGraphNode<string>, IGraphEdge<string>>()
        {
            new GraphNode<string>("Node1"),
            new GraphNode<string>("Node1"),
        };

        test1.Should().Throw<ArgumentException>();

        var test2 = () => map = new GraphMap<string, IGraphNode<string>, IGraphEdge<string>>()
        {
            new GraphNode<string>("Node1"),
            new GraphNode<string>("node1"),
        };

        test1.Should().Throw<ArgumentException>();
    }
}
