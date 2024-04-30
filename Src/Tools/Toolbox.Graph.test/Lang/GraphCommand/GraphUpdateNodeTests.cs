using FluentAssertions;
using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.Graph.test.Lang.GraphCommand;

public class GraphUpdateNodeTests
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
    [InlineData("update (key=key1, key, t2) set data { 0xFF };")]
    [InlineData("update (key=key1, key, t2) set data { 0xFF }, contract { json=0x500, data=skdfajoefief };")]
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
    public void AddSingleData()
    {
        var q = "update (key=key1, t1) set entity { abc };";

        Option<IReadOnlyList<IGraphQL>> result = GraphLang.Parse(q);
        result.IsOk().Should().BeTrue(result.ToString());

        IReadOnlyList<IGraphQL> list = result.Return();
        list.Count.Should().Be(1);

        if (list[0] is not GraphNodeUpdate query) throw new ArgumentException("Invalid node");

        query.Tags.Count.Should().Be(0);
        query.Links.Count.Should().Be(0);
        query.DataMap.Count.Should().Be(1);

        query.DataMap.Action(x =>
        {
            x.TryGetValue("entity", out var entity).Should().BeTrue();
            entity!.Count.Should().Be(1);
            entity.TryGetValue("abc", out var value).Should().BeTrue();
            value.Should().BeNull();
        });
    }

    [Fact]
    public void AddTwoData()
    {
        var q = "update (key=key1, t1) set entity { abc }, contract { json, name=contractType, data=0xFA03ADF };";

        Option<IReadOnlyList<IGraphQL>> result = GraphLang.Parse(q);
        result.IsOk().Should().BeTrue(result.ToString());

        IReadOnlyList<IGraphQL> list = result.Return();
        list.Count.Should().Be(1);

        if (list[0] is not GraphNodeUpdate query) throw new ArgumentException("Invalid node");

        query.Tags.Count.Should().Be(0);
        query.Links.Count.Should().Be(0);
        query.DataMap.Count.Should().Be(2);

        query.DataMap.Action(x =>
        {
            x.TryGetValue("entity", out var entity).Should().BeTrue();
            entity!.Count.Should().Be(1);
            entity.TryGetValue("abc", out var value).Should().BeTrue();
            value.Should().BeNull();
        });

        query.DataMap.Action(x =>
        {
            x.TryGetValue("contract", out var entity).Should().BeTrue();
            entity!.Count.Should().Be(3);

            entity.Action(y =>
            {
                y.TryGetValue("json", out var value).Should().BeTrue();
                value.Should().BeNull();
            });

            entity.Action(y =>
            {
                y.TryGetValue("name", out var value).Should().BeTrue();
                value.Should().Be("contractType");
            });

            entity.Action(y =>
            {
                y.TryGetValue("data", out var value).Should().BeTrue();
                value.Should().Be("0xFA03ADF");
            });
        });
    }
}
