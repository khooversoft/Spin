//using FluentAssertions;
//using Toolbox.Extensions;
//using Toolbox.Types;

//namespace Toolbox.Graph.test.Lang.GraphCommand;

//public class GraphUpdateNodeTests
//{
//    [Theory]
//    [InlineData("update (key=key1, tags=t1) set tags=t2;")]
//    [InlineData("update (key=key1, tags=t, t1) set tags=t2;")]
//    [InlineData("update (key=key1, add, t2) set tags=t2;")]
//    [InlineData("update (key=key1, node, t2) set tags=t2;")]
//    [InlineData("update (key=key1, edge, t2) set tags=t2;")]
//    [InlineData("update (key=key1, delete=v2, t2) set tags=t2;")]
//    [InlineData("update (key=key1, update, t2) set tags=t2;")]
//    [InlineData("update (key=key1, set) set tags=t2;")]
//    [InlineData("update (key=key1, set, t2) set tags=t2;")]
//    [InlineData("update (key=key1, key, t2) set tags=t2;")]
//    [InlineData("update (key=key1, key, t2) set data { 0xFF };")]
//    [InlineData("update (key=key1, key, t2) set data { 0xFF }, contract { json=0x500, data=skdfajoefief };")]
//    public void AddNodeWithReserveTags(string line)
//    {
//        Option<IReadOnlyList<IGraphQL>> result = GraphLang.Parse(line);
//        result.IsError().Should().BeTrue(result.ToString());
//    }

//    [Fact]
//    public void UpdateNodeSimple()
//    {
//        var q = "update (key=key1) set t2;";

//        Option<IReadOnlyList<IGraphQL>> result = GraphLang.Parse(q);
//        result.IsOk().Should().BeTrue(result.ToString());

//        IReadOnlyList<IGraphQL> list = result.Return();
//        list.Count.Should().Be(1);

//        int index = 0;
//        list[index++].Action(x =>
//        {
//            if (x is not GsNodeUpdate query) throw new ArgumentException("Invalid type");

//            query.Tags.ToTagsString().Should().Be("t2");
//            query.Search.Count.Should().Be(1);
//            query.Search.Count.Should().Be(1);

//            query.Search[0].Cast<GraphNodeSearch>().Action(x =>
//            {
//                x.Key.Should().Be("key1");
//                x.Tags.Count.Should().Be(0);
//            });
//        });
//    }

//    [Fact]
//    public void updateNode()
//    {
//        var q = "update (key=key1, t1) set t2;";

//        Option<IReadOnlyList<IGraphQL>> result = GraphLang.Parse(q);
//        result.IsOk().Should().BeTrue(result.ToString());

//        IReadOnlyList<IGraphQL> list = result.Return();
//        list.Count.Should().Be(1);

//        int index = 0;
//        list[index++].Action(x =>
//        {
//            if (x is not GsNodeUpdate query) throw new ArgumentException("Invalid type");

//            query.Tags.ToTagsString().Should().Be("t2");
//            query.Search.Count.Should().Be(1);
//            query.Search.Count.Should().Be(1);

//            query.Search[0].Cast<GraphNodeSearch>().Action(x =>
//            {
//                x.Key.Should().Be("key1");
//                x.Tags.ToTagsString().Should().Be("t1");
//            });
//        });
//    }

//    [Fact]
//    public void AddSingleData()
//    {
//        var q = "update (key=key1, t1) set entity { 'VGhpcyBpcyBhIHRlc3Q=' };";

//        Option<IReadOnlyList<IGraphQL>> result = GraphLang.Parse(q);
//        result.IsOk().Should().BeTrue(result.ToString());

//        IReadOnlyList<IGraphQL> list = result.Return();
//        list.Count.Should().Be(1);

//        if (list[0] is not GsNodeUpdate query) throw new ArgumentException("Invalid node");

//        query.Tags.Count.Should().Be(0);
//        query.DataMap.Count.Should().Be(1);

//        query.DataMap.Action(x =>
//        {
//            x.TryGetValue("entity", out var entity).Should().BeTrue();
//            entity!.Validate().IsOk().Should().BeTrue();
//            entity!.Data64.Should().Be("VGhpcyBpcyBhIHRlc3Q=");
//        });
//    }

//    [Fact]
//    public void AddTwoData()
//    {
//        var q = "update (key=key1, t1) set entity { 'VGhpcyBpcyBhIHRlc3Q=' }, contract { schema=json, typeName=contractType, data64='aGVsbG8=' };";

//        Option<IReadOnlyList<IGraphQL>> result = GraphLang.Parse(q);
//        result.IsOk().Should().BeTrue(result.ToString());

//        IReadOnlyList<IGraphQL> list = result.Return();
//        list.Count.Should().Be(1);

//        if (list[0] is not GsNodeUpdate query) throw new ArgumentException("Invalid node");

//        query.Tags.Count.Should().Be(0);
//        query.DataMap.Count.Should().Be(2);

//        query.DataMap.Action(x =>
//        {
//            x.TryGetValue("entity", out var entity).Should().BeTrue();
//            entity!.Validate().IsOk().Should().BeTrue();
//            entity!.Data64.Should().Be("VGhpcyBpcyBhIHRlc3Q=");
//        });

//        query.DataMap.Action(x =>
//        {
//            x.TryGetValue("contract", out var entity).Should().BeTrue();
//            entity!.Validate().IsOk().Should().BeTrue();

//            entity!.Data64.Should().Be("aGVsbG8=");
//        });
//    }
//}
