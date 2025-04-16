using Toolbox.Extensions;
using Toolbox.Tools.Should;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Graph.test.Command;

public class SetUniqueIndexTests
{
    private readonly ITestOutputHelper _outputHelper;

    public SetUniqueIndexTests(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
    }

    [Fact]
    public async Task SetMultipleIndexes()
    {
        using GraphHostService graphTestClient = await GraphTestStartup.CreateGraphService(logOutput: x => _outputHelper.WriteLine(x));
        var context = graphTestClient.CreateScopeContext<SetUniqueIndexTests>();

        var e1 = await graphTestClient.Execute("set node key=node1 set t1=v1, t2=v2 index t1, t2 ;", context);
        e1.IsOk().Should().BeTrue(e1.ToString());

        graphTestClient.Map.Nodes.LookupIndex("t1", "v1").Action(x =>
        {
            x.IsOk().Should().BeTrue();
            x.Return().NodeKey.Should().Be("node1");
        });

        graphTestClient.Map.Nodes.LookupIndex("t3", "v3").IsOk().Should().BeFalse();

        var e2 = await graphTestClient.Execute("set node key=node2 set t3=v3 index t3 ;", context);
        e2.IsOk().Should().BeTrue(e1.ToString());

        graphTestClient.Map.Nodes.LookupIndex("t1", "v1").Action(x =>
        {
            x.IsOk().Should().BeTrue();
            x.Return().NodeKey.Should().Be("node1");
        });

        graphTestClient.Map.Nodes.LookupIndex("t3", "v3").Action(x =>
        {
            x.IsOk().Should().BeTrue();
            x.Return().NodeKey.Should().Be("node2");
        });

        graphTestClient.Map.Nodes.LookupIndex("t3", "v1").IsOk().Should().BeFalse();
    }

    [Fact]
    public async Task SetMultipleIndexesDifferentValue()
    {
        using GraphHostService graphTestClient = await GraphTestStartup.CreateGraphService(logOutput: x => _outputHelper.WriteLine(x));
        var context = graphTestClient.CreateScopeContext<SetUniqueIndexTests>();

        var e1 = await graphTestClient.Execute("set node key=node1 set t1=v1, t2=v2 index t1, t2 ;", context);
        e1.IsOk().Should().BeTrue(e1.ToString());

        graphTestClient.Map.Nodes.LookupIndex("t1", "v1").Action(x =>
        {
            x.IsOk().Should().BeTrue();
            x.Return().NodeKey.Should().Be("node1");
        });

        graphTestClient.Map.Nodes.LookupIndex("t3", "v3").IsOk().Should().BeFalse();

        var e2 = await graphTestClient.Execute("set node key=node2 set t1=v2 index t1 ;", context);
        e2.IsOk().Should().BeTrue(e1.ToString());

        graphTestClient.Map.Nodes.LookupIndex("t1", "v1").Action(x =>
        {
            x.IsOk().Should().BeTrue();
            x.Return().NodeKey.Should().Be("node1");
        });

        graphTestClient.Map.Nodes.LookupIndex("t1", "v2").Action(x =>
        {
            x.IsOk().Should().BeTrue();
            x.Return().NodeKey.Should().Be("node2");
        });
    }

    [Fact]
    public async Task SetUniqueIndexViolation()
    {
        using GraphHostService graphTestClient = await GraphTestStartup.CreateGraphService(logOutput: x => _outputHelper.WriteLine(x));
        var context = graphTestClient.CreateScopeContext<SetUniqueIndexTests>();

        var e1 = await graphTestClient.Execute("set node key=node1 set t1=v1, t2=v2 index t1, t2 ;", context);
        e1.IsOk().Should().BeTrue(e1.ToString());

        graphTestClient.Map.Nodes.LookupIndex("t1", "v1").Action(x =>
        {
            x.IsOk().Should().BeTrue();
            x.Return().NodeKey.Should().Be("node1");
        });

        var e2 = await graphTestClient.Execute("set node key=node2 set t1=v1 index t1 ;", context);
        e2.IsError().Should().BeTrue(e1.ToString());

        graphTestClient.Map.Nodes.ContainsKey("node1").Should().BeTrue();
        graphTestClient.Map.Nodes.ContainsKey("node2").Should().BeFalse();

        graphTestClient.Map.Nodes.LookupIndex("t1", "v1").Action(x =>
        {
            x.IsOk().Should().BeTrue();
            x.Return().NodeKey.Should().Be("node1");
        });
    }

    [Fact]
    public async Task SetUniqueIndexViolation2()
    {
        using GraphHostService graphTestClient = await GraphTestStartup.CreateGraphService(logOutput: x => _outputHelper.WriteLine(x));
        var context = graphTestClient.CreateScopeContext<SetUniqueIndexTests>();

        var e1 = await graphTestClient.Execute("set node key=node1 set t1=v1, t2=v2 index t1, t2 ;", context);
        e1.IsOk().Should().BeTrue(e1.ToString());

        graphTestClient.Map.Nodes.LookupIndex("t1", "v1").Action(x =>
        {
            x.IsOk().Should().BeTrue();
            x.Return().NodeKey.Should().Be("node1");
        });

        var e2 = await graphTestClient.Execute("set node key=node3 set t1=v2 index t1 ;", context);
        e2.IsOk().Should().BeTrue(e1.ToString());

        var e3 = await graphTestClient.Execute("set node key=node2 set t1=v1 index t1 ;", context);
        e3.IsError().Should().BeTrue(e1.ToString());

        graphTestClient.Map.Nodes.ContainsKey("node1").Should().BeTrue();
        graphTestClient.Map.Nodes.ContainsKey("node3").Should().BeTrue();
        graphTestClient.Map.Nodes.ContainsKey("node2").Should().BeFalse();

        graphTestClient.Map.Nodes.LookupIndex("t1", "v1").Action(x =>
        {
            x.IsOk().Should().BeTrue();
            x.Return().NodeKey.Should().Be("node1");
        });
    }

    [Fact]
    public async Task SetAndUpdateIndex()
    {
        using GraphHostService graphTestClient = await GraphTestStartup.CreateGraphService(logOutput: x => _outputHelper.WriteLine(x));
        var context = graphTestClient.CreateScopeContext<SetUniqueIndexTests>();

        var e1 = await graphTestClient.Execute("set node key=node1 set t1=v1, t2=v2 index t1, t2 ;", context);
        e1.IsOk().Should().BeTrue(e1.ToString());

        graphTestClient.Map.Nodes.LookupIndex("t1", "v1").Action(x =>
        {
            x.IsOk().Should().BeTrue();
            x.Return().NodeKey.Should().Be("node1");
        });

        graphTestClient.Map.Nodes.LookupIndex("t1", "v2").IsError().Should().BeTrue();

        var e2 = await graphTestClient.Execute("set node key=node1 set t1=v2 ;", context);
        e2.IsOk().Should().BeTrue(e1.ToString());

        graphTestClient.Map.Nodes.LookupIndex("t1", "v1").IsError().Should().BeTrue();

        graphTestClient.Map.Nodes.LookupIndex("t1", "v2").Action(x =>
        {
            x.IsOk().Should().BeTrue();
            x.Return().NodeKey.Should().Be("node1");
        });
    }
}
