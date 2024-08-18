using FluentAssertions;
using Toolbox.Extensions;
using Toolbox.LangTools;
using Toolbox.Test.Application;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Test.LangTools.MetaSyntax;

public class ReturnDataTests : TestBase
{
    private readonly MetaSyntaxRoot _schema;

    public ReturnDataTests(ITestOutputHelper output) : base(output)
    {
        string schemaText = new[]
{
            "symbol              = regex '^[a-zA-Z][a-zA-Z0-9\\-]*$' ;",
            "comma               = ',' ;",
            "return-sym          = 'return' ;",
            "return-query        = return-sym, symbol, { comma, symbol } ;",
        }.Join(Environment.NewLine);

        _schema = MetaParser.ParseRules(schemaText);
        _schema.StatusCode.IsOk().Should().BeTrue();
    }

    [Theory]
    [InlineData("return d1 f1")]
    [InlineData("return")]
    public void FailedReturn(string command)
    {
        var parser = new SyntaxParser(_schema);
        var logger = GetScopeContext<OrRuleTests>();

        var parse = parser.Parse(command, logger);
        parse.StatusCode.IsError().Should().BeTrue(parse.Error);
    }

    [Fact]
    public void SimpleLabelAndRepeat()
    {
        var parser = new SyntaxParser(_schema);
        var logger = GetScopeContext<OrRuleTests>();

        var parse = parser.Parse("return d1", logger);
        parse.StatusCode.IsOk().Should().BeTrue(parse.Error);

        var lines = SyntaxTestTool.GenerateTestCodeSyntaxTree(parse.SyntaxTree).Join(Environment.NewLine);

        var expectedTree = new SyntaxTree
        {
            Children = new ISyntaxTree[]
            {
                new SyntaxTree
                {
                    MetaSyntaxName = "return-query",
                    Children = new ISyntaxTree[]
                    {
                        new SyntaxPair { Token = new TokenValue("return"), MetaSyntaxName = "return-sym" },
                        new SyntaxPair { Token = new TokenValue("d1"), MetaSyntaxName = "symbol" },
                    },
                },
            },
        };

        (parse.SyntaxTree == expectedTree).Should().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var syntaxLines = SyntaxTestTool.GenerateSyntaxPairs(syntaxPairs).Join(Environment.NewLine);

        var expectedPairs = new[]
        {
            new SyntaxPair { Token = new TokenValue("return"), MetaSyntaxName = "return-sym" },
            new SyntaxPair { Token = new TokenValue("d1"), MetaSyntaxName = "symbol" },
        };

        Enumerable.SequenceEqual(syntaxPairs, expectedPairs).Should().BeTrue();
    }


    [Fact]
    public void MultipleData()
    {
        var parser = new SyntaxParser(_schema);
        var logger = GetScopeContext<OrRuleTests>();

        var parse = parser.Parse("return d1, d2", logger);
        parse.StatusCode.IsOk().Should().BeTrue(parse.Error);

        var lines = SyntaxTestTool.GenerateTestCodeSyntaxTree(parse.SyntaxTree).Join(Environment.NewLine);

        var expectedTree = new SyntaxTree
        {
            Children = new ISyntaxTree[]
            {
                new SyntaxTree
                {
                    MetaSyntaxName = "return-query",
                    Children = new ISyntaxTree[]
                    {
                        new SyntaxPair { Token = new TokenValue("return"), MetaSyntaxName = "return-sym" },
                        new SyntaxPair { Token = new TokenValue("d1"), MetaSyntaxName = "symbol" },
                        new SyntaxTree
                        {
                            MetaSyntaxName = "_return-query-5-RepeatGroup",
                            Children = new ISyntaxTree[]
                            {
                                new SyntaxPair { Token = new TokenValue(","), MetaSyntaxName = "comma" },
                                new SyntaxPair { Token = new TokenValue("d2"), MetaSyntaxName = "symbol" },
                            },
                        },
                    },
                },
            },
        };

        (parse.SyntaxTree == expectedTree).Should().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var syntaxLines = SyntaxTestTool.GenerateSyntaxPairs(syntaxPairs).Join(Environment.NewLine);

        var expectedPairs = new[]
        {
            new SyntaxPair { Token = new TokenValue("return"), MetaSyntaxName = "return-sym" },
            new SyntaxPair { Token = new TokenValue("d1"), MetaSyntaxName = "symbol" },
            new SyntaxPair { Token = new TokenValue(","), MetaSyntaxName = "comma" },
            new SyntaxPair { Token = new TokenValue("d2"), MetaSyntaxName = "symbol" },
        };

        Enumerable.SequenceEqual(syntaxPairs, expectedPairs).Should().BeTrue();
    }

}
