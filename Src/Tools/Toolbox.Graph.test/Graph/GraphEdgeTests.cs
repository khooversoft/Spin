//using FluentAssertions;

//namespace Toolbox.Graph.test.Graph;

//public class GraphEdgeTests
//{
//    [Fact]
//    public void SimpleEqual()
//    {
//        string fromKey = "key1";
//        string toKey = "key2";

//        var n1 = new GraphEdge(fromKey, toKey);
//        n1.FromKey.Should().Be(fromKey);
//        n1.ToKey.Should().Be(toKey);
//        n1.Tags.Count.Should().Be(0);
//        n1.EdgeType.Should().Be("default");

//        var n2 = new GraphEdge(fromKey, toKey);
//        n2 = n2 with
//        {
//            Key = n1.Key,
//            CreatedDate = n1.CreatedDate
//        };

//        (n1 == n2).Should().BeTrue();
//    }

//    [Fact]
//    public void SimpleEqualWithTag()
//    {
//        string fromKey = "key1";
//        string toKey = "key2";
//        string tags = "t1";

//        var n1 = new GraphEdge(fromKey, toKey, tags: tags);
//        n1.FromKey.Should().Be(fromKey);
//        n1.ToKey.Should().Be(toKey);
//        n1.Tags.Count.Should().Be(1);
//        n1.Tags.ToString().Should().Be("t1");
//        n1.EdgeType.Should().Be("default");

//        var n2 = new GraphEdge(fromKey, toKey, tags: tags);
//        n2 = n2 with
//        {
//            Key = n1.Key,
//            CreatedDate = n1.CreatedDate
//        };

//        (n1 == n2).Should().BeTrue();
//    }

//    [Fact]
//    public void SimpleEqualWithTags()
//    {
//        string fromKey = "key1";
//        string toKey = "key2";
//        string tags = "t1, t2=v1";

//        var n1 = new GraphEdge(fromKey, toKey, tags: tags);
//        n1.FromKey.Should().Be(fromKey);
//        n1.ToKey.Should().Be(toKey);
//        n1.Tags.Count.Should().Be(2);
//        n1.Tags.ToString().Should().Be("t1,t2=v1");
//        n1.EdgeType.Should().Be("default");

//        var n2 = new GraphEdge(fromKey, toKey, tags: tags);
//        n2 = n2 with
//        {
//            Key = n1.Key,
//            CreatedDate = n1.CreatedDate
//        };

//        (n1 == n2).Should().BeTrue();
//    }

//    [Fact]
//    public void SimpleEqualWithTagsAndEdgeType()
//    {
//        string fromKey = "key1";
//        string toKey = "key2";
//        string tags = "t1, t2=v1";
//        string edgeType = "relationship";

//        var n1 = new GraphEdge(fromKey, toKey, edgeType: edgeType, tags: tags);
//        n1.FromKey.Should().Be(fromKey);
//        n1.ToKey.Should().Be(toKey);
//        n1.Tags.Count.Should().Be(2);
//        n1.Tags.ToString().Should().Be("t1,t2=v1");
//        n1.EdgeType.Should().Be(edgeType);

//        var n2 = new GraphEdge(fromKey, toKey, edgeType: edgeType, tags: tags);
//        n2 = n2 with
//        {
//            Key = n1.Key,
//            CreatedDate = n1.CreatedDate
//        };

//        (n1 == n2).Should().BeTrue();
//    }

//}
