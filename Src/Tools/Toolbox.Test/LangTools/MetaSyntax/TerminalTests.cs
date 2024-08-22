using FluentAssertions;
using Toolbox.Extensions;
using Toolbox.LangTools;
using Toolbox.Types;

namespace Toolbox.Test.LangTools.MetaSyntax;

public class TerminalTests
{
    [Fact]
    public void TerminalSymbol()
    {
        string schemaText = new[]
        {
            "delimiters = ';' ;",
            "number = regex '^[+-]?[0-9]+$' ;",
            "term = ';' ;",
            "alias = number, term ;"
        }.Join(Environment.NewLine);

        var schema = MetaParser.ParseRules(schemaText);
        schema.StatusCode.IsOk().Should().BeTrue(schema.Error);

        var parser = new SyntaxParser(schema);

        var parse = parser.Parse("3;", NullScopeContext.Instance);
        parse.StatusCode.IsOk().Should().BeTrue();

        var lines = SyntaxTestTool.GenerateTestCodeSyntaxTree(parse.SyntaxTree).Join(Environment.NewLine);

        var expectedTree = new SyntaxTree
        {
            Children = new ISyntaxTree[]
            {
                new SyntaxTree
                {
                    MetaSyntaxName = "alias",
                    Children = new ISyntaxTree[]
                    {
                        new SyntaxPair { Token = new TokenValue("3"), MetaSyntaxName = "number" },
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
            new SyntaxPair { Token = new TokenValue("3"), MetaSyntaxName = "number" },
            new SyntaxPair { Token = new TokenValue(";"), MetaSyntaxName = "term" },
        };

        Enumerable.SequenceEqual(syntaxPairs, expectedPairs).Should().BeTrue();
    }

    [Fact]
    public void TerminalSymbolRegexFail()
    {
        string schemaText = new[]
        {
            "delimiters = ';' ;",
            "number = regex '^[+-]?[0-9]+$' ;",
            "term = ';' ;",
            "alias = number, term ;"
        }.Join(Environment.NewLine);

        var schema = MetaParser.ParseRules(schemaText);
        schema.StatusCode.IsOk().Should().BeTrue();

        var parser = new SyntaxParser(schema);

        var parse = parser.Parse("A ;", NullScopeContext.Instance);
        parse.StatusCode.IsError().Should().BeTrue(parse.Error);
        parse.Error.Should().Be("No rules matched");
    }
}
