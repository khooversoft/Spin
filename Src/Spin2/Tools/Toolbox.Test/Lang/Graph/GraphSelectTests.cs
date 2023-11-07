using Toolbox.LangTools;
using Xunit.Abstractions;

namespace Toolbox.Test.Lang.Graph;

public class GraphSelectTests
{
    private readonly ITestOutputHelper _output;
    private readonly ILangRoot _root;

    public GraphSelectTests(ITestOutputHelper output)
    {
        _output = output;
        _root = GraphLangGrammer.Root;
    }

    [Fact]
    public void SingleNode()
    {
        var test = new QueryTest
        {
            RawData = "select (key='string value');",
            Results = new List<IQueryResult>()
            {
                new QueryResult<LsSymbol>("select"),

                new QueryResult<LsGroup>("(","pgroup"),
                new QueryResult<LsValue>("key","lvalue"),
                new QueryResult<LsToken>("=", "equal"),
                new QueryResult<LsValue>("string value", "rvalue"),
                new QueryResult<LsGroup>(")","pgroup"),
                new QueryResult<LsToken>(";", "term"),
            }
        };

        LangTestTools.Verify(_output, _root, test);
    }

    [Fact]
    public void SingleEdge()
    {
        var test = new QueryTest
        {
            RawData = "select [key='string value'];",
            Results = new List<IQueryResult>()
            {
                new QueryResult<LsSymbol>("select"),

                new QueryResult<LsGroup>("[","bgroup"),
                new QueryResult<LsValue>("key","lvalue"),
                new QueryResult<LsToken>("=", "equal"),
                new QueryResult<LsValue>("string value", "rvalue"),
                new QueryResult<LsGroup>("]","bgroup"),
                new QueryResult<LsToken>(";", "term"),
            }
        };

        LangTestTools.Verify(_output, _root, test);
    }

    [Fact]
    public void SingleNodeWithAlias()
    {
        var test = new QueryTest
        {
            RawData = "select (key='string value') a1;",
            Results = new List<IQueryResult>()
            {
                new QueryResult<LsSymbol>("select"),

                new QueryResult<LsGroup>("(","pgroup"),
                new QueryResult<LsValue>("key","lvalue"),
                new QueryResult<LsToken>("=", "equal"),
                new QueryResult<LsValue>("string value", "rvalue"),
                new QueryResult<LsGroup>(")","pgroup"),
                new QueryResult<LsValue>("a1","alias"),
                new QueryResult<LsToken>(";", "term"),
            }
        };

        LangTestTools.Verify(_output, _root, test);
    }


    [Fact]
    public void SingleEdgeWithAlias()
    {
        var test = new QueryTest
        {
            RawData = "select [key='string value'] a1;",
            Results = new List<IQueryResult>()
            {
                new QueryResult<LsSymbol>("select"),

                new QueryResult<LsGroup>("[","bgroup"),
                new QueryResult<LsValue>("key","lvalue"),
                new QueryResult<LsToken>("=", "equal"),
                new QueryResult<LsValue>("string value", "rvalue"),
                new QueryResult<LsGroup>("]","bgroup"),
                new QueryResult<LsValue>("a1","alias"),
                new QueryResult<LsToken>(";", "term"),
            }
        };

        LangTestTools.Verify(_output, _root, test);
    }

    [Fact]
    public void NodeAndEdge()
    {
        var test = new QueryTest
        {
            RawData = "select (key=key1;tags=t1) -> [schedulework:active];",
            Results = new List<IQueryResult>()
            {
                new QueryResult<LsSymbol>("select"),

                new QueryResult<LsGroup>("(","pgroup"),
                new QueryResult<LsValue>("key","lvalue"),
                new QueryResult<LsToken>("=", "equal"),
                new QueryResult<LsValue>("key1", "rvalue"),
                new QueryResult<LsToken>(";", "delimiter"),
                new QueryResult<LsValue>("tags","lvalue"),
                new QueryResult<LsToken>("=", "equal"),
                new QueryResult<LsValue>("t1", "rvalue"),
                new QueryResult<LsGroup>(")","pgroup"),

                new QueryResult<LsToken>("->", "next"),

                new QueryResult<LsGroup>("[","bgroup"),
                new QueryResult<LsValue>("schedulework:active","svalue"),
                new QueryResult<LsGroup>("]","bgroup"),
                new QueryResult<LsToken>(";", "term"),
            }
        };

        LangTestTools.Verify(_output, _root, test);
    }

