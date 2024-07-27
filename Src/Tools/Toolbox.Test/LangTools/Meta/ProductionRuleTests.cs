using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            "symbol = regex: '[a-zA-Z][a-zA-Z0-9\\-/]*' ;",
            "alias = symbol ;",
            ];

        test(rules);

        rules = [
            "symbol=regex:'[a-zA-Z][a-zA-Z0-9\\-/]*';",
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
                x.Regex.Should().BeTrue();
            });

            (root.Rule.Children[1] as ProductionRule).Action(x =>
            {
                x.NotNull();
                x.Name.Should().Be("alias");
                x.Children.Count.Should().Be(1);
                x.Children.OfType<ProductionRuleReference>().First().Action(y =>
                {
                    y.ReferenceSyntax.Should().NotBeNull();
                    y.ReferenceSyntax.Name.Should().Be("symbol");
                });
            });
        }
    }
    
    [Fact]
    public void NonTerminalRule2()
    {
        string[] rules = [
            "symbol = regex: '[a-zA-Z][a-zA-Z0-9\\-/]*' ;",
            "tag = symbol, ['=', symbol ] ;",
            ];

        test(rules);

        rules = [
            "symbol=regex:'[a-zA-Z][a-zA-Z0-9\\-/]*';",
            "tag=symbol,['=',symbol];",
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
                x.Regex.Should().BeTrue();
            });

            (root.Rule.Children[1] as ProductionRule).Action(x =>
            {
                x.NotNull();
                x.Name.Should().Be("tag");
                x.Children.Count.Should().Be(2);
                (x.Children[0] as ProductionRuleReference).Action(y =>
                {
                    y.NotNull();
                    y.Name.Should().Be("_tag-1");
                    y.ReferenceSyntax.Should().NotBeNull();
                    y.ReferenceSyntax.Name.Should().Be("symbol");
                });
                (x.Children[1] as ProductionRule).Action(y =>
                {
                    y.NotNull();
                    y.Name.Should().Be("_tag-3");
                    y.Children.Count.Should().Be(2);
                    (y.Children[0] as VirtualTerminalSymbol).Action(z =>
                    {
                        z.NotNull();
                        z.Name.Should().Be("_tag-3-1");
                        z.Text.Should().Be("=");
                    });
                    (y.Children[1] as ProductionRuleReference).Action(z =>
                    {
                        z.NotNull();
                        z.Name.Should().Be("_tag-3-3");
                        z.ReferenceSyntax.Should().NotBeNull();
                        z.ReferenceSyntax.Name.Should().Be("symbol");
                    });
                });
            }); 
        }
    }
}
