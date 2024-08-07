using FluentAssertions;
using Toolbox.Extensions;
using Toolbox.LangTools;
using Toolbox.Types;

namespace Toolbox.Test.LangTools.Meta;

public class SampleMetaSyntaxTest
{
    [Fact]
    public void SampleMetaSyntax()
    {
        string metaSyntax = MetaTestTool.ReadGraphLanauge();

        MetaSyntaxRoot root = MetaParser.ParseRules(metaSyntax);
        root.StatusCode.IsOk().Should().BeTrue(root.Error);

        var lines = MetaTestTool.GenerateTestCodeFromProductionRule(root.Rule).Join(Environment.NewLine);

        ExpectedTree().FlattenMatch(root.Rule.Children);

        var expected = ExpectedTree();
        root.Rule.Children.Count.Should().Be(expected.Count);
        Enumerable.SequenceEqual(root.Rule.Children, expected).Should().BeTrue();
    }

    private IReadOnlyList<IMetaSyntax> ExpectedTree()
    {
        var tree = new IMetaSyntax[]
        {
            new TerminalSymbol { Name = "number", Text = "^[+-]?[0-9]+$", Type = TerminalType.Regex },
            new TerminalSymbol { Name = "symbol", Text = "^[a-zA-Z][a-zA-Z0-9\\-]*$", Type = TerminalType.Regex },
            new TerminalSymbol { Name = "base64", Text = "string", Type = TerminalType.String },
            new TerminalSymbol { Name = "equal", Text = "=", Type = TerminalType.Token },
            new TerminalSymbol { Name = "join-left", Text = "->", Type = TerminalType.Token },
            new TerminalSymbol { Name = "join-inner", Text = "<->", Type = TerminalType.Token },
            new TerminalSymbol { Name = "return-sym", Text = "return", Type = TerminalType.Token },
            new TerminalSymbol { Name = "select-sym", Text = "select", Type = TerminalType.Token },
            new TerminalSymbol { Name = "delete-sym", Text = "delete", Type = TerminalType.Token },
            new TerminalSymbol { Name = "update-sym", Text = "update", Type = TerminalType.Token },
            new TerminalSymbol { Name = "upsert-syn", Text = "upsert", Type = TerminalType.Token },
            new TerminalSymbol { Name = "node-sym", Text = "node", Type = TerminalType.Token },
            new TerminalSymbol { Name = "edge-sym", Text = "edge", Type = TerminalType.Token },
            new TerminalSymbol { Name = "add-sym", Text = "add", Type = TerminalType.Token },
            new TerminalSymbol { Name = "set-sym", Text = "set", Type = TerminalType.Token },
            new TerminalSymbol { Name = "open-param", Text = "(", Type = TerminalType.Token, Tags = ["group-start","node"] },
            new TerminalSymbol { Name = "close-param", Text = ")", Type = TerminalType.Token, Tags = ["group-end","node"] },
            new TerminalSymbol { Name = "open-bracket", Text = "[", Type = TerminalType.Token, Tags = ["group-start","edge"] },
            new TerminalSymbol { Name = "close-bracket", Text = "]", Type = TerminalType.Token, Tags = ["group-end","edge"] },
            new TerminalSymbol { Name = "open-brace", Text = "{", Type = TerminalType.Token, Tags = ["group-start","data"] },
            new TerminalSymbol { Name = "close-brace", Text = "}", Type = TerminalType.Token, Tags = ["group-end","data"] },
            new TerminalSymbol { Name = "comma", Text = ",", Type = TerminalType.Token },
            new TerminalSymbol { Name = "term", Text = ";", Type = TerminalType.Token },
            new ProductionRule
            {
                Name = "alias",
                Type = ProductionRuleType.Root,
                EvaluationType = EvaluationType.Sequence,
                Children = new IMetaSyntax[]
                {
                    new ProductionRuleReference { Name = "_alias-1-symbol", ReferenceSyntax = "symbol" },
                },
            },
            new ProductionRule
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
            new ProductionRule
            {
                Name = "tags",
                Type = ProductionRuleType.Root,
                EvaluationType = EvaluationType.Sequence,
                Children = new IMetaSyntax[]
                {
                    new ProductionRuleReference { Name = "_tags-1-tag", ReferenceSyntax = "tag" },
                    new ProductionRule
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
                },
            },
            new ProductionRule
            {
                Name = "return-query",
                Type = ProductionRuleType.Root,
                EvaluationType = EvaluationType.Sequence,
                Children = new IMetaSyntax[]
                {
                    new ProductionRuleReference { Name = "_return-query-1-return-sym", ReferenceSyntax = "return-sym" },
                    new ProductionRuleReference { Name = "_return-query-3-symbol", ReferenceSyntax = "symbol" },
                    new ProductionRule
                    {
                        Name = "_return-query-5-RepeatGroup",
                        Type = ProductionRuleType.Repeat,
                        EvaluationType = EvaluationType.Sequence,
                        Children = new IMetaSyntax[]
                        {
                            new ProductionRuleReference { Name = "_return-query-5-RepeatGroup-1-comma", ReferenceSyntax = "comma" },
                            new ProductionRuleReference { Name = "_return-query-5-RepeatGroup-3-symbol", ReferenceSyntax = "symbol" },
                        },
                    },
                },
            },
            new ProductionRule
            {
                Name = "entity-data",
                Type = ProductionRuleType.Root,
                EvaluationType = EvaluationType.Sequence,
                Children = new IMetaSyntax[]
                {
                    new ProductionRuleReference { Name = "_entity-data-1-symbol", ReferenceSyntax = "symbol" },
                    new ProductionRuleReference { Name = "_entity-data-3-open-brace", ReferenceSyntax = "open-brace" },
                    new ProductionRuleReference { Name = "_entity-data-5-base64", ReferenceSyntax = "base64" },
                    new ProductionRuleReference { Name = "_entity-data-7-close-brace", ReferenceSyntax = "close-brace" },
                },
            },
            new ProductionRule
            {
                Name = "set-cmd",
                Type = ProductionRuleType.Root,
                EvaluationType = EvaluationType.Sequence,
                Children = new IMetaSyntax[]
                {
                    new ProductionRuleReference { Name = "_set-cmd-1-set-sym", ReferenceSyntax = "set-sym" },
                    new ProductionRuleReference { Name = "_set-cmd-3-tags", ReferenceSyntax = "tags" },
                },
            },
            new ProductionRule
            {
                Name = "node-spec",
                Type = ProductionRuleType.Root,
                EvaluationType = EvaluationType.Sequence,
                Children = new IMetaSyntax[]
                {
                    new ProductionRuleReference { Name = "_node-spec-1-open-param", ReferenceSyntax = "open-param" },
                    new ProductionRuleReference { Name = "_node-spec-3-tag", ReferenceSyntax = "tag" },
                    new ProductionRule
                    {
                        Name = "_node-spec-5-RepeatGroup",
                        Type = ProductionRuleType.Repeat,
                        EvaluationType = EvaluationType.Sequence,
                        Children = new IMetaSyntax[]
                        {
                            new ProductionRuleReference { Name = "_node-spec-5-RepeatGroup-1-comma", ReferenceSyntax = "comma" },
                            new ProductionRuleReference { Name = "_node-spec-5-RepeatGroup-3-tag", ReferenceSyntax = "tag" },
                        },
                    },
                    new ProductionRuleReference { Name = "_node-spec-7-close-param", ReferenceSyntax = "close-param" },
                    new ProductionRule
                    {
                        Name = "_node-spec-9-OptionGroup",
                        Type = ProductionRuleType.Optional,
                        EvaluationType = EvaluationType.None,
                        Children = new IMetaSyntax[]
                        {
                            new ProductionRuleReference { Name = "_node-spec-9-OptionGroup-1-alias", ReferenceSyntax = "alias" },
                        },
                    },
                },
            },
            new ProductionRule
            {
                Name = "edge-spec",
                Type = ProductionRuleType.Root,
                EvaluationType = EvaluationType.Sequence,
                Children = new IMetaSyntax[]
                {
                    new ProductionRuleReference { Name = "_edge-spec-1-open-bracket", ReferenceSyntax = "open-bracket" },
                    new ProductionRuleReference { Name = "_edge-spec-3-tag", ReferenceSyntax = "tag" },
                    new ProductionRule
                    {
                        Name = "_edge-spec-5-RepeatGroup",
                        Type = ProductionRuleType.Repeat,
                        EvaluationType = EvaluationType.Sequence,
                        Children = new IMetaSyntax[]
                        {
                            new ProductionRuleReference { Name = "_edge-spec-5-RepeatGroup-1-comma", ReferenceSyntax = "comma" },
                            new ProductionRuleReference { Name = "_edge-spec-5-RepeatGroup-3-tag", ReferenceSyntax = "tag" },
                        },
                    },
                    new ProductionRuleReference { Name = "_edge-spec-7-close-bracket", ReferenceSyntax = "close-bracket" },
                    new ProductionRule
                    {
                        Name = "_edge-spec-9-OptionGroup",
                        Type = ProductionRuleType.Optional,
                        EvaluationType = EvaluationType.None,
                        Children = new IMetaSyntax[]
                        {
                            new ProductionRuleReference { Name = "_edge-spec-9-OptionGroup-1-alias", ReferenceSyntax = "alias" },
                        },
                    },
                },
            },
            new ProductionRule
            {
                Name = "join",
                Type = ProductionRuleType.Root,
                EvaluationType = EvaluationType.Sequence,
                Children = new IMetaSyntax[]
                {
                    new ProductionRule
                    {
                        Name = "_join-1-OptionGroup",
                        Type = ProductionRuleType.Optional,
                        EvaluationType = EvaluationType.Or,
                        Children = new IMetaSyntax[]
                        {
                            new ProductionRuleReference { Name = "_join-1-OptionGroup-1-join-left", ReferenceSyntax = "join-left" },
                            new ProductionRuleReference { Name = "_join-1-OptionGroup-3-join-inner", ReferenceSyntax = "join-inner" },
                        },
                    },
                },
            },
            new ProductionRule
            {
                Name = "select-node-query",
                Type = ProductionRuleType.Root,
                EvaluationType = EvaluationType.Sequence,
                Children = new IMetaSyntax[]
                {
                    new ProductionRuleReference { Name = "_select-node-query-1-node-spec", ReferenceSyntax = "node-spec" },
                    new ProductionRule
                    {
                        Name = "_select-node-query-3-RepeatGroup",
                        Type = ProductionRuleType.Repeat,
                        EvaluationType = EvaluationType.Sequence,
                        Children = new IMetaSyntax[]
                        {
                            new ProductionRuleReference { Name = "_select-node-query-3-RepeatGroup-1-join", ReferenceSyntax = "join" },
                            new ProductionRuleReference { Name = "_select-node-query-3-RepeatGroup-3-edge-spec", ReferenceSyntax = "edge-spec" },
                        },
                    },
                },
            },
            new ProductionRule
            {
                Name = "select-edge-query",
                Type = ProductionRuleType.Root,
                EvaluationType = EvaluationType.Sequence,
                Children = new IMetaSyntax[]
                {
                    new ProductionRuleReference { Name = "_select-edge-query-1-edge-spec", ReferenceSyntax = "edge-spec" },
                    new ProductionRule
                    {
                        Name = "_select-edge-query-3-RepeatGroup",
                        Type = ProductionRuleType.Repeat,
                        EvaluationType = EvaluationType.Sequence,
                        Children = new IMetaSyntax[]
                        {
                            new ProductionRuleReference { Name = "_select-edge-query-3-RepeatGroup-1-join", ReferenceSyntax = "join" },
                            new ProductionRuleReference { Name = "_select-edge-query-3-RepeatGroup-3-node-spec", ReferenceSyntax = "node-spec" },
                        },
                    },
                },
            },
            new ProductionRule
            {
                Name = "addCommand",
                Type = ProductionRuleType.Root,
                EvaluationType = EvaluationType.Sequence,
                Children = new IMetaSyntax[]
                {
                    new ProductionRuleReference { Name = "_addCommand-1-add-sym", ReferenceSyntax = "add-sym" },
                    new ProductionRule
                    {
                        Name = "_addCommand-3-Group",
                        Type = ProductionRuleType.Group,
                        EvaluationType = EvaluationType.Or,
                        Children = new IMetaSyntax[]
                        {
                            new ProductionRuleReference { Name = "_addCommand-3-Group-1-node-sym", ReferenceSyntax = "node-sym" },
                            new ProductionRuleReference { Name = "_addCommand-3-Group-3-edge-sym", ReferenceSyntax = "edge-sym" },
                        },
                    },
                    new ProductionRule
                    {
                        Name = "_addCommand-5-OptionGroup",
                        Type = ProductionRuleType.Optional,
                        EvaluationType = EvaluationType.None,
                        Children = new IMetaSyntax[]
                        {
                            new ProductionRuleReference { Name = "_addCommand-5-OptionGroup-1-tags", ReferenceSyntax = "tags" },
                        },
                    },
                    new ProductionRule
                    {
                        Name = "_addCommand-7-OptionGroup",
                        Type = ProductionRuleType.Optional,
                        EvaluationType = EvaluationType.None,
                        Children = new IMetaSyntax[]
                        {
                            new ProductionRuleReference { Name = "_addCommand-7-OptionGroup-1-entity-data", ReferenceSyntax = "entity-data" },
                        },
                    },
                    new ProductionRuleReference { Name = "_addCommand-9-term", ReferenceSyntax = "term" },
                },
            },
            new ProductionRule
            {
                Name = "updateCommand",
                Type = ProductionRuleType.Root,
                EvaluationType = EvaluationType.Sequence,
                Children = new IMetaSyntax[]
                {
                    new ProductionRuleReference { Name = "_updateCommand-1-update-sym", ReferenceSyntax = "update-sym" },
                    new ProductionRule
                    {
                        Name = "_updateCommand-3-Group",
                        Type = ProductionRuleType.Group,
                        EvaluationType = EvaluationType.Or,
                        Children = new IMetaSyntax[]
                        {
                            new ProductionRuleReference { Name = "_updateCommand-3-Group-1-node-sym", ReferenceSyntax = "node-sym" },
                            new ProductionRuleReference { Name = "_updateCommand-3-Group-3-edge-sym", ReferenceSyntax = "edge-sym" },
                        },
                    },
                    new ProductionRule
                    {
                        Name = "_updateCommand-5-Group",
                        Type = ProductionRuleType.Group,
                        EvaluationType = EvaluationType.Or,
                        Children = new IMetaSyntax[]
                        {
                            new ProductionRuleReference { Name = "_updateCommand-5-Group-1-select-node-query", ReferenceSyntax = "select-node-query" },
                            new ProductionRuleReference { Name = "_updateCommand-5-Group-3-select-edge-query", ReferenceSyntax = "select-edge-query" },
                        },
                    },
                    new ProductionRuleReference { Name = "_updateCommand-7-set-cmd", ReferenceSyntax = "set-cmd" },
                    new ProductionRuleReference { Name = "_updateCommand-9-term", ReferenceSyntax = "term" },
                },
            },
            new ProductionRule
            {
                Name = "deleteCommand",
                Type = ProductionRuleType.Root,
                EvaluationType = EvaluationType.Sequence,
                Children = new IMetaSyntax[]
                {
                    new ProductionRuleReference { Name = "_deleteCommand-1-delete-sym", ReferenceSyntax = "delete-sym" },
                    new ProductionRule
                    {
                        Name = "_deleteCommand-3-Group",
                        Type = ProductionRuleType.Group,
                        EvaluationType = EvaluationType.Or,
                        Children = new IMetaSyntax[]
                        {
                            new ProductionRuleReference { Name = "_deleteCommand-3-Group-1-node-sym", ReferenceSyntax = "node-sym" },
                            new ProductionRuleReference { Name = "_deleteCommand-3-Group-3-edge-sym", ReferenceSyntax = "edge-sym" },
                        },
                    },
                    new ProductionRule
                    {
                        Name = "_deleteCommand-5-Group",
                        Type = ProductionRuleType.Group,
                        EvaluationType = EvaluationType.Or,
                        Children = new IMetaSyntax[]
                        {
                            new ProductionRuleReference { Name = "_deleteCommand-5-Group-1-select-node-query", ReferenceSyntax = "select-node-query" },
                            new ProductionRuleReference { Name = "_deleteCommand-5-Group-3-select-edge-query", ReferenceSyntax = "select-edge-query" },
                        },
                    },
                    new ProductionRuleReference { Name = "_deleteCommand-7-term", ReferenceSyntax = "term" },
                },
            },
            new ProductionRule
            {
                Name = "selectCommand",
                Type = ProductionRuleType.Root,
                EvaluationType = EvaluationType.Sequence,
                Children = new IMetaSyntax[]
                {
                    new ProductionRuleReference { Name = "_selectCommand-1-select-sym", ReferenceSyntax = "select-sym" },
                    new ProductionRule
                    {
                        Name = "_selectCommand-3-Group",
                        Type = ProductionRuleType.Group,
                        EvaluationType = EvaluationType.Or,
                        Children = new IMetaSyntax[]
                        {
                            new ProductionRuleReference { Name = "_selectCommand-3-Group-1-node-sym", ReferenceSyntax = "node-sym" },
                            new ProductionRuleReference { Name = "_selectCommand-3-Group-3-edge-sym", ReferenceSyntax = "edge-sym" },
                        },
                    },
                    new ProductionRule
                    {
                        Name = "_selectCommand-5-Group",
                        Type = ProductionRuleType.Group,
                        EvaluationType = EvaluationType.Or,
                        Children = new IMetaSyntax[]
                        {
                            new ProductionRuleReference { Name = "_selectCommand-5-Group-1-select-node-query", ReferenceSyntax = "select-node-query" },
                            new ProductionRuleReference { Name = "_selectCommand-5-Group-3-select-edge-query", ReferenceSyntax = "select-edge-query" },
                        },
                    },
                    new ProductionRule
                    {
                        Name = "_selectCommand-7-OptionGroup",
                        Type = ProductionRuleType.Optional,
                        EvaluationType = EvaluationType.None,
                        Children = new IMetaSyntax[]
                        {
                            new ProductionRuleReference { Name = "_selectCommand-7-OptionGroup-1-return-query", ReferenceSyntax = "return-query" },
                        },
                    },
                    new ProductionRuleReference { Name = "_selectCommand-9-term", ReferenceSyntax = "term" },
                },
            },
        };



        return tree;
    }
}
