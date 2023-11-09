using FluentAssertions;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.Test.Data.Lang;

public class GraphBatchTests
{
    [Fact]
    public void FullBatch()
    {
        string q = """
            add node key=key1,tags=t1;
            add edge fromKey=key1,toKey=key2,edgeType=et,tags=t2;
            update (key=key1) set tags=t2;
            update (key=key1) -> [t2] set tags=t2;
            delete [schedulework:active] a1;
            delete [schedulework:active] -> (key=k1) a2;
            select (Key=k4) a1;
            select [fromKey=k2] a1;
            """;

        Option<IReadOnlyList<IGraphQL>> result = GraphLang.Parse(q);
        result.IsOk().Should().BeTrue(result.ToString());

        IReadOnlyList<IGraphQL> list = result.Return();
        list.Count.Should().Be(8);

        int index = 0;
        list[index++].Action(x =>
        {
            if (x is not GraphNodeAdd query) throw new ArgumentException("Invalid node");

            query.Key.Should().Be("key1");
            query.Tags.Should().Be("t1");
        });

        list[index++].Action(x =>
        {
            if (x is not GraphEdgeAdd query) throw new ArgumentException("Invalid node");

            query.FromKey.Should().Be("key1");
            query.ToKey.Should().Be("key2");
            query.EdgeType.Should().Be("et");
            query.Tags.Should().Be("t2");
        });

        list[index++].Action(x =>
        {
            if (x is not GraphNodeUpdate query) throw new ArgumentException("Invalid node");

            query.Tags.Should().Be("t2");
            query.Search[0].Cast<GraphNodeSearch>().Action(x =>
            {
                x.Key.Should().Be("key1");
                x.Tags.Should().BeNull();
                x.Alias.Should().BeNull();
            });
        });

        list[index++].Action(x =>
        {
            if (x is not GraphEdgeUpdate query) throw new ArgumentException("Invalid node");

            query.Tags.Should().Be("t2");

            query.Search.Count.Should().Be(2);
            query.Search[0].Cast<GraphNodeSearch>().Action(x =>
            {
                x.Key.Should().Be("key1");
                x.Tags.Should().BeNull();
                x.Alias.Should().BeNull();
            });
            query.Search[1].Cast<GraphEdgeSearch>().Action(x =>
            {
                x.FromKey.Should().BeNull();
                x.ToKey.Should().BeNull();
                x.EdgeType.Should().BeNull();
                x.Tags.Should().Be("t2");
                x.Alias.Should().BeNull();
            });
        });

        list[index++].Action(x =>
        {
            if (x is not GraphEdgeDelete query) throw new ArgumentException("Invalid type");

            var idx = query.Search.ToCursor();
            query.Search.Count.Should().Be(1);
            idx.NextValue().Return().Cast<GraphEdgeSearch>().Action(x =>
            {
                x.FromKey.Should().BeNull();
                x.ToKey.Should().BeNull();
                x.EdgeType.Should().BeNull();
                x.Tags.Should().Be("schedulework:active");
                x.Alias.Should().Be("a1");
            });
        });

        list[index++].Action(x =>
        {
            if (x is not GraphNodeDelete query) throw new ArgumentException("Invalid type");

            var idx = query.Search.ToCursor();
            query.Search.Count.Should().Be(2);

            idx.NextValue().Return().Cast<GraphEdgeSearch>().Action(x =>
            {
                x.FromKey.Should().BeNull();
                x.ToKey.Should().BeNull();
                x.EdgeType.Should().BeNull();
                x.Tags.Should().Be("schedulework:active");
                x.Alias.Should().BeNull();
            });

            idx.NextValue().Return().Cast<GraphNodeSearch>().Action(x =>
            {
                x.Key.Should().Be("k1");
                x.Tags.Should().BeNull();
                x.Alias.Should().Be("a2");
            });
        });

        list[index++].Action(x =>
        {
            if (x is not GraphSelect query) throw new ArgumentException("Invalid type");

            var cursor = query.Search.ToCursor();
            cursor.List.Count.Should().Be(1);

            cursor.NextValue().Return().Cast<GraphNodeSearch>().Action(x =>
            {
                x.Key.Should().Be("k4");
                x.Tags.Should().BeNull();
            });
        });

        list[index++].Action(x =>
        {
            if (x is not GraphSelect query) throw new ArgumentException("Invalid type");

            var cursor = query.Search.ToCursor();
            cursor.List.Count.Should().Be(1);

            cursor.NextValue().Return().Cast<GraphEdgeSearch>().Action(x =>
            {
                x.FromKey.Should().Be("k2");
                x.ToKey.Should().BeNull();
                x.EdgeType.Should().BeNull();
                x.Tags.Should().BeNull();
            });
        });
    }
}
