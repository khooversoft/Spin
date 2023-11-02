using FluentAssertions;
using Toolbox.Extensions;
using Toolbox.LangTools;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Test.Tokenizer;

public class LangGraphTests
{
    private readonly ITestOutputHelper _output;
    private readonly ILangRoot _root;

    public LangGraphTests(ITestOutputHelper output)
    {
        _output = output;

        var equalValue = new LsRoot("k=v") + new LsValue("lvalue") + ("=", "equal") + new LsValue("rvalue");
        var valueOnly = new LsRoot("v") + new LsValue("svalue");
        var parameters = new LsRepeat("repeat") + (new LsOr("or") + equalValue + valueOnly) + new LsToken(";", "delimiter", true);

        var nodeSyntax = new LsRoot("root")
            + (new LsGroup("(", ")", "group") + parameters)
            + new LsValue("alias", true);

        var edgeSyntax = new LsRoot("edge")
            + (new LsGroup("[", "]", "group") + parameters)
            + new LsValue("alias", true);

        _root = new LsRoot() + (new LsOption("root") + nodeSyntax + edgeSyntax);
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

        LangTestTools.Verify(_output, _root, test);
    }

    [Fact]
    public void SingleAssignmentWithAlias()
    {
        var test = new QueryTest
        {
            RawData = "(key='string value') a1",
            Results = new List<IQueryResult>()
            {
                new QueryResult<LsGroup>("(","group"),
                new QueryResult<LsValue>("key","lvalue"),
                new QueryResult<LsToken>("=", "equal"),
                new QueryResult<LsValue>("string value", "rvalue"),
                new QueryResult<LsGroup>(")","group"),
                new QueryResult<LsValue>("a1","alias"),
            }
        };

        LangTestTools.Verify(_output, _root, test);
    }
}
