using FluentAssertions;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Graph;
using Toolbox.Types;

namespace Toolbox.Graph.test.Lang.GraphCommand;

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
    public void AddNodeWithReserveTagsShouldFail(string line)
    {
        Option<IReadOnlyList<IGraphQL>> result = GraphLang.Parse(line);
        result.IsError().Should().BeTrue(result.ToString());
    }

    [Theory]
    [InlineData("add node key=key1;")]
    [InlineData("add node key=key1, t2;")]
    [InlineData("add node key=key1, t1, t2;")]
    [InlineData("add node key=key1, t2, t2=v1;")]
    [InlineData("add node key=key1, t=v2, t2;")]
    [InlineData("add node key=key1, t=v2, t2=v4;")]
    public void AddNodeAreValid(string line)
    {
        Option<IReadOnlyList<IGraphQL>> result = GraphLang.Parse(line);
        result.IsOk().Should().BeTrue(result.ToString());
    }

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

        int index = 0;
        list[index++].Action(x =>
        {
            if (x is not GraphEdgeAdd query) throw new ArgumentException("Invalid node");

            query.FromKey.Should().Be("key1");
            query.ToKey.Should().Be("key2");
            query.Tags.ToString().Should().Be("t1=v1,t2=v2");
            query.Unique.Should().BeTrue();
        });
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
