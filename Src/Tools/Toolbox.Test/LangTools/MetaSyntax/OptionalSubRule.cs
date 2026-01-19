using Toolbox.Extensions;
using Toolbox.LangTools;
using Toolbox.Test.Application;
using Toolbox.Tools;
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
            "delimiters          = -> <-> ( ) [ ] ;",
            "symbol              = regex '^[a-zA-Z\\*][a-zA-Z0-9\\-\\*]*$' ;",
            "join-left           = '->' ;",
            "join-inner          = '<->' ;",
            "join                = ( join-left | join-inner ) ;",
            "select              = symbol, [ join ] ;",
        }.Join(Environment.NewLine);

        var schema = MetaParser.ParseRules(schemaText);
        schema.StatusCode.IsOk().BeTrue(schema.Error);

        var parser = new SyntaxParser(schema);
        var logger = GetLogger<OrRuleTests>();

        var parse = parser.Parse("sym", logger);
        parse.Status.IsOk().BeTrue(parse.Status.Error);

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
                        new SyntaxPair { Token = new TokenValue("sym"), Name = "symbol" },
                    },
                },
            },
        };

        (parse.SyntaxTree == expectedTree).BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var syntaxLines = SyntaxTestTool.GenerateSyntaxPairs(syntaxPairs).Join(Environment.NewLine);

        var expectedPairs = new[]
        {
            new SyntaxPair { Token = new TokenValue("sym"), Name = "symbol" },
        };

        Enumerable.SequenceEqual(syntaxPairs, expectedPairs).BeTrue();
    }
}
