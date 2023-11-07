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
        var q = "update (key=key1;tags=t1) set key=key2,tags=t2;";

        Option<IReadOnlyList<IGraphQL>> result = GraphLang.Parse(q);
        result.IsOk().Should().BeTrue(result.ToString());

        IReadOnlyList<IGraphQL> list = result.Return();
        list.Count.Should().Be(1);

        int index = 0;
        list[index++].Action(x =>
        {
            if (x is not GraphNodeUpdate query) throw new ArgumentException("Invalid type");

            query.Key.Should().Be("key2");
            query.Tags.Should().Be("t2");
            query.Search.Count.Should().Be(1);
            query.Search[0].Cast<GraphNodeSelect>().Action(x =>
            {
                x.Key.Should().Be("key1");
                x.Tags.Should().Be("t1");
            });
        });
    }

    [Fact]
    public void UpdateEdge()
    {
        var q = "update [edgeType=abc*;schedulework:active] set fromKey=key1,toKey=key2,edgeType=et,tags=t2;";

        Option<IReadOnlyList<IGraphQL>> result = GraphLang.Parse(q);
        result.IsOk().Should().BeTrue(result.ToString());

        IReadOnlyList<IGraphQL> list = result.Return();
        list.Count.Should().Be(1);

        int index = 0;
        list[index++].Action(x =>
        {
            if (x is not GraphEdgeUpdate query) throw new ArgumentException("Invalid node");

            query.FromKey.Should().Be("key1");
            query.ToKey.Should().Be("key2");
            query.EdgeType.Should().Be("et");
            query.Tags.Should().Be("t2");
            query.Search[0].Cast<GraphEdgeSelect>().Action(x =>
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
