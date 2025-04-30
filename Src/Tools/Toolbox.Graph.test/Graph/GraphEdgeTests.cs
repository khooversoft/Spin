using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

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

        var n2 = new GraphEdge(fromKey, toKey, "et");
        n2 = Set(n2, n1.CreatedDate);

        (n1 == n2).BeTrue();
    }

    private GraphEdge Set(GraphEdge edge, DateTime createdDate) => new GraphEdge(edge.FromKey, edge.ToKey, edge.EdgeType, edge.Tags, createdDate);

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
        n2 = Set(n2, n1.CreatedDate);

        (n1 == n2).BeTrue();
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
        n2 = Set(n2, n1.CreatedDate);

        (n1 == n2).BeTrue();
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
        n2 = Set(n2, n1.CreatedDate);

        (n1 == n2).BeTrue();
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
}
