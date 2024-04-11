using System.Collections.Immutable;
using FluentAssertions;

namespace Toolbox.Graph.test.Graph;

public class GraphNodeTests
{
    [Fact]
    public void SimpleEqual()
    {
        string key = "key1";

        var n1 = new GraphNode(key);
        n1.Key.Should().Be(key);
        n1.Tags.Count.Should().Be(0);
        n1.Links.Length.Should().Be(0);

        var n2 = new GraphNode(key, "", createdDate: n1.CreatedDate, []);

        (n1 == n2).Should().BeTrue();
    }

    [Fact]
    public void SimpleEqualWithTag()
    {
        string key = "key1";
        string tags = "t1";

        var n1 = new GraphNode(key, tags);
        n1.Key.Should().Be(key);
        n1.Tags.Count.Should().Be(1);
        n1.Tags.ToString().Should().Be("t1");
        n1.Links.Length.Should().Be(0);

        var n2 = new GraphNode(key, tags, n1.CreatedDate, []);

        (n1 == n2).Should().BeTrue();
    }

    [Fact]
    public void SimpleEqualWithTags()
    {
        string key = "key2";
        string tags = "t1, t2=v2";

        var n1 = new GraphNode(key, tags);
        n1.Key.Should().Be(key);
        n1.Tags.Count.Should().Be(2);
        n1.Tags.ToString().Should().Be("t1,t2=v2");
        n1.Links.Length.Should().Be(0);

        var n2 = new GraphNode(key, tags, n1.CreatedDate, []);

        (n1 == n2).Should().BeTrue();
    }

    [Fact]
    public void SimpleEqualWithTagsAndLinks()
    {
        string key = "key2";
        string tags = "t1, t2=v2";
        string[] links = ["link1", "link2"];

        var n1 = new GraphNode(key, tags, links);
        n1.Key.Should().Be(key);
        n1.Tags.Count.Should().Be(2);
        n1.Tags.ToString().Should().Be("t1,t2=v2");
        n1.Links.Length.Should().Be(2);

        var n2 = new GraphNode(key, tags, n1.CreatedDate, links.ToImmutableArray());

        (n1 == n2).Should().BeTrue();
    }
}
