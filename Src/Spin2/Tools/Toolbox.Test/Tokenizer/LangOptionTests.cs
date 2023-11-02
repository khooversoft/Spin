using Toolbox.LangTools;
using Xunit.Abstractions;

namespace Toolbox.Test.Tokenizer;

public class LangOptionTests
{
    private readonly ITestOutputHelper _output;
    private readonly ILangRoot _root;

    public LangOptionTests(ITestOutputHelper output)
    {
        _output = output;

        var equalValue = new LsRoot("equal") + new LsValue("lvalue") + ("=", "equal") + new LsValue("rvalue");
        var addValue = new LsRoot("plus") + new LsValue("lvalue") + ("+", "plusSign") + new LsValue("rvalue");

        _root = new LsRoot() + (new LsOption("optional") + equalValue + addValue);
    }

    [Fact]
    public void Failures()
    {
        var tests = new QueryTest[]
        {
            new QueryTest { RawData = "key", Results = new List<IQueryResult>() },
            new QueryTest { RawData = "=", Results = new List<IQueryResult>() },
        };

        foreach (var test in tests)
        {
            LangTestTools.Verify(_output, _root, test);
        }
    }

    [Fact]
    public void SingleAssignment()
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
    public void SingleMath()
    {
        var test = new QueryTest
        {
            RawData = "value+5",
            Results = new List<IQueryResult>()
            {
                new QueryResult<LsValue>("value","lvalue"),
                new QueryResult<LsToken>("+", "plusSign"),
                new QueryResult<LsValue>("5", "rvalue"),
            }
        };

        LangTestTools.Verify(_output, _root, test);
    }
}
