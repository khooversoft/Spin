using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Graph;
using Toolbox.Tools;

namespace Toolbox.Graph.test.Graph.Map;

public class GraphMapNodeTests
{
    [Fact]
    public void EmptyNode()
    {
        var map = new GraphMap();
    }

    [Fact]
    public void Node()
    {
        var e1 = new GraphNode("n1");
        e1.NotNull();
        e1.Key.Be("n1");

        string json = e1.ToJson();
        var r1 = json.ToObject<GraphNode>();
        (e1 == r1).BeTrue();
    }

    [Fact]
    public void OneNode()
    {
        var map = new GraphMap()
        {
            new GraphNode("Node1"),
        };

        map.Nodes.Count.Be(1);
        map.Nodes.ContainsKey("Node1").BeTrue();
        map.Edges.Count.Be(0);
    }

    [Fact]
    public void TwoNodes()
    {
        var map = new GraphMap()
        {
            new GraphNode("Node1"),
            new GraphNode("Node2"),
        };

        map.Nodes.Count.Be(2);
        map.Nodes.ContainsKey("Node1").BeTrue();
        map.Nodes.ContainsKey("Node2").BeTrue();
        map.Edges.Count.Be(0);
    }

    [Fact]
    public void Scale()
    {
        const int count = 100;
        const string sampleKey = "Node_10";
        var map = new GraphMap();

        Enumerable.Range(0, count).ForEach(x => map.Add(new GraphNode($"Node_{x}")));
        map.Nodes.Count.Be(count);
        map.Edges.Count.Be(0);

        GraphNode node = map.Nodes[sampleKey];
        node.NotNull();
        node.Key.Be(sampleKey);
        map.Nodes.TryGetValue(sampleKey, out var _).BeTrue();

        map.Nodes.Remove(sampleKey);
        map.Nodes.TryGetValue(sampleKey, out var _).BeFalse();
        map.Nodes.Count.Be(count - 1);
        map.Edges.Count.Be(0);

        Verify.Throws<KeyNotFoundException>(() =>
        {
            GraphNode node = map.Nodes[sampleKey];
        });
    }

    [Fact]
    public void TwoNodesSameKeyShouldFail()
    {
        GraphMap map = null!;

        Verify.Throws<ArgumentException>(() => map = new GraphMap()
        {
            new GraphNode("Node1"),
            new GraphNode("Node1"),
        });

        Verify.Throws<ArgumentException>(() => map = new GraphMap()
        {
            new GraphNode("Node1"),
            new GraphNode("node1"),
        });
    }
}
