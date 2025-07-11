using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

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
    private readonly ITestOutputHelper _outputHelper;

    public SelectInstructionSearchTests(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
    }

    [Fact]
    public async Task SelectAllUsers()
    {
        using GraphHostService graphTestClient = await GraphTestStartup.CreateGraphService(_map.Clone(), logOutput: x => _outputHelper.WriteLine(x));
        var context = graphTestClient.CreateScopeContext<SelectInstructionSearchTests>();

        var resultOption = await graphTestClient.Execute("select (key=account:*) ;", context);
        resultOption.IsOk().BeTrue();

        QueryResult result = resultOption.Return();
        result.Nodes.Count.Be(2);
        result.Edges.Count.Be(0);
        result.Nodes.Select(x => x.Key).OrderBy(x => x).SequenceEqual(["account:eve", "account:sam"]).BeTrue();
    }

    [Fact]
    public async Task SelectEdgeSubset()
    {
        using GraphHostService graphTestClient = await GraphTestStartup.CreateGraphService(_map.Clone(), logOutput: x => _outputHelper.WriteLine(x));
        var context = graphTestClient.CreateScopeContext<SelectInstructionSearchTests>();

        var resultOption = await graphTestClient.Execute("select [from=user:f*] ;", context);
        resultOption.IsOk().BeTrue();

        QueryResult result = resultOption.Return();
        result.Nodes.Count.Be(0);
        result.Edges.Count.Be(2);
        result.Edges.Select(x => x.FromKey).SequenceEqual(["user:fred", "user:fred"]).BeTrue();
    }

    [Fact]
    public async Task SelectNodesFromEdgeSubset()
    {
        using GraphHostService graphTestClient = await GraphTestStartup.CreateGraphService(_map.Clone(), logOutput: x => _outputHelper.WriteLine(x));
        var context = graphTestClient.CreateScopeContext<SelectInstructionSearchTests>();

        var resultOption = await graphTestClient.Execute("select [from=user:f*] -> (*) ;", context);
        resultOption.IsOk().BeTrue();

        QueryResult result = resultOption.Return();
        result.Nodes.Count.Be(2);
        result.Edges.Count.Be(0);

        var nodes = result.Nodes.Select(x => x.Key).OrderBy(x => x).ToArray();
        nodes.SequenceEqual(["user:alice", "user:diana"]).BeTrue(nodes.ToString());
    }
}
