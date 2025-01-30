using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Tools.Should;

namespace Toolbox.Graph.test.Graph.Map;

public class NodeSerializationTests
{
    [Fact]
    public void GraphNode()
    {
        var v = new GraphNode("Node1", "t1=v");

        string json = v.ToJson();
        json.Should().NotBeEmpty();

        var v2 = json.ToObject<GraphNode>();
        (v == v2).Should().BeTrue();
    }

    [Fact]
    public void NodeSerialization()
    {
        var node = new GraphNode("node1");

        string json = node.ToJson();

        var graphNode = json.ToObject<GraphNode>();
        graphNode.NotNull();
        graphNode!.Key.Should().Be("node1");
        graphNode.Tags.NotNull();
    }

    [Fact]
    public void NodeWithTags()
    {
        var node = new GraphNode("node1", tags: "t1,t2=v2");

        string json = node.ToJson();

        var graphNode = json.ToObject<GraphNode>();
        graphNode.NotNull();
        graphNode!.Key.Should().Be("node1");
        graphNode.Tags.NotNull();
        graphNode.Tags.Count.Should().Be(2);
        graphNode.Tags["t1"].BeNull();
        graphNode.Tags["t2"].Should().Be("v2");
    }

    [Fact]
    public void NodeWithIndex()
    {
        var node = new GraphNode("node1", indexes: "i1,t2");

        string json = node.ToJson();

        var graphNode = json.ToObject<GraphNode>();
        graphNode.NotNull();
        graphNode!.Key.Should().Be("node1");
        graphNode.Tags.NotNull();
        graphNode.Tags.Count.Should().Be(0);
        graphNode.Indexes.Count.Should().Be(2);
        graphNode.Indexes.Contains("i1").Should().BeTrue();
        graphNode.Indexes.Contains("t2").Should().BeTrue();
    }

    [Fact]
    public void NodeWithTagsAndIndex()
    {
        var node = new GraphNode("node1", tags: "t1,t2=v2", indexes: "i1,i2");

        string json = node.ToJson();

        var graphNode = json.ToObject<GraphNode>();
        graphNode.NotNull();
        graphNode!.Key.Should().Be("node1");

        graphNode.Tags.NotNull();
        graphNode.Tags.Count.Should().Be(2);
        graphNode.Tags["t1"].Should().Be(null);
        graphNode.Tags["t2"].Should().Be("v2");

        graphNode.Indexes.NotNull();
        graphNode.Indexes.Count.Should().Be(2);
        graphNode.Indexes.Contains("i1").Should().BeTrue();
        graphNode.Indexes.Contains("i2").Should().BeTrue();
    }
}
