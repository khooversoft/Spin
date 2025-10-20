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

    [Fact]
    public void CreatedDate_RoundTrip_Preserved()
    {
        var created = new DateTime(2024, 02, 03, 04, 05, 06, DateTimeKind.Utc);
        var edge = new GraphEdge("node1", "node2", "et", tags: "t1=v1", createdDate: created);

        string json = edge.ToJson();
        var read = json.ToObject<GraphEdge>().NotNull();

        (edge == read).BeTrue();
        read.CreatedDate.Be(created);
    }

    [Fact]
    public void Json_ShouldNotContain_ComputedProperties()
    {
        var edge = new GraphEdge("node1", "node2", "et", tags: "t1");

        string json = edge.ToJson();

        (json.Contains("\"Key\"")).BeFalse();
        (json.Contains("\"TagsString\"")).BeFalse();
    }

    [Fact]
    public void Tags_AreCaseInsensitive_AfterRoundTrip()
    {
        var edge = new GraphEdge("node1", "node2", "et", tags: "t1=v1");

        var read = edge.ToJson().ToObject<GraphEdge>().NotNull();

        read.Tags.TryGetValue("T1", out var v).BeTrue();
        v.Be("v1");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void NullOrEmptyTags_RoundTrip_ToEmptyDictionary(string? tags)
    {
        var edge = new GraphEdge("node1", "node2", "et", tags: tags);

        var read = edge.ToJson().ToObject<GraphEdge>().NotNull();

        read.Tags.Count.Be(0);
    }

    [Fact]
    public void Deserialize_DefaultCreatedDate_UsesUtcNow()
    {
        string json = """
        {"FromKey":"a","ToKey":"b","EdgeType":"et","Tags":{},"CreatedDate":"0001-01-01T00:00:00Z"}
        """;

        var read = json.ToObject<GraphEdge>().NotNull();

        (read.CreatedDate != default).BeTrue();
    }

    [Fact]
    public void DerivedProperties_AfterRoundTrip()
    {
        var edge = new GraphEdge("node1", "node2", "et", tags: "t2=v2,t1");

        var read = edge.ToJson().ToObject<GraphEdge>().NotNull();

        read.Key.Be("node1:node2:et");
        read.TagsString.Be("t1,t2=v2");
    }
}
