using FluentAssertions;
using Toolbox.Extensions;

namespace Toolbox.Graph.test.Graph.Map;

public class EdgeSerializationTests
{

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
    public void EdgeSerialization()
    {
        var edge = new GraphEdge("node1", "node2");

        string json = edge.ToJson();

        var graphEdge = json.ToObject<GraphEdge>();
        graphEdge.Should().NotBeNull();
        graphEdge!.FromKey.Should().Be("node1");
        graphEdge!.ToKey.Should().Be("node2");
        graphEdge.Tags.Should().NotBeNull();
    }

    [Fact]
    public void EdgeSerializationWithTag()
    {
        var edge = new GraphEdge("node1", "node2", tags: "t1,t2=v2");

        string json = edge.ToJson();

        var graphEdge = json.ToObject<GraphEdge>();
        graphEdge.Should().NotBeNull();
        graphEdge!.FromKey.Should().Be("node1");
        graphEdge!.ToKey.Should().Be("node2");
        graphEdge.Tags.Should().NotBeNull();
        graphEdge.Tags.Count.Should().Be(2);
        graphEdge.Tags["t1"].Should().BeNull();
        graphEdge.Tags["t2"].Should().Be("v2");
    }
}
