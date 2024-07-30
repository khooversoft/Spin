using FluentAssertions;
using Toolbox.Extensions;
using Toolbox.LangTools;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Test.LangTools.Meta;

public class ProductionRuleTests
{
    [Fact]
    public void NonTerminalRule1()
    {
        string[] rules = [
            "symbol = regex '[a-zA-Z][a-zA-Z0-9\\-/]*' ;",
            "alias = symbol ;",
            ];

        test(rules);

        rules = [
            "symbol=regex'[a-zA-Z][a-zA-Z0-9\\-/]*';",
            "alias=symbol;",
            ];

        test(rules);

        static void test(IEnumerable<string> rules)
        {
            string rule = rules.Join(Environment.NewLine);
            var root = MetaParser.ParseRules(rule);
            root.StatusCode.IsOk().Should().BeTrue();

            root.Rule.Children.Count.Should().Be(2);

            (root.Rule.Children[0] as TerminalSymbol).Action(x =>
            {
                x.NotNull();
                x.Name.Should().Be("symbol");
                x.Text.Should().Be("[a-zA-Z][a-zA-Z0-9\\-/]*");
                x.Type.Should().Be(TerminalType.Regex);
            });

            (root.Rule.Children[1] as ProductionRule).Action(x =>
            {
                x.NotNull();
                x.Name.Should().Be("alias");
                x.Children.Count.Should().Be(1);
                x.Children.OfType<ProductionRuleReference>().First().Action(y =>
                {
                    y.ReferenceSyntax.Should().NotBeNull();
                    y.ReferenceSyntax.Should().Be("symbol");
                });
            });
        }
    }

    [Fact]
    public void NonTerminalRule2()
    {
        string[] rules = [
            "symbol = regex '[a-zA-Z][a-zA-Z0-9\\-/]*' ;",
            "tag = symbol, ['=', symbol ] ;",
            ];

        test(rules);

        rules = [
            "symbol=regex'[a-zA-Z][a-zA-Z0-9\\-/]*';",
            "tag=symbol,['=',symbol];",
            ];

        test(rules);

        static void test(IEnumerable<string> rules)
        {
            string rule = rules.Join(Environment.NewLine);
            var root = MetaParser.ParseRules(rule);
            root.StatusCode.IsOk().Should().BeTrue(root.Error);

            root.Rule.Children.Count.Should().Be(2);

            (root.Rule.Children[0] as TerminalSymbol).Action(x =>
            {
                x.NotNull();
                x.Name.Should().Be("symbol");
                x.Text.Should().Be("[a-zA-Z][a-zA-Z0-9\\-/]*");
                x.Type.Should().Be(TerminalType.Regex);
            });

            (root.Rule.Children[1] as ProductionRule).Action(x =>
            {
                x.NotNull();
                x.Name.Should().Be("tag");
                x.Children.Count.Should().Be(2);
                (x.Children[0] as ProductionRuleReference).Action(y =>
                {
                    y.NotNull();
                    y.Name.Should().Be("_tag-1-symbol");
                    y.ReferenceSyntax.Should().NotBeNull();
                    y.ReferenceSyntax.Should().Be("symbol");
                });
                (x.Children[1] as ProductionRule).Action(y =>
                {
                    y.NotNull();
                    y.Name.Should().Be("_tag-3-OptionGroup");
                    y.Children.Count.Should().Be(2);
                    (y.Children[0] as VirtualTerminalSymbol).Action(z =>
                    {
                        z.NotNull();
                        z.Name.Should().Be("_tag-3-OptionGroup-1");
                        z.Text.Should().Be("=");
                    });
                    (y.Children[1] as ProductionRuleReference).Action(z =>
                    {
                        z.NotNull();
                        z.Name.Should().Be("_tag-3-OptionGroup-3-symbol");
                        z.ReferenceSyntax.Should().NotBeNull();
                        z.ReferenceSyntax.Should().Be("symbol");
                    });
                });
            });
        }
    }

