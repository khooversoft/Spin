using FluentAssertions;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Test.Data.Graph.Map;

public class GraphMapSerializationTests
{
    [Fact]
    public void GraphNode()
    {
        var v = new GraphNode("Node1", "t1=v");

        string json = v.ToJson();
        json.Should().NotBeNullOrEmpty();

        var v2 = json.ToObject<GraphNode>();
        (v == v2).Should().BeTrue();
    }

    [Fact]
    public void GraphEdge()
    {
        var v = new GraphEdge("Node1", "Node2", "edgeType", "t1=v");

        string json = v.ToJson();
        json.Should().NotBeNullOrEmpty();

        var v2 = json.ToObject<GraphEdge>();
        (v == v2).Should().BeTrue();
    }

    [Fact]
    public void EmptyMapString()
    {
        var map = new GraphMap();

        var json = map.ToJson();

        var mapResult = GraphMap.FromJson(json).NotNull();
        mapResult.Should().NotBeNull();
        mapResult.Count().Should().Be(0);
        mapResult.Nodes.Count.Should().Be(0);
        mapResult.Edges.Count.Should().Be(0);
    }

    [Fact]
    public void SingleNodeMap()
    {
        new GraphMap()
        {
            new GraphNode("Node1"),
        }.Action(x =>
        {
            var json = x.ToJson();

            var mapResult = GraphMap.FromJson(json).NotNull();
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

        new GraphMap()
        {
            new GraphNode("Node1"),
        }.Action(x =>
        {
            var json = x.ToJson();

            var mapResult = GraphMap.FromJson(json).NotNull();
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

        new GraphMap()
        {
            new GraphNode("Node1", tags: "t1;t2=v2"),
        }.Action(x =>
        {
            var json = x.ToJson();

            var mapResult = GraphMap.FromJson(json).NotNull();
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

        new GraphMap()
        {
            new GraphNode("Node1", tags: "t1;t2=v2"),
        }.Action(x =>
        {
            var json = x.ToJson();

            var mapResult = GraphMap.FromJson(json).NotNull();
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
        var map = new GraphMap()
        {
            new GraphNode("node1"),
            new GraphNode("node2"),
            new GraphNode("node3"),
            new GraphNode("node4"),

            new GraphEdge("node1", "node2"),
            new GraphEdge("node1", "node3"),
            new GraphEdge("node1", "node4"),
        };

        string json = map.ToJson();

        var mapRead = GraphMap.FromJson(json).NotNull();
        mapRead.Nodes.Count.Should().Be(4);
        mapRead.Edges.Count.Should().Be(3);

        var s1 = map.Nodes.Select(x => x.Key).OrderBy(x => x).ToArray();
        var s2 = mapRead.Nodes.Select(x => x.Key).OrderBy(x => x).ToArray();
        s1.SequenceEqual(s2).Should().BeTrue();

        var e1 = map.Edges.Select(x => x.Key).OrderBy(x => x).ToArray();
        var e2 = mapRead.Edges.Select(x => x.Key).OrderBy(x => x).ToArray();
        e1.SequenceEqual(e2).Should().BeTrue();
    }

    [Fact]
    public void NodeSerialization()
    {
        new GraphNode("node1").Action(x =>
        {
            string json = x.ToJson();

            var graphNode = json.ToObject<GraphNode>();
            graphNode.Should().NotBeNull();
            graphNode!.Key.Should().Be("node1");
            graphNode.Tags.Should().NotBeNull();
        });

        new GraphNode("node1", tags: "t1;t2=v2").Action(x =>
        {
            string json = x.ToJson();

            var graphNode = json.ToObject<GraphNode>();
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
        new GraphEdge("node1", "node2").Action(x =>
        {
            string json = x.ToJson();

            var graphEdge = json.ToObject<GraphEdge>();
            graphEdge.Should().NotBeNull();
            graphEdge!.FromKey.Should().Be("node1");
            graphEdge!.ToKey.Should().Be("node2");
            graphEdge.Tags.Should().NotBeNull();
        });

        new GraphEdge("node1", "node2", tags: "t1;t2=v2").Action(x =>
        {
            string json = x.ToJson();

            var graphEdge = json.ToObject<GraphEdge>();
            graphEdge.Should().NotBeNull();
            graphEdge!.FromKey.Should().Be("node1");
            graphEdge!.ToKey.Should().Be("node2");
            graphEdge.Tags.Should().NotBeNull();
            graphEdge.Tags.Count.Should().Be(2);
            graphEdge.Tags["t1"].Should().BeNull();
            graphEdge.Tags["t2"].Should().Be("v2");
        });
    }
}
