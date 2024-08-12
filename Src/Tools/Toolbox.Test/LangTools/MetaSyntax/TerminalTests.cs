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
            "number = regex '^[+-]?[0-9]+$' ;",
            "term = ';' ;",
            "alias = number ;"
        }.Join(Environment.NewLine);

        var schema = MetaParser.ParseRules(schemaText);
        schema.StatusCode.IsOk().Should().BeTrue();

        var parser = new SyntaxParser(schema);

        var parse = parser.Parse("3", NullScopeContext.Instance);
        parse.StatusCode.IsOk().Should().BeTrue();

        var lines = SyntaxTestTool.GenerateTestCodeSyntaxTree(parse.SyntaxTree).Join(Environment.NewLine);

        var expectedTree = new SyntaxTree
        {
            Children = new ISyntaxTree[]
            {
                new SyntaxPair
                {
                    Token = new TokenValue("3"),
                    MetaSyntaxName = "number",
                },
            },
        };

        (parse.SyntaxTree == expectedTree).Should().BeTrue();
    }

    [Fact]
    public void TerminalSymbolRegexFail()
    {
        string schemaText = new[]
        {
            "number = regex '^[+-]?[0-9]+$' ;",
            "term = ';' ;",
            "alias = number ;"
        }.Join(Environment.NewLine);

        var schema = MetaParser.ParseRules(schemaText);
        schema.StatusCode.IsOk().Should().BeTrue();

        var parser = new SyntaxParser(schema);

        var parse = parser.Parse("A", NullScopeContext.Instance);
        parse.StatusCode.IsOk().Should().BeFalse(parse.Error);
        parse.Error.Should().Be("No rules matched");
    }
}
