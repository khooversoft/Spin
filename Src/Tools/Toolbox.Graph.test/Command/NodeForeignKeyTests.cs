using Microsoft.Extensions.DependencyInjection;
using Toolbox.Extensions;
using Toolbox.Graph.test.Application;
using Toolbox.Tools;
using Toolbox.Tools.Should;
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

    private readonly ITestOutputHelper _outputHelper;

    public NodeForeignKeyTests(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
    }

    [Fact]
    public async Task TestLoadedCounters()
    {
        using GraphHostService testClient = await TestApplication.CreateTestGraphService(_map.Clone(), _outputHelper);
        var collector = testClient.Services.GetRequiredService<GraphMapCounter>();
        var context = testClient.CreateScopeContext<NodeForeignKeyTests>();

        collector.Nodes.Count.Value.Should().Be(7);
        collector.Nodes.Added.Value.Should().Be(7);
        collector.Nodes.Deleted.Value.Should().Be(0);
        collector.Nodes.Updated.Value.Should().Be(0);
        collector.Nodes.IndexHit.Value.Should().Be(0);
        collector.Nodes.IndexMissed.Value.Should().Be(0);
        collector.Nodes.IndexScan.Value.Should().Be(0);
        collector.Nodes.ForeignKeyAdded.Value.Should().Be(0);
        collector.Nodes.ForeignKeyRemoved.Value.Should().Be(0);

        collector.Edges.Count.Value.Should().Be(5);
        collector.Edges.Added.Value.Should().Be(5);
        collector.Edges.Deleted.Value.Should().Be(0);
        collector.Edges.Updated.Value.Should().Be(0);
        collector.Edges.IndexHit.Value.Should().Be(0);
        collector.Edges.IndexMissed.Value.Should().Be(0);
        collector.Edges.IndexScan.Value.Should().Be(0);

        var compareMap = GraphCommandTools.CompareMap(_map, testClient.Map);
        compareMap.Count.Should().Be(0);
    }

    [Fact]
    public async Task NoForeignKey()
    {
        using GraphHostService testClient = await TestApplication.CreateTestGraphService(_map.Clone(), _outputHelper);
        var collector = testClient.Services.GetRequiredService<GraphMapCounter>();
        var context = testClient.CreateScopeContext<NodeForeignKeyTests>();

        var newMapOption = await testClient.ExecuteBatch("add node key=node8 set t1=v1;", context);
        newMapOption.IsOk().Should().BeTrue();

        collector.Nodes.Count.Value.Should().Be(8);
        collector.Nodes.ForeignKeyAdded.Value.Should().Be(0);
        collector.Nodes.ForeignKeyRemoved.Value.Should().Be(0);

        var result = await testClient.Execute("select (key=node8) -> [*] ;", context);
        result.IsOk().Should().BeTrue();

        var queryResult = result.Return();
        queryResult.Nodes.Count.Should().Be(0);
        queryResult.Edges.Count.Should().Be(0);

        QueryBatchResult commandResults = newMapOption.Return();
        var compareMap = GraphCommandTools.CompareMap(_map, testClient.Map);
        compareMap.Count.Should().Be(1);
    }

    [Fact]
    public async Task ForeignKeyButNotInTag()
    {
        using GraphHostService testClient = await TestApplication.CreateTestGraphService(_map.Clone());
        var collector = testClient.Services.GetRequiredService<GraphMapCounter>();
        var context = testClient.CreateScopeContext<NodeForeignKeyTests>();

        var newMapOption = await testClient.ExecuteBatch("add node key=node8 set email=email:name@domain.com foreignkey t1;", context);
        newMapOption.IsOk().Should().BeTrue();

        collector.Nodes.Count.Value.Should().Be(8);
        collector.Nodes.ForeignKeyAdded.Value.Should().Be(0);
        collector.Nodes.ForeignKeyRemoved.Value.Should().Be(0);

        var result = await testClient.Execute("select (key=node8) -> [*] ;", context);
        result.IsOk().Should().BeTrue();

        var queryResult = result.Return();
        queryResult.Nodes.Count.Should().Be(0);
        queryResult.Edges.Count.Should().Be(0);

        QueryBatchResult commandResults = newMapOption.Return();
        var compareMap = GraphCommandTools.CompareMap(_map, testClient.Map);
        compareMap.Count.Should().Be(1);
        compareMap.OfType<GraphNode>().Select(x => x.Key).Contains("node8").Should().BeTrue();
    }

    [Fact]
    public async Task ForeignKeyInTagButNoReferenceNode()
    {
        using GraphHostService testClient = await TestApplication.CreateTestGraphService(_map.Clone());
        var collector = testClient.Services.GetRequiredService<GraphMapCounter>();
        var context = testClient.CreateScopeContext<NodeForeignKeyTests>();

        var newMapOption = await testClient.ExecuteBatch("add node key=node8 set email=email:name@domain.com foreignkey email;", context);
        newMapOption.IsOk().Should().BeFalse();

        collector.Nodes.Count.Value.Should().Be(7);
        collector.Nodes.ForeignKeyAdded.Value.Should().Be(0);
        collector.Nodes.ForeignKeyRemoved.Value.Should().Be(0);

        var result = await testClient.Execute("select (key=node8) -> [*] ;", context);
        result.IsOk().Should().BeTrue();

        var queryResult = result.Return();
        queryResult.Nodes.Count.Should().Be(0);
        queryResult.Edges.Count.Should().Be(0);
    }

    [Fact]
    public async Task ForeignKeyInTagWithNodeReference()
    {
        using GraphHostService testClient = await TestApplication.CreateTestGraphService(_map.Clone());
        var collector = testClient.Services.GetRequiredService<GraphMapCounter>();
        var context = testClient.CreateScopeContext<NodeForeignKeyTests>();

        var addOption = await testClient.ExecuteBatch("add node key=email:name@domain.com ;", context);
        addOption.IsOk().Should().BeTrue();

        var newMapOption = await testClient.ExecuteBatch("add node key=node8 set email=email:name@domain.com foreignkey email;", context);
        newMapOption.IsOk().Should().BeTrue();

        collector.Nodes.Count.Value.Should().Be(9);
        collector.Nodes.ForeignKeyAdded.Value.Should().Be(1);
        collector.Nodes.ForeignKeyRemoved.Value.Should().Be(0);

        var readOption = await testClient.Execute("select (key=node8) ;", context);
        readOption.IsOk().Should().BeTrue();
        readOption.Return().Action(x =>
        {
            x.Nodes.Count.Should().Be(1);
            x.Nodes[0].Action(y =>
            {
                y.Key.Should().Be("node8");
                y.Tags.Count.Should().Be(1);
                y.Tags["email"].Should().Be("email:name@domain.com");
                y.ForeignKeys.Count.Should().Be(1);
                y.ForeignKeys.First().Action(z =>
                {
                    z.Key.Should().Be("email");
                    z.Value.BeNull();
                });
            });
        });


        (await testClient.Execute("select (key=node8) -> [*] ;", context)).Action(result =>
        {
            result.IsOk().Should().BeTrue();

            result.Return().Action(x =>
            {
                x.Nodes.Count.Should().Be(0);
                x.Edges.Action(y =>
                {
                    y.Count.Should().Be(1);
                    y[0].FromKey.Should().Be("node8");
                    y[0].ToKey.Should().Be("email:name@domain.com");
                    y[0].EdgeType.Should().Be("email");
                });
            });
        });

        (await testClient.Execute("select (key=node8) -> [*] -> (*) ;", context)).Action(result =>
        {
            result.IsOk().Should().BeTrue();

            result.Return().Action(x =>
            {
                x.Nodes.Action(y =>
                {
                    y.Count.Should().Be(1);
                    y[0].Key.Should().Be("email:name@domain.com");
                });
                x.Edges.Count.Should().Be(0);
            });
        });

        (await testClient.Execute("select (key=email:name@domain.com) <- [*] <- (*) ;", context)).Action(result =>
        {
            result.IsOk().Should().BeTrue();

            result.Return().Action(x =>
            {
                x.Nodes.Action(y =>
                {
                    y.Count.Should().Be(1);
                    y[0].Key.Should().Be("node8");
                });
                x.Edges.Count.Should().Be(0);
            });
        });
    }


    [Fact]
    public async Task ForeignKeyInTagWithNodeReferenceWithWildcard()
    {
        using GraphHostService testClient = await TestApplication.CreateTestGraphService(_map.Clone());
        var collector = testClient.Services.GetRequiredService<GraphMapCounter>();
        var context = testClient.CreateScopeContext<NodeForeignKeyTests>();

        var addOption = await testClient.ExecuteBatch("add node key=email:name@domain.com ;", context);
        addOption.IsOk().Should().BeTrue();

        var newMapOption = await testClient.ExecuteBatch("add node key=node8 set email=email:name@domain.com foreignkey email=*;", context);
        newMapOption.IsOk().Should().BeTrue();

        collector.Nodes.Count.Value.Should().Be(9);
        collector.Nodes.ForeignKeyAdded.Value.Should().Be(1);
        collector.Nodes.ForeignKeyRemoved.Value.Should().Be(0);

        (await testClient.Execute("select (key=node8) -> [*] ;", context)).Action(result =>
        {
            result.IsOk().Should().BeTrue();

            result.Return().Action(x =>
            {
                x.Nodes.Count.Should().Be(0);
                x.Edges.Action(y =>
                {
                    y.Count.Should().Be(1);
                    y[0].FromKey.Should().Be("node8");
                    y[0].ToKey.Should().Be("email:name@domain.com");
                    y[0].EdgeType.Should().Be("email");
                });
            });
        });

        (await testClient.Execute("select (key=node8) -> [*] -> (*) ;", context)).Action(result =>
        {
            result.IsOk().Should().BeTrue();

            result.Return().Action(x =>
            {
                x.Nodes.Action(y =>
                {
                    y.Count.Should().Be(1);
                    y[0].Key.Should().Be("email:name@domain.com");
                });
                x.Edges.Count.Should().Be(0);
            });
        });

        (await testClient.Execute("select (key=email:name@domain.com) <- [*] <- (*) ;", context)).Action(result =>
        {
            result.IsOk().Should().BeTrue();

            result.Return().Action(x =>
            {
                x.Nodes.Action(y =>
                {
                    y.Count.Should().Be(1);
                    y[0].Key.Should().Be("node8");
                });
                x.Edges.Count.Should().Be(0);
            });
        });
    }


    [Fact]
    public async Task ForeignKeyInTagWithTwoNodeReferenceUsingMatching()
    {
        using GraphHostService testClient = await TestApplication.CreateTestGraphService(_map.Clone());
        var context = testClient.CreateScopeContext<NodeForeignKeyTests>();
        var collector = testClient.Services.GetRequiredService<GraphMapCounter>();

        (await testClient.ExecuteBatch("add node key=email:name1@domain.com ;", context)).IsOk().Should().BeTrue();
        (await testClient.ExecuteBatch("add node key=email:name2@domain.com ;", context)).IsOk().Should().BeTrue();

        var newMapOption = await testClient.ExecuteBatch("add node key=node8 set email1=email:name1@domain.com, email2=email:name2@domain.com foreignkey email=email*;", context);
        newMapOption.IsOk().Should().BeTrue();

        collector.Nodes.Count.Value.Should().Be(10);
        collector.Nodes.ForeignKeyAdded.Value.Should().Be(1);
        collector.Nodes.ForeignKeyRemoved.Value.Should().Be(0);

        var readOption = await testClient.Execute("select (key=node8) ;", context);
        readOption.IsOk().Should().BeTrue();
        readOption.Return().Action(x =>
        {
            x.Nodes.Count.Should().Be(1);
            x.Nodes[0].Action(y =>
            {
                y.Key.Should().Be("node8");
                y.Tags.Count.Should().Be(2);
                y.Tags["email1"].Should().Be("email:name1@domain.com");
                y.Tags["email2"].Should().Be("email:name2@domain.com");
                y.ForeignKeys.Count.Should().Be(1);
                y.ForeignKeys["email"].Should().Be("email*");
            });
        });

        (await testClient.Execute("select (key=node8) -> [*] ;", context)).Action(result =>
        {
            result.IsOk().Should().BeTrue();

            result.Return().Action(x =>
            {
                x.Nodes.Count.Should().Be(0);
                x.Edges.Action(y =>
                {
                    y.Count.Should().Be(2);
                    y.OrderBy(x => x.ToKey).ToArray().Action(z =>
                    {
                        y[0].FromKey.Should().Be("node8");
                        y[0].ToKey.Should().Be("email:name1@domain.com");
                        y[0].EdgeType.Should().Be("email");
                        y[1].FromKey.Should().Be("node8");
                        y[1].ToKey.Should().Be("email:name2@domain.com");
                        y[1].EdgeType.Should().Be("email");
                    });
                });
            });
        });

        (await testClient.Execute("select (key=node8) -> [*] -> (*) ;", context)).Action(result =>
        {
            result.IsOk().Should().BeTrue();

            result.Return().Action(x =>
            {
                x.Nodes.Action(y =>
                {
                    y.Count.Should().Be(2);
                    Enumerable.SequenceEqual(y.Select(x => x.Key).OrderBy(x => x), ["email:name1@domain.com", "email:name2@domain.com"]).Should().BeTrue();
                });
                x.Edges.Count.Should().Be(0);
            });
        });

        (await testClient.Execute("select (key=email:name1@domain.com) <- [*] <- (*) ;", context)).Action(result =>
        {
            result.IsOk().Should().BeTrue();

            result.Return().Action(x =>
            {
                x.Nodes.Action(y =>
                {
                    y.Count.Should().Be(1);
                    y[0].Key.Should().Be("node8");
                });
                x.Edges.Count.Should().Be(0);
            });
        });

        (await testClient.Execute("select (key=email:name2@domain.com) <- [*] <- (*) ;", context)).Action(result =>
        {
            result.IsOk().Should().BeTrue();

            result.Return().Action(x =>
            {
                x.Nodes.Action(y =>
                {
                    y.Count.Should().Be(1);
                    y[0].Key.Should().Be("node8");
                });
                x.Edges.Count.Should().Be(0);
            });
        });
    }

    [Fact]
    public async Task ForeignKeyRemoved()
    {
        using GraphHostService testClient = await TestApplication.CreateTestGraphService(_map.Clone());
        var context = testClient.CreateScopeContext<NodeForeignKeyTests>();
        var collector = testClient.Services.GetRequiredService<GraphMapCounter>();

        collector.Nodes.Count.Value.Should().Be(7);
        collector.Edges.Count.Value.Should().Be(5);

        var addOption = await testClient.ExecuteBatch("add node key=email:name@domain.com ;", context);
        addOption.IsOk().Should().BeTrue();
        collector.Nodes.Count.Value.Should().Be(8);
        collector.Edges.Count.Value.Should().Be(5);

        var newMapOption = await testClient.ExecuteBatch("add node key=node8 set email=email:name@domain.com foreignkey email;", context);
        newMapOption.IsOk().Should().BeTrue();

        collector.Nodes.Count.Value.Should().Be(9);
        collector.Nodes.ForeignKeyAdded.Value.Should().Be(1);
        collector.Nodes.ForeignKeyRemoved.Value.Should().Be(0);
        collector.Edges.Count.Value.Should().Be(6);
        collector.Edges.Added.Value.Should().Be(6);
        collector.Edges.Deleted.Value.Should().Be(0);

        (await testClient.Execute("select (key=node8) -> [*] ;", context)).Action(result =>
        {
            result.IsOk().Should().BeTrue();

            result.Return().Action(x =>
            {
                x.Nodes.Count.Should().Be(0);
                x.Edges.Action(y =>
                {
                    y.Count.Should().Be(1);
                    y[0].FromKey.Should().Be("node8");
                    y[0].ToKey.Should().Be("email:name@domain.com");
                    y[0].EdgeType.Should().Be("email");
                });
            });
        });

        (await testClient.ExecuteBatch("set node key=node8 foreignkey -email;", context)).Action(result =>
        {
            result.IsOk().Should().BeTrue();
        });

        collector.Nodes.Count.Value.Should().Be(9);
        collector.Nodes.ForeignKeyAdded.Value.Should().Be(1);
        collector.Nodes.ForeignKeyRemoved.Value.Should().Be(1);
        collector.Edges.Count.Value.Should().Be(5);
        collector.Edges.Added.Value.Should().Be(6);
        collector.Edges.Deleted.Value.Should().Be(1);

        (await testClient.Execute("select (key=node8) -> [*] ;", context)).Action(result =>
        {
            result.IsOk().Should().BeTrue();

            result.Return().Action(x =>
            {
                x.Nodes.Count.Should().Be(0);
                x.Edges.Count.Should().Be(0);
            });
        });
    }

    [Fact]
    public async Task ForeignKeyTagRemoved()
    {
        using GraphHostService testClient = await TestApplication.CreateTestGraphService(_map.Clone());
        var context = testClient.CreateScopeContext<NodeForeignKeyTests>();
        var collector = testClient.Services.GetRequiredService<GraphMapCounter>();

        collector.Nodes.Count.Value.Should().Be(7);
        collector.Edges.Count.Value.Should().Be(5);

        var addOption = await testClient.ExecuteBatch("add node key=email:name@domain.com ;", context);
        addOption.IsOk().Should().BeTrue();
        collector.Nodes.Count.Value.Should().Be(8);
        collector.Edges.Count.Value.Should().Be(5);

        var newMapOption = await testClient.ExecuteBatch("add node key=node8 set email=email:name@domain.com foreignkey email;", context);
        newMapOption.IsOk().Should().BeTrue();

        collector.Nodes.Count.Value.Should().Be(9);
        collector.Nodes.ForeignKeyAdded.Value.Should().Be(1);
        collector.Nodes.ForeignKeyRemoved.Value.Should().Be(0);
        collector.Edges.Count.Value.Should().Be(6);
        collector.Edges.Added.Value.Should().Be(6);
        collector.Edges.Deleted.Value.Should().Be(0);

        (await testClient.Execute("select (key=node8) -> [*] ;", context)).Action(result =>
        {
            result.IsOk().Should().BeTrue();

            result.Return().Action(x =>
            {
                x.Nodes.Count.Should().Be(0);
                x.Edges.Action(y =>
                {
                    y.Count.Should().Be(1);
                    y[0].FromKey.Should().Be("node8");
                    y[0].ToKey.Should().Be("email:name@domain.com");
                    y[0].EdgeType.Should().Be("email");
                });
            });
        });

        (await testClient.ExecuteBatch("set node key=node8 set -email;", context)).Action(result =>
        {
            result.IsOk().Should().BeTrue();
        });

        collector.Nodes.Count.Value.Should().Be(9);
        collector.Nodes.ForeignKeyAdded.Value.Should().Be(1);
        collector.Nodes.ForeignKeyRemoved.Value.Should().Be(1);
        collector.Edges.Count.Value.Should().Be(5);
        collector.Edges.Added.Value.Should().Be(6);
        collector.Edges.Deleted.Value.Should().Be(1);

        (await testClient.Execute("select (key=node8) -> [*] ;", context)).Action(result =>
        {
            result.IsOk().Should().BeTrue();

            result.Return().Action(x =>
            {
                x.Nodes.Count.Should().Be(0);
                x.Edges.Count.Should().Be(0);
            });
        });

        (await testClient.ExecuteBatch("set node key=node8 set email=email:name@domain.com;", context)).Action(result =>
        {
            result.IsOk().Should().BeTrue();
        });

        collector.Nodes.Count.Value.Should().Be(9);
        collector.Nodes.ForeignKeyAdded.Value.Should().Be(2);
        collector.Nodes.ForeignKeyRemoved.Value.Should().Be(1);
        collector.Edges.Count.Value.Should().Be(6);
        collector.Edges.Added.Value.Should().Be(7);
        collector.Edges.Deleted.Value.Should().Be(1);

        (await testClient.Execute("select (key=node8) -> [*] ;", context)).Action(result =>
        {
            result.IsOk().Should().BeTrue();

            result.Return().Action(x =>
            {
                x.Nodes.Count.Should().Be(0);
                x.Edges.Action(y =>
                {
                    y.Count.Should().Be(1);
                    y[0].FromKey.Should().Be("node8");
                    y[0].ToKey.Should().Be("email:name@domain.com");
                    y[0].EdgeType.Should().Be("email");
                });
            });
        });
    }

    [Fact]
    public async Task TwoForeignKeyTagRemoved()
    {
        using GraphHostService testClient = await TestApplication.CreateTestGraphService(_map.Clone());
        var context = testClient.CreateScopeContext<NodeForeignKeyTests>();
        var collector = testClient.Services.GetRequiredService<GraphMapCounter>();

        (await testClient.ExecuteBatch("add node key=email:name1@domain.com ;", context)).IsOk().Should().BeTrue();
        (await testClient.ExecuteBatch("add node key=email:name2@domain.com ;", context)).IsOk().Should().BeTrue();
        (await testClient.ExecuteBatch("add node key=email:name3@domain.com ;", context)).IsOk().Should().BeTrue();

        var newMapOption = await testClient.ExecuteBatch("add node key=node8 set email1=email:name1@domain.com, email2=email:name2@domain.com foreignkey email=email* ;", context);
        newMapOption.IsOk().Should().BeTrue();

        collector.Nodes.Count.Value.Should().Be(11);
        collector.Nodes.ForeignKeyAdded.Value.Should().Be(1);
        collector.Nodes.ForeignKeyRemoved.Value.Should().Be(0);
        collector.Edges.Count.Value.Should().Be(7);
        collector.Edges.Added.Value.Should().Be(7);
        collector.Edges.Deleted.Value.Should().Be(0);

        (await testClient.Execute("select (key=node8) -> [*] ;", context)).Action(result =>
        {
            result.IsOk().Should().BeTrue();

            result.Return().Action(x =>
            {
                x.Nodes.Count.Should().Be(0);
                x.Edges.Action(y =>
                {
                    y.Count.Should().Be(2);
                    y.OrderBy(x => x.ToKey).ToArray().Action(z =>
                    {
                        y[0].FromKey.Should().Be("node8");
                        y[0].ToKey.Should().Be("email:name1@domain.com");
                        y[0].EdgeType.Should().Be("email");
                        y[1].FromKey.Should().Be("node8");
                        y[1].ToKey.Should().Be("email:name2@domain.com");
                        y[1].EdgeType.Should().Be("email");
                    });
                });
            });
        });

        (await testClient.ExecuteBatch("set node key=node8 set -email1;", context)).Action(result =>
        {
            result.IsOk().Should().BeTrue();
        });

        collector.Nodes.Count.Value.Should().Be(11);
        collector.Nodes.ForeignKeyAdded.Value.Should().Be(1);
        collector.Nodes.ForeignKeyRemoved.Value.Should().Be(1);
        collector.Edges.Count.Value.Should().Be(5);
        collector.Edges.Added.Value.Should().Be(7);
        collector.Edges.Deleted.Value.Should().Be(2);

        (await testClient.Execute("select (key=node8) -> [*] ;", context)).Action(result =>
        {
            result.IsOk().Should().BeTrue();

            result.Return().Action(x =>
            {
                x.Nodes.Count.Should().Be(0);
                x.Edges.Action(y =>
                {
                    y.Count.Should().Be(0);
                });
            });
        });

        (await testClient.ExecuteBatch("set node key=node8 set email1=email:name3@domain.com ;", context)).Action(result =>
        {
            result.IsOk().Should().BeTrue();
        });

        collector.Nodes.Count.Value.Should().Be(11);
        collector.Nodes.ForeignKeyAdded.Value.Should().Be(2);
        collector.Nodes.ForeignKeyRemoved.Value.Should().Be(1);
        collector.Edges.Count.Value.Should().Be(7);
        collector.Edges.Added.Value.Should().Be(9);
        collector.Edges.Deleted.Value.Should().Be(2);

        (await testClient.Execute("select (key=node8) -> [*] ;", context)).Action(result =>
        {
            result.IsOk().Should().BeTrue();

            result.Return().Action(x =>
            {
                x.Nodes.Count.Should().Be(0);
                x.Edges.Action(y =>
                {
                    y.Count.Should().Be(2);
                    y.OrderBy(x => x.ToKey).ToArray().Action(z =>
                    {
                        z[0].FromKey.Should().Be("node8");
                        z[0].ToKey.Should().Be("email:name2@domain.com");
                        z[0].EdgeType.Should().Be("email");
                        z[1].FromKey.Should().Be("node8");
                        z[1].ToKey.Should().Be("email:name3@domain.com");
                        z[1].EdgeType.Should().Be("email");
                    });
                });
            });
        });
    }
}
