using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Toolbox.Data;
using Toolbox.Extensions;

namespace Toolbox.Test.Data;

public class GraphMapEdgeTests
{
    [Fact]
    public void Edge()
    {
        var e1 = new GraphEdge<string>("fk", "tk");
        e1.Should().NotBeNull();
        e1.ToNodeKey.Should().Be("tk");
        e1.FromNodeKey.Should().Be("fk");
    }

    [Fact]
    public void EdgeWithTags()
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
    public void NodeWithTags()
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
    public void TwoNodesCount()
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
    public void TwoNodesTwoEdgesCount()
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

    [Fact]
    public void EdgeReferenceNodeThatDoesNotExistShouldFail()
    {
        Action test2 = () => new GraphMap<string, IGraphNode<string>, IGraphEdge<string>>()
        {
            new GraphNode<string>("Node1"),
            new GraphNode<string>("Node2"),
            new GraphEdge<string>("Node1", "Node2"),
            new GraphEdge<string>("Node1", "Node3"),
        };

        test2.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void DuplicateEdgeReferenceNodeShouldFail()
    {
        Action test2 = () => new GraphMap<string, IGraphNode<string>, IGraphEdge<string>>()
        {
            new GraphNode<string>("Node1"),
            new GraphNode<string>("Node2"),
            new GraphEdge<string>("Node1", "Node2"),
            new GraphEdge<string>("Node1", "Node2"),
        };

        test2.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Scale()
    {
        const int count = 100;
        const string fromKey = "Node_10";
        const string toKey = "Node_11";
        var map = new GraphMap<string, GraphNode<string>, GraphEdge<string>>();

        Enumerable.Range(0, count + 1).ForEach(x => map.Add(new GraphNode<string>($"Node_{x}")));
        Enumerable.Range(0, count).ForEach(x => map.Add(new GraphEdge<string>($"Node_{x}", $"Node_{x + 1}")));
        map.Nodes.Count.Should().Be(count + 1);
        map.Edges.Count.Should().Be(count);

        map.Nodes.ContainsKey(fromKey).Should().BeTrue();
        map.Nodes.ContainsKey(toKey).Should().BeTrue();
        map.Edges.Get(fromKey, toKey, EdgeDirection.Directed).Count.Should().Be(1);
        map.Edges.Get(fromKey, toKey, EdgeDirection.Both).Count.Should().Be(1);
        map.Edges.Get(fromKey).Count.Should().Be(2);
        map.Edges.Get(toKey).Count.Should().Be(2);

        map.Nodes.Remove(fromKey).Should().BeTrue();
        map.Nodes.ContainsKey(fromKey).Should().BeFalse();
        map.Nodes.ContainsKey(toKey).Should().BeTrue();
        map.Edges.Get(fromKey, toKey, EdgeDirection.Directed).Count.Should().Be(0);
        map.Edges.Get(fromKey, toKey, EdgeDirection.Both).Count.Should().Be(0);
        map.Edges.Get(fromKey).Count.Should().Be(0);
        map.Edges.Get(toKey).Count.Should().Be(1);
    }

    [Fact]
    public void EdgeEquivalencyEqual()
    {
        var comparer = new GraphEdgeComparer<string, GraphEdge<string>>();

        var e1 = new GraphEdge<string>("n1", "n2");
        var e2 = e1;
        e1.Equals(e2).Should().BeTrue();

        e1 = new GraphEdge<string>("n1", "n2");
        e2 = new GraphEdge<string>("n1", "n2");
        e1.Equals(e2).Should().BeFalse();
        comparer.Equals(e1, e2).Should().BeTrue();

        e1 = new GraphEdge<string>("n1", "n2");
        e2 = new GraphEdge<string>("n1", "n3");
        comparer.Equals(e1, e2).Should().BeFalse();

        e1 = new GraphEdge<string>("n1", "n2", "t1");
        e2 = new GraphEdge<string>("n1", "n2", "t1");
        comparer.Equals(e1, e2).Should().BeTrue();

        e1 = new GraphEdge<string>("n1", "n2", "t1");
        e2 = new GraphEdge<string>("n1", "n2", "t2");
        comparer.Equals(e1, e2).Should().BeFalse();

        e1 = new GraphEdge<string>("n1", "n2", "t1;t2=v2");
        e2 = new GraphEdge<string>("n1", "n2", "t1;t2=v2");
        comparer.Equals(e1, e2).Should().BeTrue();

        e1 = new GraphEdge<string>("n1", "n2", "t1;t2=v2");
        e2 = new GraphEdge<string>("n1", "n2", "t1;t2=v3");
        comparer.Equals(e1, e2).Should().BeFalse();
    }

    [Fact]
    public void EdgeDuplicateCheck()
    {
        HashSet<GraphEdge<string>> h1 = new HashSet<GraphEdge<string>>(new GraphEdgeComparer<string, GraphEdge<string>>());

        var e1 = new GraphEdge<string>("n1", "n2");
        h1.Add(e1).Should().BeTrue();
        h1.Count.Should().Be(1);

        e1 = new GraphEdge<string>("n1", "n2");
        h1.Add(e1).Should().BeFalse();
        h1.Count.Should().Be(1);

        e1 = new GraphEdge<string>("n1", "n2", "t1");
        h1.Add(e1).Should().BeTrue();
        h1.Count.Should().Be(2);

        e1 = new GraphEdge<string>("n1", "n2", "t1");
        h1.Add(e1).Should().BeFalse();
        h1.Count.Should().Be(2);

        e1 = new GraphEdge<string>("n1", "n2", "t1;t2=v2");
        h1.Add(e1).Should().BeTrue();
        h1.Count.Should().Be(3);

        e1 = new GraphEdge<string>("n1", "n2", "t1;t2=v2");
        h1.Add(e1).Should().BeFalse();
        h1.Count.Should().Be(3);
    }
}
