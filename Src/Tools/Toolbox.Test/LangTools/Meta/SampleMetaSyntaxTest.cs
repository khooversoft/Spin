using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Toolbox.LangTools;
using Toolbox.Types;
using Toolbox.Extensions;

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

        CompareToExpected(root, ExpectedTree());
    }

    private TreeNode<IMetaSyntax> ExpectedTree()
    {
        var tree = new TreeNode<IMetaSyntax>
        {
            new TerminalSymbol { Name = "number", Text = "[+-]?[0-9]+", Regex = true },
            new TerminalSymbol { Name = "symbol", Text = "[a-zA-Z][a-zA-Z0-9\-/]*", Regex = true },
            new TerminalSymbol { Name = "base64", Text = "[a-zA-Z][a-zA-Z0-9\-/]*", Regex = true },
            new TerminalSymbol { Name = "equal", Text = "=", Regex = false },
            new TerminalSymbol { Name = "join-left", Text = "->", Regex = false },
            new TerminalSymbol { Name = "join-inner", Text = "<->", Regex = false },
            new TerminalSymbol { Name = "node-sym", Text = "node", Regex = false },
            new TerminalSymbol { Name = "edge-sym", Text = "edge", Regex = false },
            new TerminalSymbol { Name = "return-sym", Text = "return", Regex = false },
            new TerminalSymbol { Name = "select-sym", Text = "select", Regex = false },
            new TerminalSymbol { Name = "delete-sym", Text = "delete", Regex = false },
            new TerminalSymbol { Name = "update-sym", Text = "update", Regex = false },
            new TerminalSymbol { Name = "upsert-syn", Text = "upsert", Regex = false },
            new TerminalSymbol { Name = "add-sym", Text = "add", Regex = false },
            new TerminalSymbol { Name = "set-sym", Text = "set", Regex = false },
            new TerminalSymbol { Name = "open-param", Text = "(", Regex = false },
            new TerminalSymbol { Name = "close-param", Text = ")", Regex = false },
            new TerminalSymbol { Name = "open-bracket", Text = "[", Regex = false },
            new TerminalSymbol { Name = "close-bracket", Text = "]", Regex = false },
            new TerminalSymbol { Name = "open-brace", Text = "{", Regex = false },
            new TerminalSymbol { Name = "close-brace", Text = "}", Regex = false },
            new TerminalSymbol { Name = "comma", Text = ",", Regex = false },
            new TerminalSymbol { Name = "term", Text = ";", Regex = false },
            new ProductionRule
            {
                Name = "alias",
                Type = ProductionRuleType.Root,
                EvaluationType = EvaluationType.Sequence,
                Children = new IMetaSyntax[]
                {
                    new ProductionRuleReference { Name = "_alias-1-symbol", ReferenceSyntax = new TerminalSymbol() },
                },
            },
            new ProductionRule
            {
                Name = "tag",
                Type = ProductionRuleType.Root,
                EvaluationType = EvaluationType.Sequence,
                Children = new IMetaSyntax[]
                {
                    new ProductionRuleReference { Name = "_tag-1-symbol", ReferenceSyntax = new TerminalSymbol() },
                    new ProductionRule
                    {
                        Name = "_tag-3-OptionGroup",
                        Type = ProductionRuleType.Optional,
                        EvaluationType = EvaluationType.Sequence,
                        Children = new IMetaSyntax[]
                        {
                            new VirtualTerminalSymbol { Name = "_tag-3-OptionGroup-1", Text = "=" },
                            new ProductionRuleReference { Name = "_tag-3-OptionGroup-3-symbol", ReferenceSyntax = new TerminalSymbol() },
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
                    new ProductionRule
                    {
                        Name = "_tags-1-RepeatGroup",
                        Type = ProductionRuleType.Repeat,
                        EvaluationType = EvaluationType.Sequence,
                        Children = new IMetaSyntax[]
                        {
                            new ProductionRuleReference { Name = "_tags-1-RepeatGroup-1-comma", ReferenceSyntax = new TerminalSymbol() },
                            new ProductionRuleReference { Name = "_tags-1-RepeatGroup-3-tag", ReferenceSyntax = new TerminalSymbol() },
                        },
                    },
                },
            },
            new ProductionRule
            {
                Name = "node-spec",
                Type = ProductionRuleType.Root,
                EvaluationType = EvaluationType.Sequence,
                Children = new IMetaSyntax[]
                {
                    new ProductionRuleReference { Name = "_node-spec-1-open-param", ReferenceSyntax = new TerminalSymbol() },
                    new ProductionRuleReference { Name = "_node-spec-3-tag", ReferenceSyntax = new TerminalSymbol() },
                    new ProductionRule
                    {
                        Name = "_node-spec-5-RepeatGroup",
                        Type = ProductionRuleType.Repeat,
                        EvaluationType = EvaluationType.Sequence,
                        Children = new IMetaSyntax[]
                        {
                            new ProductionRuleReference { Name = "_node-spec-5-RepeatGroup-1-comma", ReferenceSyntax = new TerminalSymbol() },
                            new ProductionRuleReference { Name = "_node-spec-5-RepeatGroup-3-tag", ReferenceSyntax = new TerminalSymbol() },
                        },
                    },
                    new ProductionRuleReference { Name = "_node-spec-7-close-param", ReferenceSyntax = new TerminalSymbol() },
                    new ProductionRule
                    {
                        Name = "_node-spec-9-OptionGroup",
                        Type = ProductionRuleType.Optional,
                        EvaluationType = EvaluationType.None,
                        Children = new IMetaSyntax[]
                        {
                            new ProductionRuleReference { Name = "_node-spec-9-OptionGroup-1-symbol", ReferenceSyntax = new TerminalSymbol() },
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
                    new ProductionRuleReference { Name = "_edge-spec-1-open-bracket", ReferenceSyntax = new TerminalSymbol() },
                    new ProductionRuleReference { Name = "_edge-spec-3-tag", ReferenceSyntax = new TerminalSymbol() },
                    new ProductionRule
                    {
                        Name = "_edge-spec-5-RepeatGroup",
                        Type = ProductionRuleType.Repeat,
                        EvaluationType = EvaluationType.Sequence,
                        Children = new IMetaSyntax[]
                        {
                            new ProductionRuleReference { Name = "_edge-spec-5-RepeatGroup-1-comma", ReferenceSyntax = new TerminalSymbol() },
                            new ProductionRuleReference { Name = "_edge-spec-5-RepeatGroup-3-tag", ReferenceSyntax = new TerminalSymbol() },
                        },
                    },
                    new ProductionRuleReference { Name = "_edge-spec-7-close-bracket", ReferenceSyntax = new TerminalSymbol() },
                    new ProductionRule
                    {
                        Name = "_edge-spec-9-OptionGroup",
                        Type = ProductionRuleType.Optional,
                        EvaluationType = EvaluationType.None,
                        Children = new IMetaSyntax[]
                        {
                            new ProductionRuleReference { Name = "_edge-spec-9-OptionGroup-1-symbol", ReferenceSyntax = new TerminalSymbol() },
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
                            new ProductionRuleReference { Name = "_join-1-OptionGroup-1-join-left", ReferenceSyntax = new TerminalSymbol() },
                            new ProductionRuleReference { Name = "_join-1-OptionGroup-3-join-inner", ReferenceSyntax = new TerminalSymbol() },
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
                    new ProductionRuleReference { Name = "_return-query-1-return-sym", ReferenceSyntax = new TerminalSymbol() },
                    new ProductionRuleReference { Name = "_return-query-3-symbol", ReferenceSyntax = new TerminalSymbol() },
                    new ProductionRule
                    {
                        Name = "_return-query-5-RepeatGroup",
                        Type = ProductionRuleType.Repeat,
                        EvaluationType = EvaluationType.Sequence,
                        Children = new IMetaSyntax[]
                        {
                            new ProductionRuleReference { Name = "_return-query-5-RepeatGroup-1-comma", ReferenceSyntax = new TerminalSymbol() },
                            new ProductionRuleReference { Name = "_return-query-5-RepeatGroup-3-symbol", ReferenceSyntax = new TerminalSymbol() },
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
                    new ProductionRuleReference { Name = "_entity-data-1-symbol", ReferenceSyntax = new TerminalSymbol() },
                    new ProductionRuleReference { Name = "_entity-data-3-open-brace", ReferenceSyntax = new TerminalSymbol() },
                    new ProductionRuleReference { Name = "_entity-data-5-base64", ReferenceSyntax = new TerminalSymbol() },
                    new ProductionRuleReference { Name = "_entity-data-7-close-brace", ReferenceSyntax = new TerminalSymbol() },
                },
            },
            new ProductionRule
            {
                Name = "set-cmd",
                Type = ProductionRuleType.Root,
                EvaluationType = EvaluationType.Sequence,
                Children = new IMetaSyntax[]
                {
                    new ProductionRuleReference { Name = "_set-cmd-1-set-sym", ReferenceSyntax = new TerminalSymbol() },
                    new ProductionRuleReference { Name = "_set-cmd-3-tag", ReferenceSyntax = new TerminalSymbol() },
                    new ProductionRule
                    {
                        Name = "_set-cmd-5-RepeatGroup",
                        Type = ProductionRuleType.Repeat,
                        EvaluationType = EvaluationType.Sequence,
                        Children = new IMetaSyntax[]
                        {
                            new ProductionRuleReference { Name = "_set-cmd-5-RepeatGroup-1-comma", ReferenceSyntax = new TerminalSymbol() },
                            new ProductionRuleReference { Name = "_set-cmd-5-RepeatGroup-3-tag", ReferenceSyntax = new TerminalSymbol() },
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
                    new ProductionRuleReference { Name = "_select-node-query-1-node-spec", ReferenceSyntax = new TerminalSymbol() },
                    new ProductionRule
                    {
                        Name = "_select-node-query-3-RepeatGroup",
                        Type = ProductionRuleType.Repeat,
                        EvaluationType = EvaluationType.Sequence,
                        Children = new IMetaSyntax[]
                        {
                            new ProductionRuleReference { Name = "_select-node-query-3-RepeatGroup-1-join", ReferenceSyntax = new TerminalSymbol() },
                            new ProductionRuleReference { Name = "_select-node-query-3-RepeatGroup-3-edge-spec", ReferenceSyntax = new TerminalSymbol() },
                        },
                    },
                },
            },
            new ProductionRule
            {
                Name = "edge-node-query",
                Type = ProductionRuleType.Root,
                EvaluationType = EvaluationType.Sequence,
                Children = new IMetaSyntax[]
                {
                    new ProductionRuleReference { Name = "_edge-node-query-1-edge-spec", ReferenceSyntax = new TerminalSymbol() },
                    new ProductionRule
                    {
                        Name = "_edge-node-query-3-RepeatGroup",
                        Type = ProductionRuleType.Repeat,
                        EvaluationType = EvaluationType.Sequence,
                        Children = new IMetaSyntax[]
                        {
                            new ProductionRuleReference { Name = "_edge-node-query-3-RepeatGroup-1-join", ReferenceSyntax = new TerminalSymbol() },
                            new ProductionRuleReference { Name = "_edge-node-query-3-RepeatGroup-3-node-spec", ReferenceSyntax = new TerminalSymbol() },
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
                    new ProductionRuleReference { Name = "_addCommand-1-add-sym", ReferenceSyntax = new TerminalSymbol() },
                    new ProductionRule
                    {
                        Name = "_addCommand-3-Group",
                        Type = ProductionRuleType.Group,
                        EvaluationType = EvaluationType.Or,
                        Children = new IMetaSyntax[]
                        {
                            new ProductionRuleReference { Name = "_addCommand-3-Group-1-node-sym", ReferenceSyntax = new TerminalSymbol() },
                            new ProductionRuleReference { Name = "_addCommand-3-Group-3-edge-sym", ReferenceSyntax = new TerminalSymbol() },
                        },
                    },
                    new ProductionRule
                    {
                        Name = "_addCommand-5-RepeatGroup",
                        Type = ProductionRuleType.Repeat,
                        EvaluationType = EvaluationType.Sequence,
                        Children = new IMetaSyntax[]
                        {
                            new ProductionRuleReference { Name = "_addCommand-5-RepeatGroup-1-comma", ReferenceSyntax = new TerminalSymbol() },
                            new ProductionRuleReference { Name = "_addCommand-5-RepeatGroup-3-tag", ReferenceSyntax = new TerminalSymbol() },
                        },
                    },
                    new ProductionRule
                    {
                        Name = "_addCommand-7-OptionGroup",
                        Type = ProductionRuleType.Optional,
                        EvaluationType = EvaluationType.None,
                        Children = new IMetaSyntax[]
                        {
                            new ProductionRuleReference { Name = "_addCommand-7-OptionGroup-1-entity-data", ReferenceSyntax = new TerminalSymbol() },
                        },
                    },
                    new ProductionRuleReference { Name = "_addCommand-9-term", ReferenceSyntax = new TerminalSymbol() },
                },
            },
            new ProductionRule
            {
                Name = "updateCommand",
                Type = ProductionRuleType.Root,
                EvaluationType = EvaluationType.Sequence,
                Children = new IMetaSyntax[]
                {
                    new ProductionRuleReference { Name = "_updateCommand-1-update-sym", ReferenceSyntax = new TerminalSymbol() },
                    new ProductionRule
                    {
                        Name = "_updateCommand-3-Group",
                        Type = ProductionRuleType.Group,
                        EvaluationType = EvaluationType.Or,
                        Children = new IMetaSyntax[]
                        {
                            new ProductionRuleReference { Name = "_updateCommand-3-Group-1-node-sym", ReferenceSyntax = new TerminalSymbol() },
                            new ProductionRuleReference { Name = "_updateCommand-3-Group-3-edge-sym", ReferenceSyntax = new TerminalSymbol() },
                        },
                    },
                    new ProductionRule
                    {
                        Name = "_updateCommand-5-Group",
                        Type = ProductionRuleType.Group,
                        EvaluationType = EvaluationType.Or,
                        Children = new IMetaSyntax[]
                        {
                            new ProductionRuleReference { Name = "_updateCommand-5-Group-1-select-node-query", ReferenceSyntax = new TerminalSymbol() },
                            new ProductionRuleReference { Name = "_updateCommand-5-Group-3-edge-node-query", ReferenceSyntax = new TerminalSymbol() },
                        },
                    },
                    new ProductionRuleReference { Name = "_updateCommand-7-set-cmd", ReferenceSyntax = new TerminalSymbol() },
                    new ProductionRuleReference { Name = "_updateCommand-9-term", ReferenceSyntax = new TerminalSymbol() },
                },
            },
            new ProductionRule
            {
                Name = "deleteCommand",
                Type = ProductionRuleType.Root,
                EvaluationType = EvaluationType.Sequence,
                Children = new IMetaSyntax[]
                {
                    new ProductionRuleReference { Name = "_deleteCommand-1-delete-sym", ReferenceSyntax = new TerminalSymbol() },
                    new ProductionRule
                    {
                        Name = "_deleteCommand-3-Group",
                        Type = ProductionRuleType.Group,
                        EvaluationType = EvaluationType.Or,
                        Children = new IMetaSyntax[]
                        {
                            new ProductionRuleReference { Name = "_deleteCommand-3-Group-1-node-sym", ReferenceSyntax = new TerminalSymbol() },
                            new ProductionRuleReference { Name = "_deleteCommand-3-Group-3-edge-sym", ReferenceSyntax = new TerminalSymbol() },
                        },
                    },
                    new ProductionRule
                    {
                        Name = "_deleteCommand-5-Group",
                        Type = ProductionRuleType.Group,
                        EvaluationType = EvaluationType.Or,
                        Children = new IMetaSyntax[]
                        {
                            new ProductionRuleReference { Name = "_deleteCommand-5-Group-1-select-node-query", ReferenceSyntax = new TerminalSymbol() },
                            new ProductionRuleReference { Name = "_deleteCommand-5-Group-3-edge-node-query", ReferenceSyntax = new TerminalSymbol() },
                        },
                    },
                    new ProductionRuleReference { Name = "_deleteCommand-7-term", ReferenceSyntax = new TerminalSymbol() },
                },
            },
            new ProductionRule
            {
                Name = "selectCommand",
                Type = ProductionRuleType.Root,
                EvaluationType = EvaluationType.Sequence,
                Children = new IMetaSyntax[]
                {
                    new ProductionRuleReference { Name = "_selectCommand-1-select-sym", ReferenceSyntax = new TerminalSymbol() },
                    new ProductionRule
                    {
                        Name = "_selectCommand-3-Group",
                        Type = ProductionRuleType.Group,
                        EvaluationType = EvaluationType.Or,
                        Children = new IMetaSyntax[]
                        {
                            new ProductionRuleReference { Name = "_selectCommand-3-Group-1-node-sym", ReferenceSyntax = new TerminalSymbol() },
                            new ProductionRuleReference { Name = "_selectCommand-3-Group-3-edge-sym", ReferenceSyntax = new TerminalSymbol() },
                        },
                    },
                    new ProductionRule
                    {
                        Name = "_selectCommand-5-Group",
                        Type = ProductionRuleType.Group,
                        EvaluationType = EvaluationType.Or,
                        Children = new IMetaSyntax[]
                        {
                            new ProductionRuleReference { Name = "_selectCommand-5-Group-1-select-node-query", ReferenceSyntax = new TerminalSymbol() },
                            new ProductionRuleReference { Name = "_selectCommand-5-Group-3-edge-node-query", ReferenceSyntax = new TerminalSymbol() },
                        },
                    },
                    new ProductionRule
                    {
                        Name = "_selectCommand-7-OptionGroup",
                        Type = ProductionRuleType.Optional,
                        EvaluationType = EvaluationType.None,
                        Children = new IMetaSyntax[]
                        {
                            new ProductionRuleReference { Name = "_selectCommand-7-OptionGroup-1-return-query", ReferenceSyntax = new TerminalSymbol() },
                        },
                    },
                    new ProductionRuleReference { Name = "_selectCommand-9-term", ReferenceSyntax = new TerminalSymbol() },
                },
            },
        };

        return tree;
    }

    private void CompareToExpected(MetaSyntaxRoot root, TreeNode<IMetaSyntax> expectedTree)
    {
        CompareChildren(root.Rule.Children, expectedTree.Children);
    }

    void CompareChildren(IReadOnlyList<IMetaSyntax> ruleChildren, IReadOnlyList<TreeNode<IMetaSyntax>> expectedChildren)
    {
        ruleChildren.Count.Should().Be(expectedChildren.Count);
    }
}
