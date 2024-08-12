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
            "symbol              = regex '^[a-zA-Z][a-zA-Z0-9\\-]*$' ;",
            "tag                 = symbol, [ '=', symbol ] ;",
        }.Join(Environment.NewLine);

        _schema = MetaParser.ParseRules(schemaText);
        _schema.StatusCode.IsOk().Should().BeTrue();
    }

    [Theory]
    [InlineData("tag =")]
    public void SimpleAndSymbolFail(string rawData)
    {
        var parser = new SyntaxParser(_schema);
        var logger = GetScopeContext<OrRuleTests>();

        parser.Parse(rawData, logger).StatusCode.IsError().Should().BeTrue();
    }

    [Fact]
    public void OnlyRequiredOfOptional()
    {
        var parser = new SyntaxParser(_schema);
        var logger = GetScopeContext<OrRuleTests>();

        var parse = parser.Parse("t1", logger);
        parse.StatusCode.IsOk().Should().BeTrue(parse.Error);

        var lines = SyntaxTestTool.GenerateTestCodeSyntaxTree(parse.SyntaxTree).Join(Environment.NewLine);

        var expectedTree = new SyntaxTree
        {
            Children = new ISyntaxTree[]
            {
                new SyntaxPair
                {
                    Token = new TokenValue("t1"),
                    MetaSyntaxName = "symbol",
                },
            },
        };

        (parse.SyntaxTree == expectedTree).Should().BeTrue();
    }

    [Fact]
    public void OnlyRequiredAndOptional()
    {
        var parser = new SyntaxParser(_schema);
        var logger = GetScopeContext<OrRuleTests>();

        var parse = parser.Parse("t1 = v1", logger);
        parse.StatusCode.IsOk().Should().BeTrue(parse.Error);

        var lines = SyntaxTestTool.GenerateTestCodeSyntaxTree(parse.SyntaxTree).Join(Environment.NewLine);

        var expectedTree = new SyntaxTree
        {
            Children = new ISyntaxTree[]
            {
                new SyntaxPair
                {
                    Token = new TokenValue("t1"),
                    MetaSyntaxName = "symbol",
                },
                new SyntaxTree
                {
                    MetaSyntaxName = "_tag-3-OptionGroup",
                    Children = new ISyntaxTree[]
                    {
                        new SyntaxPair
                        {
                            Token = new TokenValue("="),
                            MetaSyntaxName = "_tag-3-OptionGroup-1",
                        },
                        new SyntaxPair
                        {
                            Token = new TokenValue("v1"),
                            MetaSyntaxName = "symbol",
                        },
                    },
                },
            },
        };

        (parse.SyntaxTree == expectedTree).Should().BeTrue();
    }
}
