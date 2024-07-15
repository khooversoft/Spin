using FluentAssertions;
using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.Graph.test.Lang.GraphCommand;

public class GraphUpsertNodeTests
{
    [Theory]
    [InlineData("upsert node key=key1, tags=t1, t2;")]
    [InlineData("upsert node key=key1, select, t2;")]
    [InlineData("upsert node key=key1, add, t2;")]
    [InlineData("upsert node key=key1, node, t2;")]
    [InlineData("upsert node key=key1, edge, t2;")]
    [InlineData("upsert node key=key1, delete=v2, t2;")]
    [InlineData("upsert node key=key1, update, t2;")]
    [InlineData("upsert node key=key1, set, t2;")]
    [InlineData("upsert node key=key1, key, t2;")]
    [InlineData("upsert node key=key1, key, t2, link=l1;")]
    [InlineData("upsert node key=key1, key, t2, link=l1, link=t2;")]
    public void AddNodeWithReserveTagsShouldFail(string line)
    {
        Option<IReadOnlyList<IGraphQL>> result = GraphLang.Parse(line);
        result.IsError().Should().BeTrue(result.ToString());
    }

    [Theory]
    [InlineData("upsert node key=key1;")]
    [InlineData("upsert node key=key1, t2;")]
    [InlineData("upsert node key=key1, t1, t2;")]
    [InlineData("upsert node key=key1, t2, t3=v1;")]
    [InlineData("upsert node key=key1, t=v2, t2;")]
    [InlineData("upsert node key=key1, t=v2, t2=v4;")]
    [InlineData("upsert node key=key1, t2, link=l1;")]
    [InlineData("upsert node key=key1, data { 0xFF };")]
    [InlineData("upsert node key=key1, data { 0xFF }, contract { json=0x500, data=skdfajoefief };")]
    public void AddNodeAreValid(string line)
    {
        Option<IReadOnlyList<IGraphQL>> result = GraphLang.Parse(line);
        result.IsOk().Should().BeTrue(result.ToString());
    }

    [Theory]
    [InlineData("upsert node key=key1;", "key1", "")]
    [InlineData("upsert node key=key1, t1;", "key1", "t1")]
    [InlineData("upsert node key=key1, t1, t2;", "key1", "t1,t2")]
    [InlineData("upsert node key=key1, link=l1;", "key1", "link=l1")]

    public void AddNodeValidNodes(string cmd, string key, string tags)
    {
        Option<IReadOnlyList<IGraphQL>> result = GraphLang.Parse(cmd);
        result.IsOk().Should().BeTrue(result.ToString());

        IReadOnlyList<IGraphQL> list = result.Return();
        list.Count.Should().Be(1);

        if (list[0] is not GsNodeAdd query) throw new ArgumentException("Invalid node");

        query.Key.Should().Be(key);
        query.Tags.ToTagsString().Should().Be(tags);
    }

    [Fact]
    public void AddSingleTag()
    {
        var q = "upsert node key=key1, t1;";

        Option<IReadOnlyList<IGraphQL>> result = GraphLang.Parse(q);
        result.IsOk().Should().BeTrue(result.ToString());

        IReadOnlyList<IGraphQL> list = result.Return();
        list.Count.Should().Be(1);

        if (list[0] is not GsNodeAdd query) throw new ArgumentException("Invalid node");

        query.Key.Should().Be("key1");
        query.Tags.ToTagsString().Should().Be("t1");
    }

    [Fact]
    public void AddSingleData()
    {
        var q = "upsert node key=key1, entity { 'aGVsbG8=' };";

        Option<IReadOnlyList<IGraphQL>> result = GraphLang.Parse(q);
        result.IsOk().Should().BeTrue(result.ToString());

        IReadOnlyList<IGraphQL> list = result.Return();
        list.Count.Should().Be(1);

        if (list[0] is not GsNodeAdd query) throw new ArgumentException("Invalid node");

        query.Key.Should().Be("key1");
        query.Upsert.Should().BeTrue();
        query.Tags.Count.Should().Be(0);
        query.DataMap.Count.Should().Be(1);

        query.DataMap.Action(x =>
        {
            x.TryGetValue("entity", out var entity).Should().BeTrue();
            entity!.Validate().IsOk().Should().BeTrue();
            entity!.Data64.Should().Be("aGVsbG8=");
        });
    }

    [Fact]
    public void AddTwoData()
    {
        var q = "upsert node key=key1, entity { 'aGVsbG8=' }, contract { schema=xml, typeName=contractType, data64='aGVsbG8=' };";

        Option<IReadOnlyList<IGraphQL>> result = GraphLang.Parse(q);
        result.IsOk().Should().BeTrue(result.ToString());

        IReadOnlyList<IGraphQL> list = result.Return();
        list.Count.Should().Be(1);

        if (list[0] is not GsNodeAdd query) throw new ArgumentException("Invalid node");

        query.Key.Should().Be("key1");
        query.Upsert.Should().BeTrue();
        query.Tags.Count.Should().Be(0);
        query.DataMap.Count.Should().Be(2);

        query.DataMap.Action(x =>
        {
            x.TryGetValue("entity", out var entity).Should().BeTrue();
            entity!.Validate().IsOk().Should().BeTrue();
            entity!.TypeName.Should().Be("default");
            entity.Schema.Should().Be("json");
            entity.Data64.Should().Be("aGVsbG8=");
        });

        query.DataMap.Action(x =>
        {
            x.TryGetValue("contract", out var entity).Should().BeTrue();
            entity!.Validate().IsOk().Should().BeTrue();

            entity!.TypeName.Should().Be("contractType");
            entity.Schema.Should().Be("xml");
            entity.Data64.Should().Be("aGVsbG8=");
        });
    }

