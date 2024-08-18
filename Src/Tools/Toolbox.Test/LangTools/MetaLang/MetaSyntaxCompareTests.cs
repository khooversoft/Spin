using FluentAssertions;
using Toolbox.Extensions;
using Toolbox.LangTools;
using Toolbox.Types;

namespace Toolbox.Test.LangTools.Meta;

public class MetaSyntaxCompareTests
{
    [Fact]
    public void TerminalRuleString()
    {
        var rule = "number  = regex '[+-]?[0-9]+' ;";

        var root = MetaParser.ParseRules(rule);
        root.StatusCode.IsOk().Should().BeTrue();

        var tree = new IMetaSyntax[]
        {
            new TerminalSymbol { Name = "number", Text = "[+-]?[0-9]+", Type = TerminalType.Regex },
        };

        Enumerable.SequenceEqual(root.Rule.Children, tree).Should().BeTrue();
    }

    [Fact]
    public void TerminalRuleRegex()
    {
        var rule = "number  = regex '[+-]?[0-9]+' ;";

        var root = MetaParser.ParseRules(rule);
        root.StatusCode.IsOk().Should().BeTrue();

        var tree = new IMetaSyntax[]
        {
            new TerminalSymbol { Name = "number", Text = "[+-]?[0-9]+", Type = TerminalType.Regex },
        };

        Enumerable.SequenceEqual(root.Rule.Children, tree).Should().BeTrue();
    }

    [Fact]
    public void TerminalRuleTestFail()
    {
        var rule = "number  = '[+-]?[0-9]+' ;";

        var root = MetaParser.ParseRules(rule);
        root.StatusCode.IsOk().Should().BeTrue();

        var tree = new IMetaSyntax[]
        {
            new TerminalSymbol { Name = "number", Text = "[+-]?[0-9]+ *", Type = TerminalType.Regex },
        };

        Enumerable.SequenceEqual(root.Rule.Children, tree).Should().BeFalse();
    }

    [Fact]
    public void TerminalAndProductionRule()
    {
        string[] rules = [
            "symbol = regex '[a-zA-Z][a-zA-Z0-9\\-/]*' ;",
            "alias = symbol ;",
            ];

        var root = MetaParser.ParseRules(rules.Join(Environment.NewLine));
        root.StatusCode.IsOk().Should().BeTrue();

        var tree = new IMetaSyntax[]
        {
            new TerminalSymbol { Name = "symbol", Text = "[a-zA-Z][a-zA-Z0-9\\-/]*", Type = TerminalType.Regex },
            new ProductionRule
            {
                Name = "alias",
                Type = ProductionRuleType.Sequence,
                Children = new IMetaSyntax[]
                {
                    new ProductionRuleReference { Name = "_alias-1-symbol", ReferenceSyntax = "symbol" },
                },
            },
        };

        Enumerable.SequenceEqual(root.Rule.Children, tree).Should().BeTrue();
    }

    [Fact]
    public void TerminalAndProductionRuleFail()
    {
        string[] rules = [
            "symbol = regex '[a-zA-Z][a-zA-Z0-9\\-/]*' ;",
            "alias = symbol ;",
            ];

        var root = MetaParser.ParseRules(rules.Join(Environment.NewLine));
        root.StatusCode.IsOk().Should().BeTrue();

        var tree = new IMetaSyntax[]
        {
            new TerminalSymbol { Name = "symbol", Text = "[a-zA-Z][a-zA-Z0-9\\-/]*", Type = TerminalType.Regex },
            new ProductionRule
            {
                Name = "alias",
                Type = ProductionRuleType.Sequence,
                Children = new IMetaSyntax[]
                {
                    new ProductionRuleReference { Name = "_alias-1-symbol", ReferenceSyntax = "symbol" },
                },
            },
        };

        Enumerable.SequenceEqual(root.Rule.Children, tree).Should().BeTrue();
    }

