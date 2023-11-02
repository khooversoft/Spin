using FluentAssertions;
using Toolbox.Extensions;
using Toolbox.LangTools;
using Xunit.Abstractions;

namespace Toolbox.Test.Tokenizer;

public class LangSimpleAssignmentTests
{
    private readonly ITestOutputHelper _output;

    public LangSimpleAssignmentTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void InvalidLangSyntax()
    {
        // Format: s = v

        var root = new LsRoot() + new LsValue("lvalue") + new LsToken("=", "equal") + new LsValue("rvalue");

        var lines = new[] { "s=", "=5", "s 5", "this is wrong", "no" };

        foreach (var test in lines)
        {
            LangResult tree = LangParser.Parse(root, test);
            tree.Traces.ForEach(x => _output.WriteLine(x.ToString()));
            tree.Should().NotBeNull();
            tree.IsError().Should().BeTrue();
        }
    }

    [Fact]
    public void LangSyntax()
    {
        // Format: s = v

        var roots = new[]
        {
            new LsRoot() + new LsValue("lvalue") + new LsToken("=", "equal") + new LsValue("rvalue"),
            new LsRoot() + new LsValue("lvalue") + ("=", "equal") + new LsValue("rvalue"),
            new LsRoot()
            {
                new LsValue("lvalue"),
                new LsToken("=", "equal"),
                new LsValue("rvalue"),
            },
        };

        var testFor = new List<IQueryResult>()
        {
            new QueryResult<LsValue>("s","lvalue"),
            new QueryResult<LsToken>("=", "equal"),
            new QueryResult<LsValue>("5", "rvalue"),
        };

        var tests = new QueryTest[]
        {
            new QueryTest { RawData = "s=5", Results = testFor },
            new QueryTest { RawData = "s= 5", Results = testFor },
            new QueryTest { RawData = "s =5", Results = testFor },
            new QueryTest { RawData = "s =    5", Results = testFor },
        };

        foreach (var root in roots)
        {
            foreach (var test in tests)
            {
                LangTestTools.Verify(_output, root, test);
            }
        }
    }
}
