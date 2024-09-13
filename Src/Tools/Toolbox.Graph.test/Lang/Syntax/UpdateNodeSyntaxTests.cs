//using Toolbox.LangTools;

//namespace Toolbox.Graph.test.Lang.Graph;

//public class UpdateNodeSyntaxTests
//{
//    private readonly ILangRoot _root = GraphLangGrammar.Root;

//    [Fact]
//    public void SingleNode()
//    {
//        var test = new QueryTest
//        {
//            RawData = "update (key=key1) set key=key2,tags=t2;",
//            Results = new List<IQueryResult>()
//            {
//                new QueryResult<LsSymbol>("update"),

//                new QueryResult<LsGroup>("(","node-group"),
//                new QueryResult<LsValue>("key","lvalue"),
//                new QueryResult<LsToken>("=", "equal"),
//                new QueryResult<LsValue>("key1", "rvalue"),
//                new QueryResult<LsGroup>(")","node-group"),

//                new QueryResult<LsSymbol>("set"),

//                new QueryResult<LsValue>("key","lvalue"),
//                new QueryResult<LsToken>("=", "equal"),
//                new QueryResult<LsValue>("key2", "rvalue"),
//                new QueryResult<LsToken>(",", "delimiter"),
//                new QueryResult<LsValue>("tags","lvalue"),
//                new QueryResult<LsToken>("=", "equal"),
//                new QueryResult<LsValue>("t2", "rvalue"),

//                new QueryResult<LsToken>(";", "term"),
//            }
//        };

//        LangTestTools.Verify(_root, test);
//    }

//    [Fact]
//    public void SingleTagValue()
//    {
//        var test = new QueryTest
//        {
//            RawData = "update (*) set t1;",
//            Results = new List<IQueryResult>()
//            {
//                new QueryResult<LsSymbol>("update"),

//                new QueryResult<LsGroup>("(","node-group"),
//                new QueryResult<LsValue>("*","svalue"),
//                new QueryResult<LsGroup>(")","node-group"),

//                new QueryResult<LsSymbol>("set"),

//                new QueryResult<LsValue>("t1","svalue"),
//                new QueryResult<LsToken>(";", "term"),
//            }
//        };

//        LangTestTools.Verify(_root, test);
//    }

//    [Fact]
//    public void TwoSingleTagValue()
//    {
//        var test = new QueryTest
//        {
//            RawData = "update (*) set t1, t2;",
//            Results = new List<IQueryResult>()
//            {
//                new QueryResult<LsSymbol>("update"),
//                new QueryResult<LsGroup>("(","node-group"),
//                new QueryResult<LsValue>("*","svalue"),
//                new QueryResult<LsGroup>(")","node-group"),

//                new QueryResult<LsSymbol>("set"),

//                new QueryResult<LsValue>("t1","svalue"),
//                new QueryResult<LsToken>(",","delimiter"),
//                new QueryResult<LsValue>("t2","svalue"),
//                new QueryResult<LsToken>(";", "term"),
//            }
//        };

//        LangTestTools.Verify(_root, test);
//    }

//    [Fact]
//    public void SingleTagWithValue()
//    {
//        var test = new QueryTest
//        {
//            RawData = "update (*) set t1 = v;",
//            Results = new List<IQueryResult>()
//            {
//                new QueryResult<LsSymbol>("update"),
//                new QueryResult<LsGroup>("(","node-group"),
//                new QueryResult<LsValue>("*","svalue"),
//                new QueryResult<LsGroup>(")","node-group"),

//                new QueryResult<LsSymbol>("set"),

//                new QueryResult<LsValue>("t1","lvalue"),
//                new QueryResult<LsToken>("=","equal"),
//                new QueryResult<LsValue>("v","rvalue"),
//                new QueryResult<LsToken>(";", "term"),
//            }
//        };

//        LangTestTools.Verify(_root, test);
//    }

//    [Fact]
//    public void TwoTagsWithOneValue()
//    {
//        var test = new QueryTest
//        {
//            RawData = "update (*) set t1 = v,t2;",
//            Results = new List<IQueryResult>()
//            {
//                new QueryResult<LsSymbol>("update"),
//                new QueryResult<LsGroup>("(","node-group"),
//                new QueryResult<LsValue>("*","svalue"),
//                new QueryResult<LsGroup>(")","node-group"),

//                new QueryResult<LsSymbol>("set"),

//                new QueryResult<LsValue>("t1","lvalue"),
//                new QueryResult<LsToken>("=","equal"),
//                new QueryResult<LsValue>("v","rvalue"),
//                new QueryResult<LsToken>(",","delimiter"),
//                new QueryResult<LsValue>("t2","svalue"),
//                new QueryResult<LsToken>(";", "term"),
//            }
//        };

//        LangTestTools.Verify(_root, test);
//    }

//    [Fact]
//    public void TwoTagsWithValues()
//    {
//        var test = new QueryTest
//        {
//            RawData = "update (*) set t1 = v,t2=v2;",
//            Results = new List<IQueryResult>()
//            {
//                new QueryResult<LsSymbol>("update"),
//                new QueryResult<LsGroup>("(","node-group"),
//                new QueryResult<LsValue>("*","svalue"),
//                new QueryResult<LsGroup>(")","node-group"),

