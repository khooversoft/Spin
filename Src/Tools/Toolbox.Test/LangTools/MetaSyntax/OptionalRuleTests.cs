using FluentAssertions;
using Toolbox.Extensions;
using Toolbox.LangTools;
using Toolbox.Test.Application;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Test.LangTools.MetaSyntax;

public class OptionalRuleTests : TestBase
{
    private readonly MetaSyntaxRoot _schema;

    public OptionalRuleTests(ITestOutputHelper output) : base(output)
    {
        string schemaText = new[]
        {
            "delimiters          = [ ] = ;",
            "symbol              = regex '^[a-zA-Z][a-zA-Z0-9\\-]*$' ;",
            "tag                 = symbol, [ '=', symbol ] ;",
        }.Join(Environment.NewLine);

        _schema = MetaParser.ParseRules(schemaText);
        _schema.StatusCode.IsOk().Should().BeTrue();
    }

    [Fact]
    public void OnlyRequiredOfOptional()
    {
        var parser = new SyntaxParser(_schema);
        var logger = GetScopeContext<OrRuleTests>();

        var parse = parser.Parse("t1", logger);
        parse.Status.IsOk().Should().BeTrue(parse.Status.Error);

        var lines = SyntaxTestTool.GenerateTestCodeSyntaxTree(parse.SyntaxTree).Join(Environment.NewLine);

        var expectedTree = new SyntaxTree
        {
            Children = new ISyntaxTree[]
            {
                new SyntaxTree
                {
                    MetaSyntaxName = "tag",
                    Children = new ISyntaxTree[]
                    {
                        new SyntaxPair { Token = new TokenValue("t1"), MetaSyntaxName = "symbol" },
                    },
                },
            },
        };

        (parse.SyntaxTree == expectedTree).Should().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var syntaxLines = SyntaxTestTool.GenerateSyntaxPairs(syntaxPairs).Join(Environment.NewLine);

        var expectedPairs = new[]
        {
            new SyntaxPair { Token = new TokenValue("t1"), MetaSyntaxName = "symbol" },
        };

        Enumerable.SequenceEqual(syntaxPairs, expectedPairs).Should().BeTrue();
    }

    [Fact]
    public void OnlyRequiredAndOptional()
    {
        var parser = new SyntaxParser(_schema);
        var logger = GetScopeContext<OrRuleTests>();

        var parse = parser.Parse("t1 = v1", logger);
        parse.Status.IsOk().Should().BeTrue(parse.Status.Error);

        var lines = SyntaxTestTool.GenerateTestCodeSyntaxTree(parse.SyntaxTree).Join(Environment.NewLine);

        var expectedTree = new SyntaxTree
        {
            Children = new ISyntaxTree[]
            {
                new SyntaxTree
                {
                    MetaSyntaxName = "tag",
                    Children = new ISyntaxTree[]
                    {
                        new SyntaxPair { Token = new TokenValue("t1"), MetaSyntaxName = "symbol" },
                        new SyntaxTree
                        {
                            MetaSyntaxName = "_tag-3-OptionGroup",
                            Children = new ISyntaxTree[]
                            {
                                new SyntaxPair { Token = new TokenValue("="), MetaSyntaxName = "_tag-3-OptionGroup-1" },
                                new SyntaxPair { Token = new TokenValue("v1"), MetaSyntaxName = "symbol" },
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
            new SyntaxPair { Token = new TokenValue("t1"), MetaSyntaxName = "symbol" },
            new SyntaxPair { Token = new TokenValue("="), MetaSyntaxName = "_tag-3-OptionGroup-1" },
            new SyntaxPair { Token = new TokenValue("v1"), MetaSyntaxName = "symbol" },
        };

        Enumerable.SequenceEqual(syntaxPairs, expectedPairs).Should().BeTrue();
    }
}
