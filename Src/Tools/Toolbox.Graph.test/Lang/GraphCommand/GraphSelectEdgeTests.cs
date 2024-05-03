using FluentAssertions;
using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.Graph.test.Lang.GraphCommand;

public class GraphSelectEdgeTests
{
    [Theory]
    [InlineData("select [key=key1, tags=t1);")]
    [InlineData("select [key=key1, tags=t, t1];")]
    [InlineData("select [key=key1, add, t2];")]
    [InlineData("select [key=key1, node, t2];")]
    [InlineData("select [key=key1, edge, t2];")]
    [InlineData("select [key=key1, delete=v2, t2];")]
    [InlineData("select [key=key1, update, t2];")]
    [InlineData("select [key=key1, set];")]
    [InlineData("select [key=key1, set, t2];")]
    [InlineData("select [key=key1, key, t2];")]
    public void AddNodeWithReserveTags(string line)
    {
        Option<IReadOnlyList<IGraphQL>> result = GraphLang.Parse(line);
        result.IsError().Should().BeTrue(result.ToString());
    }

    [Fact]
    public void SingleEdgeTagSyntax()
    {
        var q = "select [schedulework:active];";

        Option<IReadOnlyList<IGraphQL>> result = GraphLang.Parse(q);
        result.IsOk().Should().BeTrue();

        IReadOnlyList<IGraphQL> list = result.Return();
        list.Count.Should().Be(1);

        list[0].Action(x =>
        {
            if (x is not GsSelect query) throw new ArgumentException("Invalid type");

            var cursor = query.Search.ToCursor();
            cursor.List.Count.Should().Be(1);

            cursor.NextValue().Return().Cast<GraphEdgeSearch>().Action(x =>
            {
                x.NodeKey.Should().BeNull();
                x.FromKey.Should().BeNull();
                x.ToKey.Should().BeNull();
                x.EdgeType.Should().BeNull();
                x.Tags.ToTagsString().Should().Be("schedulework:active");
            });
        });
    }

    [Fact]
    public void SingleEdgeSyntax()
    {
        var q = "select [NodeKey=key1, fromKey=fromKey1, toKey=tokey1, edgeType=schedulework:active, t2];";

        Option<IReadOnlyList<IGraphQL>> result = GraphLang.Parse(q);
        result.IsOk().Should().BeTrue();

        IReadOnlyList<IGraphQL> list = result.Return();
        list.Count.Should().Be(1);

        list[0].Action(x =>
        {
            if (x is not GsSelect query) throw new ArgumentException("Invalid type");

            var cursor = query.Search.ToCursor();
            cursor.List.Count.Should().Be(1);

            cursor.NextValue().Return().Cast<GraphEdgeSearch>().Action(x =>
            {
                x.NodeKey.Should().Be("key1");
                x.FromKey.Should().Be("fromKey1");
                x.ToKey.Should().Be("tokey1");
                x.EdgeType.Should().Be("schedulework:active");
                x.Tags.ToTagsString().Should().Be("t2");
            });
        });
    }
}