    [Fact]
    public void ComplextUpsert()
    {
        var q = """
            upsert node key=user:user001, userEmail=user@domain.com,Name=name1-user001, entity { 'eyJpZCI6InVzZXIwMDEiLCJ1c2VyTmFtZSI6IlVzZXIgbmFtZSIsImVtYWlsIjoidXNlckBkb21haW4uY29tIiwibm9ybWFsaXplZFVzZXJOYW1lIjoidXNlcjAwMS1ub3JtYWxpemVkIiwiZW1haWxDb25maXJtZWQiOnRydWUsInBhc3N3b3JkSGFzaCI6InBhc3N3b3JkSGFzaCIsIm5hbWUiOiJuYW1lMS11c2VyMDAxIiwibG9naW5Qcm92aWRlciI6Im1pY3Jvc29mdCIsInByb3ZpZGVyS2V5IjoidXNlcjAwMS1taWNyb3NvZnQtaWQifQ==' };
            """;

        Option<IReadOnlyList<IGraphQL>> result = GraphLang.Parse(q);
        result.IsOk().Should().BeTrue(result.ToString());
    }

    [Fact]
    public void AddNode()
    {
        var q = "upsert node key=key1, t1=v1;";

        Option<IReadOnlyList<IGraphQL>> result = GraphLang.Parse(q);
        result.IsOk().Should().BeTrue(result.ToString());

        IReadOnlyList<IGraphQL> list = result.Return();
        list.Count.Should().Be(1);

        if (list[0] is not GsNodeAdd query) throw new ArgumentException("Invalid node");

        query.Key.Should().Be("key1");
        query.Tags.ToTagsString().Should().Be("t1=v1");
    }

    [Fact]
    public void AddNodeWithTwoTags()
    {
        var q = "upsert node key=key1, t1=v1, t2;";

        Option<IReadOnlyList<IGraphQL>> result = GraphLang.Parse(q);
        result.IsOk().Should().BeTrue(result.ToString());

        IReadOnlyList<IGraphQL> list = result.Return();
        list.Count.Should().Be(1);

        if (list[0] is not GsNodeAdd query) throw new ArgumentException("Invalid node");

        query.Key.Should().Be("key1");
        query.Tags.ToTagsString().Should().Be("t1=v1,t2");
    }

    [Fact]
    public void Upsert()
    {
        var q = "upsert node key=node3, contract { 'aGVsbG8=' };";

        Option<IReadOnlyList<IGraphQL>> result = GraphLang.Parse(q);
        result.IsOk().Should().BeTrue(result.ToString());

        IReadOnlyList<IGraphQL> list = result.Return();
        list.Count.Should().Be(1);

        if (list[0] is not GsNodeAdd query) throw new ArgumentException("Invalid node");

        query.Tags.Count.Should().Be(0);
        query.DataMap.Count.Should().Be(1);

        query.DataMap.Action(x =>
        {
            x.TryGetValue("contract", out var entity).Should().BeTrue();
            entity!.Validate().IsOk().Should().BeTrue();

            entity!.TypeName.Should().Be("default");
            entity.Schema.Should().Be("json");
            entity.Data64.Should().Be("aGVsbG8=");
        });
    }

    [Fact]
    public void UpsertWithTwoData()
    {
        var q = "upsert node key=key1, t1, entity { 'VGhpcyBpcyBhIHRlc3Q=' }, contract { schema=json, typeName=contractType, data64='aGVsbG8=' };";

        Option<IReadOnlyList<IGraphQL>> result = GraphLang.Parse(q);
        result.IsOk().Should().BeTrue(result.ToString());

        IReadOnlyList<IGraphQL> list = result.Return();
        list.Count.Should().Be(1);

        if (list[0] is not GsNodeAdd query) throw new ArgumentException("Invalid node");

        query.Tags.Count.Should().Be(1);
        query.Tags.ToTagsString().Should().Be("t1");
        query.DataMap.Count.Should().Be(2);

        query.DataMap.Action(x =>
        {
            x.TryGetValue("entity", out var entity).Should().BeTrue();
            entity!.Validate().IsOk().Should().BeTrue();
            entity!.TypeName.Should().Be("default");
            entity.Schema.Should().Be("json");
            entity.Data64.Should().Be("VGhpcyBpcyBhIHRlc3Q=");
        });

        query.DataMap.Action(x =>
        {
            x.TryGetValue("contract", out var entity).Should().BeTrue();
            entity!.Validate().IsOk().Should().BeTrue();

            entity!.TypeName.Should().Be("contractType");
            entity.Schema.Should().Be("json");
            entity.Data64.Should().Be("aGVsbG8=");
        });
    }
}
