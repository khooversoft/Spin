using FluentAssertions;
using Toolbox.Extensions;
using Toolbox.LangTools;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Test.Tokenizer;

/// <summary>
/// (key=key1;tags=t1) n1 -> [schedulework:active] -> (schedule) n2
/// (key=key1;tags=t1) n1 -> [edgeType=abc*;schedulework:active] -> (schedule) n2
/// (key= key1; t1) n1 -> [schedulework:*] -> (schedule) n2
/// [fromKey = key1; edgeType=abc*] -> (schedule) n1
/// (t1) -> [tags=schedulework:active] -> (tags="state=active") n1
/// </summary>
public class LangGraphTests
{
    private readonly ITestOutputHelper _output;
    private readonly ILangRoot _root;

    public LangGraphTests(ITestOutputHelper output)
    {
        _output = output;

        var equalValue = new LsRoot("equalValue") + new LsValue("lvalue") + ("=", "equal") + new LsValue("rvalue");
        var valueOnly = new LsRoot("valueOnly") + new LsValue("svalue");
        var parameters = new LsRepeat("rpt-parms") + (new LsOr("or") + equalValue + valueOnly) + new LsToken(";", "delimiter", true);

        var nodeSyntax = new LsRoot("node")
            + (new LsGroup("(", ")", "pgroup") + parameters)
            + new LsValue("alias", true);

        var edgeSyntax = new LsRoot("edge")
            + (new LsGroup("[", "]", "bgroup") + parameters)
            + new LsValue("alias", true);

        _root = new LsRoot() + (new LsRepeat("repeat-root") + (new LsOr("instr-or") + nodeSyntax + edgeSyntax) + new LsToken("->", "next", true));
    }

    [Fact]
    public void SingleNode()
    {
        var test = new QueryTest
        {
            RawData = "(key='string value')",
            Results = new List<IQueryResult>()
            {
                new QueryResult<LsGroup>("(","pgroup"),
                new QueryResult<LsValue>("key","lvalue"),
                new QueryResult<LsToken>("=", "equal"),
                new QueryResult<LsValue>("string value", "rvalue"),
                new QueryResult<LsGroup>(")","pgroup"),
            }
        };

        LangTestTools.Verify(_output, _root, test);
    }

    [Fact]
    public void SingleNodeWithAlias()
    {
        var test = new QueryTest
        {
            RawData = "(key='string value') a1",
            Results = new List<IQueryResult>()
            {
                new QueryResult<LsGroup>("(","pgroup"),
                new QueryResult<LsValue>("key","lvalue"),
                new QueryResult<LsToken>("=", "equal"),
                new QueryResult<LsValue>("string value", "rvalue"),
                new QueryResult<LsGroup>(")","pgroup"),
                new QueryResult<LsValue>("a1","alias"),
            }
        };

        LangTestTools.Verify(_output, _root, test);
    }

    [Fact]
    public void SingleEdge()
    {
        var test = new QueryTest
        {
            RawData = "[key='string value']",
            Results = new List<IQueryResult>()
            {
                new QueryResult<LsGroup>("[","bgroup"),
                new QueryResult<LsValue>("key","lvalue"),
                new QueryResult<LsToken>("=", "equal"),
                new QueryResult<LsValue>("string value", "rvalue"),
                new QueryResult<LsGroup>("]","bgroup"),
            }
        };

        LangTestTools.Verify(_output, _root, test);
    }

    [Fact]
    public void SingleEdgeWithAlias()
    {
        var test = new QueryTest
        {
            RawData = "[key='string value'] a1",
            Results = new List<IQueryResult>()
            {
                new QueryResult<LsGroup>("[","bgroup"),
                new QueryResult<LsValue>("key","lvalue"),
                new QueryResult<LsToken>("=", "equal"),
                new QueryResult<LsValue>("string value", "rvalue"),
                new QueryResult<LsGroup>("]","bgroup"),
                new QueryResult<LsValue>("a1","alias"),
            }
        };

        LangTestTools.Verify(_output, _root, test);
    }

    [Fact]
    public void NodeAndEdge()
    {
        var test = new QueryTest
        {
            RawData = "(key=key1;tags=t1) -> [schedulework:active]",
            Results = new List<IQueryResult>()
            {
                new QueryResult<LsGroup>("(","pgroup"),
                new QueryResult<LsValue>("key","lvalue"),
                new QueryResult<LsToken>("=", "equal"),
                new QueryResult<LsValue>("key1", "rvalue"),
                new QueryResult<LsToken>(";", "delimiter"),
                new QueryResult<LsValue>("tags","lvalue"),
                new QueryResult<LsToken>("=", "equal"),
                new QueryResult<LsValue>("t1", "rvalue"),
                new QueryResult<LsGroup>(")","pgroup"),

                new QueryResult<LsToken>("->", "next"),

                new QueryResult<LsGroup>("[","bgroup"),
                new QueryResult<LsValue>("schedulework:active","svalue"),
                new QueryResult<LsGroup>("]","bgroup"),
            }
        };

        LangTestTools.Verify(_output, _root, test);
    }

