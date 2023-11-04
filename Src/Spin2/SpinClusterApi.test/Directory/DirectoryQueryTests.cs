using FluentAssertions;
using SpinCluster.sdk.Actors.Directory;
using Toolbox.Data;

namespace SpinClusterApi.test.Directory;

public class DirectoryQueryTests
{
    [Fact]
    public void DirectoryForEdgeQueryForNode()
    {
        var list = new[]
        {
            new GraphNode("node1", "t1"),
            new GraphNode("node2", "t2"),
        };

        var query = new DirectoryQuery { NodeKey = "node1" };
        var result = list.Where(x => query.IsMatch(x)).ToArray();
        result.Length.Should().Be(1);
        (result[0] == list[0]).Should().BeTrue();

        query = new DirectoryQuery { NodeKey = "node2" };
        result = list.Where(x => query.IsMatch(x)).ToArray();
        result.Length.Should().Be(1);
        (result[0] == list[1]).Should().BeTrue();

        query = new DirectoryQuery { NodeKey = "*2" };
        result = list.Where(x => query.IsMatch(x)).ToArray();
        result.Length.Should().Be(1);
        (result[0] == list[1]).Should().BeTrue();

        query = new DirectoryQuery { NodeKey = "node*" };
        result = list.Where(x => query.IsMatch(x)).ToArray();
        result.Length.Should().Be(2);
        (result[0] == list[0]).Should().BeTrue();
        (result[1] == list[1]).Should().BeTrue();

        query = new DirectoryQuery { NodeKey = "node3" };
        result = list.Where(x => query.IsMatch(x)).ToArray();
        result.Length.Should().Be(0);

        query = new DirectoryQuery { NodeTags = "t1" };
        result = list.Where(x => query.IsMatch(x)).ToArray();
        result.Length.Should().Be(1);
        (result[0] == list[0]).Should().BeTrue();

        query = new DirectoryQuery { NodeTags = "t2" };
        result = list.Where(x => query.IsMatch(x)).ToArray();
        result.Length.Should().Be(1);
        (result[0] == list[1]).Should().BeTrue();

        query = new DirectoryQuery { NodeTags = "t3" };
        result = list.Where(x => query.IsMatch(x)).ToArray();
        result.Length.Should().Be(0);
    }

    [Fact]
    public void DirectoryForEdgeQueryForFromKey()
    {
        var list = new[]
        {
            new GraphEdge("fromKey1", "toKey1", "edgeType:key1", "t1"),
            new GraphEdge("fromKey2", "toKey2", "edgeType:key2", "t2"),
        };

        var query = new DirectoryQuery { FromKey = "fromKey1" };
        var result = list.Where(x => query.IsMatch(x)).ToArray();
        result.Length.Should().Be(1);
        (result[0] == list[0]).Should().BeTrue();

        query = new DirectoryQuery { FromKey = "fromKey2" };
        result = list.Where(x => query.IsMatch(x)).ToArray();
        result.Length.Should().Be(1);
        (result[0] == list[1]).Should().BeTrue();

        query = new DirectoryQuery { FromKey = "fromKey3" };
        result = list.Where(x => query.IsMatch(x)).ToArray();
        result.Length.Should().Be(0);
    }

