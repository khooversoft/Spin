using FluentAssertions;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Test.Data;

public class GraphMapSerializationTests
{
    [Fact]
    public void EmptyMapString()
    {
        var map = new GraphMap<string>();

        var json = map.ToJson();

        var mapResult = GraphMap.FromJson<string>(json).NotNull();
        mapResult.Should().NotBeNull();
        mapResult.Count().Should().Be(0);
        mapResult.Nodes.Count.Should().Be(0);
        mapResult.Edges.Count.Should().Be(0);
    }

    [Fact]
    public void SingleNodeMap()
    {
        new GraphMap<string>()
        {
            new GraphNode<string>("Node1"),
        }.Action(x =>
        {
            var json = x.ToJson();

            var mapResult = GraphMap.FromJson<string>(json).NotNull();
            mapResult.Should().NotBeNull();
            mapResult.Count().Should().Be(1);
            mapResult.Edges.Count.Should().Be(0);

            mapResult.Nodes.Count.Should().Be(1);
            mapResult.Nodes.First().Action(y =>
            {
                y.Key.Should().Be("Node1");
                y.Tags.Should().NotBeNull();
                y.Tags.Count.Should().Be(0);
            });
        });

        new GraphMap<string>()
        {
            new GraphNode<string>("Node1", "t1;t2=v2"),
        }.Action(x =>
        {
            var json = x.ToJson();

            var mapResult = GraphMap.FromJson<string>(json).NotNull();
            mapResult.Should().NotBeNull();
            mapResult.Count().Should().Be(1);
            mapResult.Edges.Count.Should().Be(0);

            mapResult.Nodes.Count.Should().Be(1);
            mapResult.Nodes.First().Action(y =>
            {
                y.Key.Should().Be("Node1");
                y.Tags.Should().NotBeNull();
                y.Tags.Count.Should().Be(2);
                y.Tags["t1"].Should().BeNull();
                y.Tags["t2"].Should().Be("v2");
            });
        });
    }

    [Fact]
    public void GraphMapSample1()
    {
        var map = new GraphMap<string>()
        {
            new GraphNode<string>("node1"),
            new GraphNode<string>("node2"),
            new GraphNode<string>("node3"),
            new GraphNode<string>("node4"),

            new GraphEdge<string>("node1", "node2"),
            new GraphEdge<string>("node1", "node3"),
            new GraphEdge<string>("node1", "node4"),
        };

        string json = map.ToJson();

        var mapRead = GraphMap.FromJson<string>(json).NotNull();
        mapRead.Nodes.Count.Should().Be(4);
        mapRead.Edges.Count.Should().Be(3);

        var s1 = map.Nodes.Select(x => x.Key).OrderBy(x => x).ToArray();
        var s2 = mapRead.Nodes.Select(x => x.Key).OrderBy(x => x).ToArray();
        Enumerable.SequenceEqual(s1, s2).Should().BeTrue();

        var e1 = map.Edges.Select(x => x.Key).OrderBy(x => x).ToArray();
        var e2 = mapRead.Edges.Select(x => x.Key).OrderBy(x => x).ToArray();
        Enumerable.SequenceEqual(e1, e2).Should().BeTrue();
    }

    [Fact]
    public void NodeSerialization()
    {
        new GraphNode<string>("node1").Action(x =>
        {
            string json = x.ToJson();

            var graphNode = json.ToObject<GraphNode<string>>();
            graphNode.Should().NotBeNull();
            graphNode!.Key.Should().Be("node1");
            graphNode.Tags.Should().NotBeNull();
        });

        new GraphNode<string>("node1", "t1;t2=v2").Action(x =>
        {
            string json = x.ToJson();

            var graphNode = json.ToObject<GraphNode<string>>();
            graphNode.Should().NotBeNull();
            graphNode!.Key.Should().Be("node1");
            graphNode.Tags.Should().NotBeNull();
            graphNode.Tags.Count.Should().Be(2);
            graphNode.Tags["t1"].Should().BeNull();
            graphNode.Tags["t2"].Should().Be("v2");
        });
    }

    [Fact]
    public void EdgeSerialization()
    {
        new GraphEdge<string>("node1", "node2").Action(x =>
        {
            string json = x.ToJson();

            var graphEdge = json.ToObject<GraphEdge<string>>();
            graphEdge.Should().NotBeNull();
            graphEdge!.FromNodeKey.Should().Be("node1");
            graphEdge!.ToNodeKey.Should().Be("node2");
            graphEdge.Tags.Should().NotBeNull();
        });

        new GraphEdge<string>("node1", "node2", "t1;t2=v2").Action(x =>
        {
            string json = x.ToJson();

            var graphEdge = json.ToObject<GraphEdge<string>>();
            graphEdge.Should().NotBeNull();
            graphEdge!.FromNodeKey.Should().Be("node1");
            graphEdge!.ToNodeKey.Should().Be("node2");
            graphEdge.Tags.Should().NotBeNull();
            graphEdge.Tags.Count.Should().Be(2);
            graphEdge.Tags["t1"].Should().BeNull();
            graphEdge.Tags["t2"].Should().Be("v2");
        });
    }
}
