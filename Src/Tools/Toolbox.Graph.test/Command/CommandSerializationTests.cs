using System.Collections.Immutable;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph.test.Command;

public class CommandSerializationTests
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
        r.NotNull();
        r.Items.Count.Be(1);
        r.Items[0].Option.StatusCode.Be(StatusCode.OK);
        r.Items[0].QueryNumber.Be(1);
        r.Items[0].Alias.Be("alias1");
    }


    [Fact]
    public async Task SelectDirectedNodeToEdgeToNodeWithAlias()
    {
        using GraphHostService testClient = await GraphTestStartup.CreateGraphService(_map.Clone());

        var newMapOption = await testClient.ExecuteBatch("select (*) a1 -> [*] a2 -> (*) a3 ;", NullScopeContext.Instance);
        newMapOption.IsOk().BeTrue();

        QueryBatchResult r1 = newMapOption.Return();

        string json = Json.Default.SerializePascal(r1);

        QueryBatchResult result = json.ToObject<QueryBatchResult>().NotNull();
        result.NotNull();

        result.Option.IsOk().BeTrue();
        result.Items.Count.Be(3);
        result.Items.Select(x => x.Alias).SequenceEqual(["a1", "a2", "a3"]).BeTrue();

        result.Items[0].Action(x =>
        {
            x.Nodes.Select(x => x.Key).OrderBy(x => x).SequenceEqual(["node1", "node2", "node3", "node4", "node5", "node6", "node7"]).BeTrue();
            x.Edges.Count.Be(0);
            x.DataLinks.Count.Be(0);
        });

        result.Items[1].Action(x =>
        {
            x.Nodes.Count.Be(0);
            x.Edges.Count.Be(6);
            x.DataLinks.Count.Be(0);
        });

        result.Items[2].Action(x =>
        {
            x.Nodes.Select(x => x.Key).OrderBy(x => x).SequenceEqual(["node2", "node3", "node4", "node5"]).BeTrue();
            x.Edges.Count.Be(0);
            x.DataLinks.Count.Be(0);
        });
    }
}
