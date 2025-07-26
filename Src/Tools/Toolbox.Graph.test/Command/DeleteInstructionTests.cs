using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Graph.test.Application;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Graph.test.Command;

public class DeleteInstructionTests
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
    };

    private readonly ITestOutputHelper _logOutput;
    public DeleteInstructionTests(ITestOutputHelper logOutput) => _logOutput = logOutput;

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
        var context = host.Services.GetRequiredService<ILogger<DeleteInstructionTests>>().ToScopeContext();
        await graphEngine.DataManager.SetMap(_map, context);

        return host;
    }


    [Fact]
    public async Task DeleteAllNode()
    {
        using var host = await CreateService();
        var context = host.Services.GetRequiredService<ILogger<DeleteInstructionTests>>().ToScopeContext();
        var graphClient = host.Services.GetRequiredService<IGraphClient>();
        var graphEngine = host.Services.GetRequiredService<IGraphEngine>();

        var newMapOption = await graphClient.Execute("delete (*) ;", context);
        newMapOption.IsOk().BeTrue();

        QueryResult result = newMapOption.Return();
        result.Option.IsOk().BeTrue();
        result.Nodes.Count.Be(7);
        result.Edges.Count.Be(0);
        result.DataLinks.Count.Be(0);

        graphEngine.DataManager.GetMap().Nodes.Count.Be(0);
        graphEngine.DataManager.GetMap().Edges.Count.Be(0);
    }

    [Fact]
    public async Task DeleteAllEdges()
    {
        using var host = await CreateService();
        var context = host.Services.GetRequiredService<ILogger<DeleteInstructionTests>>().ToScopeContext();
        var graphClient = host.Services.GetRequiredService<IGraphClient>();
        var graphEngine = host.Services.GetRequiredService<IGraphEngine>();

        var newMapOption = await graphClient.Execute("delete [*] ;", context);
        newMapOption.IsOk().BeTrue();

        QueryResult result = newMapOption.Return();
        result.Option.IsOk().BeTrue();
        result.Nodes.Count.Be(0);
        result.Edges.Count.Be(5);
        result.DataLinks.Count.Be(0);

        graphEngine.DataManager.GetMap().Nodes.Count.Be(7);
        graphEngine.DataManager.GetMap().Edges.Count.Be(0);
    }

    [Fact]
    public async Task DeleteNode()
    {
        using var host = await CreateService();
        var context = host.Services.GetRequiredService<ILogger<DeleteInstructionTests>>().ToScopeContext();
        var graphClient = host.Services.GetRequiredService<IGraphClient>();
        var graphEngine = host.Services.GetRequiredService<IGraphEngine>();

        var newMapOption = await graphClient.Execute("delete (key=node2) a1 ;", context);
        newMapOption.IsOk().BeTrue();

        QueryResult result = newMapOption.Return();
        result.Option.IsOk().BeTrue();
        result.Alias.Be("a1");
        result.Nodes.Count.Be(1);
        result.Edges.Count.Be(0);
        result.DataLinks.Count.Be(0);

        var compareMap = GraphCommandTools.CompareMap(_map, graphEngine.DataManager.GetMap());

        compareMap.Count.Be(2);
        compareMap.OfType<GraphNode>().Single().Action(x =>
        {
            x.Key.Be("node2");
        });
        compareMap.OfType<GraphEdge>().Single().Action(x =>
        {
            x.FromKey.Be("node1");
            x.ToKey.Be("node2");
        });
    }

    [Fact]
    public async Task DeleteEdgeFromNode()
    {
        using var host = await CreateService();
        var context = host.Services.GetRequiredService<ILogger<DeleteInstructionTests>>().ToScopeContext();
        var graphClient = host.Services.GetRequiredService<IGraphClient>();
        var graphEngine = host.Services.GetRequiredService<IGraphEngine>();

        var newMapOption = await graphClient.Execute("delete (key=node6) -> [*] ;", context);
        newMapOption.IsOk().BeTrue();

        QueryResult result = newMapOption.Return();
        result.Option.IsOk().BeTrue();
        result.Nodes.Count.Be(0);
        result.Edges.Count.Be(1);
        result.DataLinks.Count.Be(0);

        var compareMap = GraphCommandTools.CompareMap(_map, graphEngine.DataManager.GetMap());

        compareMap.Count.Be(1);
        compareMap.OfType<GraphEdge>().Single().Action(x =>
        {
            x.FromKey.Be("node6");
            x.ToKey.Be("node3");
        });
    }

    [Fact]
    public async Task DeleteNodeFromEdgeFromNode()
    {
        using var host = await CreateService();
        var context = host.Services.GetRequiredService<ILogger<DeleteInstructionTests>>().ToScopeContext();
        var graphClient = host.Services.GetRequiredService<IGraphClient>();
        var graphEngine = host.Services.GetRequiredService<IGraphEngine>();

        var newMapOption = await graphClient.Execute("delete (key=node6) -> [*] -> (*) ;", context);
        newMapOption.IsOk().BeTrue();

        QueryResult result = newMapOption.Return();
        result.Option.IsOk().BeTrue();
        result.Nodes.Count.Be(1);
        result.Edges.Count.Be(0);
        result.DataLinks.Count.Be(0);

        var compareMap = GraphCommandTools.CompareMap(_map, graphEngine.DataManager.GetMap());

        compareMap.Count.Be(4);
        compareMap.OfType<GraphNode>().Single().Action(x =>
        {
            x.Key.Be("node3");
        });

        var expect = new (string from, string to)[]
        {
            ("node1", "node3"),
            ("node4", "node3"),
            ("node6", "node3"),
        };

        var resultList = compareMap.OfType<GraphEdge>()
            .Select(x => (x.FromKey, x.ToKey))
            .OrderBy(x => x);

        Enumerable.SequenceEqual(expect, resultList).BeTrue();
    }
}