    [Fact]
    public void NonTerminalRule3()
    {
        string[] rules = [
            "symbol = regex '[a-zA-Z][a-zA-Z0-9\\-/]*' ;",
            "comma = ',' ;",
            "tag = symbol, ['=', symbol ] ;",
            "tags = { comma, tag } ;",
            ];

        test(rules);

        static void test(IEnumerable<string> rules)
        {
            string rule = rules.Join(Environment.NewLine);
            var root = MetaParser.ParseRules(rule);
            root.StatusCode.IsOk().Should().BeTrue();

            root.Rule.Children.Count.Should().Be(4);

            (root.Rule.Children[0] as TerminalSymbol).Action(x =>
            {
                x.NotNull();
                x.Name.Should().Be("symbol");
                x.Text.Should().Be("[a-zA-Z][a-zA-Z0-9\\-/]*");
                x.Type.Should().Be(TerminalType.Regex);
            });

            (root.Rule.Children[1] as TerminalSymbol).Action(x =>
            {
                x.NotNull();
                x.Name.Should().Be("comma");
                x.Text.Should().Be(",");
                x.Type.Should().Be(TerminalType.Token);
            });

            (root.Rule.Children[2] as ProductionRule).Action(x =>
            {
                x.NotNull();
                x.Name.Should().Be("tag");
                x.Type.Should().Be(ProductionRuleType.Root);
                x.EvaluationType.Should().Be(EvaluationType.Sequence);
                x.Children.Count.Should().Be(2);
                (x.Children[0] as ProductionRuleReference).Action(y =>
                {
                    y.NotNull();
                    y.Name.Should().Be("_tag-1-symbol");
                    y.ReferenceSyntax.Should().NotBeNull();
                    y.ReferenceSyntax.Should().Be("symbol");
                });
                (x.Children[1] as ProductionRule).Action(y =>
                {
                    y.NotNull();
                    y.Name.Should().Be("_tag-3-OptionGroup");
                    y.Type.Should().Be(ProductionRuleType.Optional);
                    y.EvaluationType.Should().Be(EvaluationType.Sequence);
                    y.Children.Count.Should().Be(2);
                    (y.Children[0] as VirtualTerminalSymbol).Action(z =>
                    {
                        z.NotNull();
                        z.Name.Should().Be("_tag-3-OptionGroup-1");
                        z.Text.Should().Be("=");
                    });
                    (y.Children[1] as ProductionRuleReference).Action(z =>
                    {
                        z.NotNull();
                        z.Name.Should().Be("_tag-3-OptionGroup-3-symbol");
                        z.ReferenceSyntax.Should().NotBeNull();
                        z.ReferenceSyntax.Should().Be("symbol");
                    });
                });
            });

            (root.Rule.Children[3] as ProductionRule).Action(x =>
            {
                x.NotNull();
                x.Name.Should().Be("tags");
                x.Children.Count.Should().Be(1);

                (x.Children[0] as ProductionRule).Action(y =>
                {
                    y.NotNull();
                    y.Name.Should().Be("_tags-1-RepeatGroup");
                    y.Type.Should().Be(ProductionRuleType.Repeat);
                    y.EvaluationType.Should().Be(EvaluationType.Sequence);
                    y.Children.Count.Should().Be(2);

                    (y.Children[0] as ProductionRuleReference).Action(z =>
                    {
                        z.NotNull();
                        z.Name.Should().Be("_tags-1-RepeatGroup-1-comma");
                        z.ReferenceSyntax.Should().NotBeNull();
                        z.ReferenceSyntax.Should().Be("comma");
                    });

                    (y.Children[1] as ProductionRuleReference).Action(z =>
                    {
                        z.NotNull();
                        z.Name.Should().Be("_tags-1-RepeatGroup-3-tag");
                        z.ReferenceSyntax.Should().NotBeNull();
                        z.ReferenceSyntax.Should().Be("tag");
                    });
                });
            });
        }
    }

