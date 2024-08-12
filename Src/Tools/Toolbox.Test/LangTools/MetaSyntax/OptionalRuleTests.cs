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
    [InlineData("t1=v1 t2")]
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
                    MetaSyntax = new TerminalSymbol { Name = "symbol", Text = "^[a-zA-Z][a-zA-Z0-9\\-]*$", Type = TerminalType.Regex },
                },
            },
        }; ;

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
                    MetaSyntax = new TerminalSymbol { Name = "symbol", Text = "^[a-zA-Z][a-zA-Z0-9\\-]*$", Type = TerminalType.Regex },
                },
                new SyntaxTree
                {
                    MetaSyntax = new ProductionRule
                    {
                        Name = "_tag-3-OptionGroup",
                        Type = ProductionRuleType.Optional,
                        EvaluationType = EvaluationType.Sequence,
                        Children = new IMetaSyntax[]
                        {
                            new VirtualTerminalSymbol { Name = "_tag-3-OptionGroup-1", Text = "=" },
                            new ProductionRuleReference { Name = "_tag-3-OptionGroup-3-symbol", ReferenceSyntax = "symbol" },
                        },
                    },
                    Children = new ISyntaxTree[]
                    {
                        new SyntaxPair
                        {
                            Token = new TokenValue("="),
                            MetaSyntax = new VirtualTerminalSymbol { Name = "_tag-3-OptionGroup-1", Text = "=" },
                        },
                        new SyntaxPair
                        {
                            Token = new TokenValue("v1"),
                            MetaSyntax = new TerminalSymbol { Name = "symbol", Text = "^[a-zA-Z][a-zA-Z0-9\\-]*$", Type = TerminalType.Regex },
                        },
                    },
                },
            },
        };

        (parse.SyntaxTree == expectedTree).Should().BeTrue();
    }
}
