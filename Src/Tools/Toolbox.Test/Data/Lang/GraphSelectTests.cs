using FluentAssertions;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.Test.Data.Lang;

//var q = "(t1)";
//var q = "(key=key1;tags=t1)";
//var q = "[schedulework:active]";
public class GraphSelectTests
{
    [Fact]
    public void FullSyntax()
    {
        var q = "select (key=key1;tags=t1) -> [NodeKey=key1;fromKey=fromKey1;toKey=tokey1;edgeType=schedulework:active;tags=t2] -> (schedule);";

        Option<IReadOnlyList<IGraphQL>> result = GraphLang.Parse(q);
        result.IsOk().Should().BeTrue(result.ToString());

        IReadOnlyList<IGraphQL> list = result.Return();
        list.Count.Should().Be(1);

        int index = 0;
        list[index++].Action(x =>
        {
            if (x is not GraphSelect query) throw new ArgumentException("Invalid type");

            var cursor = query.Search.ToCursor();
            cursor.List.Count.Should().Be(3);

            cursor.NextValue().Return().Cast<GraphNodeSearch>().Action(x =>
            {
                x.Key.Should().Be("key1");
                x.Tags.Should().Be("t1");
                x.Alias.Should().BeNull();
            });

            cursor.NextValue().Return().Cast<GraphEdgeSearch>().Action(x =>
            {
                x.NodeKey.Should().Be("key1");
                x.FromKey.Should().Be("fromKey1");
                x.ToKey.Should().Be("tokey1");
                x.EdgeType.Should().Be("schedulework:active");
                x.Tags.Should().Be("t2");
                x.Alias.Should().BeNull();
            });

            cursor.NextValue().Return().Cast<GraphNodeSearch>().Action(x =>
            {
                x.Key.Should().BeNull();
                x.Tags.Should().Be("schedule");
                x.Alias.Should().BeNull();
            });
        });
    }

    [Fact]
    public void FullSyntaxWithAlias()
    {
        var q = "select (key=key1;tags=t1) a1 -> [NodeKey=key1;fromKey=fromKey1;toKey=tokey1;edgeType=schedulework:active;tags=t2] a2 -> (schedule) a3;";

        Option<IReadOnlyList<IGraphQL>> result = GraphLang.Parse(q);
        result.IsOk().Should().BeTrue();

        IReadOnlyList<IGraphQL> list = result.Return();
        list.Count.Should().Be(1);

        int index = 0;
        list[index++].Action(x =>
        {
            if (x is not GraphSelect query) throw new ArgumentException("Invalid type");

            var cursor = query.Search.ToCursor();
            cursor.List.Count.Should().Be(3);

            cursor.NextValue().Return().Cast<GraphNodeSearch>().Action(x =>
            {
                x.Key.Should().Be("key1");
                x.Tags.Should().Be("t1");
                x.Alias.Should().Be("a1");
            });

            cursor.NextValue().Return().Cast<GraphEdgeSearch>().Action(x =>
            {
                x.NodeKey.Should().Be("key1");
                x.FromKey.Should().Be("fromKey1");
                x.ToKey.Should().Be("tokey1");
                x.EdgeType.Should().Be("schedulework:active");
                x.Tags.Should().Be("t2");
                x.Alias.Should().Be("a2");
            });

            cursor.NextValue().Return().Cast<GraphNodeSearch>().Action(x =>
            {
                x.Key.Should().BeNull();
                x.Tags.Should().Be("schedule");
                x.Alias.Should().Be("a3");
            });
        });
    }

    [Fact]
    public void SingleTagsSyntax()
    {
        var q = "select (t1);";

        Option<IReadOnlyList<IGraphQL>> result = GraphLang.Parse(q);
        result.IsOk().Should().BeTrue();

        IReadOnlyList<IGraphQL> list = result.Return();
        list.Count.Should().Be(1);

        list[0].Action(x =>
        {
            if (x is not GraphSelect query) throw new ArgumentException("Invalid type");

            var cursor = query.Search.ToCursor();
            cursor.List.Count.Should().Be(1);

            cursor.NextValue().Return().Cast<GraphNodeSearch>().Action(x =>
            {
                x.Key.Should().BeNull();
                x.Tags.Should().Be("t1");
            });
        });
    }

    [Fact]
    public void SingleSyntax()
    {
        var q = "select (key=key1;tags=t1);";

        Option<IReadOnlyList<IGraphQL>> result = GraphLang.Parse(q);
        result.IsOk().Should().BeTrue();

        IReadOnlyList<IGraphQL> list = result.Return();
        list.Count.Should().Be(1);

        list[0].Action(x =>
        {
            if (x is not GraphSelect query) throw new ArgumentException("Invalid type");

            var cursor = query.Search.ToCursor();
            cursor.List.Count.Should().Be(1);

            cursor.NextValue().Return().Cast<GraphNodeSearch>().Action(x =>
            {
                x.Key.Should().Be("key1");
                x.Tags.Should().Be("t1");
            });
        });
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
            if (x is not GraphSelect query) throw new ArgumentException("Invalid type");

            var cursor = query.Search.ToCursor();
            cursor.List.Count.Should().Be(1);

            cursor.NextValue().Return().Cast<GraphEdgeSearch>().Action(x =>
            {
                x.NodeKey.Should().BeNull();
                x.FromKey.Should().BeNull();
                x.ToKey.Should().BeNull();
                x.EdgeType.Should().BeNull();
                x.Tags.Should().Be("schedulework:active");
            });
        });
    }

    [Fact]
    public void SingleEdgeSyntax()
    {
        var q = "select [NodeKey=key1;fromKey=fromKey1;toKey=tokey1;edgeType=schedulework:active;tags=t2];";

        Option<IReadOnlyList<IGraphQL>> result = GraphLang.Parse(q);
        result.IsOk().Should().BeTrue();

        IReadOnlyList<IGraphQL> list = result.Return();
        list.Count.Should().Be(1);

        list[0].Action(x =>
        {
            if (x is not GraphSelect query) throw new ArgumentException("Invalid type");

            var cursor = query.Search.ToCursor();
            cursor.List.Count.Should().Be(1);

            cursor.NextValue().Return().Cast<GraphEdgeSearch>().Action(x =>
            {
                x.NodeKey.Should().Be("key1");
                x.FromKey.Should().Be("fromKey1");
                x.ToKey.Should().Be("tokey1");
                x.EdgeType.Should().Be("schedulework:active");
                x.Tags.Should().Be("t2");
            });
        });
    }
}
