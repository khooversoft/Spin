using Toolbox.Extensions;
using Toolbox.LangTools;
using Toolbox.Tools;
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
        schema.StatusCode.IsOk().BeTrue(schema.Error);

        var parser = new SyntaxParser(schema);

        var parse = parser.Parse("3;", NullScopeContext.Instance);
        parse.Status.IsOk().BeTrue();

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

        (parse.SyntaxTree == expectedTree).BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var syntaxLines = SyntaxTestTool.GenerateSyntaxPairs(syntaxPairs).Join(Environment.NewLine);

        var expectedPairs = new[]
        {
            new SyntaxPair { Token = new TokenValue("3"), MetaSyntaxName = "number" },
            new SyntaxPair { Token = new TokenValue(";"), MetaSyntaxName = "term" },
        };

        Enumerable.SequenceEqual(syntaxPairs, expectedPairs).BeTrue();
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
        schema.StatusCode.IsOk().BeTrue();

        var parser = new SyntaxParser(schema);

        var parse = parser.Parse("A ;", NullScopeContext.Instance);
        parse.Status.IsError().BeTrue(parse.Status.Error);
        parse.Status.Error.Be("No rules matched");
    }
}
