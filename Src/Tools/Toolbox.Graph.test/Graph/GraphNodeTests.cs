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

    [Fact]
    public void Equality_IgnoresOrder_And_IsCaseInsensitive_ForTagsIndexesAndForeignKeys()
    {
        var created = DateTime.UtcNow;

        var tags1 = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
        {
            ["t1"] = null,
            ["T2"] = "v2",
        };

        var tags2 = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
        {
            ["t2"] = "v2",
            ["T1"] = null,
        };

        var fk1 = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
        {
            ["fK1"] = "v1",
            ["fk2"] = null,
        };

        var fk2 = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
        {
            ["FK2"] = null,
            ["FK1"] = "v1",
        };

        var n1 = new GraphNode("k",
            tags: tags1,
            createdDate: created,
            dataMap: _emptyData,
            indexes: new[] { "A", "b" },
            foreignKeys: fk1);

        var n2 = new GraphNode("k",
            tags: tags2,
            createdDate: created,
            dataMap: _emptyData,
            indexes: new[] { "b", "a" },
            foreignKeys: fk2);

        (n1 == n2).BeTrue();
        n1.Indexes.Count.Be(2);
        n1.Indexes.Contains("a").BeTrue();
        n1.Indexes.Contains("A").BeTrue();
    }

    [Fact]
    public void Inequality_WhenCreatedDateDiffers()
    {
        var n1 = new GraphNode("k", "t1".ToTags(), createdDate: DateTime.UtcNow, dataMap: _emptyData, indexes: FrozenSet<string>.Empty, foreignKeys: _emptyTags);
        var n2 = new GraphNode("k", "t1".ToTags(), createdDate: n1.CreatedDate.AddSeconds(1), dataMap: _emptyData, indexes: FrozenSet<string>.Empty, foreignKeys: _emptyTags);

        (n1 == n2).BeFalse();
    }

    [Fact]
    public void Inequality_WhenKeyDiffers()
    {
        var created = DateTime.UtcNow;
        var n1 = new GraphNode("k1", "t1".ToTags(), created, _emptyData, FrozenSet<string>.Empty, _emptyTags);
        var n2 = new GraphNode("k2", "t1".ToTags(), created, _emptyData, FrozenSet<string>.Empty, _emptyTags);

        (n1 == n2).BeFalse();
    }

    [Fact]
    public void Inequality_WhenTagsDiffer()
    {
        var created = DateTime.UtcNow;
        var n1 = new GraphNode("k", "t1,t2=v2".ToTags(), created, _emptyData, FrozenSet<string>.Empty, _emptyTags);
        var n2 = new GraphNode("k", "t1".ToTags(), created, _emptyData, FrozenSet<string>.Empty, _emptyTags);

        (n1 == n2).BeFalse();
    }

    [Fact]
    public void Inequality_WhenDataMapDiffers()
    {
        var created = DateTime.UtcNow;

        var data1 = new Dictionary<string, GraphLink>(StringComparer.OrdinalIgnoreCase)
        {
            ["doc"] = new GraphLink { NodeKey = "N1", Name = "readme", FileId = "/a/readme.md" },
        };

        var data2 = new Dictionary<string, GraphLink>(StringComparer.OrdinalIgnoreCase)
        {
            ["doc"] = new GraphLink { NodeKey = "N1", Name = "readme", FileId = "/a/readme.md" },
            ["img"] = new GraphLink { NodeKey = "N1", Name = "logo", FileId = "/a/logo.png" },
        };

        var n1 = new GraphNode("k", "t1".ToTags(), created, data1, FrozenSet<string>.Empty, _emptyTags);
        var n2 = new GraphNode("k", "t1".ToTags(), created, data2, FrozenSet<string>.Empty, _emptyTags);

        (n1 == n2).BeFalse();
    }

    [Fact]
    public void Equality_DataMap_IgnoresOrder_And_KeyCase()
    {
        var created = DateTime.UtcNow;

        var data1 = new Dictionary<string, GraphLink>(StringComparer.OrdinalIgnoreCase)
        {
            ["doc"] = new GraphLink { NodeKey = "N1", Name = "readme", FileId = "/a/readme.md" },
            ["img"] = new GraphLink { NodeKey = "N1", Name = "logo", FileId = "/a/logo.png" },
        };

        var data2 = new Dictionary<string, GraphLink>(StringComparer.OrdinalIgnoreCase)
        {
            ["IMG"] = new GraphLink { NodeKey = "N1", Name = "logo", FileId = "/a/logo.png" },
            ["DOC"] = new GraphLink { NodeKey = "N1", Name = "readme", FileId = "/a/readme.md" },
        };

        var n1 = new GraphNode("k", "t1".ToTags(), created, data1, new[] { "idx1" }, "fk=v".ToTags());
        var n2 = new GraphNode("k", "t1".ToTags(), created, data2, new[] { "idx1" }, "fk=v".ToTags());

        (n1 == n2).BeTrue();
    }

    [Fact]
    public void StringCtor_ParsesIndexes_Dedupes_And_Trims()
    {
        var n = new GraphNode("k", tags: "t1, t2=v2", indexes: "A, a,  b  ,B", foreignKeys: "fk1=v1, fk2");

        // Tags normalized and parsed
        n.Tags.Count.Be(2);
        n.Tags.ToTagsString().Be("t1,t2=v2");

        // Indexes are case-insensitive set with de-duplication and trimming
        n.Indexes.Count.Be(2);
        n.Indexes.Contains("a").BeTrue();
        n.Indexes.Contains("b").BeTrue();

        // Foreign keys parsed like tags
        n.ForeignKeys.Count.Be(2);
        n.ForeignKeys.ToTagsString().Be("fk1=v1,fk2");
    }

    [Fact]
    public void Serialization_AllFields_RoundTrip()
    {
        var created = DateTime.UtcNow;

        var node = new GraphNode(
            key: "node1",
            tags: new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
            {
                ["t1"] = null,
                ["t2"] = "v2",
            },
            createdDate: created,
            dataMap: new Dictionary<string, GraphLink>(StringComparer.OrdinalIgnoreCase)
            {
                ["doc"] = new GraphLink { NodeKey = "n:1", Name = "readme", FileId = "/a/readme.md" },
            },
            indexes: new[] { "a", "b" },
            foreignKeys: new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
            {
                ["fk1"] = "v1",
            });

        string json = node.ToJson();
        var read = json.ToObject<GraphNode>().NotNull();

        (node == read).BeTrue();
        node.GetHashCode().Be(read.GetHashCode());
    }

    [Fact]
    public void HashCode_EqualNodes_SameHash()
    {
        var created = DateTime.UtcNow;

        var n1 = new GraphNode("k",
            tags: "t1,t2=v2".ToTags(),
            createdDate: created,
            dataMap: new Dictionary<string, GraphLink> { ["doc"] = new GraphLink { NodeKey = "N1", Name = "readme", FileId = "/a/readme.md" } },
            indexes: new[] { "x", "y" },
            foreignKeys: "fk=v".ToTags());

        var n2 = new GraphNode("k",
            tags: "t2=v2,t1".ToTags(),
            createdDate: created,
            dataMap: new Dictionary<string, GraphLink> { ["DOC"] = new GraphLink { NodeKey = "N1", Name = "readme", FileId = "/a/readme.md" } },
            indexes: new[] { "y", "x" },
            foreignKeys: "fk=v".ToTags());

        (n1 == n2).BeTrue();
        (n1.GetHashCode() == n2.GetHashCode()).BeTrue();
    }

    [Fact]
    public void Validator_Ok_OnMinimalValid()
    {
        var n = new GraphNode("k", tags: "t1");
        n.Validate().IsOk().BeTrue();
    }

    [Fact]
    public void Validator_Fails_OnEmptyKey_And_EmptyTagKey()
    {
        // Empty key should fail
        var invalidKey = new GraphNode("", tags: "t1");
        invalidKey.Validate().IsOk().BeFalse();

        // Empty tag key (allowed by JSON ctor) should fail validator
        var created = DateTime.UtcNow;
        var tags = new Dictionary<string, string?> { [""] = null };
        var n = new GraphNode("k", tags, created, _emptyData, FrozenSet<string>.Empty, _emptyTags);
        n.Validate().IsOk().BeFalse();
    }
}
