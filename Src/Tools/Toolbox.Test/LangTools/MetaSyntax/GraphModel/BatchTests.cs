using Toolbox.Extensions;
using Toolbox.LangTools;
using Toolbox.Test.Application;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Test.LangTools.MetaSyntax.GraphModel;

public class BatchTests : TestBase<BatchTests>
{
    private readonly ITestOutputHelper _output;
    private readonly MetaSyntaxRoot _root;
    private readonly SyntaxParser _parser;
    private readonly ScopeContext _context;

    public BatchTests(ITestOutputHelper output) : base(output)
    {
        _output = output.NotNull();

        string schema = GraphModelTool.ReadGraphLanauge2();
        _root = MetaParser.ParseRules(schema);
        _root.StatusCode.IsOk().BeTrue(_root.Error);

        _context = GetScopeContext();
        _parser = new SyntaxParser(_root);
    }

    [Fact]
    public void TwoAddCommands()
    {
        string lines = new[]
        {
            "add node key=k1;",
            "add node key=k2;",
        }.Join(Environment.NewLine);

        var parse = _parser.Parse(lines, _context);
        parse.Status.IsOk().BeTrue();

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
            new SyntaxPair { Token = new TokenValue("add"), MetaSyntaxName = "add-sym" },
            new SyntaxPair { Token = new TokenValue("node"), MetaSyntaxName = "node-sym" },
            new SyntaxPair { Token = new TokenValue("key"), MetaSyntaxName = "key-sym" },
            new SyntaxPair { Token = new TokenValue("="), MetaSyntaxName = "equal" },
            new SyntaxPair { Token = new TokenValue("k2"), MetaSyntaxName = "key-value" },
            new SyntaxPair { Token = new TokenValue(";"), MetaSyntaxName = "term" },
        };

        syntaxPairs.SequenceEqual(expectedPairs).BeTrue();
    }

    [Fact]
    public void MultipleCommands()
    {
        string lines = new[]
        {
            "add node key=k1;",
            "add node key=k2 set entity { helloBase64 };",
            "add edge from=k1, to=k2, type=user set k2=v2;",
            "select (userEmail) a1 -> [roles] a2 -> (*) a3 return data, entity;",
        }.Join(Environment.NewLine);

        var parse = _parser.Parse(lines, _context);
        parse.Status.IsOk().BeTrue();

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
            new SyntaxPair { Token = new TokenValue("add"), MetaSyntaxName = "add-sym" },
            new SyntaxPair { Token = new TokenValue("node"), MetaSyntaxName = "node-sym" },
            new SyntaxPair { Token = new TokenValue("key"), MetaSyntaxName = "key-sym" },
            new SyntaxPair { Token = new TokenValue("="), MetaSyntaxName = "equal" },
            new SyntaxPair { Token = new TokenValue("k2"), MetaSyntaxName = "key-value" },
            new SyntaxPair { Token = new TokenValue("set"), MetaSyntaxName = "set-sym" },
            new SyntaxPair { Token = new TokenValue("entity"), MetaSyntaxName = "dataName" },
            new SyntaxPair { Token = new TokenValue("{"), MetaSyntaxName = "open-brace" },
            new SyntaxPair { Token = new TokenValue("helloBase64"), MetaSyntaxName = "base64" },
            new SyntaxPair { Token = new TokenValue("}"), MetaSyntaxName = "close-brace" },
            new SyntaxPair { Token = new TokenValue(";"), MetaSyntaxName = "term" },
            new SyntaxPair { Token = new TokenValue("add"), MetaSyntaxName = "add-sym" },
            new SyntaxPair { Token = new TokenValue("edge"), MetaSyntaxName = "edge-sym" },
            new SyntaxPair { Token = new TokenValue("from"), MetaSyntaxName = "fromKey-sym" },
            new SyntaxPair { Token = new TokenValue("="), MetaSyntaxName = "equal" },
            new SyntaxPair { Token = new TokenValue("k1"), MetaSyntaxName = "fromKey-value" },
            new SyntaxPair { Token = new TokenValue(","), MetaSyntaxName = "comma" },
            new SyntaxPair { Token = new TokenValue("to"), MetaSyntaxName = "toKey-sym" },
            new SyntaxPair { Token = new TokenValue("="), MetaSyntaxName = "equal" },
            new SyntaxPair { Token = new TokenValue("k2"), MetaSyntaxName = "toKey-value" },
            new SyntaxPair { Token = new TokenValue(","), MetaSyntaxName = "comma" },
            new SyntaxPair { Token = new TokenValue("type"), MetaSyntaxName = "edgeType-sym" },
            new SyntaxPair { Token = new TokenValue("="), MetaSyntaxName = "equal" },
            new SyntaxPair { Token = new TokenValue("user"), MetaSyntaxName = "edgeType-value" },
            new SyntaxPair { Token = new TokenValue("set"), MetaSyntaxName = "set-sym" },
            new SyntaxPair { Token = new TokenValue("k2"), MetaSyntaxName = "tagKey" },
            new SyntaxPair { Token = new TokenValue("="), MetaSyntaxName = "_tag-3-OptionGroup-1" },
            new SyntaxPair { Token = new TokenValue("v2"), MetaSyntaxName = "tagValue" },
            new SyntaxPair { Token = new TokenValue(";"), MetaSyntaxName = "term" },
            new SyntaxPair { Token = new TokenValue("select"), MetaSyntaxName = "select-sym" },
            new SyntaxPair { Token = new TokenValue("("), MetaSyntaxName = "open-param" },
            new SyntaxPair { Token = new TokenValue("userEmail"), MetaSyntaxName = "tagKey" },
            new SyntaxPair { Token = new TokenValue(")"), MetaSyntaxName = "close-param" },
            new SyntaxPair { Token = new TokenValue("a1"), MetaSyntaxName = "alias" },
            new SyntaxPair { Token = new TokenValue("->"), MetaSyntaxName = "left-join" },
            new SyntaxPair { Token = new TokenValue("["), MetaSyntaxName = "open-bracket" },
            new SyntaxPair { Token = new TokenValue("roles"), MetaSyntaxName = "tagKey" },
            new SyntaxPair { Token = new TokenValue("]"), MetaSyntaxName = "close-bracket" },
            new SyntaxPair { Token = new TokenValue("a2"), MetaSyntaxName = "alias" },
            new SyntaxPair { Token = new TokenValue("->"), MetaSyntaxName = "left-join" },
            new SyntaxPair { Token = new TokenValue("("), MetaSyntaxName = "open-param" },
            new SyntaxPair { Token = new TokenValue("*"), MetaSyntaxName = "tagKey" },
            new SyntaxPair { Token = new TokenValue(")"), MetaSyntaxName = "close-param" },
            new SyntaxPair { Token = new TokenValue("a3"), MetaSyntaxName = "alias" },
            new SyntaxPair { Token = new TokenValue("return"), MetaSyntaxName = "return-sym" },
            new SyntaxPair { Token = new TokenValue("data"), MetaSyntaxName = "dataName" },
            new SyntaxPair { Token = new TokenValue(","), MetaSyntaxName = "comma" },
            new SyntaxPair { Token = new TokenValue("entity"), MetaSyntaxName = "dataName" },
            new SyntaxPair { Token = new TokenValue(";"), MetaSyntaxName = "term" },
        };

        syntaxPairs.SequenceEqual(expectedPairs).BeTrue();
    }
}
