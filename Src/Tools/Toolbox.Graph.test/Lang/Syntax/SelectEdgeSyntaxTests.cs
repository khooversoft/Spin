//using Toolbox.LangTools;

//namespace Toolbox.Graph.test.Lang.Graph;

//public class SelectEdgeSyntaxTests
//{
//    private readonly ILangRoot _root = GraphLangGrammar.Root;

//    [Fact]
//    public void SingleEdge()
//    {
//        var test = new QueryTest
//        {
//            RawData = "select [key='string value'];",
//            Results = new List<IQueryResult>()
//            {
//                new QueryResult<LsSymbol>("select"),

//                new QueryResult<LsGroup>("[","edge-group"),
//                new QueryResult<LsValue>("key","lvalue"),
//                new QueryResult<LsToken>("=", "equal"),
//                new QueryResult<LsValue>("string value", "rvalue"),
//                new QueryResult<LsGroup>("]","edge-group"),
//                new QueryResult<LsToken>(";", "term"),
//            }
//        };

//        LangTestTools.Verify(_root, test);
//    }

//    [Fact]
//    public void SingleEdgeWithAlias()
//    {
//        var test = new QueryTest
//        {
//            RawData = "select [key='string value'] a1;",
//            Results = new List<IQueryResult>()
//            {
//                new QueryResult<LsSymbol>("select"),

//                new QueryResult<LsGroup>("[","edge-group"),
//                new QueryResult<LsValue>("key","lvalue"),
//                new QueryResult<LsToken>("=", "equal"),
//                new QueryResult<LsValue>("string value", "rvalue"),
//                new QueryResult<LsGroup>("]","edge-group"),
//                new QueryResult<LsValue>("a1","alias"),
//                new QueryResult<LsToken>(";", "term"),
//            }
//        };

//        LangTestTools.Verify(_root, test);
//    }

//    [Fact]
//    public void NodeAndEdge()
//    {
//        var test = new QueryTest
//        {
//            RawData = "select (key=key1,tags=t1) -> [schedulework:active];",
//            Results = new List<IQueryResult>()
//            {
//                new QueryResult<LsSymbol>("select"),

//                new QueryResult<LsGroup>("(","node-group"),
//                new QueryResult<LsValue>("key","lvalue"),
//                new QueryResult<LsToken>("=", "equal"),
//                new QueryResult<LsValue>("key1", "rvalue"),
//                new QueryResult<LsToken>(",", "delimiter"),
//                new QueryResult<LsValue>("tags","lvalue"),
//                new QueryResult<LsToken>("=", "equal"),
//                new QueryResult<LsValue>("t1", "rvalue"),
//                new QueryResult<LsGroup>(")","node-group"),

//                new QueryResult<LsToken>("->", "select-next"),

//                new QueryResult<LsGroup>("[","edge-group"),
//                new QueryResult<LsValue>("schedulework:active","svalue"),
//                new QueryResult<LsGroup>("]","edge-group"),
//                new QueryResult<LsToken>(";", "term"),
//            }
//        };

//        LangTestTools.Verify(_root, test);
//    }

//    [Fact]
//    public void TwoNodeWildcard()
//    {
//        var test = new QueryTest
//        {
//            RawData = "select (*) -> [*];",
//            Results = new List<IQueryResult>()
//            {
//                new QueryResult<LsSymbol>("select"),

//                new QueryResult<LsGroup>("(","node-group"),
//                new QueryResult<LsValue>("*","svalue"),
//                new QueryResult<LsGroup>(")","node-group"),
//                new QueryResult<LsToken>("->","select-next"),
//                new QueryResult<LsGroup>("[","edge-group"),
//                new QueryResult<LsValue>("*","svalue"),
//                new QueryResult<LsGroup>("]","edge-group"),
//                new QueryResult<LsToken>(";", "term"),
//            }
//        };

//        LangTestTools.Verify(_root, test);
//    }
//}
