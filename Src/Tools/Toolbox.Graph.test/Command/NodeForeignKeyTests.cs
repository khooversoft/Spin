using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Graph.test.Application;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Graph.test.Command;

public class NodeForeignKeyTests
{
    private readonly GraphMap _map = new GraphMap()
    {
        new GraphNode("node1", tags: "name=marko,age=29"),
        new GraphNode("node2", tags: "name=vadas,age=27"),
        new GraphNode("node3", tags: "name=lop,lang=java"),
        new GraphNode("node4", tags: "name=josh,age=32"),
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
    public NodeForeignKeyTests(ITestOutputHelper logOutput) => _logOutput = logOutput;

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
        var context = host.Services.GetRequiredService<ILogger<NodeForeignKeyTests>>().ToScopeContext();
        await graphEngine.DataManager.SetMap(_map, context);

        return host;
    }

    [Fact]
    public async Task TestLoadedCounters()
    {
        using var host = await CreateService();
        var context = host.Services.GetRequiredService<ILogger<NodeForeignKeyTests>>().ToScopeContext();
        var graphClient = host.Services.GetRequiredService<IGraphClient>();
        var graphEngine = host.Services.GetRequiredService<IGraphEngine>();
        var collector = host.Services.GetRequiredService<GraphMapCounter>();

        collector.Nodes.Count.Value.Be(7);
        collector.Nodes.Added.Value.Be(7);
        collector.Nodes.Deleted.Value.Be(0);
        collector.Nodes.Updated.Value.Be(0);
        collector.Nodes.IndexHit.Value.Be(0);
        collector.Nodes.IndexMissed.Value.Be(0);
        collector.Nodes.IndexScan.Value.Be(0);
        collector.Nodes.ForeignKeyAdded.Value.Be(0);
        collector.Nodes.ForeignKeyRemoved.Value.Be(0);

        collector.Edges.Count.Value.Be(5);
        collector.Edges.Added.Value.Be(5);
        collector.Edges.Deleted.Value.Be(0);
        collector.Edges.Updated.Value.Be(0);
        collector.Edges.IndexHit.Value.Be(0);
        collector.Edges.IndexMissed.Value.Be(0);
        collector.Edges.IndexScan.Value.Be(0);

        var compareMap = GraphCommandTools.CompareMap(_map, graphEngine.DataManager.GetMap());
        compareMap.Count.Be(0);
    }

    [Fact]
    public async Task NoForeignKey()
    {
        using var host = await CreateService();
        var context = host.Services.GetRequiredService<ILogger<NodeForeignKeyTests>>().ToScopeContext();
        var graphClient = host.Services.GetRequiredService<IGraphClient>();
        var graphEngine = host.Services.GetRequiredService<IGraphEngine>();
        var collector = host.Services.GetRequiredService<GraphMapCounter>();

        var newMapOption = await graphClient.ExecuteBatch("add node key=node8 set t1=v1;", context);
        newMapOption.IsOk().BeTrue();

        collector.Nodes.Count.Value.Be(8);
        collector.Nodes.ForeignKeyAdded.Value.Be(0);
        collector.Nodes.ForeignKeyRemoved.Value.Be(0);

        var result = await graphClient.Execute("select (key=node8) -> [*] ;", context);
        result.IsOk().BeTrue();

        var queryResult = result.Return();
        queryResult.Nodes.Count.Be(0);
        queryResult.Edges.Count.Be(0);

        QueryBatchResult commandResults = newMapOption.Return();
        var compareMap = GraphCommandTools.CompareMap(_map, graphEngine.DataManager.GetMap());
        compareMap.Count.Be(1);
    }

