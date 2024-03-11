using Toolbox.LangTools;

namespace Toolbox.Test.LangTools;

public class LangGroupingTests
{
    private readonly ILangRoot _root;

    public LangGroupingTests()
    {
        var equalValue = new LsRoot("k=v") + new LsValue("lvalue") + ("=", "equal") + new LsValue("rvalue");
        var valueOnly = new LsRoot("v") + new LsValue("svalue");
        var repeat = new LsRepeat("repeat") + (new LsSwitch("or") + equalValue + valueOnly) + new LsToken(";", "delimiter", true);

        _root = new LsRoot("root") + (new LsGroup("(", ")", "group") + repeat);
    }

    [Fact]
    public void Failure()
    {
        var tests = new QueryTest[]
        {
            new QueryTest { RawData = "(key=)", Results = new List<IQueryResult>() },
            new QueryTest { RawData = "(key=v1;=)", Results = new List<IQueryResult>() },
        };

        foreach (var test in tests)
        {
            LangTestTools.Verify(_root, test);
        }
    }

    [Fact]
    public void SingleAssignment()
    {
        var test = new QueryTest
        {
            RawData = "(key='string value')",
            Results = new List<IQueryResult>()
            {
                new QueryResult<LsGroup>("(","group"),
                new QueryResult<LsValue>("key","lvalue"),
                new QueryResult<LsToken>("=", "equal"),
                new QueryResult<LsValue>("string value", "rvalue"),
                new QueryResult<LsGroup>(")","group"),
            }
        };

        LangTestTools.Verify(_root, test);
    }

    [Fact]
    public void SingleAssignmentWithDelimiter()
    {
        var test = new QueryTest
        {
            RawData = "(key='string value';)",
            Results = new List<IQueryResult>()
            {
                new QueryResult<LsGroup>("(","group"),
                new QueryResult<LsValue>("key","lvalue"),
                new QueryResult<LsToken>("=", "equal"),
                new QueryResult<LsValue>("string value", "rvalue"),
                new QueryResult<LsToken>(";", "delimiter"),
                new QueryResult<LsGroup>(")","group"),
            }
        };

        LangTestTools.Verify(_root, test);
    }

    [Fact]
    public void TwoAssignments()
    {
        var test = new QueryTest
        {
            RawData = "(key='string value';tags=t1)",
            Results = new List<IQueryResult>()
            {
                new QueryResult<LsGroup>("(","group"),
                new QueryResult<LsValue>("key","lvalue"),
                new QueryResult<LsToken>("=", "equal"),
                new QueryResult<LsValue>("string value", "rvalue"),
                new QueryResult<LsToken>(";", "delimiter"),
                new QueryResult<LsValue>("tags","lvalue"),
                new QueryResult<LsToken>("=", "equal"),
                new QueryResult<LsValue>("t1", "rvalue"),
                new QueryResult<LsGroup>(")","group"),
            }
        };

        LangTestTools.Verify(_root, test);
    }
}
