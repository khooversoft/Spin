using FluentAssertions;
using Toolbox.Extensions;
using Toolbox.LangTools;
using Toolbox.Test.Application;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Test.LangTools.MetaSyntax;

public class RepeatRuleTests : TestBase
{
    private readonly MetaSyntaxRoot _schema;

    public RepeatRuleTests(ITestOutputHelper output) : base(output)
    {
        string schemaText = new[]
{
            "symbol              = regex '^[a-zA-Z][a-zA-Z0-9\\-]*$' ;",
            "comma               = ',' ;",
            "tag                 = symbol, [ '=', symbol ] ;",
            "tags                = tag, { comma, tag } ;",
        }.Join(Environment.NewLine);

        _schema = MetaParser.ParseRules(schemaText);
        _schema.StatusCode.IsOk().Should().BeTrue();
    }

    [Fact]
    public void SimpleRepeat()
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
                new SyntaxTree
                {
                    MetaSyntax = new ProductionRule
                    {
                        Name = "tag",
                        Type = ProductionRuleType.Root,
                        EvaluationType = EvaluationType.Sequence,
                        Children = new IMetaSyntax[]
                        {
                            new ProductionRuleReference { Name = "_tag-1-symbol", ReferenceSyntax = "symbol" },
                            new ProductionRule
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
                        },
                    },
                    Children = new ISyntaxTree[]
                    {
                        new SyntaxPair
                        {
                            Token = new TokenValue("t1"),
                            MetaSyntax = new TerminalSymbol { Name = "symbol", Text = "^[a-zA-Z][a-zA-Z0-9\\-]*$", Type = TerminalType.Regex },
                        },
                    },
                },
            },
        };

        (parse.SyntaxTree == expectedTree).Should().BeTrue();
    }

    [Fact]
    public void SimpleTwoRepeat()
    {
        var parser = new SyntaxParser(_schema);
        var logger = GetScopeContext<OrRuleTests>();

        var parse = parser.Parse("t1, t2", logger);
        parse.StatusCode.IsOk().Should().BeTrue(parse.Error);

        var lines = SyntaxTestTool.GenerateTestCodeSyntaxTree(parse.SyntaxTree).Join(Environment.NewLine);

        var expectedTree = new SyntaxTree
        {
            Children = new ISyntaxTree[]
            {
                new SyntaxTree
                {
                    MetaSyntax = new ProductionRule
                    {
                        Name = "tag",
                        Type = ProductionRuleType.Root,
                        EvaluationType = EvaluationType.Sequence,
                        Children = new IMetaSyntax[]
                        {
                            new ProductionRuleReference { Name = "_tag-1-symbol", ReferenceSyntax = "symbol" },
                            new ProductionRule
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
                        },
                    },
                    Children = new ISyntaxTree[]
                    {
                        new SyntaxPair
                        {
                            Token = new TokenValue("t1"),
                            MetaSyntax = new TerminalSymbol { Name = "symbol", Text = "^[a-zA-Z][a-zA-Z0-9\\-]*$", Type = TerminalType.Regex },
                        },
                    },
                },
                new SyntaxTree
                {
                    MetaSyntax = new ProductionRule
                    {
                        Name = "_tags-3-RepeatGroup",
                        Type = ProductionRuleType.Repeat,
                        EvaluationType = EvaluationType.Sequence,
                        Children = new IMetaSyntax[]
                        {
                            new ProductionRuleReference { Name = "_tags-3-RepeatGroup-1-comma", ReferenceSyntax = "comma" },
                            new ProductionRuleReference { Name = "_tags-3-RepeatGroup-3-tag", ReferenceSyntax = "tag" },
                        },
                    },
                    Children = new ISyntaxTree[]
                    {
                        new SyntaxPair
                        {
                            Token = new TokenValue(","),
                            MetaSyntax = new TerminalSymbol { Name = "comma", Text = ",", Type = TerminalType.Token },
                        },
                        new SyntaxTree
                        {
                            MetaSyntax = new ProductionRule
                            {
                                Name = "tag",
                                Type = ProductionRuleType.Root,
                                EvaluationType = EvaluationType.Sequence,
                                Children = new IMetaSyntax[]
                                {
                                    new ProductionRuleReference { Name = "_tag-1-symbol", ReferenceSyntax = "symbol" },
                                    new ProductionRule
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
                                },
                            },
                            Children = new ISyntaxTree[]
                            {
                                new SyntaxPair
                                {
                                    Token = new TokenValue("t2"),
                                    MetaSyntax = new TerminalSymbol { Name = "symbol", Text = "^[a-zA-Z][a-zA-Z0-9\\-]*$", Type = TerminalType.Regex },
                                },
                            },
                        },
                    },
                },
            },
        };

        (parse.SyntaxTree == expectedTree).Should().BeTrue();
    }
}
