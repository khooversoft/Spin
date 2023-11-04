using FluentAssertions;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.Test.Data;

//var q = "(t1)";
//var q = "(key=key1;tags=t1)";
//var q = "[schedulework:active]";
public class GraphLangTests
{
    [Fact]
    public void FullSyntax()
    {
        var q = "(key=key1;tags=t1) -> [NodeKey=key1;fromKey=fromKey1;toKey=tokey1;edgeType=schedulework:active;tags=t2] -> (schedule)";

        Option<IReadOnlyList<IGraphQL>> result = GraphLang.Parse(q);
        result.IsOk().Should().BeTrue();

        IReadOnlyList<IGraphQL> list = result.Return();
        list.Count.Should().Be(3);

        list[0].Action(x =>
        {
            if (x is not GraphNodeQuery query) throw new ArgumentException("not graphNode");

            query.Key.Should().Be("key1");
            query.Tags.Should().Be("t1");
            query.Alias.Should().BeNull();
        });

        list[1].Action(x =>
        {
            if (x is not GraphEdgeQuery query) throw new ArgumentException("not graphNode");

            query.NodeKey.Should().Be("key1");
            query.FromKey.Should().Be("fromKey1");
            query.ToKey.Should().Be("tokey1");
            query.EdgeType.Should().Be("schedulework:active");
            query.Tags.Should().Be("t2");
            query.Alias.Should().BeNull();
        });

        list[2].Action(x =>
        {
            if (x is not GraphNodeQuery query) throw new ArgumentException("not graphNode");

            query.Key.Should().BeNull();
            query.Tags.Should().Be("schedule");
            query.Alias.Should().BeNull();
        });
    }

    [Fact]
    public void FullSyntaxWithAlias()
    {
        var q = "(key=key1;tags=t1) a1 -> [NodeKey=key1;fromKey=fromKey1;toKey=tokey1;edgeType=schedulework:active;tags=t2] a2 -> (schedule) a3";

        Option<IReadOnlyList<IGraphQL>> result = GraphLang.Parse(q);
        result.IsOk().Should().BeTrue();

        IReadOnlyList<IGraphQL> list = result.Return();
        list.Count.Should().Be(3);

        list[0].Action(x =>
        {
            if (x is not GraphNodeQuery query) throw new ArgumentException("not graphNode");

            query.Key.Should().Be("key1");
            query.Tags.Should().Be("t1");
            query.Alias.Should().Be("a1");
        });

        list[1].Action(x =>
        {
            if (x is not GraphEdgeQuery query) throw new ArgumentException("not graphNode");

            query.NodeKey.Should().Be("key1");
            query.FromKey.Should().Be("fromKey1");
            query.ToKey.Should().Be("tokey1");
            query.EdgeType.Should().Be("schedulework:active");
            query.Tags.Should().Be("t2");
            query.Alias.Should().Be("a2");
        });

        list[2].Action(x =>
        {
            if (x is not GraphNodeQuery query) throw new ArgumentException("not graphNode");

            query.Key.Should().BeNull();
            query.Tags.Should().Be("schedule");
            query.Alias.Should().Be("a3");
        });
    }

    [Fact]
    public void SingleTagsSyntax()
    {
        var q = "(t1)";

        Option<IReadOnlyList<IGraphQL>> result = GraphLang.Parse(q);
        result.IsOk().Should().BeTrue();

        IReadOnlyList<IGraphQL> list = result.Return();
        list.Count.Should().Be(1);

        list[0].Action(x =>
        {
            if (x is not GraphNodeQuery query) throw new ArgumentException("not graphNode");

            query.Key.Should().BeNull();
            query.Tags.Should().Be("t1");
        });
    }

    [Fact]
    public void SingleSyntax()
    {
        var q = "(key=key1;tags=t1)";

        Option<IReadOnlyList<IGraphQL>> result = GraphLang.Parse(q);
        result.IsOk().Should().BeTrue();

        IReadOnlyList<IGraphQL> list = result.Return();
        list.Count.Should().Be(1);

        list[0].Action(x =>
        {
            if (x is not GraphNodeQuery query) throw new ArgumentException("not graphNode");

            query.Key.Should().Be("key1");
            query.Tags.Should().Be("t1");
        });
    }

    [Fact]
    public void SingleEdgeTagSyntax()
    {
        var q = "[schedulework:active]";

        Option<IReadOnlyList<IGraphQL>> result = GraphLang.Parse(q);
        result.IsOk().Should().BeTrue();

        IReadOnlyList<IGraphQL> list = result.Return();
        list.Count.Should().Be(1);

        list[0].Action(x =>
        {
            if (x is not GraphEdgeQuery query) throw new ArgumentException("not graphNode");

            query.NodeKey.Should().BeNull();
            query.FromKey.Should().BeNull();
            query.ToKey.Should().BeNull();
            query.EdgeType.Should().BeNull();
            query.Tags.Should().Be("schedulework:active");
        });
    }

    [Fact]
    public void SingleEdgeSyntax()
    {
        var q = "[NodeKey=key1;fromKey=fromKey1;toKey=tokey1;edgeType=schedulework:active;tags=t2]";

        Option<IReadOnlyList<IGraphQL>> result = GraphLang.Parse(q);
        result.IsOk().Should().BeTrue();

        IReadOnlyList<IGraphQL> list = result.Return();
        list.Count.Should().Be(1);

        list[0].Action(x =>
        {
            if (x is not GraphEdgeQuery query) throw new ArgumentException("not graphNode");

            query.NodeKey.Should().Be("key1");
            query.FromKey.Should().Be("fromKey1");
            query.ToKey.Should().Be("tokey1");
            query.EdgeType.Should().Be("schedulework:active");
            query.Tags.Should().Be("t2");
        });
    }
}
