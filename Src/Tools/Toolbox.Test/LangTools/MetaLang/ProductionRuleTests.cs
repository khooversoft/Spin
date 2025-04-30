using Toolbox.Extensions;
using Toolbox.LangTools;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Test.LangTools.Meta;

public class ProductionRuleTests
{
    [Fact]
    public void MatchingRule()
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
            root.StatusCode.IsOk().BeTrue();

            root.Rule.Children.Count.Be(2);

            (root.Rule.Children[0] as TerminalSymbol).Action(x =>
            {
                x.NotNull();
                x.Name.Be("symbol");
                x.Text.Be("[a-zA-Z][a-zA-Z0-9\\-/]*");
                x.Type.Be(TerminalType.Regex);
            });

            (root.Rule.Children[1] as TerminalSymbol).Action(x =>
            {
                x.NotNull();
                x.Name.Be("alias");
                x.Text.Be("[a-zA-Z][a-zA-Z0-9\\-/]*");
            });
        }
    }

    [Fact]
    public void NonTerminalRule2()
    {
        string[] rules = [
            "symbol = regex '[a-zA-Z][a-zA-Z0-9\\-/]*' ;",
            "tag = symbol, [ '=', symbol ] ;",
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
            root.StatusCode.IsOk().BeTrue(root.Error);

            root.Rule.Children.Count.Be(2);

            (root.Rule.Children[0] as TerminalSymbol).Action(x =>
            {
                x.NotNull();
                x.Name.Be("symbol");
                x.Text.Be("[a-zA-Z][a-zA-Z0-9\\-/]*");
                x.Type.Be(TerminalType.Regex);
            });

            (root.Rule.Children[1] as ProductionRule).Action(x =>
            {
                x.NotNull();
                x.Name.Be("tag");
                x.Children.Count.Be(2);
                (x.Children[0] as ProductionRuleReference).Action(y =>
                {
                    y.NotNull();
                    y.Name.Be("_tag-1-symbol");
                    y.ReferenceSyntax.NotNull();
                    y.ReferenceSyntax.Be("symbol");
                });
                (x.Children[1] as ProductionRule).Action(y =>
                {
                    y.NotNull();
                    y.Name.Be("_tag-3-OptionGroup");
                    y.Children.Count.Be(2);
                    (y.Children[0] as VirtualTerminalSymbol).Action(z =>
                    {
                        z.NotNull();
                        z.Name.Be("_tag-3-OptionGroup-1");
                        z.Text.Be("=");
                    });
                    (y.Children[1] as ProductionRuleReference).Action(z =>
                    {
                        z.NotNull();
                        z.Name.Be("_tag-3-OptionGroup-3-symbol");
                        z.ReferenceSyntax.NotNull();
                        z.ReferenceSyntax.Be("symbol");
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
            root.StatusCode.IsOk().BeTrue();

            root.Rule.Children.Count.Be(4);

            (root.Rule.Children[0] as TerminalSymbol).Action(x =>
            {
                x.NotNull();
                x.Name.Be("symbol");
                x.Text.Be("[a-zA-Z][a-zA-Z0-9\\-/]*");
                x.Type.Be(TerminalType.Regex);
            });

            (root.Rule.Children[1] as TerminalSymbol).Action(x =>
            {
                x.NotNull();
                x.Name.Be("comma");
                x.Text.Be(",");
                x.Type.Be(TerminalType.Token);
            });

            (root.Rule.Children[2] as ProductionRule).Action(x =>
            {
                x.NotNull();
                x.Name.Be("tag");
                x.Type.Be(ProductionRuleType.Sequence);
                x.Children.Count.Be(2);
                (x.Children[0] as ProductionRuleReference).Action(y =>
                {
                    y.NotNull();
                    y.Name.Be("_tag-1-symbol");
                    y.ReferenceSyntax.NotNull();
                    y.ReferenceSyntax.Be("symbol");
                });
                (x.Children[1] as ProductionRule).Action(y =>
                {
                    y.NotNull();
                    y.Name.Be("_tag-3-OptionGroup");
                    y.Type.Be(ProductionRuleType.Optional);
                    y.Children.Count.Be(2);
                    (y.Children[0] as VirtualTerminalSymbol).Action(z =>
                    {
                        z.NotNull();
                        z.Name.Be("_tag-3-OptionGroup-1");
                        z.Text.Be("=");
                    });
                    (y.Children[1] as ProductionRuleReference).Action(z =>
                    {
                        z.NotNull();
                        z.Name.Be("_tag-3-OptionGroup-3-symbol");
                        z.ReferenceSyntax.NotNull();
                        z.ReferenceSyntax.Be("symbol");
                    });
                });
            });

            (root.Rule.Children[3] as ProductionRule).Action(x =>
            {
                x.NotNull();
                x.Name.Be("tags");
                x.Children.Count.Be(1);

                (x.Children[0] as ProductionRule).Action(y =>
                {
                    y.NotNull();
                    y.Name.Be("_tags-1-RepeatGroup");
                    y.Type.Be(ProductionRuleType.Repeat);
                    y.Children.Count.Be(2);

                    (y.Children[0] as ProductionRuleReference).Action(z =>
                    {
                        z.NotNull();
                        z.Name.Be("_tags-1-RepeatGroup-1-comma");
                        z.ReferenceSyntax.NotNull();
                        z.ReferenceSyntax.Be("comma");
                    });

                    (y.Children[1] as ProductionRuleReference).Action(z =>
                    {
                        z.NotNull();
                        z.Name.Be("_tags-1-RepeatGroup-3-tag");
                        z.ReferenceSyntax.NotNull();
                        z.ReferenceSyntax.Be("tag");
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
            "join        = [ join-left, join-inner, symbol ] ;",
            ];

        test(rules);

        static void test(IEnumerable<string> rules)
        {
            string rule = rules.Join(Environment.NewLine);
            var root = MetaParser.ParseRules(rule);
            root.StatusCode.IsOk().BeTrue();

            root.Rule.Children.Count.Be(4);

            (root.Rule.Children[0] as TerminalSymbol).Action(x =>
            {
                x.NotNull();
                x.Name.Be("symbol");
                x.Text.Be("[a-zA-Z][a-zA-Z0-9\\-/]*");
                x.Type.Be(TerminalType.Regex);
            });

            (root.Rule.Children[1] as TerminalSymbol).Action(x =>
            {
                x.NotNull();
                x.Name.Be("join-left");
                x.Text.Be("->");
                x.Type.Be(TerminalType.Token);
            });

            (root.Rule.Children[2] as TerminalSymbol).Action(x =>
            {
                x.NotNull();
                x.Name.Be("join-inner");
                x.Text.Be("<->");
                x.Type.Be(TerminalType.Token);
            });

            (root.Rule.Children[3] as ProductionRule).Action(x =>
            {
                x.NotNull();
                x.Name.Be("join");
                x.Type.Be(ProductionRuleType.Sequence);
                x.Children.Count.Be(1);

                (x.Children[0] as ProductionRule).Action(x =>
                {
                    x.NotNull();
                    x.Name.Be("_join-1-OptionGroup");
                    x.Type.Be(ProductionRuleType.Optional);
                    x.Children.Count.Be(3);

                    (x.Children[0] as ProductionRuleReference).Action(y =>
                    {
                        y.NotNull();
                        y.Name.Be("_join-1-OptionGroup-1-join-left");
                        y.ReferenceSyntax.NotNull();
                        y.ReferenceSyntax.Be("join-left");
                    });

                    (x.Children[1] as ProductionRuleReference).Action(y =>
                    {
                        y.NotNull();
                        y.Name.Be("_join-1-OptionGroup-3-join-inner");
                        y.ReferenceSyntax.NotNull();
                        y.ReferenceSyntax.Be("join-inner");
                    });

                    (x.Children[2] as ProductionRuleReference).Action(y =>
                    {
                        y.NotNull();
                        y.Name.Be("_join-1-OptionGroup-5-symbol");
                        y.ReferenceSyntax.NotNull();
                        y.ReferenceSyntax.Be("symbol");
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
            root.StatusCode.IsOk().BeTrue();

            root.Rule.Children.Count.Be(4);

            (root.Rule.Children[0] as TerminalSymbol).Action(x =>
            {
                x.NotNull();
                x.Name.Be("symbol");
                x.Text.Be("[a-zA-Z][a-zA-Z0-9\\-/]*");
                x.Type.Be(TerminalType.Regex);
            });

            (root.Rule.Children[1] as TerminalSymbol).Action(x =>
            {
                x.NotNull();
                x.Name.Be("join-left");
                x.Text.Be("->");
                x.Type.Be(TerminalType.Token);
            });

            (root.Rule.Children[2] as TerminalSymbol).Action(x =>
            {
                x.NotNull();
                x.Name.Be("join-inner");
                x.Text.Be("<->");
                x.Type.Be(TerminalType.Token);
            });

            (root.Rule.Children[3] as ProductionRule).Action(x =>
            {
                x.NotNull();
                x.Name.Be("group");
                x.Type.Be(ProductionRuleType.Sequence);
                x.Children.Count.Be(1);

                (x.Children[0] as ProductionRule).Action(x =>
                {
                    x.NotNull();
                    x.Name.Be("_group-1-OrGroup");
                    x.Type.Be(ProductionRuleType.Or);
                    x.Children.Count.Be(3);

                    (x.Children[0] as ProductionRuleReference).Action(y =>
                    {
                        y.NotNull();
                        y.Name.Be("_group-1-OrGroup-1-join-left");
                        y.ReferenceSyntax.NotNull();
                        y.ReferenceSyntax.Be("join-left");
                    });

                    (x.Children[1] as ProductionRuleReference).Action(y =>
                    {
                        y.NotNull();
                        y.Name.Be("_group-1-OrGroup-3-join-inner");
                        y.ReferenceSyntax.NotNull();
                        y.ReferenceSyntax.Be("join-inner");
                    });

                    (x.Children[2] as ProductionRuleReference).Action(y =>
                    {
                        y.NotNull();
                        y.Name.Be("_group-1-OrGroup-5-symbol");
                        y.ReferenceSyntax.NotNull();
                        y.ReferenceSyntax.Be("symbol");
                    });
                });
            });
        }
    }
}
