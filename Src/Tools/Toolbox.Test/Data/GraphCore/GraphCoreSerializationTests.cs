using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Test.Data.Graph;

public class GraphCoreSerializationTests
{
    [Fact]
    public void SerializeNode()
    {
        var node = new Node("node1", "type1".ToDataETag());

        var json = node.ToJson();

        var read = json.ToObject<Node>();
        node.Be(read);
    }

    [Fact]
    public void SerializeEdge()
    {
        var edge = new Edge("fromKey", "toKey", "type", "payload".ToDataETag());

        var json = edge.ToJson();

        var read = json.ToObject<Edge>();
        edge.Be(read);
    }

    [Fact]
    public void GraphCoreSerialzation()
    {
        const int count = 10;

        var nodes = Enumerable.Range(0, count)
            .Select(i => new Node($"node-{i}", $"payload-{i}".ToDataETag()))
            .ToArray();

        var edges = Enumerable.Range(0, count - 1)
            .Select(i => new Edge($"node-{i}", $"node-{i + 1}", $"type-{i}", $"payload-{i}".ToDataETag()))
            .ToArray();

        var graph = new GraphCore(nodes, edges);

        var json = graph.ToJson();
        var read = json.ToObject<GraphCoreSerialization>().FromSerialization();
        graph.Be(read);
    }

    [Fact]
    public void EmptyGraphSerialization()
    {
        var graph = new GraphCore();

        var json = graph.ToJson();
        var read = json.ToObject<GraphCoreSerialization>().FromSerialization();

        graph.Be(read);
    }

    [Fact]
    public void GraphCoreSerializationOrderAgnostic()
    {
        var nodes = new[]
        {
            new Node("B"),
            new Node("A"),
            new Node("C"),
        };

        var edges = new[]
        {
            new Edge("B", "A", "type2"),
            new Edge("A", "C", "type1"),
        };

        var graph = new GraphCore(nodes, edges);

        var json = graph.ToJson();
        var read = json.ToObject<GraphCoreSerialization>().FromSerialization();

        graph.Be(read);
    }

    [Fact]
    public void SerializeNodeWithOptionalType()
    {
        var payload = "payload-data".ToDataETag();
        var node = new Node("node1", payload);

        var json = node.ToJson();

        var read = json.ToObject<Node>();
        node.Be(read);
        (node == read).BeTrue();
    }
}
