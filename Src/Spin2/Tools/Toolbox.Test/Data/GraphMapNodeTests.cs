using FluentAssertions;
using Toolbox.Data;
using Toolbox.Extensions;

namespace Toolbox.Test.Data;

public class GraphMapNodeTests
{
    [Fact]
    public void EmptyNode()
    {
        var map = new GraphMap<string, IGraphNode<string>, IGraphEdge<string>>();
    }

    [Fact]
    public void Node()
    {
        var e1 = new GraphNode<string>("n1");
        e1.Should().NotBeNull();
        e1.Key.Should().Be("n1");
    }

    [Fact]
    public void OneNode()
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
    public void TwoNodes()
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
    public void Scale()
    {
        const int count = 100;
        const string sampleKey = "Node_10";
        var map = new GraphMap<string, GraphNode<string>, GraphEdge<string>>();

        Enumerable.Range(0, count).ForEach(x => map.Add(new GraphNode<string>($"Node_{x}")));
        map.Nodes.Count.Should().Be(count);
        map.Edges.Count.Should().Be(0);

        IGraphNode<string> node = map.Nodes[sampleKey];
        node.Should().NotBeNull();
        node.Key.Should().Be(sampleKey);
        map.Nodes.TryGetValue(sampleKey, out var _).Should().BeTrue();

        map.Nodes.Remove(sampleKey);
        map.Nodes.TryGetValue(sampleKey, out var _).Should().BeFalse();
        map.Nodes.Count.Should().Be(count - 1);
        map.Edges.Count.Should().Be(0);

        var action = () =>
        {
            IGraphNode<string> node = map.Nodes[sampleKey];
        };

        action.Should().Throw<KeyNotFoundException>();
    }

    [Fact]
    public void TwoNodesSameKeyShouldFail()
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
