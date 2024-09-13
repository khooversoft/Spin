//using Toolbox.LangTools;

//namespace Toolbox.Graph.test.Lang.Graph;

//public class AddNodeSyntaxTests
//{
//    private readonly ILangRoot _root = GraphLangGrammar.Root;

//    [Fact]
//    public void SingleNode()
//    {
//        var test = new QueryTest
//        {
//            RawData = "add node key=key1, tags=t1;",
//            Results = new List<IQueryResult>()
//            {
//                new QueryResult<LsSymbol>("add"),
//                new QueryResult<LsSymbol>("node"),

//                new QueryResult<LsValue>("key","lvalue"),
//                new QueryResult<LsToken>("=", "equal"),
//                new QueryResult<LsValue>("key1", "rvalue"),
//                new QueryResult<LsToken>(",", "delimiter"),
//                new QueryResult<LsValue>("tags","lvalue"),
//                new QueryResult<LsToken>("=", "equal"),
//                new QueryResult<LsValue>("t1", "rvalue"),
//                new QueryResult<LsToken>(";", "term"),
//            }
//        };

//        LangTestTools.Verify(_root, test);
//    }

//    [Fact]
//    public void MissingOperatorTerm()
//    {
//        var test = new QueryTest
//        {
//            RawData = "node key=key1,tags=t1;",
//            Results = new List<IQueryResult>(),
//        };

//        LangTestTools.Verify(_root, test);
//    }

//    [Fact]
//    public void MissingTerm()
//    {
//        var test = new QueryTest
//        {
//            RawData = "add node key=key1,tags=t1",
//            Results = new List<IQueryResult>(),
//        };

//        LangTestTools.Verify(_root, test);
//    }


//    [Fact]
//    public void MissingNodeTypeTerm()
//    {
//        var test = new QueryTest
//        {
//            RawData = "add key=key1,tags=t1;",
//            Results = new List<IQueryResult>(),
//        };

//        LangTestTools.Verify(_root, test);
//    }

//    [Fact]
//    public void SingleTagValue()
//    {
//        var test = new QueryTest
//        {
//            RawData = "add node t1;",
//            Results = new List<IQueryResult>()
//            {
//                new QueryResult<LsSymbol>("add"),
//                new QueryResult<LsSymbol>("node","node"),
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
//            RawData = "add node t1, t2;",
//            Results = new List<IQueryResult>()
//            {
//                new QueryResult<LsSymbol>("add"),
//                new QueryResult<LsSymbol>("node","node"),
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
//            RawData = "add node t1 = v;",
//            Results = new List<IQueryResult>()
//            {
//                new QueryResult<LsSymbol>("add"),
//                new QueryResult<LsSymbol>("node","node"),
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
//            RawData = "add node t1 = v,t2;",
//            Results = new List<IQueryResult>()
//            {
//                new QueryResult<LsSymbol>("add"),
//                new QueryResult<LsSymbol>("node","node"),
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
//            RawData = "add node t1 = v,t2=v2;",
//            Results = new List<IQueryResult>()
//            {
//                new QueryResult<LsSymbol>("add"),
//                new QueryResult<LsSymbol>("node","node"),
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
//    public void AddNodeWithLink()
//    {
//        var test = new QueryTest
//        {
//            RawData = "add node key=node1, link=a/b/c;",
//            Results = new List<IQueryResult>()
//            {
//                new QueryResult<LsSymbol>("add"),
//                new QueryResult<LsSymbol>("node","node"),
//                new QueryResult<LsValue>("key","lvalue"),
//                new QueryResult<LsToken>("=", "equal"),
//                new QueryResult<LsValue>("node1", "rvalue"),
//                new QueryResult<LsToken>(",","delimiter"),
//                new QueryResult<LsValue>("link","lvalue"),
//                new QueryResult<LsToken>("=","equal"),
//                new QueryResult<LsValue>("a/b/c","rvalue"),
//                new QueryResult<LsToken>(";", "term"),
//            }
//        };

//        LangTestTools.Verify(_root, test);
//    }

//    [Fact]
//    public void AddNodeWithTwoLink()
//    {
//        var test = new QueryTest
//        {
//            RawData = "add node key=node1, link=a/b/c, link=file:nodes/file.json;",
//            Results = new List<IQueryResult>()
//            {
//                new QueryResult<LsSymbol>("add"),
//                new QueryResult<LsSymbol>("node","node"),
//                new QueryResult<LsValue>("key","lvalue"),
//                new QueryResult<LsToken>("=", "equal"),
//                new QueryResult<LsValue>("node1", "rvalue"),
//                new QueryResult<LsToken>(",","delimiter"),
//                new QueryResult<LsValue>("link","lvalue"),
//                new QueryResult<LsToken>("=","equal"),
//                new QueryResult<LsValue>("a/b/c","rvalue"),
//                new QueryResult<LsToken>(",","delimiter"),
//                new QueryResult<LsValue>("link","lvalue"),
//                new QueryResult<LsToken>("=","equal"),
//                new QueryResult<LsValue>("file:nodes/file.json","rvalue"),
//                new QueryResult<LsToken>(";", "term"),
//            }
//        };

