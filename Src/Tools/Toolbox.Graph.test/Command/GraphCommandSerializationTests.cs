using System.Collections.Immutable;
using FluentAssertions;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph.test.Command;

public class GraphCommandSerializationTests
{
    private readonly GraphMap _map = new GraphMap()
    {
        new GraphNode("node1", tags: "name=marko,age=29"),
        new GraphNode("node2", tags: "name=vadas,age=27"),
        new GraphNode("node3", tags: "name=lop,lang=java;"),
        new GraphNode("node4", tags: "name=josh,age=32,user"),
        new GraphNode("node5", tags: "name=ripple,lang=java"),
        new GraphNode("node6", tags: "name=peter,age=35"),
        new GraphNode("node7", tags: "lang=java"),

        new GraphEdge("node1", "node2", edgeType: "et1", tags: "knows,level=1"),
        new GraphEdge("node1", "node3", edgeType: "et1", tags: "knows,level=1"),
        new GraphEdge("node6", "node3", edgeType: "et1", tags: "created"),
        new GraphEdge("node4", "node5", edgeType: "et1", tags: "created"),
        new GraphEdge("node4", "node3", edgeType : "et1", tags: "created"),
        new GraphEdge("node5", "node4", edgeType : "et1", tags: "created"),
    };

    [Fact]
    public void SimpleGraphResults()
    {
        var g = new QueryBatchResult
        {
            Items = new[]
            {
                new QueryResult
                {
                    Option = StatusCode.OK,
                    QueryNumber = 1,
                    Alias = "alias1",
                },
            }.ToImmutableArray(),
        };

        string json = Json.Default.SerializePascal(g);

        QueryBatchResult r = json.ToObject<QueryBatchResult>().NotNull();
        r.Should().NotBeNull();
        r.Items.Count.Should().Be(1);
        r.Items[0].Option.StatusCode.Should().Be(StatusCode.OK);
        r.Items[0].QueryNumber.Should().Be(1);
        r.Items[0].Alias.Should().Be("alias1");
    }


    [Fact]
    public async Task SelectDirectedNodeToEdgeToNodeWithAlias()
    {
        var copyMap = _map.Clone();
        var testClient = GraphTestStartup.CreateGraphTestHost(copyMap);
        var newMapOption = await testClient.ExecuteBatch("select (*) a1 -> [*] a2 -> (*) a3 ;", NullScopeContext.Default);
        newMapOption.IsOk().Should().BeTrue(newMapOption.ToString());

        QueryBatchResult r1 = newMapOption.Return();

        string json = Json.Default.SerializePascal(r1);

        QueryBatchResult result = json.ToObject<QueryBatchResult>().NotNull();
        result.Should().NotBeNull();

        result.Option.IsOk().Should().BeTrue();
        result.Items.Count.Should().Be(3);
        result.Items.Select(x => x.Alias).Should().BeEquivalentTo("a1", "a2", "a3");

        result.Items[0].Action(x =>
        {
            x.Nodes.Select(x => x.Key).Should().BeEquivalentTo("node1", "node2", "node3", "node4", "node5", "node6", "node7");
            x.Edges.Count.Should().Be(0);
            x.DataLinks.Count.Should().Be(0);
        });

        result.Items[1].Action(x =>
        {
            x.Nodes.Count.Should().Be(0);
            x.Edges.Count.Should().Be(6);
            x.DataLinks.Count.Should().Be(0);
        });

        result.Items[2].Action(x =>
        {
            x.Nodes.Select(x => x.Key).Should().BeEquivalentTo("node2", "node3", "node4", "node5");
            x.Edges.Count.Should().Be(0);
            x.DataLinks.Count.Should().Be(0);
        });
    }
}
