using Toolbox.LangTools;

namespace Toolbox.Test.LangTools;

/// <summary>
/// fromKey=key1,toKey=key2,edgeType=et,tags=t2;
/// </summary>
public class PatternPropertyTests
{
    private readonly ILangRoot _root;

    public PatternPropertyTests()
    {
        var equalValue = new LsRoot("equalValue") + new LsValue("lvalue") + ("=", "equal") + new LsValue("rvalue");

        var property = new LsRoot("property") + new LsValue("lvalue") + ("=", "equal") + new LsValue("rvalue");
        var properties = new LsRepeat("properties-rpt") + equalValue + new LsToken(",", "delimiter", true);

        _root = new LsRoot("setValues") + properties + new LsToken(";", "term", true);
    }

    [Fact]
    public void Fail()
    {
        var test = new QueryTest
        {
            RawData = "fromKey=",
            Results = new List<IQueryResult>(),
        };

        LangTestTools.Verify(_root, test);
    }

    [Fact]
    public void SingleProperty()
    {
        var test = new QueryTest
        {
            RawData = "fromKey=key1",
            Results = new List<IQueryResult>()
            {
                new QueryResult<LsValue>("fromKey","lvalue"),
                new QueryResult<LsToken>("=", "equal"),
                new QueryResult<LsValue>("key1", "rvalue"),
            }
        };

        LangTestTools.Verify(_root, test);
    }

    [Fact]
    public void AllProperty()
    {
        var test = new QueryTest
        {
            RawData = "fromKey=key1,toKey=key2,edgeType=et,tags=t2;",
            Results = new List<IQueryResult>()
            {
                new QueryResult<LsValue>("fromKey","lvalue"),
                new QueryResult<LsToken>("=", "equal"),
                new QueryResult<LsValue>("key1", "rvalue"),
                new QueryResult<LsToken>(",", "delimiter"),

                new QueryResult<LsValue>("toKey","lvalue"),
                new QueryResult<LsToken>("=", "equal"),
                new QueryResult<LsValue>("key2", "rvalue"),
                new QueryResult<LsToken>(",", "delimiter"),

                new QueryResult<LsValue>("edgeType","lvalue"),
                new QueryResult<LsToken>("=", "equal"),
                new QueryResult<LsValue>("et", "rvalue"),
                new QueryResult<LsToken>(",", "delimiter"),

                new QueryResult<LsValue>("tags","lvalue"),
                new QueryResult<LsToken>("=", "equal"),
                new QueryResult<LsValue>("t2", "rvalue"),
                new QueryResult<LsToken>(";", "term"),

            }
        };

        LangTestTools.Verify(_root, test);
    }
}
