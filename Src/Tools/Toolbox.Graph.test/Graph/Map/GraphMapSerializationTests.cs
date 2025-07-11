using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Graph.test.Graph.Map;

public class GraphMapSerializationTests
{
    [Fact]
    public void EmptyMapString()
    {
        var map = new GraphMap();

        var json = map.ToJson();

        var mapResult = GraphMapTool.FromJson(json).NotNull();
        mapResult.NotNull();
        mapResult.Count().Be(0);
        mapResult.Nodes.Count.Be(0);
        mapResult.Edges.Count.Be(0);
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

            var mapResult = GraphMapTool.FromJson(json).NotNull();
            mapResult.NotNull();
            mapResult.Count().Be(1);
            mapResult.Edges.Count.Be(0);

            mapResult.Nodes.Count.Be(1);
            mapResult.Nodes.First().Action(y =>
            {
                y.Key.Be("Node1");
                y.Tags.NotNull();
                y.Tags.Count.Be(0);
            });
        });

        new GraphMap()
        {
            new GraphNode("Node1"),
        }.Action(x =>
        {
            var json = x.ToJson();

            var mapResult = GraphMapTool.FromJson(json).NotNull();
            mapResult.NotNull();
            mapResult.Count().Be(1);
            mapResult.Edges.Count.Be(0);

            mapResult.Nodes.Count.Be(1);
            mapResult.Nodes.First().Action(y =>
            {
                y.Key.Be("Node1");
                y.Tags.NotNull();
                y.Tags.Count.Be(0);
            });
        });

        new GraphMap()
        {
            new GraphNode("Node1", tags: "t1,t2=v2"),
        }.Action(x =>
        {
            var json = x.ToJson();

            var mapResult = GraphMapTool.FromJson(json).NotNull();
            mapResult.NotNull();
            mapResult.Count().Be(1);
            mapResult.Edges.Count.Be(0);

            mapResult.Nodes.Count.Be(1);
            mapResult.Nodes.First().Action(y =>
            {
                y.Key.Be("Node1");
                y.Tags.NotNull();
                y.Tags.Count.Be(2);
                y.Tags["t1"].BeNull();
                y.Tags["t2"].Be("v2");
            });
        });

        new GraphMap()
        {
            new GraphNode("Node1", tags: "t1,t2=v2"),
        }.Action(x =>
        {
            var json = x.ToJson();

            var mapResult = GraphMapTool.FromJson(json).NotNull();
            mapResult.NotNull();
            mapResult.Count().Be(1);
            mapResult.Edges.Count.Be(0);

            mapResult.Nodes.Count.Be(1);
            mapResult.Nodes.First().Action(y =>
            {
                y.Key.Be("Node1");
                y.Tags.NotNull();
                y.Tags.Count.Be(2);
                y.Tags["t1"].BeNull();
                y.Tags["t2"].Be("v2");
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

            new GraphEdge("node1", "node2", "et"),
            new GraphEdge("node1", "node3", "et"),
            new GraphEdge("node1", "node4", "et"),
        };

        string json = map.ToJson();

        var mapRead = GraphMapTool.FromJson(json).NotNull();
        mapRead.Nodes.Count.Be(4);
        mapRead.Edges.Count.Be(3);

        var s1 = map.Nodes.Select(x => x.Key).OrderBy(x => x).ToArray();
        var s2 = mapRead.Nodes.Select(x => x.Key).OrderBy(x => x).ToArray();
        s1.SequenceEqual(s2).BeTrue();

        var e1 = map.Edges.Select(x => x.ToString()).OrderBy(x => x).ToArray();
        var e2 = mapRead.Edges.Select(x => x.ToString()).OrderBy(x => x).ToArray();
        e1.SequenceEqual(e2).BeTrue();
    }
}
