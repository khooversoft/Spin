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
            update (key=key1) set key=key2,tags=t2;
            delete [schedulework:active] a1;
            select (Key=k4) a1;
            select [fromKey=k2] a1;
            """;

        Option<IReadOnlyList<IGraphQL>> result = GraphLang.Parse(q);
        result.IsOk().Should().BeTrue(result.ToString());

        IReadOnlyList<IGraphQL> list = result.Return();
        list.Count.Should().Be(6);

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

            query.Key.Should().Be("key2");
            query.Tags.Should().Be("t2");
            query.Search[0].Cast<GraphNodeSelect>().Action(x =>
            {
                x.Key.Should().Be("key1");
                x.Tags.Should().BeNull();
                x.Alias.Should().BeNull();
            });
        });

        list[index++].Action(x =>
        {
            if (x is not GraphEdgeDelete query) throw new ArgumentException("Invalid type");

            query.FromKey.Should().BeNull();
            query.ToKey.Should().BeNull();
            query.EdgeType.Should().BeNull();
            query.Tags.Should().Be("schedulework:active");
        });

        list[index++].Action(x =>
        {
            if (x is not GraphNodeSelect query) throw new ArgumentException("Invalid type");

            query.Key.Should().Be("k4");
            query.Tags.Should().BeNull();
        });

        list[index++].Action(x =>
        {
            if (x is not GraphEdgeSelect query) throw new ArgumentException("Invalid type");

            query.FromKey.Should().Be("k2");
            query.ToKey.Should().BeNull();
            query.EdgeType.Should().BeNull();
            query.Tags.Should().BeNull();
        });
    }
}
