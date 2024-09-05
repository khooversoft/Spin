using FluentAssertions;
using Toolbox.Extensions;
using Toolbox.LangTools;
using Toolbox.Test.Application;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Test.LangTools.MetaSyntax.GraphModel.Nodes;

public class NodeAddTests : TestBase<NodeAddTests>
{
    private readonly ITestOutputHelper _output;
    private readonly MetaSyntaxRoot _root;
    private readonly SyntaxParser _parser;
    private readonly ScopeContext _context;

    public NodeAddTests(ITestOutputHelper output) : base(output)
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
    [InlineData("add node")]
    [InlineData("add node key")]
    [InlineData("add node key=")]
    [InlineData("()")]
    [InlineData("();")]
    [InlineData("data { hexdata };")]
    public void FailedReturn(string command)
    {
        var parse = _parser.Parse(command, _context);
        parse.Status.IsError().Should().BeTrue(parse.Status.Error);
    }

    [Fact]
    public void MinAddCommand()
    {
        var parse = _parser.Parse("add node key=k1;", _context);
        parse.Status.IsOk().Should().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var syntaxLines = syntaxPairs.GenerateSyntaxPairs().Join(Environment.NewLine);

        var expectedPairs = new[]
        {
            new SyntaxPair { Token = new TokenValue("add"), MetaSyntaxName = "add-sym" },
            new SyntaxPair { Token = new TokenValue("node"), MetaSyntaxName = "node-sym" },
            new SyntaxPair { Token = new TokenValue("key"), MetaSyntaxName = "key-sym" },
            new SyntaxPair { Token = new TokenValue("="), MetaSyntaxName = "equal" },
            new SyntaxPair { Token = new TokenValue("k1"), MetaSyntaxName = "key-value" },
            new SyntaxPair { Token = new TokenValue(";"), MetaSyntaxName = "term" },
        };

        syntaxPairs.SequenceEqual(expectedPairs).Should().BeTrue();
    }

    [Fact]
    public void AddCommandWithTag()
    {
        var parse = _parser.Parse("add node key=k1 set t1;", _context);
        parse.Status.IsOk().Should().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var syntaxLines = syntaxPairs.GenerateSyntaxPairs().Join(Environment.NewLine);

        var expectedPairs = new[]
        {
            new SyntaxPair { Token = new TokenValue("add"), MetaSyntaxName = "add-sym" },
            new SyntaxPair { Token = new TokenValue("node"), MetaSyntaxName = "node-sym" },
            new SyntaxPair { Token = new TokenValue("key"), MetaSyntaxName = "key-sym" },
            new SyntaxPair { Token = new TokenValue("="), MetaSyntaxName = "equal" },
            new SyntaxPair { Token = new TokenValue("k1"), MetaSyntaxName = "key-value" },
            new SyntaxPair { Token = new TokenValue("set"), MetaSyntaxName = "set-sym" },
            new SyntaxPair { Token = new TokenValue("t1"), MetaSyntaxName = "tagKey" },
            new SyntaxPair { Token = new TokenValue(";"), MetaSyntaxName = "term" },
        };

        syntaxPairs.SequenceEqual(expectedPairs).Should().BeTrue();
    }

    [Fact]
    public void AddCommandWithTwoTag()
    {
        var parse = _parser.Parse("add node key=k1 set t1, t2=v2 ;", _context);
        parse.Status.IsOk().Should().BeTrue(parse.Status.Error);

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var syntaxLines = syntaxPairs.GenerateSyntaxPairs().Join(Environment.NewLine);

        var expectedPairs = new[]
        {
            new SyntaxPair { Token = new TokenValue("add"), MetaSyntaxName = "add-sym" },
            new SyntaxPair { Token = new TokenValue("node"), MetaSyntaxName = "node-sym" },
            new SyntaxPair { Token = new TokenValue("key"), MetaSyntaxName = "key-sym" },
            new SyntaxPair { Token = new TokenValue("="), MetaSyntaxName = "equal" },
            new SyntaxPair { Token = new TokenValue("k1"), MetaSyntaxName = "key-value" },
            new SyntaxPair { Token = new TokenValue("set"), MetaSyntaxName = "set-sym" },
            new SyntaxPair { Token = new TokenValue("t1"), MetaSyntaxName = "tagKey" },
            new SyntaxPair { Token = new TokenValue(","), MetaSyntaxName = "comma" },
            new SyntaxPair { Token = new TokenValue("t2"), MetaSyntaxName = "tagKey" },
            new SyntaxPair { Token = new TokenValue("="), MetaSyntaxName = "_tag-3-OptionGroup-1" },
            new SyntaxPair { Token = new TokenValue("v2"), MetaSyntaxName = "tagValue" },
            new SyntaxPair { Token = new TokenValue(";"), MetaSyntaxName = "term" },
        };

        syntaxPairs.SequenceEqual(expectedPairs).Should().BeTrue();
    }

