using FluentAssertions;
using Toolbox.Extensions;
using Toolbox.LangTools;
using Toolbox.Test.Application;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Test.LangTools.MetaSyntax;

public class AndRuleTests : TestBase
{
    private readonly MetaSyntaxRoot _schema;

    public AndRuleTests(ITestOutputHelper output) : base(output)
    {
        string schemaText = new[]
        {
            "delimiters          = { } ';' ;",
            "symbol              = regex '^[a-zA-Z][a-zA-Z0-9\\-]*$' ;",
            "open-brace          = '{' #group-start #data ;",
            "close-brace         = '}' #group-end #data ;",
            "base64              = string ;",
            "term                = ';' ;",
            "entity-data         = symbol, open-brace, base64, close-brace, term;",
        }.Join(Environment.NewLine);

        _schema = MetaParser.ParseRules(schemaText);
        _schema.StatusCode.IsOk().Should().BeTrue();
    }

    [Theory]
    [InlineData("alias { hello }")]
    [InlineData("alias  hello };")]
    [InlineData("alias { hello ;")]
    [InlineData("alias { };")]
    [InlineData("{ hello };")]
    public void SimpleAndSymbolFail(string rawData)
    {
        var parser = new SyntaxParser(_schema);
        var logger = GetScopeContext<OrRuleTests>();

        parser.Parse(rawData, logger).StatusCode.IsError().Should().BeTrue();
    }

    [Fact]
    public void SimpleAndSymbol()
    {
        var parser = new SyntaxParser(_schema);
        var logger = GetScopeContext<OrRuleTests>();

        var parse = parser.Parse("alias { hello };", logger);
        parse.StatusCode.IsOk().Should().BeTrue();

        var lines = SyntaxTestTool.GenerateTestCodeSyntaxTree(parse.SyntaxTree).Join(Environment.NewLine);

        var expectedTree = new SyntaxTree
        {
            Children = new ISyntaxTree[]
            {
                new SyntaxTree
                {
                    MetaSyntaxName = "entity-data",
                    Children = new ISyntaxTree[]
                    {
                        new SyntaxPair { Token = new TokenValue("alias"), MetaSyntaxName = "symbol" },
                        new SyntaxPair { Token = new TokenValue("{"), MetaSyntaxName = "open-brace" },
                        new SyntaxPair { Token = new TokenValue("hello"), MetaSyntaxName = "base64" },
                        new SyntaxPair { Token = new TokenValue("}"), MetaSyntaxName = "close-brace" },
                        new SyntaxPair { Token = new TokenValue(";"), MetaSyntaxName = "term" },
                    },
                },
            },
        };

        (parse.SyntaxTree == expectedTree).Should().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var syntaxLines = SyntaxTestTool.GenerateSyntaxPairs(syntaxPairs).Join(Environment.NewLine);

        var expectedPairs = new[]
        {
            new SyntaxPair { Token = new TokenValue("alias"), MetaSyntaxName = "symbol" },
            new SyntaxPair { Token = new TokenValue("{"), MetaSyntaxName = "open-brace" },
            new SyntaxPair { Token = new TokenValue("hello"), MetaSyntaxName = "base64" },
            new SyntaxPair { Token = new TokenValue("}"), MetaSyntaxName = "close-brace" },
            new SyntaxPair { Token = new TokenValue(";"), MetaSyntaxName = "term" },
        };

        Enumerable.SequenceEqual(syntaxPairs, expectedPairs).Should().BeTrue();
    }

    [Fact]
    public void SimpleAndSymbolWithQuote()
    {
        var parser = new SyntaxParser(_schema);
        var logger = GetScopeContext<OrRuleTests>();

        var parse = parser.Parse("data { 'this is a test' };", logger);
        parse.StatusCode.IsOk().Should().BeTrue(parse.Error);

        var lines = SyntaxTestTool.GenerateTestCodeSyntaxTree(parse.SyntaxTree).Join(Environment.NewLine);

        var expectedTree = new SyntaxTree
        {
            Children = new ISyntaxTree[]
            {
                new SyntaxTree
                {
                    MetaSyntaxName = "entity-data",
                    Children = new ISyntaxTree[]
                    {
                        new SyntaxPair { Token = new TokenValue("data"), MetaSyntaxName = "symbol" },
                        new SyntaxPair { Token = new TokenValue("{"), MetaSyntaxName = "open-brace" },
                        new SyntaxPair { Token = new BlockToken("'this is a test'", '\'', '\'', 7), MetaSyntaxName = "base64" },
                        new SyntaxPair { Token = new TokenValue("}"), MetaSyntaxName = "close-brace" },
                        new SyntaxPair { Token = new TokenValue(";"), MetaSyntaxName = "term" },
                    },
                },
            },
        };

        (parse.SyntaxTree == expectedTree).Should().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var syntaxLines = SyntaxTestTool.GenerateSyntaxPairs(syntaxPairs).Join(Environment.NewLine);

        var expectedPairs = new[]
        {
            new SyntaxPair { Token = new TokenValue("data"), MetaSyntaxName = "symbol" },
            new SyntaxPair { Token = new TokenValue("{"), MetaSyntaxName = "open-brace" },
            new SyntaxPair { Token = new BlockToken("'this is a test'", '\'', '\'', 7), MetaSyntaxName = "base64" },
            new SyntaxPair { Token = new TokenValue("}"), MetaSyntaxName = "close-brace" },
            new SyntaxPair { Token = new TokenValue(";"), MetaSyntaxName = "term" },
        };

        Enumerable.SequenceEqual(syntaxPairs, expectedPairs).Should().BeTrue();
    }
}