    [Fact]
    public void NodeAndEdgeAndNode()
    {
        var test = new QueryTest
        {
            RawData = "(key=key1;tags=t1) -> [schedulework:active]->(schedule) n2",
            Results = new List<IQueryResult>()
            {
                new QueryResult<LsGroup>("(","pgroup"),
                new QueryResult<LsValue>("key","lvalue"),
                new QueryResult<LsToken>("=", "equal"),
                new QueryResult<LsValue>("key1", "rvalue"),
                new QueryResult<LsToken>(";", "delimiter"),
                new QueryResult<LsValue>("tags","lvalue"),
                new QueryResult<LsToken>("=", "equal"),
                new QueryResult<LsValue>("t1", "rvalue"),
                new QueryResult<LsGroup>(")","pgroup"),

                new QueryResult<LsToken>("->", "next"),

                new QueryResult<LsGroup>("[","bgroup"),
                new QueryResult<LsValue>("schedulework:active","svalue"),
                new QueryResult<LsGroup>("]","bgroup"),

                new QueryResult<LsToken>("->", "next"),
                new QueryResult<LsGroup>("(","pgroup"),
                new QueryResult<LsValue>("schedule","svalue"),
                new QueryResult<LsGroup>(")","pgroup"),
                new QueryResult<LsValue>("n2","alias"),
            }
        };

        LangTestTools.Verify(_output, _root, test);
    }

    [Fact]
    public void Example1()
    {
        var test = new QueryTest
        {
            RawData = "(key=key1;tags=t1) n1 -> [edgeType=abc*;schedulework:active] -> (schedule) n2",
            Results = new List<IQueryResult>()
            {
                new QueryResult<LsGroup>("(","pgroup"),
                new QueryResult<LsValue>("key","lvalue"),
                new QueryResult<LsToken>("=", "equal"),
                new QueryResult<LsValue>("key1", "rvalue"),
                new QueryResult<LsToken>(";", "delimiter"),
                new QueryResult<LsValue>("tags","lvalue"),
                new QueryResult<LsToken>("=", "equal"),
                new QueryResult<LsValue>("t1", "rvalue"),
                new QueryResult<LsGroup>(")","pgroup"),
                new QueryResult<LsValue>("n1","alias"),

                new QueryResult<LsToken>("->", "next"),

                new QueryResult<LsGroup>("[","bgroup"),
                new QueryResult<LsValue>("edgeType","lvalue"),
                new QueryResult<LsToken>("=", "equal"),
                new QueryResult<LsValue>("abc*", "rvalue"),
                new QueryResult<LsToken>(";", "delimiter"),
                new QueryResult<LsValue>("schedulework:active","svalue"),
                new QueryResult<LsGroup>("]","bgroup"),

                new QueryResult<LsToken>("->", "next"),
                new QueryResult<LsGroup>("(","pgroup"),
                new QueryResult<LsValue>("schedule","svalue"),
                new QueryResult<LsGroup>(")","pgroup"),
                new QueryResult<LsValue>("n2","alias"),
            }
        };

        LangTestTools.Verify(_output, _root, test);
    }

    [Fact]
    public void Example2()
    {
        var test = new QueryTest
        {
            RawData = "(t1) n1 -> [tags=schedulework:active]n3 -> (t2) n2",
            Results = new List<IQueryResult>()
            {
                new QueryResult<LsGroup>("(","pgroup"),
                new QueryResult<LsValue>("t1","svalue"),
                new QueryResult<LsGroup>(")","pgroup"),
                new QueryResult<LsValue>("n1","alias"),

                new QueryResult<LsToken>("->", "next"),

                new QueryResult<LsGroup>("[","bgroup"),
                new QueryResult<LsValue>("tags","lvalue"),
                new QueryResult<LsToken>("=", "equal"),
                new QueryResult<LsValue>("schedulework:active", "rvalue"),
                new QueryResult<LsGroup>("]","bgroup"),
                new QueryResult<LsValue>("n3","alias"),

                new QueryResult<LsToken>("->", "next"),

                new QueryResult<LsGroup>("(","pgroup"),
                new QueryResult<LsValue>("t2","svalue"),
                new QueryResult<LsGroup>(")","pgroup"),
                new QueryResult<LsValue>("n2","alias"),
            }
        };

        LangTestTools.Verify(_output, _root, test);
    }
}
