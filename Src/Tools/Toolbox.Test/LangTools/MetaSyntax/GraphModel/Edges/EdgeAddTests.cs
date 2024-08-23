using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.LangTools;
using Toolbox.Logging;
using Toolbox.Test.Application;
using Toolbox.Test.LangTools.MetaSyntax.GraphModel.Nodes;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Test.LangTools.MetaSyntax.GraphModel.Edges;

public class EdgeAddTests : TestBase<EdgeAddTests>
{
    private readonly ITestOutputHelper _output;
    private readonly MetaSyntaxRoot _root;
    private readonly SyntaxParser _parser;
    private readonly ScopeContext _context;

    public EdgeAddTests(ITestOutputHelper output) : base(output)
    {
        _output = output.NotNull();

        string schema = GraphModelTool.ReadGraphLanauge2();
        _root = MetaParser.ParseRules(schema);
        _root.StatusCode.IsOk().Should().BeTrue(_root.Error);
        _context = GetScopeContext();

        _parser = new SyntaxParser(_root);
    }

    [Theory]
    [InlineData("add")]
    [InlineData("add edge")]
    [InlineData("add node key")]
    [InlineData("add node from=")]
    [InlineData("add node from=k1")]
    [InlineData("add node from=k1, to=k2")]
    [InlineData("add node from=k1, to=k2, ")]
    [InlineData("add node from=k1, to=k2, edge=e1, set data { base64 } ;")]
    [InlineData("add edge from=fkey1, to=tkey1, type=label set set t1;")]
    [InlineData("()")]
    [InlineData("();")]
    [InlineData("data { hexdata };")]
    public void FailedReturn(string command)
    {
        var parse = _parser.Parse(command, _context);
        parse.StatusCode.IsError().Should().BeTrue(parse.Error);
    }

    [Fact]
    public void MinAddCommand()
    {
        var parse = _parser.Parse("add edge from=fkey1, to=tkey1, type=label;", _context);
        parse.StatusCode.IsOk().Should().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var syntaxLines = syntaxPairs.GenerateSyntaxPairs().Join(Environment.NewLine);

        var expectedPairs = new[]
        {
            new SyntaxPair { Token = new TokenValue("add"), MetaSyntaxName = "add-sym" },
            new SyntaxPair { Token = new TokenValue("edge"), MetaSyntaxName = "edge-sym" },
            new SyntaxPair { Token = new TokenValue("from"), MetaSyntaxName = "fromKey-sym" },
            new SyntaxPair { Token = new TokenValue("="), MetaSyntaxName = "equal" },
            new SyntaxPair { Token = new TokenValue("fkey1"), MetaSyntaxName = "fromKey-value" },
            new SyntaxPair { Token = new TokenValue(","), MetaSyntaxName = "comma" },
            new SyntaxPair { Token = new TokenValue("to"), MetaSyntaxName = "toKey-sym" },
            new SyntaxPair { Token = new TokenValue("="), MetaSyntaxName = "equal" },
            new SyntaxPair { Token = new TokenValue("tkey1"), MetaSyntaxName = "toKey-value" },
            new SyntaxPair { Token = new TokenValue(","), MetaSyntaxName = "comma" },
            new SyntaxPair { Token = new TokenValue("type"), MetaSyntaxName = "edgeType-sym" },
            new SyntaxPair { Token = new TokenValue("="), MetaSyntaxName = "equal" },
            new SyntaxPair { Token = new TokenValue("label"), MetaSyntaxName = "edgeType-value" },
            new SyntaxPair { Token = new TokenValue(";"), MetaSyntaxName = "term" },
        };

        syntaxPairs.SequenceEqual(expectedPairs).Should().BeTrue();
    }