    [Fact]
    public async Task ForeignKeyButNotInTag()
    {
        using var host = await CreateService();
        var context = host.Services.GetRequiredService<ILogger<NodeForeignKeyTests>>().ToScopeContext();
        var graphClient = host.Services.GetRequiredService<IGraphClient>();
        var graphEngine = host.Services.GetRequiredService<IGraphEngine>();
        var collector = host.Services.GetRequiredService<GraphMapCounter>();

        var newMapOption = await graphClient.ExecuteBatch("add node key=node8 set email=email:name@domain.com foreignkey t1;", context);
        newMapOption.IsOk().BeTrue();

        collector.Nodes.Count.Value.Be(8);
        collector.Nodes.ForeignKeyAdded.Value.Be(0);
        collector.Nodes.ForeignKeyRemoved.Value.Be(0);

        var result = await graphClient.Execute("select (key=node8) -> [*] ;", context);
        result.IsOk().BeTrue();

        var queryResult = result.Return();
        queryResult.Nodes.Count.Be(0);
        queryResult.Edges.Count.Be(0);

        QueryBatchResult commandResults = newMapOption.Return();
        var compareMap = GraphCommandTools.CompareMap(_map, graphEngine.DataManager.GetMap());
        compareMap.Count.Be(1);
        compareMap.OfType<GraphNode>().Select(x => x.Key).Contains("node8").BeTrue();
    }

    [Fact]
    public async Task ForeignKeyInTagButNoReferenceNode()
    {
        using var host = await CreateService();
        var context = host.Services.GetRequiredService<ILogger<NodeForeignKeyTests>>().ToScopeContext();
        var graphClient = host.Services.GetRequiredService<IGraphClient>();
        var graphEngine = host.Services.GetRequiredService<IGraphEngine>();
        var collector = host.Services.GetRequiredService<GraphMapCounter>();

        var newMapOption = await graphClient.ExecuteBatch("add node key=node8 set email=email:name@domain.com foreignkey email;", context);
        newMapOption.IsOk().BeFalse();

        collector.Nodes.Count.Value.Be(7);
        collector.Nodes.ForeignKeyAdded.Value.Be(0);
        collector.Nodes.ForeignKeyRemoved.Value.Be(0);

        var result = await graphClient.Execute("select (key=node8) -> [*] ;", context);
        result.IsOk().BeTrue();

        var queryResult = result.Return();
        queryResult.Nodes.Count.Be(0);
        queryResult.Edges.Count.Be(0);
    }

    [Fact]
    public async Task ForeignKeyInTagWithNodeReference()
    {
        using var host = await CreateService();
        var context = host.Services.GetRequiredService<ILogger<NodeForeignKeyTests>>().ToScopeContext();
        var graphClient = host.Services.GetRequiredService<IGraphClient>();
        var graphEngine = host.Services.GetRequiredService<IGraphEngine>();
        var collector = host.Services.GetRequiredService<GraphMapCounter>();

        var addOption = await graphClient.ExecuteBatch("add node key=email:name@domain.com ;", context);
        addOption.IsOk().BeTrue();

        var newMapOption = await graphClient.ExecuteBatch("add node key=node8 set email=email:name@domain.com foreignkey email;", context);
        newMapOption.IsOk().BeTrue();

        collector.Nodes.Count.Value.Be(9);
        collector.Nodes.ForeignKeyAdded.Value.Be(1);
        collector.Nodes.ForeignKeyRemoved.Value.Be(0);

        var readOption = await graphClient.Execute("select (key=node8) ;", context);
        readOption.IsOk().BeTrue();
        readOption.Return().Action(x =>
        {
            x.Nodes.Count.Be(1);
            x.Nodes[0].Action(y =>
            {
                y.Key.Be("node8");
                y.Tags.Count.Be(1);
                y.Tags["email"].Be("email:name@domain.com");
                y.ForeignKeys.Count.Be(1);
                y.ForeignKeys.First().Action(z =>
                {
                    z.Key.Be("email");
                    z.Value.BeNull();
                });
            });
        });


        (await graphClient.Execute("select (key=node8) -> [*] ;", context)).Action(result =>
        {
            result.IsOk().BeTrue();

            result.Return().Action(x =>
            {
                x.Nodes.Count.Be(0);
                x.Edges.Action(y =>
                {
                    y.Count.Be(1);
                    y[0].FromKey.Be("node8");
                    y[0].ToKey.Be("email:name@domain.com");
                    y[0].EdgeType.Be("email");
                });
            });
        });

        (await graphClient.Execute("select (key=node8) -> [*] -> (*) ;", context)).Action(result =>
        {
            result.IsOk().BeTrue();

            result.Return().Action(x =>
            {
                x.Nodes.Action(y =>
                {
                    y.Count.Be(1);
                    y[0].Key.Be("email:name@domain.com");
                });
                x.Edges.Count.Be(0);
            });
        });

        (await graphClient.Execute("select (key=email:name@domain.com) <- [*] <- (*) ;", context)).Action(result =>
        {
            result.IsOk().BeTrue();

            result.Return().Action(x =>
            {
                x.Nodes.Action(y =>
                {
                    y.Count.Be(1);
                    y[0].Key.Be("node8");
                });
                x.Edges.Count.Be(0);
            });
        });
    }


