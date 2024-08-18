using FluentAssertions;
using Toolbox.Extensions;
using Toolbox.LangTools;
using Toolbox.Test.Application;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Test.LangTools.MetaSyntax;

public class OptionalSubRule : TestBase
{
    public OptionalSubRule(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public void LeftJoin()
    {
        string schemaText = new[]
        {
            "symbol              = regex '^[a-zA-Z\\*][a-zA-Z0-9\\-\\*]*$' ;",
            "join-left           = '->' ;",
            "join-inner          = '<->' ;",
            "join                = ( join-left | join-inner ) ;",
            "select              = symbol, [ join ] ;",
        }.Join(Environment.NewLine);

        var schema = MetaParser.ParseRules(schemaText);
        schema.StatusCode.IsOk().Should().BeTrue(schema.Error);

        var parser = new SyntaxParser(schema);
        var logger = GetScopeContext<OrRuleTests>();

        var parse = parser.Parse("sym", logger);
        parse.StatusCode.IsOk().Should().BeTrue(parse.Error);

        var lines = SyntaxTestTool.GenerateTestCodeSyntaxTree(parse.SyntaxTree).Join(Environment.NewLine);

        var expectedTree = new SyntaxTree
        {
            Children = new ISyntaxTree[]
            {
                new SyntaxTree
                {
                    MetaSyntaxName = "select",
                    Children = new ISyntaxTree[]
                    {
                        new SyntaxPair { Token = new TokenValue("sym"), MetaSyntaxName = "symbol" },
                    },
                },
            },
        };

        (parse.SyntaxTree == expectedTree).Should().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var syntaxLines = SyntaxTestTool.GenerateSyntaxPairs(syntaxPairs).Join(Environment.NewLine);

        var expectedPairs = new[]
        {
            new SyntaxPair { Token = new TokenValue("sym"), MetaSyntaxName = "symbol" },
        };

        Enumerable.SequenceEqual(syntaxPairs, expectedPairs).Should().BeTrue();
    }
}
