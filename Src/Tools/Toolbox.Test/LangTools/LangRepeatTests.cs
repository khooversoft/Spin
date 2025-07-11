//using Toolbox.LangTools;

//namespace Toolbox.Test.LangTools;

//public class LangRepeatTests
//{
//    private readonly ILangRoot _root;

//    public LangRepeatTests()
//    {
//        _root = new LsRoot()
//            + (new LsRepeat("valueEqual") + new LsValue("lvalue") + ("=", "equal") + new LsValue("rvalue") + new LsToken(";", true));
//    }

//    [Fact]
//    public void FailureTests()
//    {
//        var tests = new QueryTest[]
//        {
//            new QueryTest { RawData = "key=;name=v1;second=v3", Results = new List<IQueryResult>() },
//            new QueryTest { RawData = "key='string value';name=;second=v3", Results = new List<IQueryResult>() },
//            new QueryTest { RawData = "key='string value' name=v1;second=v3", Results = new List<IQueryResult>() },
//            new QueryTest { RawData = "key='string value';name=v1;=v3", Results = new List<IQueryResult>() },
//        };

//        foreach (var test in tests)
//        {
//            LangTestTools.Verify(_root, test);
//        }
//    }

//    [Fact]
//    public void SingleAssignment()
//    {
//        var test = new QueryTest
//        {
//            RawData = "key='string value'",
//            Results = new List<IQueryResult>()
//            {
//                new QueryResult<LsValue>("key","lvalue"),
//                new QueryResult<LsToken>("=", "equal"),
//                new QueryResult<LsValue>("string value", "rvalue"),
//            }
//        };

//        LangTestTools.Verify(_root, test);
//    }


//    [Fact]
//    public void SingleAssignmentWithDelimiter()
//    {
//        var test = new QueryTest
//        {
//            RawData = "key='string value';",
//            Results = new List<IQueryResult>()
//            {
//                new QueryResult<LsValue>("key","lvalue"),
//                new QueryResult<LsToken>("=", "equal"),
//                new QueryResult<LsValue>("string value", "rvalue"),
//                new QueryResult<LsToken>(";"),
//            }
//        };

//        LangTestTools.Verify(_root, test);
//    }

//    [Fact]
//    public void TwoAssignment()
//    {
//        var test = new QueryTest
//        {
//            RawData = "key='string value';name=v1;",
//            Results = new List<IQueryResult>()
//            {
//                new QueryResult<LsValue>("key","lvalue"),
//                new QueryResult<LsToken>("=", "equal"),
//                new QueryResult<LsValue>("string value", "rvalue"),
//                new QueryResult<LsToken>(";"),
//                new QueryResult<LsValue>("name","lvalue"),
//                new QueryResult<LsToken>("=", "equal"),
//                new QueryResult<LsValue>("v1", "rvalue"),
//                new QueryResult<LsToken>(";"),
//            }
//        };

//        LangTestTools.Verify(_root, test);
//    }

//    [Fact]
//    public void ThreeAssginments()
//    {
//        var test = new QueryTest
//        {
//            RawData = "key='string value';name=v1;second=v3",
//            Results = new List<IQueryResult>()
//            {
//                new QueryResult<LsValue>("key","lvalue"),
//                new QueryResult<LsToken>("=", "equal"),
//                new QueryResult<LsValue>("string value", "rvalue"),
//                new QueryResult<LsToken>(";"),
//                new QueryResult<LsValue>("name","lvalue"),
//                new QueryResult<LsToken>("=", "equal"),
//                new QueryResult<LsValue>("v1", "rvalue"),
//                new QueryResult<LsToken>(";"),
//                new QueryResult<LsValue>("second","lvalue"),
//                new QueryResult<LsToken>("=", "equal"),
//                new QueryResult<LsValue>("v3", "rvalue"),
//            }
//        };

//        LangTestTools.Verify(_root, test);
//    }
//}
