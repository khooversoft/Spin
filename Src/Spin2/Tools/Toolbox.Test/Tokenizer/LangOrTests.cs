using Toolbox.LangTools;
using Xunit.Abstractions;

namespace Toolbox.Test.Tokenizer;

public class LangOrTests
{
    private readonly ITestOutputHelper _output;
    private readonly ILangRoot _root;

    public LangOrTests(ITestOutputHelper output)
    {
        _output = output;

        var equalValue = new LsRoot("equal") + new LsValue("lvalue") + ("=", "equal") + new LsValue("rvalue");
        var valueOnly = new LsRoot("single") + new LsValue("svalue");

        _root = new LsRoot() + (new LsOr("or") + equalValue + valueOnly);
    }

    [Fact]
    public void Failure()
    {
        var test = new QueryTest { RawData = "=", Results = new List<IQueryResult>() };
        LangTestTools.Verify(_output, _root, test);
    }

    [Fact]
    public void FirstOrPattern()
    {
        var test = new QueryTest
        {
            RawData = "key='string value'",
            Results = new List<IQueryResult>()
            {
                new QueryResult<LsValue>("key","lvalue"),
                new QueryResult<LsToken>("=", "equal"),
                new QueryResult<LsValue>("string value", "rvalue"),
            }
        };

        LangTestTools.Verify(_output, _root, test);
    }

    [Fact]
    public void SecondOrPattern()
    {
        var test = new QueryTest
        {
            RawData = "key",
            Results = new List<IQueryResult>()
            {
                new QueryResult<LsValue>("key","svalue"),
            }
        };

        LangTestTools.Verify(_output, _root, test);
    }
}
