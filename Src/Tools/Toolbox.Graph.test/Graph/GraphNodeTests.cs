using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Toolbox.Data;
using Toolbox.Extensions;

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

        var n2 = new GraphNode(key);
        n2 = n2 with { CreatedDate = n1.CreatedDate };

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

        var n2 = new GraphNode(key, tags);
        n2 = n2 with { CreatedDate = n1.CreatedDate };

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

        var n2 = new GraphNode(key, tags);
        n2 = n2 with { CreatedDate = n1.CreatedDate };

        (n1 == n2).Should().BeTrue();
    }
}
