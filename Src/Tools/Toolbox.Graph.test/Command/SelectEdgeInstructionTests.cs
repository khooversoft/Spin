using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Graph.test.Command;

public class SelectEdgeInstructionTests
{
    private GraphMap _map = null!;

    private static GraphMap CreateGraphMap(IHost host)
    {
        ILogger<GraphMap> logger = host.Services.GetRequiredService<ILogger<GraphMap>>();

        return new GraphMap(logger)
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
            new GraphEdge("node6", "node3", edgeType: "et3", tags: "created"),
            new GraphEdge("node4", "node5", edgeType: "et3", tags: "created"),
            new GraphEdge("node4", "node3", edgeType : "et3", tags: "created"),
            new GraphEdge("node5", "node4", edgeType : "et2", tags: "created"),
        };
    }

    private readonly ITestOutputHelper _logOutput;
    public SelectEdgeInstructionTests(ITestOutputHelper logOutput) => _logOutput = logOutput;

    private async Task<IHost> CreateService()
    {
        var host = Host.CreateDefaultBuilder()
            .AddDebugLogging(x => _logOutput.WriteLine(x))
            .ConfigureServices((context, services) =>
            {
                services.AddInMemoryKeyStore();
                services.AddGraphEngine(config => config.BasePath = "basePath");
            })
            .Build();

        _map = CreateGraphMap(host);

        IGraphEngine graphEngine = host.Services.GetRequiredService<IGraphEngine>();
        await graphEngine.DataManager.SetMap(_map);

        return host;
    }

    [Fact]
    public async Task SelectAllEdges()
    {
        using var host = await CreateService();
        var graphClient = host.Services.GetRequiredService<IGraphClient>();
        var graphEngine = host.Services.GetRequiredService<IGraphEngine>();

        var newMapOption = await graphClient.Execute("select [*] ;");
        newMapOption.IsOk().BeTrue();

        QueryResult result = newMapOption.Return();
        result.Option.IsOk().BeTrue();
        result.Alias.NotEmpty();
        result.Nodes.Count.Be(0);
        result.Edges.Count.Be(6);
        result.DataLinks.Count.Be(0);

        var expected = new (string FromKey, string ToKey, string EdgeType)[]
        {
            ("node1", "node2", "et1"),
            ("node1", "node3", "et1"),
            ("node6", "node3", "et3"),
            ("node4", "node5", "et3"),
            ("node4", "node3", "et3"),
            ("node5", "node4", "et2"),
        }.OrderBy(x => x).ToArray();

        result.Edges.Select(x => (x.FromKey, x.ToKey, x.EdgeType)).OrderBy(x => x).SequenceEqual(expected).BeTrue();
    }

    [Fact]
    public async Task SelectAllEdgesWithTag()
    {
        using var host = await CreateService();
        var graphClient = host.Services.GetRequiredService<IGraphClient>();
        var graphEngine = host.Services.GetRequiredService<IGraphEngine>();

        var newMapOption = await graphClient.Execute("select [*, level=1] ;");
        newMapOption.IsOk().BeTrue();

        QueryResult result = newMapOption.Return();
        result.Option.IsOk().BeTrue();
        result.Alias.NotEmpty();
        result.Nodes.Count.Be(0);
        result.Edges.Count.Be(2);
        result.DataLinks.Count.Be(0);

        var expected = new (string FromKey, string ToKey, string EdgeType)[]
        {
            ("node1", "node2", "et1"),
            ("node1", "node3", "et1"),
        }.OrderBy(x => x).ToArray();

        result.Edges.Select(x => (x.FromKey, x.ToKey, x.EdgeType)).OrderBy(x => x).SequenceEqual(expected).BeTrue();
    }

    [Fact]
    public async Task SelectEdgesWithTag()
    {
        using var host = await CreateService();
        var graphClient = host.Services.GetRequiredService<IGraphClient>();
        var graphEngine = host.Services.GetRequiredService<IGraphEngine>();

        var newMapOption = await graphClient.Execute("select [level=1] ;");
        newMapOption.IsOk().BeTrue();

        QueryResult result = newMapOption.Return();
        result.Option.IsOk().BeTrue();
        result.Alias.NotEmpty();
        result.Nodes.Count.Be(0);
        result.Edges.Count.Be(2);
        result.DataLinks.Count.Be(0);

        var expected = new (string FromKey, string ToKey, string EdgeType)[]
        {
            ("node1", "node2", "et1"),
            ("node1", "node3", "et1"),
        }.OrderBy(x => x).ToArray();

        result.Edges.Select(x => (x.FromKey, x.ToKey, x.EdgeType)).OrderBy(x => x).SequenceEqual(expected).BeTrue();
    }

    [Fact]
    public async Task SelectEdgesByFrom()
    {
        using var host = await CreateService();
        var graphClient = host.Services.GetRequiredService<IGraphClient>();
        var graphEngine = host.Services.GetRequiredService<IGraphEngine>();

        var newMapOption = await graphClient.Execute("select [from=node1] ;");
        newMapOption.IsOk().BeTrue();

        QueryResult result = newMapOption.Return();
        result.Option.IsOk().BeTrue();
        result.Alias.NotEmpty();
        result.Nodes.Count.Be(0);
        result.Edges.Count.Be(2);
        result.DataLinks.Count.Be(0);

        var expected = new (string FromKey, string ToKey, string EdgeType)[]
        {
            ("node1", "node2", "et1"),
            ("node1", "node3", "et1"),
        }.OrderBy(x => x).ToArray();

        result.Edges.Select(x => (x.FromKey, x.ToKey, x.EdgeType)).OrderBy(x => x).SequenceEqual(expected).BeTrue();
    }

    [Fact]
    public async Task SelectEdgesByTo()
    {
        using var host = await CreateService();
        var graphClient = host.Services.GetRequiredService<IGraphClient>();
        var graphEngine = host.Services.GetRequiredService<IGraphEngine>();

        var newMapOption = await graphClient.Execute("select [to=node3] ;");
        newMapOption.IsOk().BeTrue();

        QueryResult result = newMapOption.Return();
        result.Option.IsOk().BeTrue();
        result.Alias.NotEmpty();
        result.Nodes.Count.Be(0);
        result.Edges.Count.Be(3);
        result.DataLinks.Count.Be(0);

        var expected = new (string FromKey, string ToKey, string EdgeType)[]
        {
            ("node1", "node3", "et1"),
            ("node6", "node3", "et3"),
            ("node4", "node3", "et3"),
        }.OrderBy(x => x).ToArray();

        result.Edges.Select(x => (x.FromKey, x.ToKey, x.EdgeType)).OrderBy(x => x).SequenceEqual(expected).BeTrue();
    }

    [Fact]
    public async Task SelectEdgesByEdgeType()
    {
        using var host = await CreateService();
        var graphClient = host.Services.GetRequiredService<IGraphClient>();
        var graphEngine = host.Services.GetRequiredService<IGraphEngine>();

        var newMapOption = await graphClient.Execute("select [type=et3] ;");
        newMapOption.IsOk().BeTrue();

        QueryResult result = newMapOption.Return();
        result.Option.IsOk().BeTrue();
        result.Alias.NotEmpty();
        result.Nodes.Count.Be(0);
        result.Edges.Count.Be(3);
        result.DataLinks.Count.Be(0);

        var expected = new (string FromKey, string ToKey, string EdgeType)[]
        {
            ("node6", "node3", "et3"),
            ("node4", "node5", "et3"),
            ("node4", "node3", "et3"),
        }.OrderBy(x => x).ToArray();

        result.Edges.Select(x => (x.FromKey, x.ToKey, x.EdgeType)).OrderBy(x => x).SequenceEqual(expected).BeTrue();
    }
}
