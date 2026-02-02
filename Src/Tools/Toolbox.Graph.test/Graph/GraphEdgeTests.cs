using Toolbox.Extensions;
using Toolbox.Graph;
using Toolbox.Tools;

namespace Toolbox.Graph.test.Graph;

public class GraphEdgeTests
{
    [Fact]
    public void SimpleEqual()
    {
        string fromKey = "key1";
        string toKey = "key2";

        var n1 = new GraphEdge(fromKey, toKey, "et");
        n1.FromKey.Be(fromKey);
        n1.ToKey.Be(toKey);
        n1.Tags.Count.Be(0);
        n1.EdgeType.Be("et");
        n1.Key.Be("key1:key2:et");
        n1.ToString().Be("{ FromKey=key1 -> ToKey=key2 (EdgeType) }".Replace("EdgeType", "et"));

        var n2 = new GraphEdge(fromKey, toKey, "et");
        n2 = SetCreated(n2, n1.CreatedDate);

        (n1 == n2).BeTrue();
        (n1.GetHashCode() == n2.GetHashCode()).BeTrue();
    }

    private GraphEdge SetCreated(GraphEdge edge, DateTime createdDate) =>
        new GraphEdge(edge.FromKey, edge.ToKey, edge.EdgeType, edge.Tags, createdDate);

    [Fact]
    public void SimpleEqualWithTag()
    {
        string fromKey = "key1";
        string toKey = "key2";
        string tags = "t1";

        var n1 = new GraphEdge(fromKey, toKey, "et", tags: tags);
        n1.FromKey.Be(fromKey);
        n1.ToKey.Be(toKey);
        n1.Tags.Count.Be(1);
        n1.Tags.ToTagsString().Be("t1");
        n1.EdgeType.Be("et");

        var n2 = new GraphEdge(fromKey, toKey, "et", tags: tags);
        n2 = SetCreated(n2, n1.CreatedDate);

        (n1 == n2).BeTrue();
        (n1.GetHashCode() == n2.GetHashCode()).BeTrue();
    }

    [Fact]
    public void SimpleEqualWithTags()
    {
        string fromKey = "key1";
        string toKey = "key2";
        string tags = "t1, t2=v1";

        var n1 = new GraphEdge(fromKey, toKey, "et", tags: tags);
        n1.FromKey.Be(fromKey);
        n1.ToKey.Be(toKey);
        n1.Tags.Count.Be(2);
        n1.Tags.ToTagsString().Be("t1,t2=v1");
        n1.EdgeType.Be("et");

        var n2 = new GraphEdge(fromKey, toKey, "et", tags: tags);
        n2 = SetCreated(n2, n1.CreatedDate);

        (n1 == n2).BeTrue();
        (n1.GetHashCode() == n2.GetHashCode()).BeTrue();
    }

    [Fact]
    public void SimpleEqualWithTagsAndEdgeType()
    {
        string fromKey = "key1";
        string toKey = "key2";
        string tags = "t1, t2=v1";
        string edgeType = "relationship";

        var n1 = new GraphEdge(fromKey, toKey, edgeType: edgeType, tags: tags);
        n1.FromKey.Be(fromKey);
        n1.ToKey.Be(toKey);
        n1.Tags.Count.Be(2);
        n1.Tags.ToTagsString().Be("t1,t2=v1");
        n1.EdgeType.Be(edgeType);

        var n2 = new GraphEdge(fromKey, toKey, edgeType: edgeType, tags: tags);
        n2 = SetCreated(n2, n1.CreatedDate);

        (n1 == n2).BeTrue();
        (n1.GetHashCode() == n2.GetHashCode()).BeTrue();
    }

    [Fact]
    public void CaseInsensitiveEquality_ForKeysAndEdgeType_And_TagKeyOrderIgnored()
    {
        var e1 = new GraphEdge("FromA", "ToB", "REL", tags: "t1=v1,t2");
        var e2 = new GraphEdge("froma", "tob", "rel", tags: "T2,t1=v1");

        e2 = SetCreated(e2, e1.CreatedDate);

        (e1 == e2).BeTrue();
        (e1.GetHashCode() == e2.GetHashCode()).BeTrue();
    }

