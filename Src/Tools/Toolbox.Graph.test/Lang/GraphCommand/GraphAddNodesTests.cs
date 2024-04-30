using FluentAssertions;
using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.Graph.test.Lang.GraphCommand;

public class GraphAddNodesTests
{
    [Theory]
    [InlineData("add node key=key1, tags=t1, t2;")]
    [InlineData("add node key=key1, select, t2;")]
    [InlineData("add node key=key1, add, t2;")]
    [InlineData("add node key=key1, node, t2;")]
    [InlineData("add node key=key1, edge, t2;")]
    [InlineData("add node key=key1, delete=v2, t2;")]
    [InlineData("add node key=key1, update, t2;")]
    [InlineData("add node key=key1, set, t2;")]
    [InlineData("add node key=key1, key, t2;")]
    [InlineData("add node key=key1, key, t2, link=l1;")]
    [InlineData("add node key=key1, key, t2, link=l1, link=t2;")]
    public void AddNodeWithReserveTagsShouldFail(string line)
    {
        Option<IReadOnlyList<IGraphQL>> result = GraphLang.Parse(line);
        result.IsError().Should().BeTrue(result.ToString());
    }

    [Theory]
    [InlineData("add node key=key1;")]
    [InlineData("add node key=key1, t2;")]
    [InlineData("add node key=key1, t1, t2;")]
    [InlineData("add node key=key1, t2, t3=v1;")]
    [InlineData("add node key=key1, t=v2, t2;")]
    [InlineData("add node key=key1, t=v2, t2=v4;")]
    [InlineData("add node key=key1, t2, link=l1;")]
    [InlineData("add node key=key1, t2, link=l1, link=t2;")]
    [InlineData("add node key=key1, data { 0xFF };")]
    [InlineData("add node key=key1, data { 0xFF }, contract { json=0x500, data=skdfajoefief };")]
    public void AddNodeAreValid(string line)
    {
        Option<IReadOnlyList<IGraphQL>> result = GraphLang.Parse(line);
        result.IsOk().Should().BeTrue(result.ToString());
    }

    [Theory]
    [InlineData("add node key=key1;", "key1", "", "")]
    [InlineData("add node key=key1, t1;", "key1", "t1", "")]
    [InlineData("add node key=key1, t1, t2;", "key1", "t1,t2", "")]
    [InlineData("add node key=key1, link=l1;", "key1", "", "l1")]
    [InlineData("add node key=key1, link=l1, link=l2;", "key1", "", "l1,l2")]
    [InlineData("add node key=key1, t1, t2, link=l1, link=l2;", "key1", "t1,t2", "l1,l2")]
    public void AddNodeValidNodes(string cmd, string key, string tags, string links)
    {
        Option<IReadOnlyList<IGraphQL>> result = GraphLang.Parse(cmd);
        result.IsOk().Should().BeTrue(result.ToString());

        IReadOnlyList<IGraphQL> list = result.Return();
        list.Count.Should().Be(1);

        if (list[0] is not GraphNodeAdd query) throw new ArgumentException("Invalid node");

        query.Key.Should().Be(key);
        query.Tags.ToTagsString().Should().Be(tags);
        query.Links.OrderBy(x => x).Join(',').Should().Be(links);
    }

    [Fact]
    public void AddSingleTag()
    {
        var q = "add node key=key1, t1;";

        Option<IReadOnlyList<IGraphQL>> result = GraphLang.Parse(q);
        result.IsOk().Should().BeTrue(result.ToString());

        IReadOnlyList<IGraphQL> list = result.Return();
        list.Count.Should().Be(1);

        if (list[0] is not GraphNodeAdd query) throw new ArgumentException("Invalid node");

        query.Key.Should().Be("key1");
        query.Tags.ToTagsString().Should().Be("t1");
    }

    [Fact]
    public void AddSingleData()
    {
        var q = "add node key=key1, entity { abc };";

        Option<IReadOnlyList<IGraphQL>> result = GraphLang.Parse(q);
        result.IsOk().Should().BeTrue(result.ToString());

        IReadOnlyList<IGraphQL> list = result.Return();
        list.Count.Should().Be(1);

        if (list[0] is not GraphNodeAdd query) throw new ArgumentException("Invalid node");

        query.Key.Should().Be("key1");
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
        var q = "add node key=key1, entity { abc }, contract { json, name=contractType, data=0xFA03ADF };";

        Option<IReadOnlyList<IGraphQL>> result = GraphLang.Parse(q);
        result.IsOk().Should().BeTrue(result.ToString());

        IReadOnlyList<IGraphQL> list = result.Return();
        list.Count.Should().Be(1);

        if (list[0] is not GraphNodeAdd query) throw new ArgumentException("Invalid node");

        query.Key.Should().Be("key1");
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

    [Fact]
    public void AddNode()
    {
        var q = "add node key=key1, t1=v1;";

        Option<IReadOnlyList<IGraphQL>> result = GraphLang.Parse(q);
        result.IsOk().Should().BeTrue(result.ToString());

        IReadOnlyList<IGraphQL> list = result.Return();
        list.Count.Should().Be(1);

        if (list[0] is not GraphNodeAdd query) throw new ArgumentException("Invalid node");

        query.Key.Should().Be("key1");
        query.Tags.ToTagsString().Should().Be("t1=v1");
    }

    [Fact]
    public void AddNodeWithTwoTags()
    {
        var q = "add node key=key1, t1=v1, t2;";

        Option<IReadOnlyList<IGraphQL>> result = GraphLang.Parse(q);
        result.IsOk().Should().BeTrue(result.ToString());

        IReadOnlyList<IGraphQL> list = result.Return();
        list.Count.Should().Be(1);

        if (list[0] is not GraphNodeAdd query) throw new ArgumentException("Invalid node");

        query.Key.Should().Be("key1");
        query.Tags.ToTagsString().Should().Be("t1=v1,t2");
    }

    [Fact]
    public void AddNodeWithTagsAndLinks()
    {
        var q = "add node key=key1, t1=v1, t2, link=a/b/c, link=schema:path1/path2/path3;";

        Option<IReadOnlyList<IGraphQL>> result = GraphLang.Parse(q);
        result.IsOk().Should().BeTrue(result.ToString());

        IReadOnlyList<IGraphQL> list = result.Return();
        list.Count.Should().Be(1);

        if (list[0] is not GraphNodeAdd query) throw new ArgumentException("Invalid node");

        query.Key.Should().Be("key1");
        query.Tags.ToTagsString().Should().Be("t1=v1,t2");
        query.Links.OrderBy(x => x).Join(',').ToString().Should().Be("a/b/c,schema:path1/path2/path3");
    }
}