    [Fact]
    public async Task ForeignKeyInTagWithNodeReferenceWithWildcard()
    {
        using var host = await CreateService();
        var context = host.Services.GetRequiredService<ILogger<NodeForeignKeyTests>>().ToScopeContext();
        var graphClient = host.Services.GetRequiredService<IGraphClient>();
        var graphEngine = host.Services.GetRequiredService<IGraphEngine>();
        var collector = host.Services.GetRequiredService<GraphMapCounter>();

        var addOption = await graphClient.ExecuteBatch("add node key=email:name@domain.com ;", context);
        addOption.IsOk().BeTrue();

        var newMapOption = await graphClient.ExecuteBatch("add node key=node8 set email=email:name@domain.com foreignkey email=*;", context);
        newMapOption.IsOk().BeTrue();

        collector.Nodes.Count.Value.Be(9);
        collector.Nodes.ForeignKeyAdded.Value.Be(1);
        collector.Nodes.ForeignKeyRemoved.Value.Be(0);

        (await graphClient.Execute("select (key=node8) -> [*] ;", context)).Action(result =>
        {
            result.IsOk().BeTrue();

            result.Return().Action(x =>
            {
                x.Nodes.Count.Be(0);
                x.Edges.Action(y =>
                {
                    y.Count.Be(1);
                    y[0].FromKey.Be("node8");
                    y[0].ToKey.Be("email:name@domain.com");
                    y[0].EdgeType.Be("email");
                });
            });
        });

        (await graphClient.Execute("select (key=node8) -> [*] -> (*) ;", context)).Action(result =>
        {
            result.IsOk().BeTrue();

            result.Return().Action(x =>
            {
                x.Nodes.Action(y =>
                {
                    y.Count.Be(1);
                    y[0].Key.Be("email:name@domain.com");
                });
                x.Edges.Count.Be(0);
            });
        });

        (await graphClient.Execute("select (key=email:name@domain.com) <- [*] <- (*) ;", context)).Action(result =>
        {
            result.IsOk().BeTrue();

            result.Return().Action(x =>
            {
                x.Nodes.Action(y =>
                {
                    y.Count.Be(1);
                    y[0].Key.Be("node8");
                });
                x.Edges.Count.Be(0);
            });
        });
    }