    [Fact]
    public void AddWithSetTagCommand()
    {
        var parse = _parser.Parse("add edge from=fkey1, to=tkey1, type=label set t1;", _context);
        parse.StatusCode.IsOk().Should().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var syntaxLines = syntaxPairs.GenerateSyntaxPairs().Join(Environment.NewLine);

        var expectedPairs = new[]
        {
            new SyntaxPair { Token = new TokenValue("add"), MetaSyntaxName = "add-sym" },
            new SyntaxPair { Token = new TokenValue("edge"), MetaSyntaxName = "edge-sym" },
            new SyntaxPair { Token = new TokenValue("from"), MetaSyntaxName = "fromKey-sym" },
            new SyntaxPair { Token = new TokenValue("="), MetaSyntaxName = "equal" },
            new SyntaxPair { Token = new TokenValue("fkey1"), MetaSyntaxName = "fromKey-value" },
            new SyntaxPair { Token = new TokenValue(","), MetaSyntaxName = "comma" },
            new SyntaxPair { Token = new TokenValue("to"), MetaSyntaxName = "toKey-sym" },
            new SyntaxPair { Token = new TokenValue("="), MetaSyntaxName = "equal" },
            new SyntaxPair { Token = new TokenValue("tkey1"), MetaSyntaxName = "toKey-value" },
            new SyntaxPair { Token = new TokenValue(","), MetaSyntaxName = "comma" },
            new SyntaxPair { Token = new TokenValue("type"), MetaSyntaxName = "edgeType-sym" },
            new SyntaxPair { Token = new TokenValue("="), MetaSyntaxName = "equal" },
            new SyntaxPair { Token = new TokenValue("label"), MetaSyntaxName = "edgeType-value" },
            new SyntaxPair { Token = new TokenValue("set"), MetaSyntaxName = "set-sym" },
            new SyntaxPair { Token = new TokenValue("t1"), MetaSyntaxName = "tagKey" },
            new SyntaxPair { Token = new TokenValue(";"), MetaSyntaxName = "term" },
        };

        syntaxPairs.SequenceEqual(expectedPairs).Should().BeTrue();
    }

    [Fact]
    public void AddWithSetMultipleTagCommand()
    {
        var parse = _parser.Parse("add edge from=fkey1, to=tkey1, type=label set t1, t2=v3, t3;", _context);
        parse.StatusCode.IsOk().Should().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var syntaxLines = syntaxPairs.GenerateSyntaxPairs().Join(Environment.NewLine);

        var expectedPairs = new[]
        {
            new SyntaxPair { Token = new TokenValue("add"), MetaSyntaxName = "add-sym" },
            new SyntaxPair { Token = new TokenValue("edge"), MetaSyntaxName = "edge-sym" },
            new SyntaxPair { Token = new TokenValue("from"), MetaSyntaxName = "fromKey-sym" },
            new SyntaxPair { Token = new TokenValue("="), MetaSyntaxName = "equal" },
            new SyntaxPair { Token = new TokenValue("fkey1"), MetaSyntaxName = "fromKey-value" },
            new SyntaxPair { Token = new TokenValue(","), MetaSyntaxName = "comma" },
            new SyntaxPair { Token = new TokenValue("to"), MetaSyntaxName = "toKey-sym" },
            new SyntaxPair { Token = new TokenValue("="), MetaSyntaxName = "equal" },
            new SyntaxPair { Token = new TokenValue("tkey1"), MetaSyntaxName = "toKey-value" },
            new SyntaxPair { Token = new TokenValue(","), MetaSyntaxName = "comma" },
            new SyntaxPair { Token = new TokenValue("type"), MetaSyntaxName = "edgeType-sym" },
            new SyntaxPair { Token = new TokenValue("="), MetaSyntaxName = "equal" },
            new SyntaxPair { Token = new TokenValue("label"), MetaSyntaxName = "edgeType-value" },
            new SyntaxPair { Token = new TokenValue("set"), MetaSyntaxName = "set-sym" },
            new SyntaxPair { Token = new TokenValue("t1"), MetaSyntaxName = "tagKey" },
            new SyntaxPair { Token = new TokenValue(","), MetaSyntaxName = "comma" },
            new SyntaxPair { Token = new TokenValue("t2"), MetaSyntaxName = "tagKey" },
            new SyntaxPair { Token = new TokenValue("="), MetaSyntaxName = "_tag-3-OptionGroup-1" },
            new SyntaxPair { Token = new TokenValue("v3"), MetaSyntaxName = "tagValue" },
            new SyntaxPair { Token = new TokenValue(","), MetaSyntaxName = "comma" },
            new SyntaxPair { Token = new TokenValue("t3"), MetaSyntaxName = "tagKey" },
            new SyntaxPair { Token = new TokenValue(";"), MetaSyntaxName = "term" },
        };

        syntaxPairs.SequenceEqual(expectedPairs).Should().BeTrue();
    }
}
