using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Graph.test.Command;

public class SelectNodeBatchTests
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

    private readonly ITestOutputHelper _logOutput;
    public SelectNodeBatchTests(ITestOutputHelper logOutput) => _logOutput = logOutput;

    private async Task<IHost> CreateService()
    {
        var host = Host.CreateDefaultBuilder()
            .ConfigureLogging(config =>
            {
                //config.AddFilter(x => true);
                config.AddLambda(x => _logOutput.WriteLine(x));
            })
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
    public async Task SelectBatchNodeWithAlias()
    {
        using var host = await CreateService();
        var context = host.Services.GetRequiredService<ILogger<AddEdgeCommandTests>>().ToScopeContext();
        var graphClient = host.Services.GetRequiredService<IGraphClient>();
        var collector = host.Services.GetRequiredService<GraphMapCounter>();

        var cmd = new string[]
        {
            "select (key=node2) a1 ;",
            "select (key=node3) a2 ;",
            "select (key=node4) a3 ;"
        }.Join(Environment.NewLine);

        QueryBatchResult result = (await graphClient.ExecuteBatch(cmd, context)).BeOk().Return();

        result.Items.Count.Be(3);
        result.Items.ForEach(x =>
        {
            x.Nodes.Select(x => x.Key).SequenceEqual(new[] { "node2", "node3", "node4" });
            x.Edges.Count.Be(0);
            x.DataLinks.Count.Be(0);
        });
        result.Items.Select(x => x.Alias).SequenceEqual(new[] { "a1", "a2", "a3" }).BeTrue();
    }

    [Fact]
    public async Task SelectBatchNodeWithOutAlias()
    {
        using var host = await CreateService();
        var context = host.Services.GetRequiredService<ILogger<AddEdgeCommandTests>>().ToScopeContext();
        var graphClient = host.Services.GetRequiredService<IGraphClient>();
        var collector = host.Services.GetRequiredService<GraphMapCounter>();

        var cmd = new string[]
        {
            "select (key=node2) ;",
            "select (key=node3) ;",
            "select (key=node4) ;"
        }.Join(Environment.NewLine);

        QueryBatchResult result = (await graphClient.ExecuteBatch(cmd, context)).BeOk().Return();

        result.Items.Count.Be(1);
        result.Items.ForEach(x =>
        {
            x.Alias.NotEmpty();
            x.Nodes.Select(x => x.Key).SequenceEqual(new[] { "node4" });
            x.Edges.Count.Be(0);
            x.DataLinks.Count.Be(0);
        });
    }
}