    [Fact]
    public async Task ForeignKeyInTagWithTwoNodeReferenceUsingMatching()
    {
        using var host = await CreateService();
        var context = host.Services.GetRequiredService<ILogger<NodeForeignKeyTests>>().ToScopeContext();
        var graphClient = host.Services.GetRequiredService<IGraphClient>();
        var graphEngine = host.Services.GetRequiredService<IGraphEngine>();
        var collector = host.Services.GetRequiredService<GraphMapCounter>();

        (await graphClient.ExecuteBatch("add node key=email:name1@domain.com ;", context)).IsOk().BeTrue();
        (await graphClient.ExecuteBatch("add node key=email:name2@domain.com ;", context)).IsOk().BeTrue();

        var newMapOption = await graphClient.ExecuteBatch("add node key=node8 set email1=email:name1@domain.com, email2=email:name2@domain.com foreignkey email=email*;", context);
        newMapOption.IsOk().BeTrue();

        collector.Nodes.Count.Value.Be(10);
        collector.Nodes.ForeignKeyAdded.Value.Be(1);
        collector.Nodes.ForeignKeyRemoved.Value.Be(0);

        var readOption = await graphClient.Execute("select (key=node8) ;", context);
        readOption.IsOk().BeTrue();
        readOption.Return().Action(x =>
        {
            x.Nodes.Count.Be(1);
            x.Nodes[0].Action(y =>
            {
                y.Key.Be("node8");
                y.Tags.Count.Be(2);
                y.Tags["email1"].Be("email:name1@domain.com");
                y.Tags["email2"].Be("email:name2@domain.com");
                y.ForeignKeys.Count.Be(1);
                y.ForeignKeys["email"].Be("email*");
            });
        });

        (await graphClient.Execute("select (key=node8) -> [*] ;", context)).Action(result =>
        {
            result.IsOk().BeTrue();

            result.Return().Action(x =>
            {
                x.Nodes.Count.Be(0);
                x.Edges.Action(y =>
                {
                    y.Count.Be(2);
                    y.OrderBy(x => x.ToKey).ToArray().Action(z =>
                    {
                        z[0].FromKey.Be("node8");
                        z[0].ToKey.Be("email:name1@domain.com");
                        z[0].EdgeType.Be("email");
                        z[1].FromKey.Be("node8");
                        z[1].ToKey.Be("email:name2@domain.com");
                        z[1].EdgeType.Be("email");
                    });
                });
            });
        });

        (await graphClient.Execute("select (key=node8) -> [*] -> (*) ;", context)).Action(result =>
        {
            result.IsOk().BeTrue();

            result.Return().Action(x =>
            {
                x.Nodes.Action(y =>
                {
                    y.Count.Be(2);
                    Enumerable.SequenceEqual(y.Select(x => x.Key).OrderBy(x => x), ["email:name1@domain.com", "email:name2@domain.com"]).BeTrue();
                });
                x.Edges.Count.Be(0);
            });
        });

        (await graphClient.Execute("select (key=email:name1@domain.com) <- [*] <- (*) ;", context)).Action(result =>
        {
            result.IsOk().BeTrue();

            result.Return().Action(x =>
            {
                x.Nodes.Action(y =>
                {
                    y.Count.Be(1);
                    y[0].Key.Be("node8");
                });
                x.Edges.Count.Be(0);
            });
        });

        (await graphClient.Execute("select (key=email:name2@domain.com) <- [*] <- (*) ;", context)).Action(result =>
        {
            result.IsOk().BeTrue();

            result.Return().Action(x =>
            {
                x.Nodes.Action(y =>
                {
                    y.Count.Be(1);
                    y[0].Key.Be("node8");
                });
                x.Edges.Count.Be(0);
            });
        });
    }

    [Fact]
    public async Task ForeignKeyRemoved()
    {
        using var host = await CreateService();
        var context = host.Services.GetRequiredService<ILogger<NodeForeignKeyTests>>().ToScopeContext();
        var graphClient = host.Services.GetRequiredService<IGraphClient>();
        var graphEngine = host.Services.GetRequiredService<IGraphEngine>();
        var collector = host.Services.GetRequiredService<GraphMapCounter>();

        collector.Nodes.Count.Value.Be(7);
        collector.Edges.Count.Value.Be(5);

        var addOption = await graphClient.ExecuteBatch("add node key=email:name@domain.com ;", context);
        addOption.IsOk().BeTrue();
        collector.Nodes.Count.Value.Be(8);
        collector.Edges.Count.Value.Be(5);

        var newMapOption = await graphClient.ExecuteBatch("add node key=node8 set email=email:name@domain.com foreignkey email;", context);
        newMapOption.IsOk().BeTrue();

        collector.Nodes.Count.Value.Be(9);
        collector.Nodes.ForeignKeyAdded.Value.Be(1);
        collector.Nodes.ForeignKeyRemoved.Value.Be(0);
        collector.Edges.Count.Value.Be(6);
        collector.Edges.Added.Value.Be(6);
        collector.Edges.Deleted.Value.Be(0);

        (await graphClient.Execute("select (key=node8) -> [*] ;", context)).Action(result =>
        {
            result.IsOk().BeTrue();

            result.Return().Action(x =>
            {
                x.Nodes.Count.Be(0);
                x.Edges.Action(y =>
                {
                    y.Count.Be(1);
                    y[0].FromKey.Be("node8");
                    y[0].ToKey.Be("email:name@domain.com");
                    y[0].EdgeType.Be("email");
                });
            });
        });

        (await graphClient.ExecuteBatch("set node key=node8 foreignkey -email;", context)).Action(result =>
        {
            result.IsOk().BeTrue();
        });

        collector.Nodes.Count.Value.Be(9);
        collector.Nodes.ForeignKeyAdded.Value.Be(1);
        collector.Nodes.ForeignKeyRemoved.Value.Be(1);
        collector.Edges.Count.Value.Be(5);
        collector.Edges.Added.Value.Be(6);
        collector.Edges.Deleted.Value.Be(1);

        (await graphClient.Execute("select (key=node8) -> [*] ;", context)).Action(result =>
        {
            result.IsOk().BeTrue();

            result.Return().Action(x =>
            {
                x.Nodes.Count.Be(0);
                x.Edges.Count.Be(0);
            });
        });
    }

