using Toolbox.Extensions;
using Toolbox.Tools.Should;
using Toolbox.Types;

namespace Toolbox.Graph.test.Command;

public class SelectInstructionSearchTests
{
    private readonly GraphMap _map = new GraphMap()
    {
        new GraphNode("user:fred", tags: "name=marko,age=29"),
        new GraphNode("user:alice", tags: "name=vadas,age=27"),
        new GraphNode("user:bob", tags: "name=lop,lang=java;"),
        new GraphNode("user:charlie", tags: "name=josh,age=32,user"),
        new GraphNode("user:diana", tags: "name=ripple,lang=java"),
        new GraphNode("user:eve", tags: "name=peter,age=35"),
        new GraphNode("account:sam", tags: "name=peter,age=35"),
        new GraphNode("account:eve", tags: "name=peter,age=35"),

        new GraphEdge("user:fred", "user:alice", edgeType: "et1", tags: "knows,level=1"),
        new GraphEdge("user:alice", "user:charlie", edgeType: "et1", tags: "knows,level=1"),
        new GraphEdge("user:fred", "user:diana", edgeType: "et1", tags: "created"),
        new GraphEdge("user:bob", "user:fred", edgeType: "et1", tags: "created"),
        new GraphEdge("user:charlie", "user:fred", edgeType : "et1", tags: "created"),
        new GraphEdge("user:diana", "user:bob", edgeType : "et1", tags: "created"),
    };

    [Fact]
    public async Task SelectAllUsers()
    {
        var copyMap = _map.Clone();
        var testClient = GraphTestStartup.CreateGraphTestHost(copyMap);
        var resultOption = await testClient.Execute("select (key=account:*) ;", NullScopeContext.Default);
        resultOption.IsOk().Should().BeTrue();

        QueryResult result = resultOption.Return();
        result.Nodes.Count.Should().Be(2);
        result.Edges.Count.Should().Be(0);
        result.Nodes.Select(x => x.Key).OrderBy(x => x).SequenceEqual(["account:eve", "account:sam"]).Should().BeTrue();
    }

    [Fact]
    public async Task SelectEdgeSubset()
    {
        var copyMap = _map.Clone();
        var testClient = GraphTestStartup.CreateGraphTestHost(copyMap);
        var resultOption = await testClient.Execute("select [from=user:f*] ;", NullScopeContext.Default);
        resultOption.IsOk().Should().BeTrue();

        QueryResult result = resultOption.Return();
        result.Nodes.Count.Should().Be(0);
        result.Edges.Count.Should().Be(2);
        result.Edges.Select(x => x.FromKey).SequenceEqual(["user:fred", "user:fred"]).Should().BeTrue();
    }

    [Fact]
    public async Task SelectNodesFromEdgeSubset()
    {
        var copyMap = _map.Clone();
        var testClient = GraphTestStartup.CreateGraphTestHost(copyMap);
        var resultOption = await testClient.Execute("select [from=user:f*] -> (*) ;", NullScopeContext.Default);
        resultOption.IsOk().Should().BeTrue();

        QueryResult result = resultOption.Return();
        result.Nodes.Count.Should().Be(2);
        result.Edges.Count.Should().Be(0);

        var nodes = result.Nodes.Select(x => x.Key).ToArray();
        nodes.SequenceEqual(["user:alice", "user:diana"]).Should().BeTrue(nodes.ToString());
    }
}