    [Fact]
    public void Inequality_WhenCreatedDateDiffers()
    {
        var n1 = new GraphEdge("k1", "k2", "et", tags: "t1");
        var n2 = new GraphEdge("k1", "k2", "et", tags: "t1");

        // Different created dates by default
        (n1 == n2).BeFalse();

        // Now force same created date and they should be equal
        n2 = SetCreated(n2, n1.CreatedDate);
        (n1 == n2).BeTrue();

        // Move by 1 second to make unequal again
        var n3 = SetCreated(n2, n1.CreatedDate.AddSeconds(1));
        (n1 == n3).BeFalse();
    }

    [Fact]
    public void Inequality_WhenFromToOrEdgeTypeDiffers()
    {
        var baseEdge = new GraphEdge("f", "t", "e", tags: "t1=v1,t2");
        var sameCreated = baseEdge.CreatedDate;

        var diffFrom = new GraphEdge("F2", "t", "e", tags: "t1=v1,t2");
        diffFrom = SetCreated(diffFrom, sameCreated);
        (baseEdge == diffFrom).BeFalse();

        var diffTo = new GraphEdge("f", "T2", "e", tags: "t1=v1,t2");
        diffTo = SetCreated(diffTo, sameCreated);
        (baseEdge == diffTo).BeFalse();

        var diffEdgeType = new GraphEdge("f", "t", "E2", tags: "t1=v1,t2");
        diffEdgeType = SetCreated(diffEdgeType, sameCreated);
        (baseEdge == diffEdgeType).BeFalse();
    }

    [Fact]
    public void Inequality_WhenTagValuesDiffer_IncludingCaseSensitivity()
    {
        var e1 = new GraphEdge("f", "t", "e", tags: "t1=v1,t2");
        var e2 = new GraphEdge("f", "t", "e", tags: "t1=V1,t2"); // value differs by case

        e2 = SetCreated(e2, e1.CreatedDate);

        (e1 == e2).BeFalse();
    }

    [Fact]
    public void JsonCtor_RoundTrip_WithExplicitCreatedDate()
    {
        var created = DateTime.UtcNow;

        var tagsDict = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
        {
            ["t1"] = null,
            ["t2"] = "v2",
        };

        var e = new GraphEdge("k1", "k2", "et", tagsDict, created);
        e.CreatedDate.Be(created);
        e.Tags.Count.Be(2);
        e.Tags.ToTagsString().Be("t1,t2=v2");
        e.Key.Be("k1:k2:et");

        var json = e.ToJson();
        var read = json.ToObject<GraphEdge>().NotNull();

        (e == read).BeTrue();
        (e.GetHashCode() == read.GetHashCode()).BeTrue();
    }

    [Fact]
    public void SerializationGraphEdge()
    {
        string fromKey = "key1";
        string toKey = "key2";
        string tags = "t1, t2=v1";
        string edgeType = "relationship";

        var edge = new GraphEdge(fromKey, toKey, edgeType: edgeType, tags: tags);

        string json = edge.ToJson();

        GraphEdge read = json.ToObject<GraphEdge>().NotNull();
        (edge == read).BeTrue();
    }

    [Fact]
    public void HashCode_EqualEdges_SameHash_OrderInsensitiveTags()
    {
        var e1 = new GraphEdge("a", "b", "et", tags: "t1=v1,t2=v2");
        var e2 = new GraphEdge("A", "B", "ET", tags: "t2=v2,t1=v1");

        e2 = SetCreated(e2, e1.CreatedDate);

        (e1 == e2).BeTrue();
        (e1.GetHashCode() == e2.GetHashCode()).BeTrue();
    }

    [Fact]
    public void Validator_Ok_OnValid()
    {
        var e = new GraphEdge("from", "to", "type", tags: "t1");
        e.Validate().BeOk();
    }

    [Fact]
    public void Ctor_Throws_On_EmptyInputs_And_FromEqualsTo()
    {
        // Empty from
        Assert.Throws<ArgumentNullException>(() => new GraphEdge("", "to", "type"));

        // Empty to
        Assert.Throws<ArgumentNullException>(() => new GraphEdge("from", "", "type"));

        // Empty edgeType
        Assert.Throws<ArgumentNullException>(() => new GraphEdge("from", "to", ""));

        // From equals To (case-insensitive)
        Assert.Throws<ArgumentException>(() => new GraphEdge("Same", "same", "type"));
    }
}
