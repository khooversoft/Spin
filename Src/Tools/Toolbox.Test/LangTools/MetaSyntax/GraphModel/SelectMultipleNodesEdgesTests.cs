using FluentAssertions;
using Toolbox.Extensions;
using Toolbox.LangTools;
using Toolbox.Test.Application;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Test.LangTools.MetaSyntax.GraphModel;

public class SelectMultipleNodesEdgesTests : TestBase<SelectMultipleNodesEdgesTests>
{
    private readonly ITestOutputHelper _output;
    private readonly MetaSyntaxRoot _root;
    private readonly SyntaxParser _parser;
    private readonly ScopeContext _context;

    public SelectMultipleNodesEdgesTests(ITestOutputHelper output) : base(output)
    {
        _output = output.NotNull();

        string schema = GraphModelTool.ReadGraphLanauge2();
        _root = MetaParser.ParseRules(schema);
        _root.StatusCode.IsOk().Should().BeTrue(_root.Error);

        _context = GetScopeContext();
        _parser = new SyntaxParser(_root);
    }

    [Fact]
    public void SelectCommand()
    {
        var parse = _parser.Parse("upsert node key=k1 set t1, entity { entityBase64 }, t2=v3, t3, data { base64 } ;", _context);
        parse.Status.IsOk().Should().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var syntaxLines = SyntaxTestTool.GenerateSyntaxPairs(syntaxPairs).Join(Environment.NewLine);

        var expectedPairs = new[]
        {
            new SyntaxPair { Token = new TokenValue("upsert"), MetaSyntaxName = "upsert-sym" },
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

        Enumerable.SequenceEqual(syntaxPairs, expectedPairs).Should().BeTrue();
    }

}
