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

            query.Search.Count.Should().Be(1);
            int idx = 0;
            query.Search[idx++].Cast<GraphNodeSearch>().Action(x =>
            {
                x.Key.Should().Be("key1");
                x.Tags.Should().Be("t1");
                x.Alias.Should().BeNull();
            });
        });
    }

    [Fact]
    public void DeleteEdge1()
    {
        var q = "delete (key=key1;tags=t1) a1 -> [t2] a2;";

        Option<IReadOnlyList<IGraphQL>> result = GraphLang.Parse(q);
        result.IsOk().Should().BeTrue(result.ToString());

        IReadOnlyList<IGraphQL> list = result.Return();
        list.Count.Should().Be(1);

        int index = 0;
        list[index++].Action(x =>
        {
            if (x is not GraphEdgeDelete query) throw new ArgumentException("Invalid type");

            query.Search.Count.Should().Be(2);
            int idx = 0;
            query.Search[idx++].Cast<GraphNodeSearch>().Action(x =>
            {
                x.Key.Should().Be("key1");
                x.Tags.Should().Be("t1");
                x.Alias.Should().Be("a1");
            });

            query.Search[idx++].Cast<GraphEdgeSearch>().Action(x =>
            {
                x.FromKey.Should().BeNull();
                x.ToKey.Should().BeNull();
                x.EdgeType.Should().BeNull();
                x.Tags.Should().Be("t2");
                x.Alias.Should().Be("a2");
            });

        });
    }

    [Fact]
    public void DeleteEdge2()
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

            query.Search.Count.Should().Be(1);
            int idx = 0;
            query.Search[idx++].Cast<GraphEdgeSearch>().Action(x =>
            {
                x.FromKey.Should().BeNull();
                x.ToKey.Should().BeNull();
                x.EdgeType.Should().Be("abc*");
                x.Tags.Should().Be("schedulework:active");
                x.Alias.Should().BeNull();
            });
        });
    }

    [Fact]
    public void DeleteEdge3()
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

            query.Search.Count.Should().Be(1);
            int idx = 0;
            query.Search[idx++].Cast<GraphEdgeSearch>().Action(x =>
            {
                x.FromKey.Should().Be("key1");
                x.ToKey.Should().Be("key2");
                x.EdgeType.Should().Be("abc*");
                x.Tags.Should().Be("schedulework:active");
                x.Alias.Should().BeNull();
            });
        });
    }

    [Fact]
    public void DeleteEdgeScopedByNode()
    {
        var q = "delete (key=key91;tags='t9=v99') a1 -> [schedulework:active] a2;";

        Option<IReadOnlyList<IGraphQL>> result = GraphLang.Parse(q);
        result.IsOk().Should().BeTrue(result.ToString());

        IReadOnlyList<IGraphQL> list = result.Return();
        list.Count.Should().Be(1);

        int index = 0;

        list[index++].Action(x =>
        {
            if (x is not GraphEdgeDelete query) throw new ArgumentException("Invalid type");

            query.Search.Count.Should().Be(2);
            int idx = 0;
            query.Search[idx++].Cast<GraphNodeSearch>().Action(x =>
            {
                x.Key.Should().Be("key91");
                x.Tags.Should().Be("t9=v99");
                x.Alias.Should().Be("a1");
            });

            query.Search[idx++].Cast<GraphEdgeSearch>().Action(x =>
            {
                x.FromKey.Should().BeNull();
                x.ToKey.Should().BeNull();
                x.EdgeType.Should().BeNull();
                x.Tags.Should().Be("schedulework:active");
                x.Alias.Should().Be("a2");
            });
        });
    }
}
