using FluentAssertions;
using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.Graph.test.Lang.GraphCommand;

public class GraphDeleteTests
{
    [Theory]
    [InlineData("delete (key=key1, tags=t1);")]
    [InlineData("delete (key=key1, tags=t, t1);")]
    [InlineData("delete (key=key1, add, t2);")]
    [InlineData("delete (key=key1, node, t2);")]
    [InlineData("delete (key=key1, edge, t2);")]
    [InlineData("delete (key=key1, delete=v2, t2);")]
    [InlineData("delete (key=key1, update, t2);")]
    [InlineData("delete (key=key1, set);")]
    [InlineData("delete (key=key1, set, t2);")]
    [InlineData("delete (key=key1, key, t2);")]
    [InlineData("delete force (key=key1, key, t2);")]
    public void AddNodeWithReserveTags(string line)
    {
        Option<IReadOnlyList<IGraphQL>> result = GraphLang.Parse(line);
        result.IsError().Should().BeTrue(result.ToString());
    }

    [Theory]
    [InlineData("delete (key=key1);")]
    [InlineData("delete force (key=key1);")]
    [InlineData("delete force (key=key1, t1, t2);")]
    public void ValidDelete(string line)
    {
        Option<IReadOnlyList<IGraphQL>> result = GraphLang.Parse(line);
        result.IsOk().Should().BeTrue(result.ToString());
    }

    [Fact]
    public void DeleteNode()
    {
        var q = "delete (key=key1, t1);";

        Option<IReadOnlyList<IGraphQL>> result = GraphLang.Parse(q);
        result.IsOk().Should().BeTrue(result.ToString());

        IReadOnlyList<IGraphQL> list = result.Return();
        list.Count.Should().Be(1);

        int index = 0;
        list[index++].Action(x =>
        {
            if (x is not GsNodeDelete query) throw new ArgumentException("Invalid type");

            query.Search.Length.Should().Be(1);
            int idx = 0;
            query.Search[idx++].Cast<GraphNodeSearch>().Action(x =>
            {
                x.Key.Should().Be("key1");
                x.Tags.ToTagsString().Should().Be("t1");
                x.Alias.Should().BeNull();
            });
        });
    }

    [Fact]
    public void DeleteNodeWithForce()
    {
        var q = "delete force (key=key1, t1);";

        Option<IReadOnlyList<IGraphQL>> result = GraphLang.Parse(q);
        result.IsOk().Should().BeTrue(result.ToString());

        IReadOnlyList<IGraphQL> list = result.Return();
        list.Count.Should().Be(1);

        int index = 0;
        list[index++].Action(x =>
        {
            if (x is not GsNodeDelete query) throw new ArgumentException("Invalid type");

            query.Search.Length.Should().Be(1);
            query.Force.Should().BeTrue();

            int idx = 0;
            query.Search[idx++].Cast<GraphNodeSearch>().Action(x =>
            {
                x.Key.Should().Be("key1");
                x.Tags.ToTagsString().Should().Be("t1");
                x.Alias.Should().BeNull();
            });
        });
    }

    [Fact]
    public void DeleteEdge1()
    {
        var q = "delete (key=key1, t1) a1 -> [t2] a2;";

        Option<IReadOnlyList<IGraphQL>> result = GraphLang.Parse(q);
        result.IsOk().Should().BeTrue(result.ToString());

        IReadOnlyList<IGraphQL> list = result.Return();
        list.Count.Should().Be(1);

        int index = 0;
        list[index++].Action(x =>
        {
            if (x is not GsEdgeDelete query) throw new ArgumentException("Invalid type");

            query.Search.Length.Should().Be(2);
            int idx = 0;
            query.Search[idx++].Cast<GraphNodeSearch>().Action(x =>
            {
                x.Key.Should().Be("key1");
                x.Tags.ToTagsString().Should().Be("t1");
                x.Alias.Should().Be("a1");
            });

            query.Search[idx++].Cast<GraphEdgeSearch>().Action(x =>
            {
                x.FromKey.Should().BeNull();
                x.ToKey.Should().BeNull();
                x.EdgeType.Should().BeNull();
                x.Tags.ToTagsString().Should().Be("t2");
                x.Alias.Should().Be("a2");
            });
        });
    }

    [Fact]
    public void DeleteEdge2()
    {
        var q = "delete [edgeType=abc*, schedulework:active];";

        Option<IReadOnlyList<IGraphQL>> result = GraphLang.Parse(q);
        result.IsOk().Should().BeTrue(result.ToString());

        IReadOnlyList<IGraphQL> list = result.Return();
        list.Count.Should().Be(1);

        int index = 0;
        list[index++].Action(x =>
        {
            if (x is not GsEdgeDelete query) throw new ArgumentException("Invalid type");

            query.Search.Length.Should().Be(1);
            int idx = 0;
            query.Search[idx++].Cast<GraphEdgeSearch>().Action(x =>
            {
                x.FromKey.Should().BeNull();
                x.ToKey.Should().BeNull();
                x.EdgeType.Should().Be("abc*");
                x.Tags.ToTagsString().Should().Be("schedulework:active");
                x.Alias.Should().BeNull();
            });
        });
    }

    [Fact]
    public void DeleteEdge3()
    {
        var q = "delete [fromKey=key1, toKey=key2, edgeType=abc*, schedulework:active];";

        Option<IReadOnlyList<IGraphQL>> result = GraphLang.Parse(q);
        result.IsOk().Should().BeTrue(result.ToString());

        IReadOnlyList<IGraphQL> list = result.Return();
        list.Count.Should().Be(1);

        int index = 0;
        list[index++].Action(x =>
        {
            if (x is not GsEdgeDelete query) throw new ArgumentException("Invalid type");

            query.Search.Length.Should().Be(1);
            int idx = 0;
            query.Search[idx++].Cast<GraphEdgeSearch>().Action(x =>
            {
                x.FromKey.Should().Be("key1");
                x.ToKey.Should().Be("key2");
                x.EdgeType.Should().Be("abc*");
                x.Tags.ToTagsString().Should().Be("schedulework:active");
                x.Alias.Should().BeNull();
            });
        });
    }

    [Fact]
    public void DeleteEdgeScopedByNode()
    {
        var q = "delete (key=key91, t9=v99) a1 -> [schedulework:active] a2;";

        Option<IReadOnlyList<IGraphQL>> result = GraphLang.Parse(q);
        result.IsOk().Should().BeTrue(result.ToString());

        IReadOnlyList<IGraphQL> list = result.Return();
        list.Count.Should().Be(1);

        int index = 0;

        list[index++].Action(x =>
        {
            if (x is not GsEdgeDelete query) throw new ArgumentException("Invalid type");

            query.Search.Length.Should().Be(2);
            int idx = 0;
            query.Search[idx++].Cast<GraphNodeSearch>().Action(x =>
            {
                x.Key.Should().Be("key91");
                x.Tags.ToTagsString().Should().Be("t9=v99");
                x.Alias.Should().Be("a1");
            });

            query.Search[idx++].Cast<GraphEdgeSearch>().Action(x =>
            {
                x.FromKey.Should().BeNull();
                x.ToKey.Should().BeNull();
                x.EdgeType.Should().BeNull();
                x.Tags.ToTagsString().Should().Be("schedulework:active");
                x.Alias.Should().Be("a2");
            });
        });
    }
}
