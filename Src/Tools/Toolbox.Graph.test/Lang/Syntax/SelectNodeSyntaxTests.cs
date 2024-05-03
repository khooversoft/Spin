using Toolbox.LangTools;

namespace Toolbox.Graph.test.Lang.Graph;

public class SelectNodeSyntaxTests
{
    private readonly ILangRoot _root = GraphLangGrammar.Root;

    [Fact]
    public void SingleNode()
    {
        var test = new QueryTest
        {
            RawData = "select (key='string value');",
            Results = new List<IQueryResult>()
            {
                new QueryResult<LsSymbol>("select"),

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
    public void SingleWithReturnNode()
    {
        var test = new QueryTest
        {
            RawData = "select (key='string value') return data;",
            Results = new List<IQueryResult>()
            {
                new QueryResult<LsSymbol>("select"),

                new QueryResult<LsGroup>("(","node-group"),
                new QueryResult<LsValue>("key","lvalue"),
                new QueryResult<LsToken>("=", "equal"),
                new QueryResult<LsValue>("string value", "rvalue"),
                new QueryResult<LsGroup>(")","node-group"),
                new QueryResult<LsSymbol>("return", "return"),
                new QueryResult<LsValue>("data", "svalue"),
                new QueryResult<LsToken>(";", "term"),
            }
        };

        LangTestTools.Verify(_root, test);
    }


    [Fact]
    public void SingleWithReturnTwoDataNode()
    {
        var test = new QueryTest
        {
            RawData = "select (key='string value') return entity, contract;",
            Results = new List<IQueryResult>()
            {
                new QueryResult<LsSymbol>("select"),

                new QueryResult<LsGroup>("(","node-group"),
                new QueryResult<LsValue>("key","lvalue"),
                new QueryResult<LsToken>("=", "equal"),
                new QueryResult<LsValue>("string value", "rvalue"),
                new QueryResult<LsGroup>(")","node-group"),
                new QueryResult<LsSymbol>("return", "return"),
                new QueryResult<LsValue>("entity", "svalue"),
                new QueryResult<LsToken>(",", "delimiter"),
                new QueryResult<LsValue>("contract", "svalue"),
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
            RawData = "select (key='string value') a1;",
            Results = new List<IQueryResult>()
            {
                new QueryResult<LsSymbol>("select"),

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
    public void NodeAndEdgeAndNode()
    {
        var test = new QueryTest
        {
            RawData = "select (key=key1,tags=t1) -> [schedulework:active]->(schedule) n2;",
            Results = new List<IQueryResult>()
            {
                new QueryResult<LsSymbol>("select"),

                new QueryResult<LsGroup>("(","node-group"),
                new QueryResult<LsValue>("key","lvalue"),
                new QueryResult<LsToken>("=", "equal"),
                new QueryResult<LsValue>("key1", "rvalue"),
                new QueryResult<LsToken>(",", "delimiter"),
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
            RawData = "select (key=key1,tags=t1) n1 -> [edgeType=abc*,schedulework:active] -> (schedule) n2;",
            Results = new List<IQueryResult>()
            {
                new QueryResult<LsSymbol>("select"),

                new QueryResult<LsGroup>("(","node-group"),
                new QueryResult<LsValue>("key","lvalue"),
                new QueryResult<LsToken>("=", "equal"),
                new QueryResult<LsValue>("key1", "rvalue"),
                new QueryResult<LsToken>(",", "delimiter"),
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
                new QueryResult<LsToken>(",", "delimiter"),
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
            RawData = "select (t1) n1 -> [tags=schedulework:active]n3 -> (t2) n2 return entity, contract;",
            Results = new List<IQueryResult>()
            {
                new QueryResult<LsSymbol>("select"),

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

                new QueryResult<LsSymbol>("return", "return"),
                new QueryResult<LsValue>("entity", "svalue"),
                new QueryResult<LsToken>(",", "delimiter"),
                new QueryResult<LsValue>("contract", "svalue"),

                new QueryResult<LsToken>(";", "term"),
            }
        };

        LangTestTools.Verify(_root, test);
    }

    [Fact]
    public void ThreeNodeWildcard()
    {
        var test = new QueryTest
        {
            RawData = "select (*) -> [*] -> (*);",
            Results = new List<IQueryResult>()
            {
                new QueryResult<LsSymbol>("select"),

                new QueryResult<LsGroup>("(","node-group"),
                new QueryResult<LsValue>("*","svalue"),
                new QueryResult<LsGroup>(")","node-group"),
                new QueryResult<LsToken>("->","select-next"),
                new QueryResult<LsGroup>("[","edge-group"),
                new QueryResult<LsValue>("*","svalue"),
                new QueryResult<LsGroup>("]","edge-group"),
                new QueryResult<LsToken>("->","select-next"),
                new QueryResult<LsGroup>("(","node-group"),
                new QueryResult<LsValue>("*","svalue"),
                new QueryResult<LsGroup>(")","node-group"),
                new QueryResult<LsToken>(";", "term"),
            }
        };

        LangTestTools.Verify(_root, test);
    }
}