    [Fact]
    public void NodeAndEdgeAndNode()
    {
        var test = new QueryTest
        {
            RawData = "select (key=key1;tags=t1) -> [schedulework:active]->(schedule) n2;",
            Results = new List<IQueryResult>()
            {
                new QueryResult<LsSymbol>("select"),

                new QueryResult<LsGroup>("(","pgroup"),
                new QueryResult<LsValue>("key","lvalue"),
                new QueryResult<LsToken>("=", "equal"),
                new QueryResult<LsValue>("key1", "rvalue"),
                new QueryResult<LsToken>(";", "delimiter"),
                new QueryResult<LsValue>("tags","lvalue"),
                new QueryResult<LsToken>("=", "equal"),
                new QueryResult<LsValue>("t1", "rvalue"),
                new QueryResult<LsGroup>(")","pgroup"),

                new QueryResult<LsToken>("->", "next"),

                new QueryResult<LsGroup>("[","bgroup"),
                new QueryResult<LsValue>("schedulework:active","svalue"),
                new QueryResult<LsGroup>("]","bgroup"),

                new QueryResult<LsToken>("->", "next"),
                new QueryResult<LsGroup>("(","pgroup"),
                new QueryResult<LsValue>("schedule","svalue"),
                new QueryResult<LsGroup>(")","pgroup"),
                new QueryResult<LsValue>("n2","alias"),
                new QueryResult<LsToken>(";", "term"),
            }
        };

        LangTestTools.Verify(_output, _root, test);
    }

    [Fact]
    public void Example1()
    {
        var test = new QueryTest
        {
            RawData = "select (key=key1;tags=t1) n1 -> [edgeType=abc*;schedulework:active] -> (schedule) n2;",
            Results = new List<IQueryResult>()
            {
                new QueryResult<LsSymbol>("select"),

                new QueryResult<LsGroup>("(","pgroup"),
                new QueryResult<LsValue>("key","lvalue"),
                new QueryResult<LsToken>("=", "equal"),
                new QueryResult<LsValue>("key1", "rvalue"),
                new QueryResult<LsToken>(";", "delimiter"),
                new QueryResult<LsValue>("tags","lvalue"),
                new QueryResult<LsToken>("=", "equal"),
                new QueryResult<LsValue>("t1", "rvalue"),
                new QueryResult<LsGroup>(")","pgroup"),
                new QueryResult<LsValue>("n1","alias"),

                new QueryResult<LsToken>("->", "next"),

                new QueryResult<LsGroup>("[","bgroup"),
                new QueryResult<LsValue>("edgeType","lvalue"),
                new QueryResult<LsToken>("=", "equal"),
                new QueryResult<LsValue>("abc*", "rvalue"),
                new QueryResult<LsToken>(";", "delimiter"),
                new QueryResult<LsValue>("schedulework:active","svalue"),
                new QueryResult<LsGroup>("]","bgroup"),

                new QueryResult<LsToken>("->", "next"),
                new QueryResult<LsGroup>("(","pgroup"),
                new QueryResult<LsValue>("schedule","svalue"),
                new QueryResult<LsGroup>(")","pgroup"),
                new QueryResult<LsValue>("n2","alias"),
                new QueryResult<LsToken>(";", "term"),
            }
        };

        LangTestTools.Verify(_output, _root, test);
    }

    [Fact]
    public void Example2()
    {
        var test = new QueryTest
        {
            RawData = "select (t1) n1 -> [tags=schedulework:active]n3 -> (t2) n2;",
            Results = new List<IQueryResult>()
            {
                new QueryResult<LsSymbol>("select"),

                new QueryResult<LsGroup>("(","pgroup"),
                new QueryResult<LsValue>("t1","svalue"),
                new QueryResult<LsGroup>(")","pgroup"),
                new QueryResult<LsValue>("n1","alias"),

                new QueryResult<LsToken>("->", "next"),

                new QueryResult<LsGroup>("[","bgroup"),
                new QueryResult<LsValue>("tags","lvalue"),
                new QueryResult<LsToken>("=", "equal"),
                new QueryResult<LsValue>("schedulework:active", "rvalue"),
                new QueryResult<LsGroup>("]","bgroup"),
                new QueryResult<LsValue>("n3","alias"),

                new QueryResult<LsToken>("->", "next"),

                new QueryResult<LsGroup>("(","pgroup"),
                new QueryResult<LsValue>("t2","svalue"),
                new QueryResult<LsGroup>(")","pgroup"),
                new QueryResult<LsValue>("n2","alias"),
                new QueryResult<LsToken>(";", "term"),
            }
        };

        LangTestTools.Verify(_output, _root, test);
    }
}
