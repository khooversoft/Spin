using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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

    private readonly ITestOutputHelper _logOutput;
    public SelectInstructionSearchTests(ITestOutputHelper logOutput) => _logOutput = logOutput;

    private async Task<IHost> CreateService()
    {
        var host = Host.CreateDefaultBuilder()
            .ConfigureLogging(config => config.AddFilter(x => true).AddLambda(x => _logOutput.WriteLine(x)))
            .ConfigureServices((context, services) =>
            {
                services.AddInMemoryFileStore();
                services.AddGraphEngine(config => config.BasePath = "basePath");
            })
            .Build();

        IGraphEngine graphEngine = host.Services.GetRequiredService<IGraphEngine>();
        var context = host.Services.GetRequiredService<ILogger<NodeInstructionsIndexTests>>().ToScopeContext();
        await graphEngine.DataManager.SetMap(_map, context);

        return host;
    }

    [Fact]
    public async Task SelectAllUsers()
    {
        using var host = await CreateService();
        var context = host.Services.GetRequiredService<ILogger<AddEdgeCommandTests>>().ToScopeContext();
        var graphClient = host.Services.GetRequiredService<IGraphClient>();

        var resultOption = await graphClient.Execute("select (key=account:*) ;", context);
        resultOption.IsOk().BeTrue();

        QueryResult result = resultOption.Return();
        result.Nodes.Count.Be(2);
        result.Edges.Count.Be(0);
        result.Nodes.Select(x => x.Key).OrderBy(x => x).SequenceEqual(["account:eve", "account:sam"]).BeTrue();
    }

    [Fact]
    public async Task SelectEdgeSubset()
    {
        using var host = await CreateService();
        var context = host.Services.GetRequiredService<ILogger<AddEdgeCommandTests>>().ToScopeContext();
        var graphClient = host.Services.GetRequiredService<IGraphClient>();

        var resultOption = await graphClient.Execute("select [from=user:f*] ;", context);
        resultOption.IsOk().BeTrue();

        QueryResult result = resultOption.Return();
        result.Nodes.Count.Be(0);
        result.Edges.Count.Be(2);
        result.Edges.Select(x => x.FromKey).SequenceEqual(["user:fred", "user:fred"]).BeTrue();
    }

    [Fact]
    public async Task SelectNodesFromEdgeSubset()
    {
        using var host = await CreateService();
        var context = host.Services.GetRequiredService<ILogger<AddEdgeCommandTests>>().ToScopeContext();
        var graphClient = host.Services.GetRequiredService<IGraphClient>();

        var resultOption = await graphClient.Execute("select [from=user:f*] -> (*) ;", context);
        resultOption.IsOk().BeTrue();

        QueryResult result = resultOption.Return();
        result.Nodes.Count.Be(2);
        result.Edges.Count.Be(0);

        var nodes = result.Nodes.Select(x => x.Key).OrderBy(x => x).ToArray();
        nodes.SequenceEqual(["user:alice", "user:diana"]).BeTrue(nodes.ToString());
    }
}
