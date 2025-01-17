using Toolbox.Tools.Should;
using Toolbox.Types;

namespace Toolbox.Graph.test.Builders;

public class SelectCommandBuilderTests
{
    [Fact]
    public void SelectNodeWildCard()
    {
        var graphQuery = new SelectCommandBuilder()
            .AddNodeSearch()
            .Build();

        Verify(graphQuery, "select (*) ;");
    }

    [Fact]
    public void SelectEdgeWildcard()
    {
        var graphQuery = new SelectCommandBuilder()
            .AddEdgeSearch()
            .Build();

        Verify(graphQuery, "select [*] ;");
    }

    [Fact]
    public void SelectNode()
    {
        var graphQuery = new SelectCommandBuilder()
            .AddNodeSearch(x => x.SetNodeKey("nodeKey1"))
            .Build();

        Verify(graphQuery, "select (key=nodeKey1) ;");
    }

    [Fact]
    public void SelectDataName()
    {
        var graphQuery = new SelectCommandBuilder()
            .AddNodeSearch(x => x.SetNodeKey("nodeKey1"))
            .AddDataName("entity")
            .Build();

        Verify(graphQuery, "select (key=nodeKey1) return entity ;");
    }

    [Fact]
    public void SelectDataName2()
    {
        var graphQuery = new SelectCommandBuilder()
            .AddNodeSearch(x => x.SetNodeKey("nodeKey1"))
            .AddDataName("entity")
            .AddDataName("data2")
            .Build();

        Verify(graphQuery, "select (key=nodeKey1) return data2,entity ;");
    }

    [Fact]
    public void SelectByTag()
    {
        var graphQuery = new SelectCommandBuilder()
            .AddNodeSearch(x => x.AddTag("t1"))
            .Build();

        Verify(graphQuery, "select (t1) ;");
    }

    [Fact]
    public void SelectByTag2()
    {
        var graphQuery = new SelectCommandBuilder()
            .AddNodeSearch(x => x.AddTag("t1=v1"))
            .Build();

        Verify(graphQuery, "select (t1=v1) ;");
    }

    [Fact]
    public void SelectByWildcard()
    {
        var graphQuery = new SelectCommandBuilder()
            .AddNodeSearch(x => x.AddTag("*"))
            .Build();

        Verify(graphQuery, "select (*) ;");
    }

    [Fact]
    public void SelectByNodeAndTag()
    {
        var graphQuery = new SelectCommandBuilder()
            .AddNodeSearch(x => x.SetNodeKey("nodeKey1").AddTag("t1=v1"))
            .Build();

        Verify(graphQuery, "select (key=nodeKey1, t1=v1) ;");
    }

    [Fact]
    public void SelectEdgeFrom()
    {
        var graphQuery = new SelectCommandBuilder()
            .AddEdgeSearch(x => x.SetFromKey("nodeKey1"))
            .Build();

        Verify(graphQuery, "select [from=nodeKey1] ;");
    }

    [Fact]
    public void SelectEdgeTo()
    {
        var graphQuery = new SelectCommandBuilder()
            .AddEdgeSearch(x => x.SetToKey("toNodeKey1"))
            .Build();

        Verify(graphQuery, "select [to=toNodeKey1] ;");
    }

    [Fact]
    public void SelectEdgeType()
    {
        var graphQuery = new SelectCommandBuilder()
            .AddEdgeSearch(x => x.SetEdgeType("edgeType2"))
            .Build();

        Verify(graphQuery, "select [type=edgeType2] ;");
    }

    [Fact]
    public void SelectEdgeAll()
    {
        var graphQuery = new SelectCommandBuilder()
            .AddEdgeSearch(x => x.SetFromKey("nodeKey1").SetToKey("ToKey1").SetEdgeType("edgeType1"))
            .Build();

        Verify(graphQuery, "select [from=nodeKey1, to=ToKey1, type=edgeType1] ;");
    }

    [Fact]
    public void SelectEdgeTags()
    {
        var graphQuery = new SelectCommandBuilder()
            .AddEdgeSearch(x => x.AddTag("t1"))
            .Build();

        Verify(graphQuery, "select [t1] ;");
    }

    [Fact]
    public void SelectEdgeTags2()
    {
        var graphQuery = new SelectCommandBuilder()
            .AddEdgeSearch(x => x.AddTag("t1").AddTag("t2", "v2"))
            .Build();

        Verify(graphQuery, "select [t1,t2=v2] ;");
    }

    private void Verify(string graphQuery, string expected)
    {
        graphQuery.Should().Be(expected);
        var parse = GraphLanguageTool.GetSyntaxRoot().Parse(graphQuery, NullScopeContext.Default);
        parse.Status.IsOk().Should().BeTrue();
    }
}