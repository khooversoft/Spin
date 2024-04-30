using FluentAssertions;
using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.Graph.test.Lang.GraphCommand;

public class GraphUpdateTests
{
    [Theory]
    [InlineData("update (key=key1, tags=t1) set tags=t2;")]
    [InlineData("update (key=key1, tags=t, t1) set tags=t2;")]
    [InlineData("update (key=key1, add, t2) set tags=t2;")]
    [InlineData("update (key=key1, node, t2) set tags=t2;")]
    [InlineData("update (key=key1, edge, t2) set tags=t2;")]
    [InlineData("update (key=key1, delete=v2, t2) set tags=t2;")]
    [InlineData("update (key=key1, update, t2) set tags=t2;")]
    [InlineData("update (key=key1, set) set tags=t2;")]
    [InlineData("update (key=key1, set, t2) set tags=t2;")]
    [InlineData("update (key=key1, key, t2) set tags=t2;")]
    [InlineData("update [key=key1, tags=t1) set tags=t2;")]
    [InlineData("update [key=key1, tags=t, t1] set tags=t2;")]
    [InlineData("update [key=key1, add, t2] set tags=t2;")]
    [InlineData("update [key=key1, node, t2] set tags=t2;")]
    [InlineData("update [key=key1, edge, t2] set tags=t2;")]
    [InlineData("update [key=key1, delete=v2, t2] set tags=t2;")]
    [InlineData("update [key=key1, update, t2] set tags=t2;")]
    [InlineData("update [key=key1, set] set tags=t2;")]
    [InlineData("update [key=key1, set, t2] set tags=t2;")]
    [InlineData("update [key=key1, key, t2] set tags=t2;")]
    public void AddNodeWithReserveTags(string line)
    {
        Option<IReadOnlyList<IGraphQL>> result = GraphLang.Parse(line);
        result.IsError().Should().BeTrue(result.ToString());
    }

    [Fact]
    public void UpdateNodeSimple()
    {
        var q = "update (key=key1) set t2;";

        Option<IReadOnlyList<IGraphQL>> result = GraphLang.Parse(q);
        result.IsOk().Should().BeTrue(result.ToString());

        IReadOnlyList<IGraphQL> list = result.Return();
        list.Count.Should().Be(1);

        int index = 0;
        list[index++].Action(x =>
        {
            if (x is not GraphNodeUpdate query) throw new ArgumentException("Invalid type");

            query.Tags.ToTagsString().Should().Be("t2");
            query.Search.Count.Should().Be(1);
            query.Search.Count.Should().Be(1);

            query.Search[0].Cast<GraphNodeSearch>().Action(x =>
            {
                x.Key.Should().Be("key1");
                x.Tags.Count.Should().Be(0);
            });
        });
    }

    [Fact]
    public void updateNode()
    {
        var q = "update (key=key1, t1) set t2;";

        Option<IReadOnlyList<IGraphQL>> result = GraphLang.Parse(q);
        result.IsOk().Should().BeTrue(result.ToString());

        IReadOnlyList<IGraphQL> list = result.Return();
        list.Count.Should().Be(1);

        int index = 0;
        list[index++].Action(x =>
        {
            if (x is not GraphNodeUpdate query) throw new ArgumentException("Invalid type");

            query.Tags.ToTagsString().Should().Be("t2");
            query.Search.Count.Should().Be(1);
            query.Search.Count.Should().Be(1);

            query.Search[0].Cast<GraphNodeSearch>().Action(x =>
            {
                x.Key.Should().Be("key1");
                x.Tags.ToTagsString().Should().Be("t1");
            });
        });
    }

    [Fact]
    public void updateNodeWithLink()
    {
        var q = "update (key=key1, t1) set t2, link=l1;";

        Option<IReadOnlyList<IGraphQL>> result = GraphLang.Parse(q);
        result.IsOk().Should().BeTrue(result.ToString());

        IReadOnlyList<IGraphQL> list = result.Return();
        list.Count.Should().Be(1);

        int index = 0;
        list[index++].Action(x =>
        {
            if (x is not GraphNodeUpdate query) throw new ArgumentException("Invalid type");

            query.Tags.ToTagsString().Should().Be("t2");
            query.Links.Join(',').Should().Be("l1");
            query.Search.Count.Should().Be(1);
            query.Search.Count.Should().Be(1);

            query.Search[0].Cast<GraphNodeSearch>().Action(x =>
            {
                x.Key.Should().Be("key1");
                x.Tags.ToTagsString().Should().Be("t1");
            });
        });
    }

    [Fact]
    public void UpdateEdge()
    {
        var q = "update [edgeType=abc*, schedulework:active] set edgeType=et, t2;";

        Option<IReadOnlyList<IGraphQL>> result = GraphLang.Parse(q);
        result.IsOk().Should().BeTrue(result.ToString());

        IReadOnlyList<IGraphQL> list = result.Return();
        list.Count.Should().Be(1);

        int index = 0;
        list[index++].Action(x =>
        {
            if (x is not GraphEdgeUpdate query) throw new ArgumentException("Invalid node");

            query.EdgeType.Should().Be("et");
            query.Tags.ToTagsString().Should().Be("t2");
            query.Search.Count.Should().Be(1);

            query.Search[0].Cast<GraphEdgeSearch>().Action(x =>
            {
                x.NodeKey.Should().BeNull();
                x.FromKey.Should().BeNull();
                x.ToKey.Should().BeNull();
                x.EdgeType.Should().Be("abc*");
                x.Tags.ToTagsString().Should().Be("schedulework:active"); ;
            });
        });
    }

    [Fact]
    public void UpdateEdgeViaNode()
    {
        var q = "update (key=k*) -> [edgeType=abc*, schedulework:active] set edgeType=et,t2;";

        Option<IReadOnlyList<IGraphQL>> result = GraphLang.Parse(q);
        result.IsOk().Should().BeTrue(result.ToString());

        IReadOnlyList<IGraphQL> list = result.Return();
        list.Count.Should().Be(1);

        int index = 0;

        list[index++].Action(x =>
        {
            if (x is not GraphEdgeUpdate query) throw new ArgumentException("Invalid node");

            query.EdgeType.Should().Be("et");
            query.Tags.ToTagsString().Should().Be("t2");
            query.Search.Count.Should().Be(2);

            int idx = 0;
            query.Search[idx++].Cast<GraphNodeSearch>().Action(x =>
            {
                x.Key.Should().Be("k*");
                x.Tags.Count.Should().Be(0);
            });

            query.Search[idx++].Cast<GraphEdgeSearch>().Action(x =>
            {
                x.NodeKey.Should().BeNull();
                x.FromKey.Should().BeNull();
                x.ToKey.Should().BeNull();
                x.EdgeType.Should().Be("abc*");
                x.Tags.ToTagsString().Should().Be("schedulework:active"); ;
            });
        });
    }
}