    [Fact]
    public void AddCommandWithData()
    {
        var parse = _parser.Parse("add node key=k1 set data { base64 } ;", _context);
        parse.Status.IsOk().Should().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var syntaxLines = syntaxPairs.GenerateSyntaxPairs().Join(Environment.NewLine);

        var expectedPairs = new[]
        {
            new SyntaxPair { Token = new TokenValue("add"), MetaSyntaxName = "add-sym" },
            new SyntaxPair { Token = new TokenValue("node"), MetaSyntaxName = "node-sym" },
            new SyntaxPair { Token = new TokenValue("key"), MetaSyntaxName = "key-sym" },
            new SyntaxPair { Token = new TokenValue("="), MetaSyntaxName = "equal" },
            new SyntaxPair { Token = new TokenValue("k1"), MetaSyntaxName = "key-value" },
            new SyntaxPair { Token = new TokenValue("set"), MetaSyntaxName = "set-sym" },
            new SyntaxPair { Token = new TokenValue("data"), MetaSyntaxName = "dataName" },
            new SyntaxPair { Token = new TokenValue("{"), MetaSyntaxName = "open-brace" },
            new SyntaxPair { Token = new TokenValue("base64"), MetaSyntaxName = "base64" },
            new SyntaxPair { Token = new TokenValue("}"), MetaSyntaxName = "close-brace" },
            new SyntaxPair { Token = new TokenValue(";"), MetaSyntaxName = "term" },
        };

        syntaxPairs.SequenceEqual(expectedPairs).Should().BeTrue();
    }

    [Fact]
    public void AddCommandWithTwoData()
    {
        var parse = _parser.Parse("add node key=k1 set data { base64 }, entity { entityData64 } ;", _context);
        parse.Status.IsOk().Should().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var syntaxLines = syntaxPairs.GenerateSyntaxPairs().Join(Environment.NewLine);

        var expectedPairs = new[]
        {
            new SyntaxPair { Token = new TokenValue("add"), MetaSyntaxName = "add-sym" },
            new SyntaxPair { Token = new TokenValue("node"), MetaSyntaxName = "node-sym" },
            new SyntaxPair { Token = new TokenValue("key"), MetaSyntaxName = "key-sym" },
            new SyntaxPair { Token = new TokenValue("="), MetaSyntaxName = "equal" },
            new SyntaxPair { Token = new TokenValue("k1"), MetaSyntaxName = "key-value" },
            new SyntaxPair { Token = new TokenValue("set"), MetaSyntaxName = "set-sym" },
            new SyntaxPair { Token = new TokenValue("data"), MetaSyntaxName = "dataName" },
            new SyntaxPair { Token = new TokenValue("{"), MetaSyntaxName = "open-brace" },
            new SyntaxPair { Token = new TokenValue("base64"), MetaSyntaxName = "base64" },
            new SyntaxPair { Token = new TokenValue("}"), MetaSyntaxName = "close-brace" },
            new SyntaxPair { Token = new TokenValue(","), MetaSyntaxName = "comma" },
            new SyntaxPair { Token = new TokenValue("entity"), MetaSyntaxName = "dataName" },
            new SyntaxPair { Token = new TokenValue("{"), MetaSyntaxName = "open-brace" },
            new SyntaxPair { Token = new TokenValue("entityData64"), MetaSyntaxName = "base64" },
            new SyntaxPair { Token = new TokenValue("}"), MetaSyntaxName = "close-brace" },
            new SyntaxPair { Token = new TokenValue(";"), MetaSyntaxName = "term" },
        };

        syntaxPairs.SequenceEqual(expectedPairs).Should().BeTrue();
    }

