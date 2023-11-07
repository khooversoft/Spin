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
        var q = "update (key=key1;tags=t1) set key=key1,tags=t1;";

        Option<IReadOnlyList<IGraphQL>> result = GraphLang.Parse(q);
        result.IsOk().Should().BeTrue(result.ToString());

        IReadOnlyList<IGraphQL> list = result.Return();
        list.Count.Should().Be(2);

        int index = 0;
        list[index++].Action(x =>
        {
            if (x is not GraphNodeSelect query) throw new ArgumentException("Invalid type");

            query.Key.Should().Be("key1");
            query.Tags.Should().Be("t1");
            query.Alias.Should().BeNull();
        });

        list[index++].Action(x =>
        {
            if (x is not GraphNodeUpdate query) throw new ArgumentException("Invalid node");

            query.Key.Should().Be("key1");
            query.Tags.Should().Be("t1");
        });
    }

    [Fact]
    public void UpdateEdge()
    {
        var q = "update [edgeType=abc*;schedulework:active] set fromKey=key1,toKey=key2,edgeType=et,tags=t2;";

        Option<IReadOnlyList<IGraphQL>> result = GraphLang.Parse(q);
        result.IsOk().Should().BeTrue(result.ToString());

        IReadOnlyList<IGraphQL> list = result.Return();
        list.Count.Should().Be(2);

        int index = 0;
        list[index++].Action(x =>
        {
            if (x is not GraphEdgeSelect query) throw new ArgumentException("Invalid type");

            query.NodeKey.Should().BeNull();
            query.FromKey.Should().BeNull();
            query.ToKey.Should().BeNull();
            query.EdgeType.Should().Be("abc*");
            query.Tags.Should().Be("schedulework:active");
        });

        list[index++].Action(x =>
        {
            if (x is not GraphEdgeUpdate query) throw new ArgumentException("Invalid node");

            query.FromKey.Should().Be("key1");
            query.ToKey.Should().Be("key2");
            query.EdgeType.Should().Be("et");
            query.Tags.Should().Be("t2");
        });
    }
}
