using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph.test.Builders;

public class EdgeCommandBuilderTest
{
    private GraphLanguageParser BuildLanguageParser()
    {
        var host = Host.CreateDefaultBuilder()
            .ConfigureServices(service => service.AddSingleton<GraphLanguageParser>())
            .Build();

        return host.Services.GetRequiredService<GraphLanguageParser>();
    }

    [Fact]
    public void AddEdgeOnly()
    {
        var graphQuery = new EdgeCommandBuilder()
            .SetFromKey("fromNodeKey")
            .SetToKey("toNodeKey")
            .SetEdgeType("edgeType1")
            .Build();

        graphQuery.Be("add edge from=fromNodeKey, to=toNodeKey, type=edgeType1 ;");

        var parse = BuildLanguageParser().Parse(graphQuery);
        parse.Status.IsOk().BeTrue();
    }

    [Fact]
    public void SetEdgeOnly()
    {
        var graphQuery = new EdgeCommandBuilder()
            .UseSet()
            .SetFromKey("fromNodeKey")
            .SetToKey("toNodeKey")
            .SetEdgeType("edgeType1")
            .Build();

        graphQuery.Be("set edge from=fromNodeKey, to=toNodeKey, type=edgeType1 ;");

        var parse = BuildLanguageParser().Parse(graphQuery);
        parse.Status.IsOk().BeTrue();
    }

    [Fact]
    public void Tags()
    {
        var graphQuery = new EdgeCommandBuilder()
            .SetFromKey("fromNodeKey")
            .SetToKey("toNodeKey")
            .SetEdgeType("edgeType1")
            .AddTag("t1", "v1")
            .Build();

        graphQuery.Be("add edge from=fromNodeKey, to=toNodeKey, type=edgeType1 set t1=v1 ;");

        var parse = BuildLanguageParser().Parse(graphQuery);
        parse.Status.IsOk().BeTrue();
    }

    [Fact]
    public void TagWithNoValue()
    {
        var graphQuery = new EdgeCommandBuilder()
            .UseSet()
            .SetFromKey("fromNodeKey")
            .SetToKey("toNodeKey")
            .SetEdgeType("edgeType1")
            .AddTag("t1")
            .Build();

        graphQuery.Be("set edge from=fromNodeKey, to=toNodeKey, type=edgeType1 set t1 ;");

        var parse = BuildLanguageParser().Parse(graphQuery);
        parse.Status.IsOk().BeTrue();
    }

    [Fact]
    public void TagFormat()
    {
        var graphQuery = new EdgeCommandBuilder()
            .SetFromKey("fromNodeKey")
            .SetToKey("toNodeKey")
            .SetEdgeType("edgeType1")
            .AddTag("t1=v1")
            .Build();

        graphQuery.Be("add edge from=fromNodeKey, to=toNodeKey, type=edgeType1 set t1=v1 ;");

        var parse = BuildLanguageParser().Parse(graphQuery);
        parse.Status.IsOk().BeTrue();
    }

    [Fact]
    public void TagFormat2()
    {
        var graphQuery = new EdgeCommandBuilder()
            .SetFromKey("fromNodeKey")
            .SetToKey("toNodeKey")
            .SetEdgeType("edgeType1")
            .AddTag("t1=v1")
            .AddTag("t2=v2")
            .Build();

        graphQuery.Be("add edge from=fromNodeKey, to=toNodeKey, type=edgeType1 set t1=v1,t2=v2 ;");

        var parse = BuildLanguageParser().Parse(graphQuery);
        parse.Status.IsOk().BeTrue();
    }

    [Fact]
    public void TagRemove()
    {
        var graphQuery = new EdgeCommandBuilder()
            .UseSet()
            .SetFromKey("fromNodeKey")
            .SetToKey("toNodeKey")
            .SetEdgeType("edgeType1")
            .AddTag("-t1")
            .Build();

        graphQuery.Be("set edge from=fromNodeKey, to=toNodeKey, type=edgeType1 set -t1 ;");

        var parse = BuildLanguageParser().Parse(graphQuery);
        parse.Status.IsOk().BeTrue();
    }
}
