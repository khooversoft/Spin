using FluentAssertions;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.Test.Data.Lang;

public class GraphAddTests
{
    [Theory]
    [InlineData("add node key=key1, tags=t1, t2;")]
    [InlineData("add node key=key1, select, t2;")]
    [InlineData("add node key=key1, add, t2;")]
    [InlineData("add node key=key1, node, t2;")]
    [InlineData("add node key=key1, edge, t2;")]
    [InlineData("add node key=key1, delete=v2, t2;")]
    [InlineData("add node key=key1, update, t2;")]
    [InlineData("add node key=key1, set, t2;")]
    [InlineData("add node key=key1, key, t2;")]
    public void AddNodeWithReserveTags(string line)
    {
        Option<IReadOnlyList<IGraphQL>> result = GraphLang.Parse(line);
        result.IsError().Should().BeTrue(result.ToString());
    }

    [Fact]
    public void AddSingleTag()
    {
        var q = "add node key=key1, t1;";

        Option<IReadOnlyList<IGraphQL>> result = GraphLang.Parse(q);
        result.IsOk().Should().BeTrue(result.ToString());

        IReadOnlyList<IGraphQL> list = result.Return();
        list.Count.Should().Be(1);

        int index = 0;
        list[index++].Action(x =>
        {
            if (x is not GraphNodeAdd query) throw new ArgumentException("Invalid node");

            query.Key.Should().Be("key1");
            query.Tags.ToString().Should().Be("t1");
        });
    }

    [Fact]
    public void AddNode()
    {
        var q = "add node key=key1, t1=v1;";

        Option<IReadOnlyList<IGraphQL>> result = GraphLang.Parse(q);
        result.IsOk().Should().BeTrue(result.ToString());

        IReadOnlyList<IGraphQL> list = result.Return();
        list.Count.Should().Be(1);

        int index = 0;
        list[index++].Action(x =>
        {
            if (x is not GraphNodeAdd query) throw new ArgumentException("Invalid node");

            query.Key.Should().Be("key1");
            query.Tags.ToString().Should().Be("t1=v1");
        });
    }

    [Fact]
    public void AddNodeWithTwoTags()
    {
        var q = "add node key=key1, t1=v1, t2;";

        Option<IReadOnlyList<IGraphQL>> result = GraphLang.Parse(q);
        result.IsOk().Should().BeTrue(result.ToString());

        IReadOnlyList<IGraphQL> list = result.Return();
        list.Count.Should().Be(1);

        int index = 0;
        list[index++].Action(x =>
        {
            if (x is not GraphNodeAdd query) throw new ArgumentException("Invalid node");

            query.Key.Should().Be("key1");
            query.Tags.ToString().Should().Be("t1=v1,t2");
        });
    }

    [Fact]
    public void AddEdge()
    {
        var q = "add edge fromKey=key1,toKey=key2,edgeType=et,t2;";

        Option<IReadOnlyList<IGraphQL>> result = GraphLang.Parse(q);
        result.IsOk().Should().BeTrue(result.ToString());

        IReadOnlyList<IGraphQL> list = result.Return();
        list.Count.Should().Be(1);

        int index = 0;
        list[index++].Action(x =>
        {
            if (x is not GraphEdgeAdd query) throw new ArgumentException("Invalid node");

            query.FromKey.Should().Be("key1");
            query.ToKey.Should().Be("key2");
            query.EdgeType.Should().Be("et");
            query.Tags.ToString().Should().Be("t2");
        });
    }
}