//                new QueryResult<LsSymbol>("set"),

//                new QueryResult<LsValue>("t1","lvalue"),
//                new QueryResult<LsToken>("=","equal"),
//                new QueryResult<LsValue>("v","rvalue"),
//                new QueryResult<LsToken>(",","delimiter"),
//                new QueryResult<LsValue>("t2","lvalue"),
//                new QueryResult<LsToken>("=","equal"),
//                new QueryResult<LsValue>("v2","rvalue"),
//                new QueryResult<LsToken>(";", "term"),
//            }
//        };

//        LangTestTools.Verify(_root, test);
//    }

//    [Fact]
//    public void SingleDataTagValue()
//    {
//        var test = new QueryTest
//        {
//            RawData = "update (*) set entity { abc };",
//            Results = new List<IQueryResult>()
//            {
//                new QueryResult<LsSymbol>("update"),

//                new QueryResult<LsGroup>("(","node-group"),
//                new QueryResult<LsValue>("*","svalue"),
//                new QueryResult<LsGroup>(")","node-group"),

//                new QueryResult<LsSymbol>("set"),

//                new QueryResult<LsValue>("entity", "dataName"),
//                new QueryResult<LsGroup>("{","dataGroup"),
//                new QueryResult<LsValue>("abc","svalue"),
//                new QueryResult<LsGroup>("}","dataGroup"),

//                new QueryResult<LsToken>(";", "term"),
//            }
//        };

//        LangTestTools.Verify(_root, test);
//    }

//    [Fact]
//    public void SingleDataWithTagValue()
//    {
//        var test = new QueryTest
//        {
//            RawData = "update (*) set entity { abc }, t1=v,t2=v2;",
//            Results = new List<IQueryResult>()
//            {
//                new QueryResult<LsSymbol>("update"),

//                new QueryResult<LsGroup>("(","node-group"),
//                new QueryResult<LsValue>("*","svalue"),
//                new QueryResult<LsGroup>(")","node-group"),

//                new QueryResult<LsSymbol>("set"),

//                new QueryResult<LsValue>("entity", "dataName"),
//                new QueryResult<LsGroup>("{","dataGroup"),
//                new QueryResult<LsValue>("abc","svalue"),
//                new QueryResult<LsGroup>("}","dataGroup"),
//                new QueryResult<LsToken>(",","delimiter"),

//                new QueryResult<LsValue>("t1","lvalue"),
//                new QueryResult<LsToken>("=","equal"),
//                new QueryResult<LsValue>("v","rvalue"),
//                new QueryResult<LsToken>(",","delimiter"),
//                new QueryResult<LsValue>("t2","lvalue"),
//                new QueryResult<LsToken>("=","equal"),
//                new QueryResult<LsValue>("v2","rvalue"),

//                new QueryResult<LsToken>(";", "term"),
//            }
//        };

//        LangTestTools.Verify(_root, test);
//    }

//    [Fact]
//    public void SingleTwoDataWithTagValue()
//    {
//        var test = new QueryTest
//        {
//            RawData = "update (*) set entity { abc }, t1=v,t2=v2, contract { ckey=k1, ckey2 };",
//            Results = new List<IQueryResult>()
//            {
//                new QueryResult<LsSymbol>("update"),

//                new QueryResult<LsGroup>("(","node-group"),
//                new QueryResult<LsValue>("*","svalue"),
//                new QueryResult<LsGroup>(")","node-group"),

//                new QueryResult<LsSymbol>("set"),

//                new QueryResult<LsValue>("entity", "dataName"),
//                new QueryResult<LsGroup>("{","dataGroup"),
//                new QueryResult<LsValue>("abc","svalue"),
//                new QueryResult<LsGroup>("}","dataGroup"),
//                new QueryResult<LsToken>(",","delimiter"),

//                new QueryResult<LsValue>("t1","lvalue"),
//                new QueryResult<LsToken>("=","equal"),
//                new QueryResult<LsValue>("v","rvalue"),
//                new QueryResult<LsToken>(",","delimiter"),
//                new QueryResult<LsValue>("t2","lvalue"),
//                new QueryResult<LsToken>("=","equal"),
//                new QueryResult<LsValue>("v2","rvalue"),
//                new QueryResult<LsToken>(",","delimiter"),

//                new QueryResult<LsValue>("contract", "dataName"),
//                new QueryResult<LsGroup>("{","dataGroup"),

//                new QueryResult<LsValue>("ckey","lvalue"),
//                new QueryResult<LsToken>("=", "equal"),
//                new QueryResult<LsValue>("k1", "rvalue"),
//                new QueryResult<LsToken>(",","delimiter"),
//                new QueryResult<LsValue>("ckey2","svalue"),

//                new QueryResult<LsGroup>("}","dataGroup"),


//                new QueryResult<LsToken>(";", "term"),
//            }
//        };

//        LangTestTools.Verify(_root, test);
//    }
//}
