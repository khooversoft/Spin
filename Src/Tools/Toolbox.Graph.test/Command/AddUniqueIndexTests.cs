using Toolbox.Extensions;
using Toolbox.Tools.Should;
using Toolbox.Types;

namespace Toolbox.Graph.test.Command;

public class AddUniqueIndexTests
{
    [Fact]
    public async Task AddMultipleIndexes()
    {
        using GraphHostService testClient = await GraphTestStartup.CreateGraphService();
        var context = testClient.CreateScopeContext<AddUniqueIndexTests>();

        var e1 = await testClient.Execute("add node key=node1 set t1=v1, t2=v2 index t1, t2 ;", context);
        e1.IsOk().Should().BeTrue();

        testClient.Map.Nodes.LookupIndex("t1", "v1").Action(x =>
        {
            x.IsOk().Should().BeTrue();
            x.Return().NodeKey.Should().Be("node1");
        });

        testClient.Map.Nodes.LookupIndex("t3", "v3").IsOk().Should().BeFalse();

        var e2 = await testClient.Execute("add node key=node2 set t3=v3 index t3 ;", context);
        e2.IsOk().Should().BeTrue();

        testClient.Map.Nodes.LookupIndex("t1", "v1").Action(x =>
        {
            x.IsOk().Should().BeTrue();
            x.Return().NodeKey.Should().Be("node1");
        });

        testClient.Map.Nodes.LookupIndex("t3", "v3").Action(x =>
        {
            x.IsOk().Should().BeTrue();
            x.Return().NodeKey.Should().Be("node2");
        });

        testClient.Map.Nodes.LookupIndex("t3", "v1").IsOk().Should().BeFalse();
    }

    [Fact]
    public async Task AddMultipleIndexesDifferentValue()
    {
        using GraphHostService testClient = await GraphTestStartup.CreateGraphService();
        var context = testClient.CreateScopeContext<AddUniqueIndexTests>();

        var e1 = await testClient.Execute("add node key=node1 set t1=v1, t2=v2 index t1, t2 ;", context);
        e1.IsOk().Should().BeTrue();

        testClient.Map.Nodes.LookupIndex("t1", "v1").Action(x =>
        {
            x.IsOk().Should().BeTrue();
            x.Return().NodeKey.Should().Be("node1");
        });

        testClient.Map.Nodes.LookupIndex("t3", "v3").IsOk().Should().BeFalse();

        var e2 = await testClient.Execute("add node key=node2 set t1=v2 index t1 ;", context);
        e2.IsOk().Should().BeTrue();

        testClient.Map.Nodes.LookupIndex("t1", "v1").Action(x =>
        {
            x.IsOk().Should().BeTrue();
            x.Return().NodeKey.Should().Be("node1");
        });

        testClient.Map.Nodes.LookupIndex("t1", "v2").Action(x =>
        {
            x.IsOk().Should().BeTrue();
            x.Return().NodeKey.Should().Be("node2");
        });
    }

    [Fact]
    public async Task UniqueIndexViolation()
    {
        using GraphHostService testClient = await GraphTestStartup.CreateGraphService();
        var context = testClient.CreateScopeContext<AddUniqueIndexTests>();

        var e1 = await testClient.Execute("add node key=node1 set t1=v1, t2=v2 index t1, t2 ;", context);
        e1.IsOk().Should().BeTrue();

        testClient.Map.Nodes.LookupIndex("t1", "v1").Action(x =>
        {
            x.IsOk().Should().BeTrue();
            x.Return().NodeKey.Should().Be("node1");
        });

        var e2 = await testClient.Execute("add node key=node2 set t1=v1 index t1 ;", context);
        e2.IsError().Should().BeTrue();

        testClient.Map.Nodes.ContainsKey("node1").Should().BeTrue();
        testClient.Map.Nodes.ContainsKey("node2").Should().BeFalse();

        testClient.Map.Nodes.LookupIndex("t1", "v1").Action(x =>
        {
            x.IsOk().Should().BeTrue();
            x.Return().NodeKey.Should().Be("node1");
        });
    }

    [Fact]
    public async Task UniqueIndexViolation2()
    {
        using GraphHostService testClient = await GraphTestStartup.CreateGraphService();
        var context = testClient.CreateScopeContext<AddUniqueIndexTests>();

        var e1 = await testClient.Execute("add node key=node1 set t1=v1, t2=v2 index t1, t2 ;", context);
        e1.IsOk().Should().BeTrue();

        testClient.Map.Nodes.LookupIndex("t1", "v1").Action(x =>
        {
            x.IsOk().Should().BeTrue();
            x.Return().NodeKey.Should().Be("node1");
        });

        var e2 = await testClient.Execute("add node key=node3 set t1=v2 index t1 ;", context);
        e2.IsOk().Should().BeTrue();

        var e3 = await testClient.Execute("add node key=node2 set t1=v1 index t1 ;", context);
        e3.IsError().Should().BeTrue();

        testClient.Map.Nodes.ContainsKey("node1").Should().BeTrue();
        testClient.Map.Nodes.ContainsKey("node3").Should().BeTrue();
        testClient.Map.Nodes.ContainsKey("node2").Should().BeFalse();

        testClient.Map.Nodes.LookupIndex("t1", "v1").Action(x =>
        {
            x.IsOk().Should().BeTrue();
            x.Return().NodeKey.Should().Be("node1");
        });
    }
}