    [Fact]
    public async Task ForeignKeyTagRemoved()
    {
        using var host = await CreateService();
        var context = host.Services.GetRequiredService<ILogger<NodeForeignKeyTests>>().ToScopeContext();
        var graphClient = host.Services.GetRequiredService<IGraphClient>();
        var graphEngine = host.Services.GetRequiredService<IGraphEngine>();
        var collector = host.Services.GetRequiredService<GraphMapCounter>();

        collector.Nodes.Count.Value.Be(7);
        collector.Edges.Count.Value.Be(5);

        var addOption = await graphClient.ExecuteBatch("add node key=email:name@domain.com ;", context);
        addOption.IsOk().BeTrue();
        collector.Nodes.Count.Value.Be(8);
        collector.Edges.Count.Value.Be(5);

        var newMapOption = await graphClient.ExecuteBatch("add node key=node8 set email=email:name@domain.com foreignkey email;", context);
        newMapOption.IsOk().BeTrue();

        collector.Nodes.Count.Value.Be(9);
        collector.Nodes.ForeignKeyAdded.Value.Be(1);
        collector.Nodes.ForeignKeyRemoved.Value.Be(0);
        collector.Edges.Count.Value.Be(6);
        collector.Edges.Added.Value.Be(6);
        collector.Edges.Deleted.Value.Be(0);

        (await graphClient.Execute("select (key=node8) -> [*] ;", context)).Action(result =>
        {
            result.IsOk().BeTrue();

            result.Return().Action(x =>
            {
                x.Nodes.Count.Be(0);
                x.Edges.Action(y =>
                {
                    y.Count.Be(1);
                    y[0].FromKey.Be("node8");
                    y[0].ToKey.Be("email:name@domain.com");
                    y[0].EdgeType.Be("email");
                });
            });
        });

        (await graphClient.ExecuteBatch("set node key=node8 set -email;", context)).Action(result =>
        {
            result.IsOk().BeTrue();
        });

        collector.Nodes.Count.Value.Be(9);
        collector.Nodes.ForeignKeyAdded.Value.Be(1);
        collector.Nodes.ForeignKeyRemoved.Value.Be(1);
        collector.Edges.Count.Value.Be(5);
        collector.Edges.Added.Value.Be(6);
        collector.Edges.Deleted.Value.Be(1);

        (await graphClient.Execute("select (key=node8) -> [*] ;", context)).Action(result =>
        {
            result.IsOk().BeTrue();

            result.Return().Action(x =>
            {
                x.Nodes.Count.Be(0);
                x.Edges.Count.Be(0);
            });
        });

        (await graphClient.ExecuteBatch("set node key=node8 set email=email:name@domain.com;", context)).Action(result =>
        {
            result.IsOk().BeTrue();
        });

        collector.Nodes.Count.Value.Be(9);
        collector.Nodes.ForeignKeyAdded.Value.Be(2);
        collector.Nodes.ForeignKeyRemoved.Value.Be(1);
        collector.Edges.Count.Value.Be(6);
        collector.Edges.Added.Value.Be(7);
        collector.Edges.Deleted.Value.Be(1);

        (await graphClient.Execute("select (key=node8) -> [*] ;", context)).Action(result =>
        {
            result.IsOk().BeTrue();

            result.Return().Action(x =>
            {
                x.Nodes.Count.Be(0);
                x.Edges.Action(y =>
                {
                    y.Count.Be(1);
                    y[0].FromKey.Be("node8");
                    y[0].ToKey.Be("email:name@domain.com");
                    y[0].EdgeType.Be("email");
                });
            });
        });
    }

