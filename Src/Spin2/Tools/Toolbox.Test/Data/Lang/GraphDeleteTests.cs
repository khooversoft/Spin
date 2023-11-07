using FluentAssertions;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.Test.Data.Lang;

public class GraphDeleteTests
{
    [Fact]
    public void DeleteNode()
    {
        var q = "delete (key=key1;tags=t1);";

        Option<IReadOnlyList<IGraphQL>> result = GraphLang.Parse(q);
        result.IsOk().Should().BeTrue(result.ToString());

        IReadOnlyList<IGraphQL> list = result.Return();
        list.Count.Should().Be(1);

        int index = 0;
        list[index++].Action(x =>
        {
            if (x is not GraphNodeDelete query) throw new ArgumentException("Invalid type");

            query.Key.Should().Be("key1");
            query.Tags.Should().Be("t1");
        });
    }

    [Fact]
    public void DeleteEdge1()
    {
        var q = "delete [edgeType=abc*;schedulework:active];";

        Option<IReadOnlyList<IGraphQL>> result = GraphLang.Parse(q);
        result.IsOk().Should().BeTrue(result.ToString());

        IReadOnlyList<IGraphQL> list = result.Return();
        list.Count.Should().Be(1);

        int index = 0;
        list[index++].Action(x =>
        {
            if (x is not GraphEdgeDelete query) throw new ArgumentException("Invalid type");

            query.FromKey.Should().BeNull();
            query.ToKey.Should().BeNull();
            query.EdgeType.Should().Be("abc*");
            query.Tags.Should().Be("schedulework:active");
        });
    }

    [Fact]
    public void DeleteEdge2()
    {
        var q = "delete [fromKey=key1;toKey=key2;edgeType=abc*;schedulework:active];";

        Option<IReadOnlyList<IGraphQL>> result = GraphLang.Parse(q);
        result.IsOk().Should().BeTrue(result.ToString());

        IReadOnlyList<IGraphQL> list = result.Return();
        list.Count.Should().Be(1);

        int index = 0;
        list[index++].Action(x =>
        {
            if (x is not GraphEdgeDelete query) throw new ArgumentException("Invalid type");

            query.FromKey.Should().Be("key1");
            query.ToKey.Should().Be("key2");
            query.EdgeType.Should().Be("abc*");
            query.Tags.Should().Be("schedulework:active");
        });
    }

    [Fact]
    public void DeleteEdgeScopedByNode()
    {
        var q = "delete (key=key91;tags='t9=v99') a1 -> [schedulework:active] a2;";

        Option<IReadOnlyList<IGraphQL>> result = GraphLang.Parse(q);
        result.IsOk().Should().BeTrue(result.ToString());

        IReadOnlyList<IGraphQL> list = result.Return();
        list.Count.Should().Be(2);

        int index = 0;

        list[index++].Action(x =>
        {
            if (x is not GraphNodeDelete query) throw new ArgumentException("Invalid type");

            query.Key.Should().Be("key91");
            query.Tags.Should().Be("t9=v99");
        });

        list[index++].Action(x =>
        {
            if (x is not GraphEdgeDelete query) throw new ArgumentException("Invalid type");

            query.FromKey.Should().BeNull();
            query.ToKey.Should().BeNull();
            query.EdgeType.Should().BeNull();
            query.Tags.Should().Be("schedulework:active");
        });
    }
}