    [Fact]
    public void DirectoryForEdgeQueryForToKey()
    {
        var list = new[]
        {
            new GraphEdge("fromKey1", "toKey1", "edgeType:key1", "t1"),
            new GraphEdge("fromKey2", "toKey2", "edgeType:key2", "t2"),
        };

        var query = new DirectoryQuery { ToKey = "toKey1" };
        var result = list.Where(x => query.IsMatch(x)).ToArray();
        result.Length.Should().Be(1);
        (result[0] == list[0]).Should().BeTrue();

        query = new DirectoryQuery { ToKey = "toKey2" };
        result = list.Where(x => query.IsMatch(x)).ToArray();
        result.Length.Should().Be(1);
        (result[0] == list[1]).Should().BeTrue();

        query = new DirectoryQuery { ToKey = "*2" };
        result = list.Where(x => query.IsMatch(x)).ToArray();
        result.Length.Should().Be(1);
        (result[0] == list[1]).Should().BeTrue();

        query = new DirectoryQuery { ToKey = "*2" };
        result = list.Where(x => query.IsMatch(x)).ToArray();
        result.Length.Should().Be(1);
        (result[0] == list[1]).Should().BeTrue();

        query = new DirectoryQuery { ToKey = "toKey*" };
        result = list.Where(x => query.IsMatch(x)).ToArray();
        result.Length.Should().Be(2);
        (result[0] == list[0]).Should().BeTrue();
        (result[1] == list[1]).Should().BeTrue();

        query = new DirectoryQuery { ToKey = "toKey3" };
        result = list.Where(x => query.IsMatch(x)).ToArray();
        result.Length.Should().Be(0);
    }

    [Fact]
    public void DirectoryForEdgeQueryForEdgeType()
    {
        var list = new[]
        {
            new GraphEdge("fromKey1", "toKey1", "edgeType:key1", "t1"),
            new GraphEdge("fromKey2", "toKey2", "edgeType:key2", "t2"),
        };

        var query = new DirectoryQuery { EdgeType = "edgeType:key1" };
        var result = list.Where(x => query.IsMatch(x)).ToArray();
        result.Length.Should().Be(1);
        (result[0] == list[0]).Should().BeTrue();

        query = new DirectoryQuery { EdgeType = "edgeType:key2" };
        result = list.Where(x => query.IsMatch(x)).ToArray();
        result.Length.Should().Be(1);
        (result[0] == list[1]).Should().BeTrue();

        query = new DirectoryQuery { EdgeType = "edgeType:key3" };
        result = list.Where(x => query.IsMatch(x)).ToArray();
        result.Length.Should().Be(0);

        query = new DirectoryQuery { EdgeType = "edgeType:*" };
        result = list.Where(x => query.IsMatch(x)).ToArray();
        result.Length.Should().Be(2);
        (result[0] == list[0]).Should().BeTrue();
        (result[1] == list[1]).Should().BeTrue();

        query = new DirectoryQuery { EdgeType = "edgeType:*1" };
        result = list.Where(x => query.IsMatch(x)).ToArray();
        result.Length.Should().Be(1);
        (result[0] == list[0]).Should().BeTrue();
    }

    [Fact]
    public void DirectoryForEdgeQueryForTags()
    {
        var list = new[]
        {
            new GraphEdge("fromKey1", "toKey1", "edgeType:key1", "t1"),
            new GraphEdge("fromKey2", "toKey2", "edgeType:key2", "t2"),
        };

        var query = new DirectoryQuery { EdgeTags = "t1" };
        var result = list.Where(x => query.IsMatch(x)).ToArray();
        result.Length.Should().Be(1);
        (result[0] == list[0]).Should().BeTrue();

        query = new DirectoryQuery { EdgeTags = "t2" };
        result = list.Where(x => query.IsMatch(x)).ToArray();
        result.Length.Should().Be(1);
        (result[0] == list[1]).Should().BeTrue();

        query = new DirectoryQuery { EdgeTags = "t3" };
        result = list.Where(x => query.IsMatch(x)).ToArray();
        result.Length.Should().Be(0);
    }

