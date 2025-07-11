using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Graph.test.Graph.Map;

public class EdgeSerializationTests
{

    [Fact]
    public void GraphEdge()
    {
        var v = new GraphEdge("Node1", "Node2", "edgeType", "t1=v");

        string json = v.ToJson();
        json.NotEmpty();

        var v2 = json.ToObject<GraphEdge>();
        (v == v2).BeTrue();
    }

    [Fact]
    public void EdgeSerialization()
    {
        var edge = new GraphEdge("node1", "node2", "et");

        string json = edge.ToJson();

        var graphEdge = json.ToObject<GraphEdge>();
        graphEdge.NotNull();
        graphEdge!.FromKey.Be("node1");
        graphEdge!.ToKey.Be("node2");
        graphEdge.Tags.NotNull();
    }

    [Fact]
    public void EdgeSerializationWithTag()
    {
        var edge = new GraphEdge("node1", "node2", "et", tags: "t1,t2=v2");

        string json = edge.ToJson();

        var graphEdge = json.ToObject<GraphEdge>();
        graphEdge.NotNull();
        graphEdge!.FromKey.Be("node1");
        graphEdge!.ToKey.Be("node2");
        graphEdge.Tags.NotNull();
        graphEdge.Tags.Count.Be(2);
        graphEdge.Tags["t1"].BeNull();
        graphEdge.Tags["t2"].Be("v2");
    }
}
