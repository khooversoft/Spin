using Toolbox.Tools.Should;
using Toolbox.Types;

namespace Toolbox.Graph.test.Builders;

public class SelectNodeAndEdgeTests
{
    [Fact]
    public void SelectNodeToEdge()
    {
        var graphQuery = new SelectCommandBuilder()
            .AddNodeSearch(x => x.SetNodeKey("nodeKey1"))
            .AddLeftJoin()
            .AddEdgeSearch()
            .Build();

        Verify(graphQuery, "select (key=nodeKey1) -> [*] ;");
    }

    [Fact]
    public void SelectNodeToEdgeToNode()
    {
        var graphQuery = new SelectCommandBuilder()
            .AddNodeSearch(x => x.SetNodeKey("nodeKey1"))
            .AddLeftJoin()
            .AddEdgeSearch()
            .AddRightJoin()
            .AddNodeSearch()
            .Build();

        Verify(graphQuery, "select (key=nodeKey1) -> [*] <- (*) ;");
    }

    [Fact]
    public void SelectNodeToEdgeFromKeyLeftJoin()
    {
        var graphQuery = new SelectCommandBuilder()
            .AddNodeSearch(x => x.SetNodeKey("nodeKey1"))
            .AddLeftJoin()
            .AddEdgeSearch(x => x.SetFromKey("fromKey1"))
            .Build();

        Verify(graphQuery, "select (key=nodeKey1) -> [from=fromKey1] ;");
    }

    [Fact]
    public void SelectNodeToEdgeFromKeyRightJoin()
    {
        var graphQuery = new SelectCommandBuilder()
            .AddNodeSearch(x => x.SetNodeKey("nodeKey1"))
            .AddRightJoin()
            .AddEdgeSearch(x => x.SetFromKey("fromKey1"))
            .Build();

        Verify(graphQuery, "select (key=nodeKey1) <- [from=fromKey1] ;");
    }

    [Fact]
    public void SelectNodeToEdgeByTag()
    {
        var graphQuery = new SelectCommandBuilder()
            .AddNodeSearch(x => x.SetNodeKey("nodeKey1"))
            .AddLeftJoin()
            .AddEdgeSearch(x => x.AddTag("proposal"))
            .Build();

        Verify(graphQuery, "select (key=nodeKey1) -> [proposal] ;");
    }

    [Fact]
    public void SelectEdgeToNode()
    {
        var graphQuery = new SelectCommandBuilder()
            .AddEdgeSearch(x => x.AddTag("proposal"))
            .AddLeftJoin()
            .AddNodeSearch(x => x.SetNodeKey("nodeKey1"))
            .Build();

        Verify(graphQuery, "select [proposal] -> (key=nodeKey1) ;");
    }


    [Fact]
    public void SelectNodeToNode()
    {
        var graphQuery = new SelectCommandBuilder()
            .AddNodeSearch(x => x.SetNodeKey("nodeKey1"))
            .AddLeftJoin()
            .AddEdgeSearch(x => x.AddTag("proposal"))
            .AddLeftJoin()
            .AddNodeSearch(x => x.SetNodeKey("user:*"))
            .Build();

        Verify(graphQuery, "select (key=nodeKey1) -> [proposal] -> (key=user:*) ;");
    }


    private void Verify(string graphQuery, string expected)
    {
        graphQuery.Should().Be(expected);
        var parse = GraphLanguageTool.GetSyntaxRoot().Parse(graphQuery, NullScopeContext.Default);
        parse.Status.IsOk().Should().BeTrue();
    }
}
