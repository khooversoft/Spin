using FluentAssertions;
using Toolbox.Types;

namespace Toolbox.Graph.test.Lang.GraphCommand;

public class GraphAddEdgeTests
{
    [Theory]
    [InlineData("add edge fromKey=key1, toKey=key2, tags=t1, t2;")]
    [InlineData("add edge fromKey=key1, toKey=key2, select, t2;")]
    [InlineData("add edge fromKey=key1, toKey=key2, add, t2;")]
    [InlineData("add edge fromKey=key1, toKey=key2, node, t2;")]
    [InlineData("add edge fromKey=key1, toKey=key2, edge, t2;")]
    [InlineData("add edge fromKey=key1, toKey=key2, delete=v2, t2;")]
    [InlineData("add edge fromKey=key1, toKey=key2, update, t2;")]
    [InlineData("add edge fromKey=key1, toKey=key2, set, t2;")]
    [InlineData("add edge fromKey=key1, toKey=key2, key, t2;")]
    public void AddEdgeWithReserveTagsShouldFail(string line)
    {
        Option<IReadOnlyList<IGraphQL>> result = GraphLang.Parse(line);
        result.IsError().Should().BeTrue(result.ToString());
    }

    [Theory]
    [InlineData("add edge fromKey=key1, toKey=t1;")]
    [InlineData("add edge fromKey=key1, toKey=t1, t1;")]
    [InlineData("add edge fromKey=key1, toKey=t1, t1=v1;")]
    [InlineData("add edge fromKey=key1, toKey=t1, t1=v1, t2;")]
    [InlineData("add edge fromKey=key1, toKey=t1, t1=v1, t2=v2;")]
    [InlineData("add unique edge fromKey=key1, toKey=t1;")]
    [InlineData("add unique edge fromKey=key1, toKey=t1, t1;")]
    [InlineData("add unique edge fromKey=key1, toKey=t1, t1=v1;")]
    [InlineData("add unique edge fromKey=key1, toKey=t1, t1=v1, t2;")]
    [InlineData("add unique edge fromKey=key1, toKey=t1, t1=v1, t2=v2;")]
    [InlineData("add unique edge fromKey=key1, toKey=key2;")]
    public void AddEdgeAreValid(string line)
    {
        Option<IReadOnlyList<IGraphQL>> result = GraphLang.Parse(line);
        result.IsOk().Should().BeTrue(result.ToString());
    }

    [Fact]
    public void UniqueAddEdge()
    {
        var q = "add unique edge fromKey=key1, toKey=key2, t1=v1, t2=v2;";

        Option<IReadOnlyList<IGraphQL>> result = GraphLang.Parse(q);
        result.IsOk().Should().BeTrue(result.ToString());

        IReadOnlyList<IGraphQL> list = result.Return();
        list.Count.Should().Be(1);

        if (list[0] is not GsEdgeAdd query) throw new ArgumentException("Invalid node");

        query.FromKey.Should().Be("key1");
        query.ToKey.Should().Be("key2");
        query.Tags.ToTagsString().Should().Be("t1=v1,t2=v2");
        query.Unique.Should().BeTrue();
    }


    [Fact]
    public void AddEdge()
    {
        var q = "add edge fromKey=key1,toKey=key2,edgeType=et,t2;";

        Option<IReadOnlyList<IGraphQL>> result = GraphLang.Parse(q);
        result.IsOk().Should().BeTrue(result.ToString());

        IReadOnlyList<IGraphQL> list = result.Return();
        list.Count.Should().Be(1);

        if (list[0] is not GsEdgeAdd query) throw new ArgumentException("Invalid node");

        query.FromKey.Should().Be("key1");
        query.ToKey.Should().Be("key2");
        query.EdgeType.Should().Be("et");
        query.Tags.ToTagsString().Should().Be("t2");
        query.Unique.Should().BeFalse();
    }
}