    [Fact]
    public void DirectoryForNodeQueryForMultipleTags()
    {
        var list = new[]
        {
            new GraphNode("node1", "t1;t2"),
            new GraphNode("node2", "t2;t4=v1"),
        };

        var query = new DirectoryQuery { NodeTags = "t1" };
        var result = list.Where(x => query.IsMatch(x)).ToArray();
        result.Length.Should().Be(1);
        (result[0] == list[0]).Should().BeTrue();

        query = new DirectoryQuery { NodeTags = "t2" };
        result = list.Where(x => query.IsMatch(x)).ToArray();
        result.Length.Should().Be(2);
        (result[0] == list[0]).Should().BeTrue();
        (result[1] == list[1]).Should().BeTrue();

        query = new DirectoryQuery { NodeTags = "t4" };
        result = list.Where(x => query.IsMatch(x)).ToArray();
        result.Length.Should().Be(1);
        (result[0] == list[1]).Should().BeTrue();

        query = new DirectoryQuery { NodeTags = "t4=v1" };
        result = list.Where(x => query.IsMatch(x)).ToArray();
        result.Length.Should().Be(1);
        (result[0] == list[1]).Should().BeTrue();

        query = new DirectoryQuery { NodeTags = "t3" };
        result = list.Where(x => query.IsMatch(x)).ToArray();
        result.Length.Should().Be(0);
    }

    [Fact]
    public void DirectoryForEdgeQueryForMultipleTags()
    {
        var list = new[]
        {
            new GraphEdge("fromKey1", "toKey1", "edgeType:key1", "t1;t2"),
            new GraphEdge("fromKey2", "toKey2", "edgeType:key2", "t2;t4=v1"),
        };

        var query = new DirectoryQuery { EdgeTags = "t1" };
        var result = list.Where(x => query.IsMatch(x)).ToArray();
        result.Length.Should().Be(1);
        (result[0] == list[0]).Should().BeTrue();

        query = new DirectoryQuery { EdgeTags = "t2" };
        result = list.Where(x => query.IsMatch(x)).ToArray();
        result.Length.Should().Be(2);
        (result[0] == list[0]).Should().BeTrue();
        (result[1] == list[1]).Should().BeTrue();

        query = new DirectoryQuery { EdgeTags = "t4" };
        result = list.Where(x => query.IsMatch(x)).ToArray();
        result.Length.Should().Be(1);
        (result[0] == list[1]).Should().BeTrue();

        query = new DirectoryQuery { EdgeTags = "t4=v1" };
        result = list.Where(x => query.IsMatch(x)).ToArray();
        result.Length.Should().Be(1);
        (result[0] == list[1]).Should().BeTrue();

        query = new DirectoryQuery { EdgeTags = "t3" };
        result = list.Where(x => query.IsMatch(x)).ToArray();
        result.Length.Should().Be(0);
    }

    [Fact]
    public void DirectoryForEdgeQueryForMultipleTagsWithValues()
    {
        var list = new[]
        {
            new GraphEdge("fromKey1", "toKey1", "edgeType:key1", "t1;t2;t4=v1"),
            new GraphEdge("fromKey2", "toKey2", "edgeType:key2", "t2;t4=v1"),
        };

        var query = new DirectoryQuery { EdgeTags = "t1" };
        var result = list.Where(x => query.IsMatch(x)).ToArray();
        result.Length.Should().Be(1);
        (result[0] == list[0]).Should().BeTrue();

        query = new DirectoryQuery { EdgeTags = "t2" };
        result = list.Where(x => query.IsMatch(x)).ToArray();
        result.Length.Should().Be(2);
        (result[0] == list[0]).Should().BeTrue();
        (result[1] == list[1]).Should().BeTrue();

        query = new DirectoryQuery { EdgeTags = "t4" };
        result = list.Where(x => query.IsMatch(x)).ToArray();
        result.Length.Should().Be(2);
        (result[0] == list[0]).Should().BeTrue();
        (result[1] == list[1]).Should().BeTrue();

        query = new DirectoryQuery { EdgeTags = "t4=v1" };
        result = list.Where(x => query.IsMatch(x)).ToArray();
        result.Length.Should().Be(2);
        (result[0] == list[0]).Should().BeTrue();
        (result[1] == list[1]).Should().BeTrue();

        query = new DirectoryQuery { EdgeTags = "t3" };
        result = list.Where(x => query.IsMatch(x)).ToArray();
        result.Length.Should().Be(0);
    }
}