    [Fact]
    public async Task TwoForeignKeyTagRemoved()
    {
        using var host = await CreateService();
        var context = host.Services.GetRequiredService<ILogger<NodeForeignKeyTests>>().ToScopeContext();
        var graphClient = host.Services.GetRequiredService<IGraphClient>();
        var graphEngine = host.Services.GetRequiredService<IGraphEngine>();
        var collector = host.Services.GetRequiredService<GraphMapCounter>();

        (await graphClient.ExecuteBatch("add node key=email:name1@domain.com ;", context)).IsOk().BeTrue();
        (await graphClient.ExecuteBatch("add node key=email:name2@domain.com ;", context)).IsOk().BeTrue();
        (await graphClient.ExecuteBatch("add node key=email:name3@domain.com ;", context)).IsOk().BeTrue();

        var newMapOption = await graphClient.ExecuteBatch("add node key=node8 set email1=email:name1@domain.com, email2=email:name2@domain.com foreignkey email=email* ;", context);
        newMapOption.IsOk().BeTrue();

        collector.Nodes.Count.Value.Be(11);
        collector.Nodes.ForeignKeyAdded.Value.Be(1);
        collector.Nodes.ForeignKeyRemoved.Value.Be(0);
        collector.Edges.Count.Value.Be(7);
        collector.Edges.Added.Value.Be(7);
        collector.Edges.Deleted.Value.Be(0);

        (await graphClient.Execute("select (key=node8) -> [*] ;", context)).Action(result =>
        {
            result.IsOk().BeTrue();

            result.Return().Action(x =>
            {
                x.Nodes.Count.Be(0);
                x.Edges.Action(y =>
                {
                    y.Count.Be(2);
                    y.OrderBy(x => x.ToKey).ToArray().Action(z =>
                    {
                        z[0].FromKey.Be("node8");
                        z[0].ToKey.Be("email:name1@domain.com");
                        z[0].EdgeType.Be("email");
                        z[1].FromKey.Be("node8");
                        z[1].ToKey.Be("email:name2@domain.com");
                        z[1].EdgeType.Be("email");
                    });
                });
            });
        });

        (await graphClient.ExecuteBatch("set node key=node8 set -email1;", context)).Action(result =>
        {
            result.IsOk().BeTrue();
        });

        collector.Nodes.Count.Value.Be(11);
        collector.Nodes.ForeignKeyAdded.Value.Be(1);
        collector.Nodes.ForeignKeyRemoved.Value.Be(1);
        collector.Edges.Count.Value.Be(5);
        collector.Edges.Added.Value.Be(7);
        collector.Edges.Deleted.Value.Be(2);

        (await graphClient.Execute("select (key=node8) -> [*] ;", context)).Action(result =>
        {
            result.IsOk().BeTrue();

            result.Return().Action(x =>
            {
                x.Nodes.Count.Be(0);
                x.Edges.Action(y =>
                {
                    y.Count.Be(0);
                });
            });
        });

        (await graphClient.ExecuteBatch("set node key=node8 set email1=email:name3@domain.com ;", context)).Action(result =>
        {
            result.IsOk().BeTrue();
        });

        collector.Nodes.Count.Value.Be(11);
        collector.Nodes.ForeignKeyAdded.Value.Be(2);
        collector.Nodes.ForeignKeyRemoved.Value.Be(1);
        collector.Edges.Count.Value.Be(7);
        collector.Edges.Added.Value.Be(9);
        collector.Edges.Deleted.Value.Be(2);

        (await graphClient.Execute("select (key=node8) -> [*] ;", context)).Action(result =>
        {
            result.IsOk().BeTrue();

            result.Return().Action(x =>
            {
                x.Nodes.Count.Be(0);
                x.Edges.Action(y =>
                {
                    y.Count.Be(2);
                    y.OrderBy(x => x.ToKey).ToArray().Action(z =>
                    {
                        z[0].FromKey.Be("node8");
                        z[0].ToKey.Be("email:name2@domain.com");
                        z[0].EdgeType.Be("email");
                        z[1].FromKey.Be("node8");
                        z[1].ToKey.Be("email:name3@domain.com");
                        z[1].EdgeType.Be("email");
                    });
                });
            });
        });
    }
}
