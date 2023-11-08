using Toolbox.Data;
using Toolbox.LangTools;
using Xunit.Abstractions;

namespace Toolbox.Test.Lang.Graph;

public class GraphDeleteTests
{
    private readonly ILangRoot _root;

    public GraphDeleteTests()
    {
        _root = GraphLangGrammer.Root;
    }

    [Fact]
    public void SingleNode()
    {
        var test = new QueryTest
        {
            RawData = "delete (key='string value');",
            Results = new List<IQueryResult>()
            {
                new QueryResult<LsSymbol>("delete"),

                new QueryResult<LsGroup>("(","node-group"),
                new QueryResult<LsValue>("key","lvalue"),
                new QueryResult<LsToken>("=", "equal"),
                new QueryResult<LsValue>("string value", "rvalue"),
                new QueryResult<LsGroup>(")","node-group"),
                new QueryResult<LsToken>(";", "term"),
            }
        };

        LangTestTools.Verify(_root, test);
    }

    [Fact]
    public void SingleEdge()
    {
        var test = new QueryTest
        {
            RawData = "delete [key='string value'];",
            Results = new List<IQueryResult>()
            {
                new QueryResult<LsSymbol>("delete"),

                new QueryResult<LsGroup>("[","edge-group"),
                new QueryResult<LsValue>("key","lvalue"),
                new QueryResult<LsToken>("=", "equal"),
                new QueryResult<LsValue>("string value", "rvalue"),
                new QueryResult<LsGroup>("]","edge-group"),
                new QueryResult<LsToken>(";", "term"),
            }
        };

        LangTestTools.Verify(_root, test);
    }

    [Fact]
    public void SingleNodeWithAlias()
    {
        var test = new QueryTest
        {
            RawData = "delete (key='string value') a1;",
            Results = new List<IQueryResult>()
            {
                new QueryResult<LsSymbol>("delete"),

                new QueryResult<LsGroup>("(","node-group"),
                new QueryResult<LsValue>("key","lvalue"),
                new QueryResult<LsToken>("=", "equal"),
                new QueryResult<LsValue>("string value", "rvalue"),
                new QueryResult<LsGroup>(")","node-group"),
                new QueryResult<LsValue>("a1","alias"),
                new QueryResult<LsToken>(";", "term"),
            }
        };

        LangTestTools.Verify(_root, test);
    }


    [Fact]
    public void SingleEdgeWithAlias()
    {
        var test = new QueryTest
        {
            RawData = "delete [key='string value'] a1;",
            Results = new List<IQueryResult>()
            {
                new QueryResult<LsSymbol>("delete"),

                new QueryResult<LsGroup>("[","edge-group"),
                new QueryResult<LsValue>("key","lvalue"),
                new QueryResult<LsToken>("=", "equal"),
                new QueryResult<LsValue>("string value", "rvalue"),
                new QueryResult<LsGroup>("]","edge-group"),
                new QueryResult<LsValue>("a1","alias"),
                new QueryResult<LsToken>(";", "term"),
            }
        };

        LangTestTools.Verify(_root, test);
    }

    [Fact]
    public void NodeAndEdge()
    {
        var test = new QueryTest
        {
            RawData = "delete (key=key1;tags=t1) -> [schedulework:active];",
            Results = new List<IQueryResult>()
            {
                new QueryResult<LsSymbol>("delete"),

                new QueryResult<LsGroup>("(","node-group"),
                new QueryResult<LsValue>("key","lvalue"),
                new QueryResult<LsToken>("=", "equal"),
                new QueryResult<LsValue>("key1", "rvalue"),
                new QueryResult<LsToken>(";", "delimiter"),
                new QueryResult<LsValue>("tags","lvalue"),
                new QueryResult<LsToken>("=", "equal"),
                new QueryResult<LsValue>("t1", "rvalue"),
                new QueryResult<LsGroup>(")","node-group"),

                new QueryResult<LsToken>("->", "select-next"),

                new QueryResult<LsGroup>("[","edge-group"),
                new QueryResult<LsValue>("schedulework:active","svalue"),
                new QueryResult<LsGroup>("]","edge-group"),
                new QueryResult<LsToken>(";", "term"),
            }
        };

        LangTestTools.Verify(_root, test);
    }