    [Fact]
    public void TerminalAndProductionRuleWithSubRules()
    {
        string[] rules = [
            "symbol = regex '[a-zA-Z][a-zA-Z0-9\\-/]*' ;",
            "alias = symbol ;",
            "tag = symbol, ['=', symbol ] ;",
            ];

        var root = MetaParser.ParseRules(rules.Join(Environment.NewLine));
        root.StatusCode.IsOk().Should().BeTrue();
        string c = MetaTestTool.GenerateTestCodeFromProductionRule(root.Rule).Join(Environment.NewLine);

        var tree = new IMetaSyntax[]
        {
            new TerminalSymbol { Name = "symbol", Text = "[a-zA-Z][a-zA-Z0-9\\-/]*", Type = TerminalType.Regex },
            new ProductionRule
            {
                Name = "alias",
                Type = ProductionRuleType.Sequence,
                Children = new IMetaSyntax[]
                {
                    new ProductionRuleReference { Name = "_alias-1-symbol", ReferenceSyntax = "symbol" },
                },
            },
            new ProductionRule
            {
                Name = "tag",
                Type = ProductionRuleType.Sequence,
                Children = new IMetaSyntax[]
                {
                    new ProductionRuleReference { Name = "_tag-1-symbol", ReferenceSyntax = "symbol" },
                    new ProductionRule
                    {
                        Name = "_tag-3-OptionGroup",
                        Type = ProductionRuleType.Optional,
                        Children = new IMetaSyntax[]
                        {
                            new VirtualTerminalSymbol { Name = "_tag-3-OptionGroup-1", Text = "=" },
                            new ProductionRuleReference { Name = "_tag-3-OptionGroup-3-symbol", ReferenceSyntax = "symbol" },
                        },
                    },
                },
            },
        };

        tree.FlattenMatch(tree);

        Enumerable.SequenceEqual(root.Rule.Children, tree).Should().BeTrue();
    }

    [Fact]
    public void ManyTerminalCommands()
    {
        string[] rules = [
            "number              = regex '[+-]?[0-9]+' ;",
            "symbol              = regex '[a-zA-Z][a-zA-Z0-9\\-/]*' ;",
            "base64              = string ;",
            "equal               = '=' ;",
            "join-left           = '->' ;",
            "join-inner          = '<->' ;",
            "node-sym            = 'node' ;",
            "edge-sym            = 'edge' ;",
            "return-sym          = 'return' ;",
            "select-sym          = 'select' ;",
            "delete-sym          = 'delete' ;",
            "update-sym          = 'update' ;",
            "upsert-syn          = 'upsert' ;",
            "add-sym             = 'add' ;",
            "set-sym             = 'set' ;",
            "open-param          = '(' ;",
            "close-param         = ')' ;",
            "open-bracket        = '[' ;",
            "close-bracket       = ']' ;",
            "open-brace          = '{' ;",
            "close-brace         = '}' ;",
            "comma               = ',' ;",
            "term                = ';' ;",
            ];

        string rule = rules.Join(Environment.NewLine);
        var root = MetaParser.ParseRules(rule);
        root.StatusCode.IsOk().Should().BeTrue(root.Error);

        var lines = MetaTestTool.GenerateTestCodeFromProductionRule(root.Rule).Join(Environment.NewLine);

        var tree = new IMetaSyntax[]
        {
            new TerminalSymbol { Name = "number", Text = "[+-]?[0-9]+", Type = TerminalType.Regex },
            new TerminalSymbol { Name = "symbol", Text = "[a-zA-Z][a-zA-Z0-9\\-/]*", Type = TerminalType.Regex },
            new TerminalSymbol { Name = "base64", Text = "string", Type = TerminalType.String },
            new TerminalSymbol { Name = "equal", Text = "=" },
            new TerminalSymbol { Name = "join-left", Text = "->" },
            new TerminalSymbol { Name = "join-inner", Text = "<->" },
            new TerminalSymbol { Name = "node-sym", Text = "node" },
            new TerminalSymbol { Name = "edge-sym", Text = "edge" },
            new TerminalSymbol { Name = "return-sym", Text = "return" },
            new TerminalSymbol { Name = "select-sym", Text = "select" },
            new TerminalSymbol { Name = "delete-sym", Text = "delete" },
            new TerminalSymbol { Name = "update-sym", Text = "update" },
            new TerminalSymbol { Name = "upsert-syn", Text = "upsert" },
            new TerminalSymbol { Name = "add-sym", Text = "add" },
            new TerminalSymbol { Name = "set-sym", Text = "set" },
            new TerminalSymbol { Name = "open-param", Text = "(" },
            new TerminalSymbol { Name = "close-param", Text = ")" },
            new TerminalSymbol { Name = "open-bracket", Text = "[" },
            new TerminalSymbol { Name = "close-bracket", Text = "]" },
            new TerminalSymbol { Name = "open-brace", Text = "{" },
            new TerminalSymbol { Name = "close-brace", Text = "}" },
            new TerminalSymbol { Name = "comma", Text = "," },
            new TerminalSymbol { Name = "term", Text = ";" },
        };

        Enumerable.SequenceEqual(root.Rule.Children, tree).Should().BeTrue();
    }
}