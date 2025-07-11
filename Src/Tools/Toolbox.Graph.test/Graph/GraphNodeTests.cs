using System.Collections.Frozen;
using Toolbox.Extensions;
using Toolbox.Graph;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph.test.Graph;

public class GraphNodeTests
{
    private static IReadOnlyDictionary<string, string?> _emptyTags = FrozenDictionary<string, string?>.Empty;
    private static IReadOnlyDictionary<string, GraphLink> _emptyData = FrozenDictionary<string, GraphLink>.Empty;

    [Fact]
    public void SimpleEqual()
    {
        string key = "key1";

        var n1 = new GraphNode(key);
        n1.Key.Be(key);
        n1.Tags.Count.Be(0);

        var n2 = new GraphNode(key, _emptyTags, n1.CreatedDate, _emptyData, FrozenSet<string>.Empty, _emptyTags);

        (n1 == n2).BeTrue();
    }

    [Fact]
    public void SimpleEqualWithTag()
    {
        string key = "key1";
        string tags = "t1";
        var tagsDict = "t1".ToTags();

        var n1 = new GraphNode(key, tags);
        n1.Key.Be(key);
        n1.Tags.Count.Be(1);
        n1.Tags.ToTagsString().Be("t1");

        var n2 = new GraphNode(key, tagsDict, n1.CreatedDate, _emptyData, FrozenSet<string>.Empty, _emptyTags);

        (n1 == n2).BeTrue();
    }

    [Fact]
    public void SimpleEqualWithTags()
    {
        string key = "key2";
        string tags = "t1, t2=v2";
        var tagsDict = tags.ToTags();

        var n1 = new GraphNode(key, tags);
        n1.Key.Be(key);
        n1.Tags.Count.Be(2);
        n1.Tags.ToTagsString().Be("t1,t2=v2");

        var n2 = new GraphNode(key, tagsDict, n1.CreatedDate, _emptyData, FrozenSet<string>.Empty, _emptyTags);

        (n1 == n2).BeTrue();
    }

    [Fact]
    public void SimpleEqualWithTagsAndLinks()
    {
        string key = "key2";
        string tags = "t1, t2=v2";

        var n1 = new GraphNode(key, tags.ToTags(), DateTime.UtcNow, _emptyData, FrozenSet<string>.Empty, _emptyTags);
        n1.Key.Be(key);
        n1.Tags.Count.Be(2);
        n1.Tags.ToTagsString().Be("t1,t2=v2");

        var n2 = new GraphNode(key, tags.ToTags(), n1.CreatedDate, _emptyData, FrozenSet<string>.Empty, _emptyTags);

        (n1 == n2).BeTrue();
    }

    [Fact]
    public void StandardConstruction()
    {
        new GraphNode("node1").Action(x =>
        {
            string json = x.ToJson();

            var graphNode = json.ToObject<GraphNode>();
            graphNode.NotNull();
            graphNode!.Key.Be("node1");
            graphNode.Tags.NotNull();
        });

        new GraphNode("node1", tags: "t1,t2=v2").Action(x =>
        {
            string json = x.ToJson();

            var graphNode = json.ToObject<GraphNode>();
            graphNode.NotNull();
            graphNode!.Key.Be("node1");
            graphNode.Tags.NotNull();
            graphNode.Tags.Count.Be(2);
            graphNode.Tags["t1"].BeNull();
            graphNode.Tags["t2"].Be("v2");
        });
    }

    [Fact]
    public void SerializationGraphNode()
    {
        var node = new GraphNode("node1", tags: "t1,t2=v2");

        string json = node.ToJson();

        GraphNode read = json.ToObject<GraphNode>().NotNull();
        (node == read).BeTrue();
    }

    [Fact]
    public void SerializationGraphNode2()
    {
        var node = new GraphNode(
            "node1",
            tags: "t1,t2=v2".ToTags(),
            createdDate: DateTime.UtcNow,
            dataMap: _emptyData,
            indexes: FrozenSet<string>.Empty,
            foreignKeys: _emptyTags
            );

        string json = node.ToJson();

        GraphNode read = json.ToObject<GraphNode>().NotNull();
        (node == read).BeTrue();
    }

    [Fact]
    public void SerializationGraphNodeWithIndex()
    {
        var node = new GraphNode(
            "node1",
            tags: "t1,t2=v2".ToTags(),
            createdDate: DateTime.UtcNow,
            dataMap: _emptyData,
            indexes: ["t1"],
            foreignKeys: _emptyTags
            );

        string json = node.ToJson();

        GraphNode read = json.ToObject<GraphNode>().NotNull();
        (node == read).BeTrue();
    }

    [Fact]
    public void SerializationGraphNodeWithForeignKeys()
    {
        var node = new GraphNode(
            "node1",
            tags: "t1,t2=v2".ToTags(),
            createdDate: DateTime.UtcNow,
            dataMap: _emptyData,
            indexes: FrozenSet<string>.Empty,
            foreignKeys: "t1".ToTags()
            );

        string json = node.ToJson();

        GraphNode read = json.ToObject<GraphNode>().NotNull();
        (node == read).BeTrue();
    }
}
