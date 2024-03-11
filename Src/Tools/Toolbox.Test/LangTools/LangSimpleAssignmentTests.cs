using FluentAssertions;
using Toolbox.LangTools;

namespace Toolbox.Test.LangTools;

public class LangSimpleAssignmentTests
{
    [Fact]
    public void InvalidLangSyntax()
    {
        // Format: s = v

        var root = new LsRoot() + new LsValue("lvalue") + new LsToken("=", "equal") + new LsValue("rvalue");

        var lines = new[] { "s=", "=5", "s 5", "this is wrong", "no" };

        foreach (var test in lines)
        {
            LangResult tree = root.Parse(test);
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
    }
}