    [Fact]
    public void OptionalOrRule3()
    {
        string[] rules = [
            "symbol      = regex '[a-zA-Z][a-zA-Z0-9\\-/]*' ;",
            "join-left   = '->' ;",
            "join-inner  = '<->' ;",
            "join        = [ join-left | join-inner | symbol ] ;",
            ];

        test(rules);

        static void test(IEnumerable<string> rules)
        {
            string rule = rules.Join(Environment.NewLine);
            var root = MetaParser.ParseRules(rule);
            root.StatusCode.IsOk().Should().BeTrue();

            root.Rule.Children.Count.Should().Be(4);

            (root.Rule.Children[0] as TerminalSymbol).Action(x =>
            {
                x.NotNull();
                x.Name.Should().Be("symbol");
                x.Text.Should().Be("[a-zA-Z][a-zA-Z0-9\\-/]*");
                x.Type.Should().Be(TerminalType.Regex);
            });

            (root.Rule.Children[1] as TerminalSymbol).Action(x =>
            {
                x.NotNull();
                x.Name.Should().Be("join-left");
                x.Text.Should().Be("->");
                x.Type.Should().Be(TerminalType.Token);
            });

            (root.Rule.Children[2] as TerminalSymbol).Action(x =>
            {
                x.NotNull();
                x.Name.Should().Be("join-inner");
                x.Text.Should().Be("<->");
                x.Type.Should().Be(TerminalType.Token);
            });

            (root.Rule.Children[3] as ProductionRule).Action(x =>
            {
                x.NotNull();
                x.Name.Should().Be("join");
                x.Type.Should().Be(ProductionRuleType.Root);
                x.Children.Count.Should().Be(1);

                (x.Children[0] as ProductionRule).Action(x =>
                {
                    x.NotNull();
                    x.Name.Should().Be("_join-1-OptionGroup");
                    x.Type.Should().Be(ProductionRuleType.Optional);
                    x.EvaluationType.Should().Be(EvaluationType.Or);
                    x.Children.Count.Should().Be(3);

                    (x.Children[0] as ProductionRuleReference).Action(y =>
                    {
                        y.NotNull();
                        y.Name.Should().Be("_join-1-OptionGroup-1-join-left");
                        y.ReferenceSyntax.Should().NotBeNull();
                        y.ReferenceSyntax.Should().Be("join-left");
                    });

                    (x.Children[1] as ProductionRuleReference).Action(y =>
                    {
                        y.NotNull();
                        y.Name.Should().Be("_join-1-OptionGroup-3-join-inner");
                        y.ReferenceSyntax.Should().NotBeNull();
                        y.ReferenceSyntax.Should().Be("join-inner");
                    });

                    (x.Children[2] as ProductionRuleReference).Action(y =>
                    {
                        y.NotNull();
                        y.Name.Should().Be("_join-1-OptionGroup-5-symbol");
                        y.ReferenceSyntax.Should().NotBeNull();
                        y.ReferenceSyntax.Should().Be("symbol");
                    });
                });
            });
        }
    }

    [Fact]
    public void GroupRule1()
    {
        string[] rules = [
            "symbol      = regex '[a-zA-Z][a-zA-Z0-9\\-/]*' ;",
            "join-left   = '->' ;",
            "join-inner  = '<->' ;",
            "group       = ( join-left | join-inner | symbol ) ;",
            ];

        test(rules);

        static void test(IEnumerable<string> rules)
        {
            string rule = rules.Join(Environment.NewLine);
            var root = MetaParser.ParseRules(rule);
            root.StatusCode.IsOk().Should().BeTrue();

            root.Rule.Children.Count.Should().Be(4);

            (root.Rule.Children[0] as TerminalSymbol).Action(x =>
            {
                x.NotNull();
                x.Name.Should().Be("symbol");
                x.Text.Should().Be("[a-zA-Z][a-zA-Z0-9\\-/]*");
                x.Type.Should().Be(TerminalType.Regex);
            });

            (root.Rule.Children[1] as TerminalSymbol).Action(x =>
            {
                x.NotNull();
                x.Name.Should().Be("join-left");
                x.Text.Should().Be("->");
                x.Type.Should().Be(TerminalType.Token);
            });

            (root.Rule.Children[2] as TerminalSymbol).Action(x =>
            {
                x.NotNull();
                x.Name.Should().Be("join-inner");
                x.Text.Should().Be("<->");
                x.Type.Should().Be(TerminalType.Token);
            });

            (root.Rule.Children[3] as ProductionRule).Action(x =>
            {
                x.NotNull();
                x.Name.Should().Be("group");
                x.Type.Should().Be(ProductionRuleType.Root);
                x.Children.Count.Should().Be(1);

                (x.Children[0] as ProductionRule).Action(x =>
                {
                    x.NotNull();
                    x.Name.Should().Be("_group-1-Group");
                    x.Type.Should().Be(ProductionRuleType.Group);
                    x.EvaluationType.Should().Be(EvaluationType.Or);
                    x.Children.Count.Should().Be(3);

                    (x.Children[0] as ProductionRuleReference).Action(y =>
                    {
                        y.NotNull();
                        y.Name.Should().Be("_group-1-Group-1-join-left");
                        y.ReferenceSyntax.Should().NotBeNull();
                        y.ReferenceSyntax.Should().Be("join-left");
                    });

                    (x.Children[1] as ProductionRuleReference).Action(y =>
                    {
                        y.NotNull();
                        y.Name.Should().Be("_group-1-Group-3-join-inner");
                        y.ReferenceSyntax.Should().NotBeNull();
                        y.ReferenceSyntax.Should().Be("join-inner");
                    });

                    (x.Children[2] as ProductionRuleReference).Action(y =>
                    {
                        y.NotNull();
                        y.Name.Should().Be("_group-1-Group-5-symbol");
                        y.ReferenceSyntax.Should().NotBeNull();
                        y.ReferenceSyntax.Should().Be("symbol");
                    });
                });
            });
        }
    }
}
