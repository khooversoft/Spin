using Toolbox.Extensions;
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
    public async Task NoForeignKey()
    {
        await using GraphHostService graphTestClient = await GraphTestStartup.CreateGraphService(_map.Clone(), logOutput: x => _outputHelper.WriteLine(x));
        var context = graphTestClient.CreateScopeContext<NodeForeignKeyTests>();

        var newMapOption = await graphTestClient.ExecuteBatch("add node key=node8 set t1=v1;", context);
        newMapOption.IsOk().Should().BeTrue();

        graphTestClient.Map.Meter.Node.GetCount().Should().Be(8);
        graphTestClient.Map.Meter.Node.GetForeignKeyAdded().Should().Be(0);
        graphTestClient.Map.Meter.Node.GetForeignKeyRemoved().Should().Be(0);

        var result = await graphTestClient.Execute("select (key=node8) -> [*] ;", context);
        result.IsOk().Should().BeTrue();

        var queryResult = result.Return();
        queryResult.Nodes.Count.Should().Be(0);
        queryResult.Edges.Count.Should().Be(0);

        QueryBatchResult commandResults = newMapOption.Return();
        var compareMap = GraphCommandTools.CompareMap(_map, graphTestClient.Map);
        compareMap.Count.Should().Be(1);
    }

    [Fact]
    public async Task ForeignKeyButNotInTag()
    {
        await using GraphHostService graphTestClient = await GraphTestStartup.CreateGraphService(_map.Clone(), logOutput: x => _outputHelper.WriteLine(x));
        var context = graphTestClient.CreateScopeContext<NodeForeignKeyTests>();

        var newMapOption = await graphTestClient.ExecuteBatch("add node key=node8 set email=email:name@domain.com foreignkey t1;", context);
        newMapOption.IsOk().Should().BeTrue();

        graphTestClient.Map.Meter.Node.GetCount().Should().Be(8);
        graphTestClient.Map.Meter.Node.GetForeignKeyAdded().Should().Be(0);
        graphTestClient.Map.Meter.Node.GetForeignKeyRemoved().Should().Be(0);

        var result = await graphTestClient.Execute("select (key=node8) -> [*] ;", context);
        result.IsOk().Should().BeTrue();

        var queryResult = result.Return();
        queryResult.Nodes.Count.Should().Be(0);
        queryResult.Edges.Count.Should().Be(0);

        QueryBatchResult commandResults = newMapOption.Return();
        var compareMap = GraphCommandTools.CompareMap(_map, graphTestClient.Map);
        compareMap.Count.Should().Be(1);
        compareMap.OfType<GraphNode>().Select(x => x.Key).Contains("node8").Should().BeTrue();
    }

    [Fact]
    public async Task ForeignKeyInTagButNoReferenceNode()
    {
        await using GraphHostService graphTestClient = await GraphTestStartup.CreateGraphService(_map.Clone(), logOutput: x => _outputHelper.WriteLine(x));
        var context = graphTestClient.CreateScopeContext<NodeForeignKeyTests>();

        var newMapOption = await graphTestClient.ExecuteBatch("add node key=node8 set email=email:name@domain.com foreignkey email;", context);
        newMapOption.IsOk().Should().BeFalse();

        graphTestClient.Map.Meter.Node.GetCount().Should().Be(7);
        graphTestClient.Map.Meter.Node.GetForeignKeyAdded().Should().Be(0);
        graphTestClient.Map.Meter.Node.GetForeignKeyRemoved().Should().Be(0);

        var result = await graphTestClient.Execute("select (key=node8) -> [*] ;", context);
        result.IsOk().Should().BeTrue();

        var queryResult = result.Return();
        queryResult.Nodes.Count.Should().Be(0);
        queryResult.Edges.Count.Should().Be(0);
    }

    [Fact]
    public async Task ForeignKeyInTagWithNodeReference()
    {
        await using GraphHostService graphTestClient = await GraphTestStartup.CreateGraphService(_map.Clone(), logOutput: x => _outputHelper.WriteLine(x));
        var context = graphTestClient.CreateScopeContext<NodeForeignKeyTests>();

        var addOption = await graphTestClient.ExecuteBatch("add node key=email:name@domain.com ;", context);
        addOption.IsOk().Should().BeTrue();

        var newMapOption = await graphTestClient.ExecuteBatch("add node key=node8 set email=email:name@domain.com foreignkey email;", context);
        newMapOption.IsOk().Should().BeTrue();

        graphTestClient.Map.Meter.Node.GetCount().Should().Be(9);
        graphTestClient.Map.Meter.Node.GetForeignKeyAdded().Should().Be(1);
        graphTestClient.Map.Meter.Node.GetForeignKeyRemoved().Should().Be(0);

        var readOption = await graphTestClient.Execute("select (key=node8) ;", context);
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


        (await graphTestClient.Execute("select (key=node8) -> [*] ;", context)).Action(result =>
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

        (await graphTestClient.Execute("select (key=node8) -> [*] -> (*) ;", context)).Action(result =>
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

        (await graphTestClient.Execute("select (key=email:name@domain.com) <- [*] <- (*) ;", context)).Action(result =>
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
        await using GraphHostService graphTestClient = await GraphTestStartup.CreateGraphService(_map.Clone(), logOutput: x => _outputHelper.WriteLine(x));
        var context = graphTestClient.CreateScopeContext<NodeForeignKeyTests>();

        var addOption = await graphTestClient.ExecuteBatch("add node key=email:name@domain.com ;", context);
        addOption.IsOk().Should().BeTrue();

        var newMapOption = await graphTestClient.ExecuteBatch("add node key=node8 set email=email:name@domain.com foreignkey email=*;", context);
        newMapOption.IsOk().Should().BeTrue();

        graphTestClient.Map.Meter.Node.GetCount().Should().Be(9);
        graphTestClient.Map.Meter.Node.GetForeignKeyAdded().Should().Be(1);
        graphTestClient.Map.Meter.Node.GetForeignKeyRemoved().Should().Be(0);

        (await graphTestClient.Execute("select (key=node8) -> [*] ;", context)).Action(result =>
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

        (await graphTestClient.Execute("select (key=node8) -> [*] -> (*) ;", context)).Action(result =>
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

        (await graphTestClient.Execute("select (key=email:name@domain.com) <- [*] <- (*) ;", context)).Action(result =>
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
        await using GraphHostService graphTestClient = await GraphTestStartup.CreateGraphService(_map.Clone(), logOutput: x => _outputHelper.WriteLine(x));
        var context = graphTestClient.CreateScopeContext<NodeForeignKeyTests>();

        (await graphTestClient.ExecuteBatch("add node key=email:name1@domain.com ;", context)).IsOk().Should().BeTrue();
        (await graphTestClient.ExecuteBatch("add node key=email:name2@domain.com ;", context)).IsOk().Should().BeTrue();

        var newMapOption = await graphTestClient.ExecuteBatch("add node key=node8 set email1=email:name1@domain.com, email2=email:name2@domain.com foreignkey email=email*;", context);
        newMapOption.IsOk().Should().BeTrue();

        graphTestClient.Map.Meter.Node.GetCount().Should().Be(10);
        graphTestClient.Map.Meter.Node.GetForeignKeyAdded().Should().Be(1);
        graphTestClient.Map.Meter.Node.GetForeignKeyRemoved().Should().Be(0);

        var readOption = await graphTestClient.Execute("select (key=node8) ;", context);
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

        (await graphTestClient.Execute("select (key=node8) -> [*] ;", context)).Action(result =>
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

        (await graphTestClient.Execute("select (key=node8) -> [*] -> (*) ;", context)).Action(result =>
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

        (await graphTestClient.Execute("select (key=email:name1@domain.com) <- [*] <- (*) ;", context)).Action(result =>
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

        (await graphTestClient.Execute("select (key=email:name2@domain.com) <- [*] <- (*) ;", context)).Action(result =>
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
        await using GraphHostService graphTestClient = await GraphTestStartup.CreateGraphService(_map.Clone(), logOutput: x => _outputHelper.WriteLine(x));
        var context = graphTestClient.CreateScopeContext<NodeForeignKeyTests>();

        graphTestClient.Map.Meter.Node.GetCount().Should().Be(7);
        graphTestClient.Map.Meter.Edge.GetCount().Should().Be(5);

        var addOption = await graphTestClient.ExecuteBatch("add node key=email:name@domain.com ;", context);
        addOption.IsOk().Should().BeTrue();
        graphTestClient.Map.Meter.Node.GetCount().Should().Be(8);
        graphTestClient.Map.Meter.Edge.GetCount().Should().Be(5);

        var newMapOption = await graphTestClient.ExecuteBatch("add node key=node8 set email=email:name@domain.com foreignkey email;", context);
        newMapOption.IsOk().Should().BeTrue();

        graphTestClient.Map.Meter.Node.GetCount().Should().Be(9);
        graphTestClient.Map.Meter.Node.GetForeignKeyAdded().Should().Be(1);
        graphTestClient.Map.Meter.Node.GetForeignKeyRemoved().Should().Be(0);
        graphTestClient.Map.Meter.Edge.GetCount().Should().Be(6);
        graphTestClient.Map.Meter.Edge.GetAdded().Should().Be(6);
        graphTestClient.Map.Meter.Edge.GetDeleted().Should().Be(0);

        (await graphTestClient.Execute("select (key=node8) -> [*] ;", context)).Action(result =>
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

        (await graphTestClient.ExecuteBatch("set node key=node8 foreignkey -email;", context)).Action(result =>
        {
            result.IsOk().Should().BeTrue();
        });

        graphTestClient.Map.Meter.Node.GetCount().Should().Be(9);
        graphTestClient.Map.Meter.Node.GetForeignKeyAdded().Should().Be(1);
        graphTestClient.Map.Meter.Node.GetForeignKeyRemoved().Should().Be(1);
        graphTestClient.Map.Meter.Edge.GetCount().Should().Be(5);
        graphTestClient.Map.Meter.Edge.GetAdded().Should().Be(6);
        graphTestClient.Map.Meter.Edge.GetDeleted().Should().Be(1);

        (await graphTestClient.Execute("select (key=node8) -> [*] ;", context)).Action(result =>
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
        await using GraphHostService graphTestClient = await GraphTestStartup.CreateGraphService(_map.Clone(), logOutput: x => _outputHelper.WriteLine(x));
        var context = graphTestClient.CreateScopeContext<NodeForeignKeyTests>();

        graphTestClient.Map.Meter.Node.GetCount().Should().Be(7);
        graphTestClient.Map.Meter.Edge.GetCount().Should().Be(5);

        var addOption = await graphTestClient.ExecuteBatch("add node key=email:name@domain.com ;", context);
        addOption.IsOk().Should().BeTrue();
        graphTestClient.Map.Meter.Node.GetCount().Should().Be(8);
        graphTestClient.Map.Meter.Edge.GetCount().Should().Be(5);

        var newMapOption = await graphTestClient.ExecuteBatch("add node key=node8 set email=email:name@domain.com foreignkey email;", context);
        newMapOption.IsOk().Should().BeTrue();

        graphTestClient.Map.Meter.Node.GetCount().Should().Be(9);
        graphTestClient.Map.Meter.Node.GetForeignKeyAdded().Should().Be(1);
        graphTestClient.Map.Meter.Node.GetForeignKeyRemoved().Should().Be(0);
        graphTestClient.Map.Meter.Edge.GetCount().Should().Be(6);
        graphTestClient.Map.Meter.Edge.GetAdded().Should().Be(6);
        graphTestClient.Map.Meter.Edge.GetDeleted().Should().Be(0);

        (await graphTestClient.Execute("select (key=node8) -> [*] ;", context)).Action(result =>
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

        (await graphTestClient.ExecuteBatch("set node key=node8 set -email;", context)).Action(result =>
        {
            result.IsOk().Should().BeTrue();
        });

        graphTestClient.Map.Meter.Node.GetCount().Should().Be(9);
        graphTestClient.Map.Meter.Node.GetForeignKeyAdded().Should().Be(1);
        graphTestClient.Map.Meter.Node.GetForeignKeyRemoved().Should().Be(1);
        graphTestClient.Map.Meter.Edge.GetCount().Should().Be(5);
        graphTestClient.Map.Meter.Edge.GetAdded().Should().Be(6);
        graphTestClient.Map.Meter.Edge.GetDeleted().Should().Be(1);

        (await graphTestClient.Execute("select (key=node8) -> [*] ;", context)).Action(result =>
        {
            result.IsOk().Should().BeTrue();

            result.Return().Action(x =>
            {
                x.Nodes.Count.Should().Be(0);
                x.Edges.Count.Should().Be(0);
            });
        });

        (await graphTestClient.ExecuteBatch("set node key=node8 set email=email:name@domain.com;", context)).Action(result =>
        {
            result.IsOk().Should().BeTrue();
        });

        graphTestClient.Map.Meter.Node.GetCount().Should().Be(9);
        graphTestClient.Map.Meter.Node.GetForeignKeyAdded().Should().Be(2);
        graphTestClient.Map.Meter.Node.GetForeignKeyRemoved().Should().Be(1);
        graphTestClient.Map.Meter.Edge.GetCount().Should().Be(6);
        graphTestClient.Map.Meter.Edge.GetAdded().Should().Be(7);
        graphTestClient.Map.Meter.Edge.GetDeleted().Should().Be(1);

        (await graphTestClient.Execute("select (key=node8) -> [*] ;", context)).Action(result =>
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
        await using GraphHostService graphTestClient = await GraphTestStartup.CreateGraphService(_map.Clone(), logOutput: x => _outputHelper.WriteLine(x));
        var context = graphTestClient.CreateScopeContext<NodeForeignKeyTests>();

        (await graphTestClient.ExecuteBatch("add node key=email:name1@domain.com ;", context)).IsOk().Should().BeTrue();
        (await graphTestClient.ExecuteBatch("add node key=email:name2@domain.com ;", context)).IsOk().Should().BeTrue();
        (await graphTestClient.ExecuteBatch("add node key=email:name3@domain.com ;", context)).IsOk().Should().BeTrue();

        var newMapOption = await graphTestClient.ExecuteBatch("add node key=node8 set email1=email:name1@domain.com, email2=email:name2@domain.com foreignkey email=email* ;", context);
        newMapOption.IsOk().Should().BeTrue();

        graphTestClient.Map.Meter.Node.GetCount().Should().Be(11);
        graphTestClient.Map.Meter.Node.GetForeignKeyAdded().Should().Be(1);
        graphTestClient.Map.Meter.Node.GetForeignKeyRemoved().Should().Be(0);
        graphTestClient.Map.Meter.Edge.GetCount().Should().Be(7);
        graphTestClient.Map.Meter.Edge.GetAdded().Should().Be(7);
        graphTestClient.Map.Meter.Edge.GetDeleted().Should().Be(0);

        (await graphTestClient.Execute("select (key=node8) -> [*] ;", context)).Action(result =>
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

        (await graphTestClient.ExecuteBatch("set node key=node8 set -email1;", context)).Action(result =>
        {
            result.IsOk().Should().BeTrue();
        });

        graphTestClient.Map.Meter.Node.GetCount().Should().Be(11);
        graphTestClient.Map.Meter.Node.GetForeignKeyAdded().Should().Be(1);
        graphTestClient.Map.Meter.Node.GetForeignKeyRemoved().Should().Be(1);
        graphTestClient.Map.Meter.Edge.GetCount().Should().Be(5);
        graphTestClient.Map.Meter.Edge.GetAdded().Should().Be(7);
        graphTestClient.Map.Meter.Edge.GetDeleted().Should().Be(2);

        (await graphTestClient.Execute("select (key=node8) -> [*] ;", context)).Action(result =>
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

        (await graphTestClient.ExecuteBatch("set node key=node8 set email1=email:name3@domain.com ;", context)).Action(result =>
        {
            result.IsOk().Should().BeTrue();
        });

        graphTestClient.Map.Meter.Node.GetCount().Should().Be(11);
        graphTestClient.Map.Meter.Node.GetForeignKeyAdded().Should().Be(2);
        graphTestClient.Map.Meter.Node.GetForeignKeyRemoved().Should().Be(1);
        graphTestClient.Map.Meter.Edge.GetCount().Should().Be(7);
        graphTestClient.Map.Meter.Edge.GetAdded().Should().Be(9);
        graphTestClient.Map.Meter.Edge.GetDeleted().Should().Be(2);

        (await graphTestClient.Execute("select (key=node8) -> [*] ;", context)).Action(result =>
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
