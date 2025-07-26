using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Graph.test.Command;

public class GraphLifecycleTest
{
    private readonly ITestOutputHelper _logOutput;
    public GraphLifecycleTest(ITestOutputHelper logOutput) => _logOutput = logOutput;

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
        var context = host.Services.GetRequiredService<ILogger<GraphLifecycleTest>>().ToScopeContext();
        await graphEngine.DataManager.LoadDatabase(context);

        return host;
    }

    [Fact]
    public async Task SingleNode()
    {
        using var host = await CreateService();
        var context = host.Services.GetRequiredService<ILogger<GraphLifecycleTest>>().ToScopeContext();
        var graphClient = host.Services.GetRequiredService<IGraphClient>();
        var graphEngine = host.Services.GetRequiredService<IGraphEngine>();

        Option<QueryBatchResult> addResult = await graphClient.ExecuteBatch("set node key=node1 set t1,t2=v1;", context);
        addResult.IsOk().BeTrue();

        graphEngine.DataManager.GetMap().Nodes.Count.Be(1);
        graphEngine.DataManager.GetMap().Edges.Count.Be(0);

        (await graphClient.Execute("select (key=node1);", context)).ThrowOnError().Return().Action(x =>
        {
            x.Option.IsOk().BeTrue();
            x.Nodes.Count.Be(1);
            x.Edges.Count.Be(0);
            x.DataLinks.Count.Be(0);

            x.Nodes[0].Action(x =>
            {
                x.Key.Be("node1");
                x.Tags.ToTagsString().Be("t1,t2=v1");
            });
        });

        Option<QueryBatchResult> removeResult = await graphClient.ExecuteBatch("delete node key=node1;", context);
        removeResult.IsOk().BeTrue();
        graphEngine.DataManager.GetMap().Nodes.Count.Be(0);
        graphEngine.DataManager.GetMap().Edges.Count.Be(0);

        (await graphClient.Execute("select (key=node1);", context)).ThrowOnError().Return().Action(x =>
        {
            x.Option.IsOk().BeTrue();
            x.Nodes.Count.Be(0);
            x.Edges.Count.Be(0);
            x.DataLinks.Count.Be(0);
        });
    }

    [Fact]
    public async Task TwoNodes()
    {
        using var host = await CreateService();
        var context = host.Services.GetRequiredService<ILogger<GraphLifecycleTest>>().ToScopeContext();
        var graphClient = host.Services.GetRequiredService<IGraphClient>();
        var graphEngine = host.Services.GetRequiredService<IGraphEngine>();

        Option<QueryBatchResult> addResult1 = await graphClient.ExecuteBatch("set node key=node1 set t1,t2=v1;", context);
        addResult1.IsOk().BeTrue();

        graphEngine.DataManager.GetMap().Nodes.Count.Be(1);
        graphEngine.DataManager.GetMap().Edges.Count.Be(0);

        Option<QueryBatchResult> addResult2 = await graphClient.ExecuteBatch("set node key=node2 set t10,t20=v10;", context);
        addResult2.IsOk().BeTrue();
        graphEngine.DataManager.GetMap().Nodes.Count.Be(2);
        graphEngine.DataManager.GetMap().Edges.Count.Be(0);

        (await graphClient.Execute("select (key=node1);", context)).ThrowOnError().Return().Action(x =>
        {
            x.Option.IsOk().BeTrue();
            x.Nodes.Count.Be(1);
            x.Edges.Count.Be(0);
            x.DataLinks.Count.Be(0);

            x.Nodes[0].Action(x =>
            {
                x.Key.Be("node1");
                x.Tags.ToTagsString().Be("t1,t2=v1");
            });
        });

        (await graphClient.Execute("select (key=node2);", context)).ThrowOnError().Return().Action(x =>
        {
            x.Option.IsOk().BeTrue();
            x.Nodes.Count.Be(1);
            x.Edges.Count.Be(0);
            x.DataLinks.Count.Be(0);

            x.Nodes[0].Action(x =>
            {
                x.Key.Be("node2");
                x.Tags.ToTagsString().Be("t10,t20=v10");
            });
        });

        (await graphClient.ExecuteBatch("delete node key=node1;", context)).Action(x =>
        {
            x.IsOk().BeTrue();
            graphEngine.DataManager.GetMap().Nodes.Count.Be(1);
            graphEngine.DataManager.GetMap().Edges.Count.Be(0);
        });

        (await graphClient.Execute("select (key=node1);", context)).ThrowOnError().Return().Action(x =>
        {
            x.Option.IsOk().BeTrue();
            x.Nodes.Count.Be(0);
            x.Edges.Count.Be(0);
            x.DataLinks.Count.Be(0);
        });

        (await graphClient.Execute("select (key=node2);", context)).ThrowOnError().Return().Action(x =>
        {
            x.Option.IsOk().BeTrue();
            x.Nodes.Count.Be(1);
            x.Edges.Count.Be(0);
            x.DataLinks.Count.Be(0);

            x.Nodes[0].Action(x =>
            {
                x.Key.Be("node2");
                x.Tags.ToTagsString().Be("t10,t20=v10");
            });
        });

        (await graphClient.ExecuteBatch("delete node key=node2;", context)).Action(x =>
        {
            x.IsOk().BeTrue();
            x.Return().Items.Count.Be(1);
        });

        graphEngine.DataManager.GetMap().Nodes.Count.Be(0);
        graphEngine.DataManager.GetMap().Edges.Count.Be(0);
    }

    [Fact]
    public async Task UpdateTags()
    {
        using var host = await CreateService();
        var context = host.Services.GetRequiredService<ILogger<GraphLifecycleTest>>().ToScopeContext();
        var graphClient = host.Services.GetRequiredService<IGraphClient>();
        var graphEngine = host.Services.GetRequiredService<IGraphEngine>();

        string q = """
            set node key=node1 set t1;
            set node key=node1 set t2,client;
            """;

        (await graphClient.ExecuteBatch(q, context)).Action(x =>
        {
            x.IsOk().BeTrue();
            x.Value.Items.Count.Be(2);
            x.Value.Items[0].Action(y =>
            {
                y.Option.IsOk().BeTrue();
            });
            x.Value.Items[1].Action(y =>
            {
                y.Option.IsOk().BeTrue();
            });
        });

        graphEngine.DataManager.GetMap().Nodes.Count.Be(1);
        graphEngine.DataManager.GetMap().Nodes["node1"].Action(x =>
        {
            x.Key.Be("node1");
            x.Tags.ToTagsString().Be("client,t1,t2");
        });
        graphEngine.DataManager.GetMap().Edges.Count.Be(0);
    }

    [Fact]
    public async Task TwoNodesWithRelationship()
    {
        using var host = await CreateService();
        var context = host.Services.GetRequiredService<ILogger<GraphLifecycleTest>>().ToScopeContext();
        var graphClient = host.Services.GetRequiredService<IGraphClient>();
        var graphEngine = host.Services.GetRequiredService<IGraphEngine>();

        string q = """
            set node key=node1 set t1;
            set node key=node2 set t2,client;
            set edge from=node1 ,to=node2, type=et set e2, worksFor;
            """;

        (await graphClient.ExecuteBatch(q, context)).Action(x =>
        {
            x.IsOk().BeTrue();
            x.Return().Items.Count.Be(3);
        });

        graphEngine.DataManager.GetMap().Nodes.Count.Be(2);
        graphEngine.DataManager.GetMap().Edges.Count.Be(1);

        QueryBatchResult batch = (await graphClient.ExecuteBatch("select (key=node1) a0 -> [*] a1 -> (*) a2;", context)).ThrowOnError().Return();
        batch.Option.IsOk().Be(true);
        batch.Items.Count.Be(3);

        batch.Items[0].Action(x =>
        {
            x.Alias.Be("a0");
            x.Nodes.Count.Be(1);
            x.Edges.Count.Be(0);
            x.DataLinks.Count.Be(0);
            x.Nodes[0].Key.Be("node1");
            x.Nodes[0].Tags.ToTagsString().Be("t1");
        });

        batch.Items[1].Action(x =>
        {
            x.Alias.Be("a1");
            x.Nodes.Count.Be(0);
            x.Edges.Count.Be(1);
            x.DataLinks.Count.Be(0);
            x.Edges[0].FromKey.Be("node1");
            x.Edges[0].ToKey.Be("node2");
            x.Edges[0].EdgeType.Be("et");
            x.Edges[0].Tags.ToTagsString().Be("e2,worksFor");
        });

        batch.Items[2].Action(x =>
        {
            x.Alias.Be("a2");
            x.Nodes.Count.Be(1);
            x.Edges.Count.Be(0);
            x.DataLinks.Count.Be(0);
            x.Nodes[0].Key.Be("node2");
            x.Nodes[0].Tags.ToTagsString().Be("client,t2");
        });
    }

    [Fact]
    public async Task TwoNodesWithFullRelationship()
    {
        using var host = await CreateService();
        var context = host.Services.GetRequiredService<ILogger<GraphLifecycleTest>>().ToScopeContext();
        var graphClient = host.Services.GetRequiredService<IGraphClient>();
        var graphEngine = host.Services.GetRequiredService<IGraphEngine>();

        string q = """
            set node key=node1 set t1;
            set node key=node2 set t2,client;
            set edge from=node1 ,to=node2, type=et set e2, worksFor;
            """;

        (await graphClient.ExecuteBatch(q, context)).Action(x =>
        {
            x.IsOk().BeTrue();
            x.Return().Items.Count.Be(3);
        });

        graphEngine.DataManager.GetMap().Nodes.Count.Be(2);
        graphEngine.DataManager.GetMap().Edges.Count.Be(1);

        QueryBatchResult batch = (await graphClient.ExecuteBatch("select (key=node2) a0 <-> [*] a1 <-> (*) a2;", context)).ThrowOnError().Return();
        batch.Option.IsOk().Be(true);
        batch.Items.Count.Be(3);

        batch.Items[0].Action(x =>
        {
            x.Alias.Be("a0");
            x.Nodes.Count.Be(1);
            x.Edges.Count.Be(0);
            x.DataLinks.Count.Be(0);
            x.Nodes[0].Key.Be("node2");
            x.Nodes[0].Tags.ToTagsString().Be("client,t2");
        });

        batch.Items[1].Action(x =>
        {
            x.Alias.Be("a1");
            x.Nodes.Count.Be(0);
            x.Edges.Count.Be(1);
            x.DataLinks.Count.Be(0);
            x.Edges[0].FromKey.Be("node1");
            x.Edges[0].ToKey.Be("node2");
            x.Edges[0].EdgeType.Be("et");
            x.Edges[0].Tags.ToTagsString().Be("e2,worksFor");
        });

        batch.Items[2].Action(x =>
        {
            x.Alias.Be("a2");
            x.Nodes.Count.Be(2);
            x.Edges.Count.Be(0);
            x.DataLinks.Count.Be(0);
            x.Nodes.OrderBy(x => x.Key).ToArray().Action(y =>
            {
                y[0].Key.Be("node1");
                y[0].Tags.ToTagsString().Be("t1");
                y[1].Key.Be("node2");
                y[1].Tags.ToTagsString().Be("client,t2");
            });
        });
    }

    [Fact]
    public async Task TwoNodesWithRelationshipLargerSet()
    {
        using var host = await CreateService();
        var context = host.Services.GetRequiredService<ILogger<GraphLifecycleTest>>().ToScopeContext();
        var graphClient = host.Services.GetRequiredService<IGraphClient>();
        var graphEngine = host.Services.GetRequiredService<IGraphEngine>();

        string q = """
            set node key=node1 set t1;
            set node key=node2 set t2,client;
            set node key=node3 set t3,client;
            set node key=node4 set t4,client;
            set node key=node5 set t5,client;
            set edge from=node1,to=node2,type=et set e2,worksFor;
            set edge from=node3,to=node4,type=et set e3,worksFor;
            """;

        (await graphClient.ExecuteBatch(q, context)).Action(x =>
        {
            x.IsOk().BeTrue();
            x.Return().Items.Count.Be(7);
        });

        graphEngine.DataManager.GetMap().Nodes.Count.Be(5);
        graphEngine.DataManager.GetMap().Edges.Count.Be(2);

        (await graphClient.ExecuteBatch("select (key=node3) a0 -> [*] a1 -> (*) a2;", context)).ThrowOnError().Return().Action(batch =>
        {
            batch.Option.IsOk().Be(true);
            batch.Items.Count.Be(3);

            batch.Items[0].Action(x =>
            {
                x.Alias.Be("a0");
                x.Nodes.Count.Be(1);
                x.Edges.Count.Be(0);
                x.DataLinks.Count.Be(0);
                x.Nodes[0].Key.Be("node3");
                x.Nodes[0].Tags.ToTagsString().Be("client,t3");
            });

            batch.Items[1].Action(x =>
            {
                x.Alias.Be("a1");
                x.Nodes.Count.Be(0);
                x.Edges.Count.Be(1);
                x.DataLinks.Count.Be(0);
                x.Edges[0].FromKey.Be("node3");
                x.Edges[0].ToKey.Be("node4");
                x.Edges[0].EdgeType.Be("et");
                x.Edges[0].Tags.ToTagsString().Be("e3,worksFor");
            });

            batch.Items[2].Action(x =>
            {
                x.Alias.Be("a2");
                x.Nodes.Count.Be(1);
                x.Edges.Count.Be(0);
                x.DataLinks.Count.Be(0);
                x.Nodes.OrderBy(x => x.Key).ToArray().Action(y =>
                {
                    y[0].Key.Be("node4");
                    y[0].Tags.ToTagsString().Be("client,t4");

                });
            });
        });
    }
}
