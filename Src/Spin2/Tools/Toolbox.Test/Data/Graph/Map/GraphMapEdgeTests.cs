using FluentAssertions;
using Toolbox.Data;
using Toolbox.Extensions;

namespace Toolbox.Test.Data.Graph.Map;

public class GraphMapEdgeTests
{
    [Fact]
    public void ImplicitConversionForTags()
    {
        var v = new GraphEdge { FromKey = "a", ToKey = "b", Tags = "t1" };
        v.Should().NotBeNull();
        v.FromKey.Should().Be("a");
        v.ToKey.Should().Be("b");
        v.Tags.ToString().Should().Be("t1");
    }

    [Fact]
    public void Edge()
    {
        var e1 = new GraphEdge("fk", "tk");
        e1.Should().NotBeNull();
        e1.ToKey.Should().Be("tk");
        e1.FromKey.Should().Be("fk");
        e1.EdgeType.Should().Be("default");

        var json = e1.ToJson();
        json.Should().NotBeNullOrEmpty();
        var r1 = json.ToObject<GraphEdge>();
        (e1 == r1).Should().BeTrue();

        e1 = e1 with { EdgeType = "non-standard" };
        e1.ToKey.Should().Be("tk");
        e1.FromKey.Should().Be("fk");
        e1.EdgeType.Should().Be("non-standard");
    }

    [Fact]
    public void EdgeWithTags()
    {
        var e1 = new GraphEdge("fk", "tk", tags: "t1;t2=v2");
        e1.Should().NotBeNull();
        e1.ToKey.Should().Be("tk");
        e1.FromKey.Should().Be("fk");
        e1.EdgeType.Should().Be("default");
        e1.Tags.Should().NotBeNull();
        e1.Tags.ContainsKey("t1").Should().BeTrue();
        e1.Tags["t1"].Should().BeNull();
        e1.Tags.ContainsKey("t2").Should().BeTrue();
        e1.Tags["t2"].Should().Be("v2");
    }

    [Fact]
    public void NodeWithTags()
    {
        var e1 = new GraphNode("n1", tags: "t1;t2=v2");
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
        var map = new GraphMap()
            {
                new GraphNode("Node1"),
                new GraphNode("Node2"),
                new GraphEdge("Node1", "Node2"),
            };

        map.Nodes.Count.Should().Be(2);
        map.Nodes.ContainsKey("Node1").Should().BeTrue();
        map.Nodes.ContainsKey("Node2").Should().BeTrue();
        map.Edges.Count.Should().Be(1);
        map.Edges.First().FromKey.Should().Be("Node1");
        map.Edges.First().ToKey.Should().Be("Node2");
    }

    [Fact]
    public void TwoNodesTwoEdgesCount()
    {
        var map = new GraphMap()
        {
            new GraphNode("Node1"),
            new GraphNode("Node2"),
            new GraphEdge("Node1", "Node2"),
            new GraphEdge("Node2", "Node1"),
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

        var inSet = map.Edges.Select(x => (x.FromKey, x.ToKey)).OrderBy(x => x.FromKey).ToArray();
        inSet.SequenceEqual(shouldBe).Should().BeTrue();
    }

    [Fact]
    public void EdgeSameNodeTestShouldFail()
    {
        GraphMap map = null!;

        Action test1 = () =>
        {
            map = new GraphMap()
            {
                new GraphNode("Node1"),
                new GraphNode("Node2"),
                new GraphEdge("Node1", "Node1"),
            };
        };

        map.Should().BeNull();
        test1.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void EdgeReferenceNodeThatDoesNotExistShouldFail()
    {
        Action test2 = () => new GraphMap()
        {
            new GraphNode("Node1"),
            new GraphNode("Node2"),
            new GraphEdge("Node1", "Node2"),
            new GraphEdge("Node1", "Node3"),
        };

        test2.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void DuplicateEdgeReferenceNodeShouldFail()
    {
        Action test2 = () => new GraphMap()
        {
            new GraphNode("Node1"),
            new GraphNode("Node2"),
            new GraphEdge("Node1", "Node2"),
            new GraphEdge("Node1", "Node2"),
        };

        test2.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Scale()
    {
        const int count = 100;
        const string fromKey = "Node_10";
        const string toKey = "Node_11";
        var map = new GraphMap();

        Enumerable.Range(0, count + 1).ForEach(x => map.Add(new GraphNode($"Node_{x}")));
        Enumerable.Range(0, count).ForEach(x => map.Add(new GraphEdge($"Node_{x}", $"Node_{x + 1}")));
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
        var comparer = new GraphEdgeComparer();

        var e1 = new GraphEdge("n1", "n2");
        var e2 = e1;
        e1.Equals(e2).Should().BeTrue();

        e1 = new GraphEdge("n1", "n2");
        e2 = new GraphEdge("n1", "n2");
        e1.Equals(e2).Should().BeFalse();
        comparer.Equals(e1, e2).Should().BeTrue();

        e1 = new GraphEdge("n1", "n2", "edgeType");
        e2 = new GraphEdge("n1", "n2", "edgeType");
        comparer.Equals(e1, e2).Should().BeTrue();

        e1 = new GraphEdge("n1", "n2", "edgeType");
        e2 = new GraphEdge("n1", "n2", "edgeType1");
        comparer.Equals(e1, e2).Should().BeFalse();

        e1 = new GraphEdge("n1", "n2");
        e2 = new GraphEdge("n1", "n3");
        comparer.Equals(e1, e2).Should().BeFalse();

        e1 = new GraphEdge("n1", "n2", tags: "t1");
        e2 = new GraphEdge("n1", "n2", tags: "t1");
        comparer.Equals(e1, e2).Should().BeTrue();

        e1 = new GraphEdge("n1", "n2", tags: "t1");
        e2 = new GraphEdge("n1", "n2", tags: "t2");
        comparer.Equals(e1, e2).Should().BeTrue();

        e1 = new GraphEdge("n1", "n2", "edgeType", tags: "t1;t2=v2");
        e2 = new GraphEdge("n1", "n2", "edgeType", tags: "t1;t2=v2");
        comparer.Equals(e1, e2).Should().BeTrue();

        e1 = new GraphEdge("n1", "n2", "edgeType", tags: "t1;t2=v2");
        e2 = new GraphEdge("n1", "n2", "edgeType2", tags: "t1;t2=v2");
        comparer.Equals(e1, e2).Should().BeFalse();
    }

    [Fact]
    public void EdgeDuplicateCheck()
    {
        HashSet<GraphEdge> h1 = new HashSet<GraphEdge>(new GraphEdgeComparer());

        var e1 = new GraphEdge("n1", "n2");
        h1.Add(e1).Should().BeTrue();
        h1.Count.Should().Be(1);

        e1 = new GraphEdge("n1", "n2");
        h1.Add(e1).Should().BeFalse();
        h1.Count.Should().Be(1);

        e1 = new GraphEdge("n1", "n2", edgeType: "t1");
        h1.Add(e1).Should().BeTrue();
        h1.Count.Should().Be(2);

        e1 = new GraphEdge("n1", "n2", edgeType: "t1");
        h1.Add(e1).Should().BeFalse();
        h1.Count.Should().Be(2);

        e1 = new GraphEdge("n1", "n2", edgeType: "t1;t2=v2");
        h1.Add(e1).Should().BeTrue();
        h1.Count.Should().Be(3);

        e1 = new GraphEdge("n1", "n2", edgeType: "t1;t2=v2");
        h1.Add(e1).Should().BeFalse();
        h1.Count.Should().Be(3);
    }
}
