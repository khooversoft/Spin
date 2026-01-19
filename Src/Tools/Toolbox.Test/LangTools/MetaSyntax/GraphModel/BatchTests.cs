using Microsoft.Extensions.Logging;
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
    private readonly ILogger _logger;

    public BatchTests(ITestOutputHelper output) : base(output)
    {
        _output = output.NotNull();

        string schema = GraphModelTool.ReadGraphLanauge2();
        _root = MetaParser.ParseRules(schema);
        _root.StatusCode.IsOk().BeTrue(_root.Error);

        _logger = GetLogger();
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

        var parse = _parser.Parse(lines, _logger);
        parse.Status.IsOk().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var syntaxLines = syntaxPairs.GenerateSyntaxPairs().Join(Environment.NewLine);

        var expectedPairs = new[]
        {
            new SyntaxPair { Token = new TokenValue("add"), Name = "add-sym" },
            new SyntaxPair { Token = new TokenValue("node"), Name = "node-sym" },
            new SyntaxPair { Token = new TokenValue("key"), Name = "key-sym" },
            new SyntaxPair { Token = new TokenValue("="), Name = "equal" },
            new SyntaxPair { Token = new TokenValue("k1"), Name = "key-value" },
            new SyntaxPair { Token = new TokenValue(";"), Name = "term" },
            new SyntaxPair { Token = new TokenValue("add"), Name = "add-sym" },
            new SyntaxPair { Token = new TokenValue("node"), Name = "node-sym" },
            new SyntaxPair { Token = new TokenValue("key"), Name = "key-sym" },
            new SyntaxPair { Token = new TokenValue("="), Name = "equal" },
            new SyntaxPair { Token = new TokenValue("k2"), Name = "key-value" },
            new SyntaxPair { Token = new TokenValue(";"), Name = "term" },
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

        var parse = _parser.Parse(lines, _logger);
        parse.Status.IsOk().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var syntaxLines = syntaxPairs.GenerateSyntaxPairs().Join(Environment.NewLine);

        var expectedPairs = new[]
        {
            new SyntaxPair { Token = new TokenValue("add"), Name = "add-sym" },
            new SyntaxPair { Token = new TokenValue("node"), Name = "node-sym" },
            new SyntaxPair { Token = new TokenValue("key"), Name = "key-sym" },
            new SyntaxPair { Token = new TokenValue("="), Name = "equal" },
            new SyntaxPair { Token = new TokenValue("k1"), Name = "key-value" },
            new SyntaxPair { Token = new TokenValue(";"), Name = "term" },
            new SyntaxPair { Token = new TokenValue("add"), Name = "add-sym" },
            new SyntaxPair { Token = new TokenValue("node"), Name = "node-sym" },
            new SyntaxPair { Token = new TokenValue("key"), Name = "key-sym" },
            new SyntaxPair { Token = new TokenValue("="), Name = "equal" },
            new SyntaxPair { Token = new TokenValue("k2"), Name = "key-value" },
            new SyntaxPair { Token = new TokenValue("set"), Name = "set-sym" },
            new SyntaxPair { Token = new TokenValue("entity"), Name = "dataName" },
            new SyntaxPair { Token = new TokenValue("{"), Name = "open-brace" },
            new SyntaxPair { Token = new TokenValue("helloBase64"), Name = "base64" },
            new SyntaxPair { Token = new TokenValue("}"), Name = "close-brace" },
            new SyntaxPair { Token = new TokenValue(";"), Name = "term" },
            new SyntaxPair { Token = new TokenValue("add"), Name = "add-sym" },
            new SyntaxPair { Token = new TokenValue("edge"), Name = "edge-sym" },
            new SyntaxPair { Token = new TokenValue("from"), Name = "fromKey-sym" },
            new SyntaxPair { Token = new TokenValue("="), Name = "equal" },
            new SyntaxPair { Token = new TokenValue("k1"), Name = "fromKey-value" },
            new SyntaxPair { Token = new TokenValue(","), Name = "comma" },
            new SyntaxPair { Token = new TokenValue("to"), Name = "toKey-sym" },
            new SyntaxPair { Token = new TokenValue("="), Name = "equal" },
            new SyntaxPair { Token = new TokenValue("k2"), Name = "toKey-value" },
            new SyntaxPair { Token = new TokenValue(","), Name = "comma" },
            new SyntaxPair { Token = new TokenValue("type"), Name = "edgeType-sym" },
            new SyntaxPair { Token = new TokenValue("="), Name = "equal" },
            new SyntaxPair { Token = new TokenValue("user"), Name = "edgeType-value" },
            new SyntaxPair { Token = new TokenValue("set"), Name = "set-sym" },
            new SyntaxPair { Token = new TokenValue("k2"), Name = "tagKey" },
            new SyntaxPair { Token = new TokenValue("="), Name = "_tag-3-OptionGroup-1" },
            new SyntaxPair { Token = new TokenValue("v2"), Name = "tagValue" },
            new SyntaxPair { Token = new TokenValue(";"), Name = "term" },
            new SyntaxPair { Token = new TokenValue("select"), Name = "select-sym" },
            new SyntaxPair { Token = new TokenValue("("), Name = "open-param" },
            new SyntaxPair { Token = new TokenValue("userEmail"), Name = "tagKey" },
            new SyntaxPair { Token = new TokenValue(")"), Name = "close-param" },
            new SyntaxPair { Token = new TokenValue("a1"), Name = "alias" },
            new SyntaxPair { Token = new TokenValue("->"), Name = "left-join" },
            new SyntaxPair { Token = new TokenValue("["), Name = "open-bracket" },
            new SyntaxPair { Token = new TokenValue("roles"), Name = "tagKey" },
            new SyntaxPair { Token = new TokenValue("]"), Name = "close-bracket" },
            new SyntaxPair { Token = new TokenValue("a2"), Name = "alias" },
            new SyntaxPair { Token = new TokenValue("->"), Name = "left-join" },
            new SyntaxPair { Token = new TokenValue("("), Name = "open-param" },
            new SyntaxPair { Token = new TokenValue("*"), Name = "tagKey" },
            new SyntaxPair { Token = new TokenValue(")"), Name = "close-param" },
            new SyntaxPair { Token = new TokenValue("a3"), Name = "alias" },
            new SyntaxPair { Token = new TokenValue("return"), Name = "return-sym" },
            new SyntaxPair { Token = new TokenValue("data"), Name = "dataName" },
            new SyntaxPair { Token = new TokenValue(","), Name = "comma" },
            new SyntaxPair { Token = new TokenValue("entity"), Name = "dataName" },
            new SyntaxPair { Token = new TokenValue(";"), Name = "term" },
        };

        syntaxPairs.SequenceEqual(expectedPairs).BeTrue();
    }
}
