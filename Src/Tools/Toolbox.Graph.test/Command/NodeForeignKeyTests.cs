using FluentAssertions;
using Toolbox.Extensions;
using Toolbox.Types;

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

    [Fact]
    public async Task NoForeignKey()
    {
        var copyMap = _map.Clone();
        var testClient = GraphTestStartup.CreateGraphTestHost(copyMap);
        var newMapOption = await testClient.ExecuteBatch("add node key=node8 set t1=v1;", NullScopeContext.Default);
        newMapOption.IsOk().Should().BeTrue(newMapOption.ToString());

        copyMap.Meter.Node.GetCount().Should().Be(8);
        copyMap.Meter.Node.GetForeignKeyAdded().Should().Be(0);
        copyMap.Meter.Node.GetForeignKeyRemoved().Should().Be(0);

        var result = await testClient.Execute("select (key=node8) -> [*] ;", NullScopeContext.Default);
        result.IsOk().Should().BeTrue(result.ToString());

        var queryResult = result.Return();
        queryResult.Nodes.Count.Should().Be(0);
        queryResult.Edges.Count.Should().Be(0);

        QueryBatchResult commandResults = newMapOption.Return();
        var compareMap = GraphCommandTools.CompareMap(_map, copyMap);
        compareMap.Count.Should().Be(1);
    }

    [Fact]
    public async Task ForeignKeyButNotInTag()
    {
        var copyMap = _map.Clone();
        var testClient = GraphTestStartup.CreateGraphTestHost(copyMap);
        var newMapOption = await testClient.ExecuteBatch("add node key=node8 set email=email:name@domain.com foreignkey t1;", NullScopeContext.Default);
        newMapOption.IsOk().Should().BeTrue(newMapOption.ToString());

        copyMap.Meter.Node.GetCount().Should().Be(8);
        copyMap.Meter.Node.GetForeignKeyAdded().Should().Be(0);
        copyMap.Meter.Node.GetForeignKeyRemoved().Should().Be(0);

        var result = await testClient.Execute("select (key=node8) -> [*] ;", NullScopeContext.Default);
        result.IsOk().Should().BeTrue(result.ToString());

        var queryResult = result.Return();
        queryResult.Nodes.Count.Should().Be(0);
        queryResult.Edges.Count.Should().Be(0);

        QueryBatchResult commandResults = newMapOption.Return();
        var compareMap = GraphCommandTools.CompareMap(_map, copyMap);
        compareMap.Count.Should().Be(1);
        compareMap.OfType<GraphNode>().Select(x => x.Key).Contains("node8").Should().BeTrue();
    }

    [Fact]
    public async Task ForeignKeyInTagButNoReferenceNode()
    {
        var copyMap = _map.Clone();
        var testClient = GraphTestStartup.CreateGraphTestHost(copyMap);
        var newMapOption = await testClient.ExecuteBatch("add node key=node8 set email=email:name@domain.com foreignkey email;", NullScopeContext.Default);
        newMapOption.IsOk().Should().BeFalse(newMapOption.ToString());

        copyMap.Meter.Node.GetCount().Should().Be(7);
        copyMap.Meter.Node.GetForeignKeyAdded().Should().Be(0);
        copyMap.Meter.Node.GetForeignKeyRemoved().Should().Be(0);

        var result = await testClient.Execute("select (key=node8) -> [*] ;", NullScopeContext.Default);
        result.IsOk().Should().BeTrue(result.ToString());

        var queryResult = result.Return();
        queryResult.Nodes.Count.Should().Be(0);
        queryResult.Edges.Count.Should().Be(0);
    }

    [Fact]
    public async Task ForeignKeyInTagWithNodeReference()
    {
        var copyMap = _map.Clone();
        var testClient = GraphTestStartup.CreateGraphTestHost(copyMap);

        var addOption = await testClient.ExecuteBatch("add node key=email:name@domain.com ;", NullScopeContext.Default);
        addOption.IsOk().Should().BeTrue();

        var newMapOption = await testClient.ExecuteBatch("add node key=node8 set email=email:name@domain.com foreignkey email;", NullScopeContext.Default);
        newMapOption.IsOk().Should().BeTrue(newMapOption.ToString());

        copyMap.Meter.Node.GetCount().Should().Be(9);
        copyMap.Meter.Node.GetForeignKeyAdded().Should().Be(1);
        copyMap.Meter.Node.GetForeignKeyRemoved().Should().Be(0);

        (await testClient.Execute("select (key=node8) -> [*] ;", NullScopeContext.Default)).Action(result =>
        {
            result.IsOk().Should().BeTrue(result.ToString());

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

        (await testClient.Execute("select (key=node8) -> [*] -> (*) ;", NullScopeContext.Default)).Action(result =>
        {
            result.IsOk().Should().BeTrue(result.ToString());

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

        (await testClient.Execute("select (key=email:name@domain.com) <- [*] <- (*) ;", NullScopeContext.Default)).Action(result =>
        {
            result.IsOk().Should().BeTrue(result.ToString());

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
        var copyMap = _map.Clone();
        var testClient = GraphTestStartup.CreateGraphTestHost(copyMap);
        copyMap.Meter.Node.GetCount().Should().Be(7);
        copyMap.Meter.Edge.GetCount().Should().Be(5);

        var addOption = await testClient.ExecuteBatch("add node key=email:name@domain.com ;", NullScopeContext.Default);
        addOption.IsOk().Should().BeTrue();
        copyMap.Meter.Node.GetCount().Should().Be(8);
        copyMap.Meter.Edge.GetCount().Should().Be(5);

        var newMapOption = await testClient.ExecuteBatch("add node key=node8 set email=email:name@domain.com foreignkey email;", NullScopeContext.Default);
        newMapOption.IsOk().Should().BeTrue(newMapOption.ToString());

        copyMap.Meter.Node.GetCount().Should().Be(9);
        copyMap.Meter.Node.GetForeignKeyAdded().Should().Be(1);
        copyMap.Meter.Node.GetForeignKeyRemoved().Should().Be(0);
        copyMap.Meter.Edge.GetCount().Should().Be(6);
        copyMap.Meter.Edge.GetAdded().Should().Be(6);
        copyMap.Meter.Edge.GetDeleted().Should().Be(0);

        (await testClient.Execute("select (key=node8) -> [*] ;", NullScopeContext.Default)).Action(result =>
        {
            result.IsOk().Should().BeTrue(result.ToString());

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

        (await testClient.ExecuteBatch("set node key=node8 foreignkey -email;", NullScopeContext.Default)).Action(result =>
        {
            result.IsOk().Should().BeTrue(result.ToString());
        });

        copyMap.Meter.Node.GetCount().Should().Be(9);
        copyMap.Meter.Node.GetForeignKeyAdded().Should().Be(1);
        copyMap.Meter.Node.GetForeignKeyRemoved().Should().Be(1);
        copyMap.Meter.Edge.GetCount().Should().Be(5);
        copyMap.Meter.Edge.GetAdded().Should().Be(6);
        copyMap.Meter.Edge.GetDeleted().Should().Be(1);

        (await testClient.Execute("select (key=node8) -> [*] ;", NullScopeContext.Default)).Action(result =>
        {
            result.IsOk().Should().BeTrue(result.ToString());

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
        var copyMap = _map.Clone();
        var testClient = GraphTestStartup.CreateGraphTestHost(copyMap);
        copyMap.Meter.Node.GetCount().Should().Be(7);
        copyMap.Meter.Edge.GetCount().Should().Be(5);

        var addOption = await testClient.ExecuteBatch("add node key=email:name@domain.com ;", NullScopeContext.Default);
        addOption.IsOk().Should().BeTrue();
        copyMap.Meter.Node.GetCount().Should().Be(8);
        copyMap.Meter.Edge.GetCount().Should().Be(5);

        var newMapOption = await testClient.ExecuteBatch("add node key=node8 set email=email:name@domain.com foreignkey email;", NullScopeContext.Default);
        newMapOption.IsOk().Should().BeTrue(newMapOption.ToString());

        copyMap.Meter.Node.GetCount().Should().Be(9);
        copyMap.Meter.Node.GetForeignKeyAdded().Should().Be(1);
        copyMap.Meter.Node.GetForeignKeyRemoved().Should().Be(0);
        copyMap.Meter.Edge.GetCount().Should().Be(6);
        copyMap.Meter.Edge.GetAdded().Should().Be(6);
        copyMap.Meter.Edge.GetDeleted().Should().Be(0);

        (await testClient.Execute("select (key=node8) -> [*] ;", NullScopeContext.Default)).Action(result =>
        {
            result.IsOk().Should().BeTrue(result.ToString());

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

        (await testClient.ExecuteBatch("set node key=node8 set -email;", NullScopeContext.Default)).Action(result =>
        {
            result.IsOk().Should().BeTrue(result.ToString());
        });

        copyMap.Meter.Node.GetCount().Should().Be(9);
        copyMap.Meter.Node.GetForeignKeyAdded().Should().Be(1);
        copyMap.Meter.Node.GetForeignKeyRemoved().Should().Be(1);
        copyMap.Meter.Edge.GetCount().Should().Be(5);
        copyMap.Meter.Edge.GetAdded().Should().Be(6);
        copyMap.Meter.Edge.GetDeleted().Should().Be(1);

        (await testClient.Execute("select (key=node8) -> [*] ;", NullScopeContext.Default)).Action(result =>
        {
            result.IsOk().Should().BeTrue(result.ToString());

            result.Return().Action(x =>
            {
                x.Nodes.Count.Should().Be(0);
                x.Edges.Count.Should().Be(0);
            });
        });

        (await testClient.ExecuteBatch("set node key=node8 set email=email:name@domain.com;", NullScopeContext.Default)).Action(result =>
        {
            result.IsOk().Should().BeTrue(result.ToString());
        });

        copyMap.Meter.Node.GetCount().Should().Be(9);
        copyMap.Meter.Node.GetForeignKeyAdded().Should().Be(2);
        copyMap.Meter.Node.GetForeignKeyRemoved().Should().Be(1);
        copyMap.Meter.Edge.GetCount().Should().Be(6);
        copyMap.Meter.Edge.GetAdded().Should().Be(7);
        copyMap.Meter.Edge.GetDeleted().Should().Be(1);

        (await testClient.Execute("select (key=node8) -> [*] ;", NullScopeContext.Default)).Action(result =>
        {
            result.IsOk().Should().BeTrue(result.ToString());

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
}