    [Fact]
    public void NodeAndEdgeAndNode()
    {
        var test = new QueryTest
        {
            RawData = "delete (key=key1;tags=t1) -> [schedulework:active]->(schedule) n2;",
            Results = new List<IQueryResult>()
            {
                new QueryResult<LsSymbol>("delete"),

                new QueryResult<LsGroup>("(","node-group"),
                new QueryResult<LsValue>("key","lvalue"),
                new QueryResult<LsToken>("=", "equal"),
                new QueryResult<LsValue>("key1", "rvalue"),
                new QueryResult<LsToken>(";", "delimiter"),
                new QueryResult<LsValue>("tags","lvalue"),
                new QueryResult<LsToken>("=", "equal"),
                new QueryResult<LsValue>("t1", "rvalue"),
                new QueryResult<LsGroup>(")","node-group"),

                new QueryResult<LsToken>("->", "select-next"),

                new QueryResult<LsGroup>("[","edge-group"),
                new QueryResult<LsValue>("schedulework:active","svalue"),
                new QueryResult<LsGroup>("]","edge-group"),

                new QueryResult<LsToken>("->", "select-next"),
                new QueryResult<LsGroup>("(","node-group"),
                new QueryResult<LsValue>("schedule","svalue"),
                new QueryResult<LsGroup>(")","node-group"),
                new QueryResult<LsValue>("n2","alias"),
                new QueryResult<LsToken>(";", "term"),
            }
        };

        LangTestTools.Verify(_root, test);
    }

    [Fact]
    public void Example1()
    {
        var test = new QueryTest
        {
            RawData = "delete (key=key1;tags=t1) n1 -> [edgeType=abc*;schedulework:active] -> (schedule) n2;",
            Results = new List<IQueryResult>()
            {
                new QueryResult<LsSymbol>("delete"),

                new QueryResult<LsGroup>("(","node-group"),
                new QueryResult<LsValue>("key","lvalue"),
                new QueryResult<LsToken>("=", "equal"),
                new QueryResult<LsValue>("key1", "rvalue"),
                new QueryResult<LsToken>(";", "delimiter"),
                new QueryResult<LsValue>("tags","lvalue"),
                new QueryResult<LsToken>("=", "equal"),
                new QueryResult<LsValue>("t1", "rvalue"),
                new QueryResult<LsGroup>(")","node-group"),
                new QueryResult<LsValue>("n1","alias"),

                new QueryResult<LsToken>("->", "select-next"),

                new QueryResult<LsGroup>("[","edge-group"),
                new QueryResult<LsValue>("edgeType","lvalue"),
                new QueryResult<LsToken>("=", "equal"),
                new QueryResult<LsValue>("abc*", "rvalue"),
                new QueryResult<LsToken>(";", "delimiter"),
                new QueryResult<LsValue>("schedulework:active","svalue"),
                new QueryResult<LsGroup>("]","edge-group"),

                new QueryResult<LsToken>("->", "select-next"),
                new QueryResult<LsGroup>("(","node-group"),
                new QueryResult<LsValue>("schedule","svalue"),
                new QueryResult<LsGroup>(")","node-group"),
                new QueryResult<LsValue>("n2","alias"),
                new QueryResult<LsToken>(";", "term"),
            }
        };

        LangTestTools.Verify(_root, test);
    }

    [Fact]
    public void Example2()
    {
        var test = new QueryTest
        {
            RawData = "delete (t1) n1 -> [tags=schedulework:active]n3 -> (t2) n2;",
            Results = new List<IQueryResult>()
            {
                new QueryResult<LsSymbol>("delete"),

                new QueryResult<LsGroup>("(","node-group"),
                new QueryResult<LsValue>("t1","svalue"),
                new QueryResult<LsGroup>(")","node-group"),
                new QueryResult<LsValue>("n1","alias"),

                new QueryResult<LsToken>("->", "select-next"),

                new QueryResult<LsGroup>("[","edge-group"),
                new QueryResult<LsValue>("tags","lvalue"),
                new QueryResult<LsToken>("=", "equal"),
                new QueryResult<LsValue>("schedulework:active", "rvalue"),
                new QueryResult<LsGroup>("]","edge-group"),
                new QueryResult<LsValue>("n3","alias"),

                new QueryResult<LsToken>("->", "select-next"),

                new QueryResult<LsGroup>("(","node-group"),
                new QueryResult<LsValue>("t2","svalue"),
                new QueryResult<LsGroup>(")","node-group"),
                new QueryResult<LsValue>("n2","alias"),
                new QueryResult<LsToken>(";", "term"),
            }
        };

        LangTestTools.Verify(_root, test);
    }
}
