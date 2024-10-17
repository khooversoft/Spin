using FluentAssertions;
using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.Graph.test.Command;

public class AddUniqueIndexTests
{
    [Fact]
    public async Task AddMultipleIndexes()
    {
        var map = new GraphMap();
        var testClient = GraphTestStartup.CreateGraphTestHost(map);
        var e1 = await testClient.Execute("add node key=node1 set t1=v1, t2=v2 index t1, t2 ;", NullScopeContext.Instance);
        e1.IsOk().Should().BeTrue(e1.ToString());

        map.Nodes.LookupIndex("t1", "v1").Action(x =>
        {
            x.IsOk().Should().BeTrue();
            x.Return().NodeKey.Should().Be("node1");
        });

        map.Nodes.LookupIndex("t3", "v3").IsOk().Should().BeFalse();

        var e2 = await testClient.Execute("add node key=node2 set t3=v3 index t3 ;", NullScopeContext.Instance);
        e2.IsOk().Should().BeTrue(e1.ToString());

        map.Nodes.LookupIndex("t1", "v1").Action(x =>
        {
            x.IsOk().Should().BeTrue();
            x.Return().NodeKey.Should().Be("node1");
        });

        map.Nodes.LookupIndex("t3", "v3").Action(x =>
        {
            x.IsOk().Should().BeTrue();
            x.Return().NodeKey.Should().Be("node2");
        });

        map.Nodes.LookupIndex("t3", "v1").IsOk().Should().BeFalse();
    }

    [Fact]
    public async Task AddMultipleIndexesDifferentValue()
    {
        var map = new GraphMap();
        var testClient = GraphTestStartup.CreateGraphTestHost(map);
        var e1 = await testClient.Execute("add node key=node1 set t1=v1, t2=v2 index t1, t2 ;", NullScopeContext.Instance);
        e1.IsOk().Should().BeTrue(e1.ToString());

        map.Nodes.LookupIndex("t1", "v1").Action(x =>
        {
            x.IsOk().Should().BeTrue();
            x.Return().NodeKey.Should().Be("node1");
        });

        map.Nodes.LookupIndex("t3", "v3").IsOk().Should().BeFalse();

        var e2 = await testClient.Execute("add node key=node2 set t1=v2 index t1 ;", NullScopeContext.Instance);
        e2.IsOk().Should().BeTrue(e1.ToString());

        map.Nodes.LookupIndex("t1", "v1").Action(x =>
        {
            x.IsOk().Should().BeTrue();
            x.Return().NodeKey.Should().Be("node1");
        });

        map.Nodes.LookupIndex("t1", "v2").Action(x =>
        {
            x.IsOk().Should().BeTrue();
            x.Return().NodeKey.Should().Be("node2");
        });
    }

    [Fact]
    public async Task UniqueIndexViolation()
    {
        var map = new GraphMap();
        var testClient = GraphTestStartup.CreateGraphTestHost(map);
        var e1 = await testClient.Execute("add node key=node1 set t1=v1, t2=v2 index t1, t2 ;", NullScopeContext.Instance);
        e1.IsOk().Should().BeTrue(e1.ToString());

        map.Nodes.LookupIndex("t1", "v1").Action(x =>
        {
            x.IsOk().Should().BeTrue();
            x.Return().NodeKey.Should().Be("node1");
        });

        var e2 = await testClient.Execute("add node key=node2 set t1=v1 index t1 ;", NullScopeContext.Instance);
        e2.IsError().Should().BeTrue(e1.ToString());

        map.Nodes.ContainsKey("node1").Should().BeTrue();
        map.Nodes.ContainsKey("node2").Should().BeFalse();

        map.Nodes.LookupIndex("t1", "v1").Action(x =>
        {
            x.IsOk().Should().BeTrue();
            x.Return().NodeKey.Should().Be("node1");
        });
    }

    [Fact]
    public async Task UniqueIndexViolation2()
    {
        var map = new GraphMap();
        var testClient = GraphTestStartup.CreateGraphTestHost(map);
        var e1 = await testClient.Execute("add node key=node1 set t1=v1, t2=v2 index t1, t2 ;", NullScopeContext.Instance);
        e1.IsOk().Should().BeTrue(e1.ToString());

        map.Nodes.LookupIndex("t1", "v1").Action(x =>
        {
            x.IsOk().Should().BeTrue();
            x.Return().NodeKey.Should().Be("node1");
        });

        var e2 = await testClient.Execute("add node key=node3 set t1=v2 index t1 ;", NullScopeContext.Instance);
        e2.IsOk().Should().BeTrue(e1.ToString());

        var e3 = await testClient.Execute("add node key=node2 set t1=v1 index t1 ;", NullScopeContext.Instance);
        e3.IsError().Should().BeTrue(e1.ToString());

        map.Nodes.ContainsKey("node1").Should().BeTrue();
        map.Nodes.ContainsKey("node3").Should().BeTrue();
        map.Nodes.ContainsKey("node2").Should().BeFalse();

        map.Nodes.LookupIndex("t1", "v1").Action(x =>
        {
            x.IsOk().Should().BeTrue();
            x.Return().NodeKey.Should().Be("node1");
        });
    }
}