//        LangTestTools.Verify(_root, test);
//    }

//    [Fact]
//    public void SingleData()
//    {
//        var test = new QueryTest
//        {
//            RawData = "add node key=node1, entity { abc };",
//            Results = new List<IQueryResult>()
//            {
//                new QueryResult<LsSymbol>("add"),
//                new QueryResult<LsSymbol>("node","node"),
//                new QueryResult<LsValue>("key","lvalue"),
//                new QueryResult<LsToken>("=", "equal"),
//                new QueryResult<LsValue>("node1", "rvalue"),
//                new QueryResult<LsToken>(",","delimiter"),
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
//    public void SingleDataMultipleValues()
//    {
//        var test = new QueryTest
//        {
//            RawData = "add node key=node1, entity { abc, k2=v2 };",
//            Results = new List<IQueryResult>()
//            {
//                new QueryResult<LsSymbol>("add"),
//                new QueryResult<LsSymbol>("node","node"),
//                new QueryResult<LsValue>("key","lvalue"),
//                new QueryResult<LsToken>("=", "equal"),
//                new QueryResult<LsValue>("node1", "rvalue"),
//                new QueryResult<LsToken>(",","delimiter"),

//                new QueryResult<LsValue>("entity", "dataName"),
//                new QueryResult<LsGroup>("{","dataGroup"),

//                new QueryResult<LsValue>("abc","svalue"),
//                new QueryResult<LsToken>(",","delimiter"),

//                new QueryResult<LsValue>("k2","lvalue"),
//                new QueryResult<LsToken>("=", "equal"),
//                new QueryResult<LsValue>("v2", "rvalue"),

//                new QueryResult<LsGroup>("}","dataGroup"),
//                new QueryResult<LsToken>(";", "term"),
//            }
//        };

//        LangTestTools.Verify(_root, test);
//    }


//    [Fact]
//    public void TwoData()
//    {
//        var test = new QueryTest
//        {
//            RawData = "add node key=node1, entity { abc }, contract { ckey=k1, ckey2 };",
//            Results = new List<IQueryResult>()
//            {
//                new QueryResult<LsSymbol>("add"),
//                new QueryResult<LsSymbol>("node","node"),
//                new QueryResult<LsValue>("key","lvalue"),
//                new QueryResult<LsToken>("=", "equal"),
//                new QueryResult<LsValue>("node1", "rvalue"),
//                new QueryResult<LsToken>(",","delimiter"),

//                new QueryResult<LsValue>("entity", "dataName"),
//                new QueryResult<LsGroup>("{","dataGroup"),
//                new QueryResult<LsValue>("abc","svalue"),
//                new QueryResult<LsGroup>("}","dataGroup"),
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

//    [Fact]
//    public void FullAddNode()
//    {
//        var test = new QueryTest
//        {
//            RawData = "add node key=node1, t1 = v,t2, entity { abc }, link=a/b/c;",
//            Results = new List<IQueryResult>()
//            {
//                new QueryResult<LsSymbol>("add"),
//                new QueryResult<LsSymbol>("node","node"),
//                new QueryResult<LsValue>("key","lvalue"),
//                new QueryResult<LsToken>("=", "equal"),
//                new QueryResult<LsValue>("node1", "rvalue"),
//                new QueryResult<LsToken>(",","delimiter"),
//                new QueryResult<LsValue>("t1","lvalue"),
//                new QueryResult<LsToken>("=","equal"),
//                new QueryResult<LsValue>("v","rvalue"),
//                new QueryResult<LsToken>(",","delimiter"),
//                new QueryResult<LsValue>("t2","svalue"),
//                new QueryResult<LsToken>(",","delimiter"),
//                new QueryResult<LsValue>("entity", "dataName"),
//                new QueryResult<LsGroup>("{","dataGroup"),
//                new QueryResult<LsValue>("abc","svalue"),
//                new QueryResult<LsGroup>("}","dataGroup"),
//                new QueryResult<LsToken>(",","delimiter"),
//                new QueryResult<LsValue>("link","lvalue"),
//                new QueryResult<LsToken>("=","equal"),
//                new QueryResult<LsValue>("a/b/c","rvalue"),
//                new QueryResult<LsToken>(";", "term"),
//            }
//        };

//        LangTestTools.Verify(_root, test);
//    }
//}
