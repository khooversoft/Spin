using FluentAssertions;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.Test.Data.Lang;

public class GraphUpdateTests
{
    [Fact]
    public void updateNode()
    {
        var q = "update (key=key1;tags=t1) set tags=t2;";

        Option<IReadOnlyList<IGraphQL>> result = GraphLang.Parse(q);
        result.IsOk().Should().BeTrue(result.ToString());

        IReadOnlyList<IGraphQL> list = result.Return();
        list.Count.Should().Be(1);

        int index = 0;
        list[index++].Action(x =>
        {
            if (x is not GraphNodeUpdate query) throw new ArgumentException("Invalid type");

            query.Tags.Should().Be("t2");
            query.Search.Count.Should().Be(1);
            query.Search.Count.Should().Be(1);

            query.Search[0].Cast<GraphNodeSearch>().Action(x =>
            {
                x.Key.Should().Be("key1");
                x.Tags.Should().Be("t1");
            });
        });
    }

    [Fact]
    public void UpdateEdge()
    {
        var q = "update [edgeType=abc*;schedulework:active] set edgeType=et,tags=t2;";

        Option<IReadOnlyList<IGraphQL>> result = GraphLang.Parse(q);
        result.IsOk().Should().BeTrue(result.ToString());

        IReadOnlyList<IGraphQL> list = result.Return();
        list.Count.Should().Be(1);

        int index = 0;
        list[index++].Action(x =>
        {
            if (x is not GraphEdgeUpdate query) throw new ArgumentException("Invalid node");

            query.EdgeType.Should().Be("et");
            query.Tags.Should().Be("t2");
            query.Search.Count.Should().Be(1);

            query.Search[0].Cast<GraphEdgeSearch>().Action(x =>
            {
                x.NodeKey.Should().BeNull();
                x.FromKey.Should().BeNull();
                x.ToKey.Should().BeNull();
                x.EdgeType.Should().Be("abc*");
                x.Tags.Should().Be("schedulework:active"); ;
            });
        });
    }

    [Fact]
    public void UpdateEdgeViaNode()
    {
        var q = "update (key=k*) -> [edgeType=abc*;schedulework:active] set edgeType=et,tags=t2;";

        Option<IReadOnlyList<IGraphQL>> result = GraphLang.Parse(q);
        result.IsOk().Should().BeTrue(result.ToString());

        IReadOnlyList<IGraphQL> list = result.Return();
        list.Count.Should().Be(1);

        int index = 0;

        list[index++].Action(x =>
        {
            if (x is not GraphEdgeUpdate query) throw new ArgumentException("Invalid node");

            query.EdgeType.Should().Be("et");
            query.Tags.Should().Be("t2");
            query.Search.Count.Should().Be(2);

            int idx = 0;
            query.Search[idx++].Cast<GraphNodeSearch>().Action(x =>
            {
                x.Key.Should().Be("k*");
                x.Tags.Should().BeNull(); ;
            });

            query.Search[idx++].Cast<GraphEdgeSearch>().Action(x =>
            {
                x.NodeKey.Should().BeNull();
                x.FromKey.Should().BeNull();
                x.ToKey.Should().BeNull();
                x.EdgeType.Should().Be("abc*");
                x.Tags.Should().Be("schedulework:active"); ;
            });
        });
    }
}
