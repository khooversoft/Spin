using Toolbox.Tools.Should;
using Toolbox.Types;

namespace Toolbox.Graph.test.Builders;

public class EdgeCommandBuilderTest
{
    [Fact]
    public void AddEdgeOnly()
    {
        var graphQuery = new EdgeCommandBuilder()
            .SetFromKey("fromNodeKey")
            .SetToKey("toNodeKey")
            .SetEdgeType("edgeType1")
            .Build();

        graphQuery.Should().Be("add edge from=fromNodeKey, to=toNodeKey, type=edgeType1 ;");

        var parse = GraphLanguageTool.GetSyntaxRoot().Parse(graphQuery, NullScopeContext.Default);
        parse.Status.IsOk().Should().BeTrue();
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

        graphQuery.Should().Be("set edge from=fromNodeKey, to=toNodeKey, type=edgeType1 ;");

        var parse = GraphLanguageTool.GetSyntaxRoot().Parse(graphQuery, NullScopeContext.Default);
        parse.Status.IsOk().Should().BeTrue();
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

        graphQuery.Should().Be("add edge from=fromNodeKey, to=toNodeKey, type=edgeType1 set t1=v1 ;");

        var parse = GraphLanguageTool.GetSyntaxRoot().Parse(graphQuery, NullScopeContext.Default);
        parse.Status.IsOk().Should().BeTrue();
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

        graphQuery.Should().Be("set edge from=fromNodeKey, to=toNodeKey, type=edgeType1 set t1 ;");

        var parse = GraphLanguageTool.GetSyntaxRoot().Parse(graphQuery, NullScopeContext.Default);
        parse.Status.IsOk().Should().BeTrue();
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

        graphQuery.Should().Be("add edge from=fromNodeKey, to=toNodeKey, type=edgeType1 set t1=v1 ;");

        var parse = GraphLanguageTool.GetSyntaxRoot().Parse(graphQuery, NullScopeContext.Default);
        parse.Status.IsOk().Should().BeTrue();
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

        graphQuery.Should().Be("add edge from=fromNodeKey, to=toNodeKey, type=edgeType1 set t1=v1,t2=v2 ;");

        var parse = GraphLanguageTool.GetSyntaxRoot().Parse(graphQuery, NullScopeContext.Default);
        parse.Status.IsOk().Should().BeTrue();
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

        graphQuery.Should().Be("set edge from=fromNodeKey, to=toNodeKey, type=edgeType1 set -t1 ;");

        var parse = GraphLanguageTool.GetSyntaxRoot().Parse(graphQuery, NullScopeContext.Default);
        parse.Status.IsOk().Should().BeTrue();
    }
}
