using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Graph.test.Command;

public class AddUniqueIndexTests
{
    private readonly ITestOutputHelper _logOutput;
    public AddUniqueIndexTests(ITestOutputHelper logOutput) => _logOutput = logOutput;

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
        var context = host.Services.GetRequiredService<ILogger<AddUniqueIndexTests>>().ToScopeContext();
        await graphEngine.DataManager.LoadDatabase(context);

        return host;
    }

    [Fact]
    public async Task AddMultipleIndexes()
    {
        using var host = await CreateService();
        var context = host.Services.GetRequiredService<ILogger<AddUniqueIndexTests>>().ToScopeContext();
        var graphClient = host.Services.GetRequiredService<IGraphClient>();
        var graphEngine = host.Services.GetRequiredService<IGraphEngine>();

        var e1 = await graphClient.Execute("add node key=node1 set t1=v1, t2=v2 index t1, t2 ;", context);
        e1.IsOk().BeTrue();

        graphEngine.DataManager.GetMap().Nodes.LookupIndex("t1", "v1").Action(x =>
        {
            x.IsOk().BeTrue();
            x.Return().NodeKey.Be("node1");
        });

        graphEngine.DataManager.GetMap().Nodes.LookupIndex("t3", "v3").IsOk().BeFalse();

        var e2 = await graphClient.Execute("add node key=node2 set t3=v3 index t3 ;", context);
        e2.IsOk().BeTrue();

        graphEngine.DataManager.GetMap().Nodes.LookupIndex("t1", "v1").Action(x =>
        {
            x.IsOk().BeTrue();
            x.Return().NodeKey.Be("node1");
        });

        graphEngine.DataManager.GetMap().Nodes.LookupIndex("t3", "v3").Action(x =>
        {
            x.IsOk().BeTrue();
            x.Return().NodeKey.Be("node2");
        });

        graphEngine.DataManager.GetMap().Nodes.LookupIndex("t3", "v1").IsOk().BeFalse();
    }

    [Fact]
    public async Task AddMultipleIndexesDifferentValue()
    {
        using var host = await CreateService();
        var context = host.Services.GetRequiredService<ILogger<AddUniqueIndexTests>>().ToScopeContext();
        var graphClient = host.Services.GetRequiredService<IGraphClient>();
        var graphEngine = host.Services.GetRequiredService<IGraphEngine>();

        var e1 = await graphClient.Execute("add node key=node1 set t1=v1, t2=v2 index t1, t2 ;", context);
        e1.IsOk().BeTrue();

        graphEngine.DataManager.GetMap().Nodes.LookupIndex("t1", "v1").Action(x =>
        {
            x.IsOk().BeTrue();
            x.Return().NodeKey.Be("node1");
        });

        graphEngine.DataManager.GetMap().Nodes.LookupIndex("t3", "v3").IsOk().BeFalse();

        var e2 = await graphClient.Execute("add node key=node2 set t1=v2 index t1 ;", context);
        e2.IsOk().BeTrue();

        graphEngine.DataManager.GetMap().Nodes.LookupIndex("t1", "v1").Action(x =>
        {
            x.IsOk().BeTrue();
            x.Return().NodeKey.Be("node1");
        });

        graphEngine.DataManager.GetMap().Nodes.LookupIndex("t1", "v2").Action(x =>
        {
            x.IsOk().BeTrue();
            x.Return().NodeKey.Be("node2");
        });
    }

    [Fact]
    public async Task UniqueIndexViolation()
    {
        using var host = await CreateService();
        var context = host.Services.GetRequiredService<ILogger<AddUniqueIndexTests>>().ToScopeContext();
        var graphClient = host.Services.GetRequiredService<IGraphClient>();
        var graphEngine = host.Services.GetRequiredService<IGraphEngine>();

        var e1 = await graphClient.Execute("add node key=node1 set t1=v1, t2=v2 index t1, t2 ;", context);
        e1.IsOk().BeTrue();

        graphEngine.DataManager.GetMap().Nodes.LookupIndex("t1", "v1").Action(x =>
        {
            x.IsOk().BeTrue();
            x.Return().NodeKey.Be("node1");
        });

        var e2 = await graphClient.Execute("add node key=node2 set t1=v1 index t1 ;", context);
        e2.IsError().BeTrue();

        graphEngine.DataManager.GetMap().Nodes.ContainsKey("node1").BeTrue();
        graphEngine.DataManager.GetMap().Nodes.ContainsKey("node2").BeFalse();

        graphEngine.DataManager.GetMap().Nodes.LookupIndex("t1", "v1").Action(x =>
        {
            x.IsOk().BeTrue();
            x.Return().NodeKey.Be("node1");
        });
    }

    [Fact]
    public async Task UniqueIndexViolation2()
    {
        using var host = await CreateService();
        var context = host.Services.GetRequiredService<ILogger<AddUniqueIndexTests>>().ToScopeContext();
        var graphClient = host.Services.GetRequiredService<IGraphClient>();
        var graphEngine = host.Services.GetRequiredService<IGraphEngine>();

        var e1 = await graphClient.Execute("add node key=node1 set t1=v1, t2=v2 index t1, t2 ;", context);
        e1.IsOk().BeTrue();

        graphEngine.DataManager.GetMap().Nodes.LookupIndex("t1", "v1").Action(x =>
        {
            x.IsOk().BeTrue();
            x.Return().NodeKey.Be("node1");
        });

        var e2 = await graphClient.Execute("add node key=node3 set t1=v2 index t1 ;", context);
        e2.IsOk().BeTrue();

        var e3 = await graphClient.Execute("add node key=node2 set t1=v1 index t1 ;", context);
        e3.IsError().BeTrue();

        graphEngine.DataManager.GetMap().Nodes.ContainsKey("node1").BeTrue();
        graphEngine.DataManager.GetMap().Nodes.ContainsKey("node3").BeTrue();
        graphEngine.DataManager.GetMap().Nodes.ContainsKey("node2").BeFalse();

        graphEngine.DataManager.GetMap().Nodes.LookupIndex("t1", "v1").Action(x =>
        {
            x.IsOk().BeTrue();
            x.Return().NodeKey.Be("node1");
        });
    }
}