    [Fact]
    public void AddCommandWithTagsData()
    {
        var parse = _parser.Parse("add node key=k1 set t1, t2=v3, t3, data { base64 } ;", _context);
        parse.Status.IsOk().Should().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var syntaxLines = syntaxPairs.GenerateSyntaxPairs().Join(Environment.NewLine);

        var expectedPairs = new[]
        {
            new SyntaxPair { Token = new TokenValue("add"), MetaSyntaxName = "add-sym" },
            new SyntaxPair { Token = new TokenValue("node"), MetaSyntaxName = "node-sym" },
            new SyntaxPair { Token = new TokenValue("key"), MetaSyntaxName = "key-sym" },
            new SyntaxPair { Token = new TokenValue("="), MetaSyntaxName = "equal" },
            new SyntaxPair { Token = new TokenValue("k1"), MetaSyntaxName = "key-value" },
            new SyntaxPair { Token = new TokenValue("set"), MetaSyntaxName = "set-sym" },
            new SyntaxPair { Token = new TokenValue("t1"), MetaSyntaxName = "tagKey" },
            new SyntaxPair { Token = new TokenValue(","), MetaSyntaxName = "comma" },
            new SyntaxPair { Token = new TokenValue("t2"), MetaSyntaxName = "tagKey" },
            new SyntaxPair { Token = new TokenValue("="), MetaSyntaxName = "_tag-3-OptionGroup-1" },
            new SyntaxPair { Token = new TokenValue("v3"), MetaSyntaxName = "tagValue" },
            new SyntaxPair { Token = new TokenValue(","), MetaSyntaxName = "comma" },
            new SyntaxPair { Token = new TokenValue("t3"), MetaSyntaxName = "tagKey" },
            new SyntaxPair { Token = new TokenValue(","), MetaSyntaxName = "comma" },
            new SyntaxPair { Token = new TokenValue("data"), MetaSyntaxName = "dataName" },
            new SyntaxPair { Token = new TokenValue("{"), MetaSyntaxName = "open-brace" },
            new SyntaxPair { Token = new TokenValue("base64"), MetaSyntaxName = "base64" },
            new SyntaxPair { Token = new TokenValue("}"), MetaSyntaxName = "close-brace" },
            new SyntaxPair { Token = new TokenValue(";"), MetaSyntaxName = "term" },
        };

        syntaxPairs.SequenceEqual(expectedPairs).Should().BeTrue();
    }

    [Fact]
    public void AddCommandWithTagsTwoData()
    {
        var parse = _parser.Parse("add node key=k1 set t1, entity { entityBase64 }, t2=v3, t3, data { base64 } ;", _context);
        parse.Status.IsOk().Should().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var syntaxLines = syntaxPairs.GenerateSyntaxPairs().Join(Environment.NewLine);

        var expectedPairs = new[]
        {
            new SyntaxPair { Token = new TokenValue("add"), MetaSyntaxName = "add-sym" },
            new SyntaxPair { Token = new TokenValue("node"), MetaSyntaxName = "node-sym" },
            new SyntaxPair { Token = new TokenValue("key"), MetaSyntaxName = "key-sym" },
            new SyntaxPair { Token = new TokenValue("="), MetaSyntaxName = "equal" },
            new SyntaxPair { Token = new TokenValue("k1"), MetaSyntaxName = "key-value" },
            new SyntaxPair { Token = new TokenValue("set"), MetaSyntaxName = "set-sym" },
            new SyntaxPair { Token = new TokenValue("t1"), MetaSyntaxName = "tagKey" },
            new SyntaxPair { Token = new TokenValue(","), MetaSyntaxName = "comma" },
            new SyntaxPair { Token = new TokenValue("entity"), MetaSyntaxName = "dataName" },
            new SyntaxPair { Token = new TokenValue("{"), MetaSyntaxName = "open-brace" },
            new SyntaxPair { Token = new TokenValue("entityBase64"), MetaSyntaxName = "base64" },
            new SyntaxPair { Token = new TokenValue("}"), MetaSyntaxName = "close-brace" },
            new SyntaxPair { Token = new TokenValue(","), MetaSyntaxName = "comma" },
            new SyntaxPair { Token = new TokenValue("t2"), MetaSyntaxName = "tagKey" },
            new SyntaxPair { Token = new TokenValue("="), MetaSyntaxName = "_tag-3-OptionGroup-1" },
            new SyntaxPair { Token = new TokenValue("v3"), MetaSyntaxName = "tagValue" },
            new SyntaxPair { Token = new TokenValue(","), MetaSyntaxName = "comma" },
            new SyntaxPair { Token = new TokenValue("t3"), MetaSyntaxName = "tagKey" },
            new SyntaxPair { Token = new TokenValue(","), MetaSyntaxName = "comma" },
            new SyntaxPair { Token = new TokenValue("data"), MetaSyntaxName = "dataName" },
            new SyntaxPair { Token = new TokenValue("{"), MetaSyntaxName = "open-brace" },
            new SyntaxPair { Token = new TokenValue("base64"), MetaSyntaxName = "base64" },
            new SyntaxPair { Token = new TokenValue("}"), MetaSyntaxName = "close-brace" },
            new SyntaxPair { Token = new TokenValue(";"), MetaSyntaxName = "term" },
        };

        syntaxPairs.SequenceEqual(expectedPairs).Should().BeTrue();
    }
}
