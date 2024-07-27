using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Toolbox.Extensions;
using Toolbox.LangTools;
using Toolbox.Types;

namespace Toolbox.Test.LangTools.Meta;

public class ParseTerminalTests
{
    [Theory]
    [InlineData("number=regex:'[+-]?[0-9]+'")]
    [InlineData("number=redgex:'[+-]?[0-9]+';")]
    [InlineData("number= regex;")]
    [InlineData("='=';")]
    [InlineData("equal '=';")]
    public void ShouldFail(string rule)
    {
        var root = MetaParser.ParseRules(rule);
        root.StatusCode.IsError().Should().BeTrue();
    }

    [Fact]
    public void TerminalRule()
    {
        var rule = "number  = regex: '[+-]?[0-9]+' ;";
        test(rule);

        rule = "number=regex:'[+-]?[0-9]+';";
        test(rule);

        static void test(string rule)
        {
            var root = MetaParser.ParseRules(rule);
            root.StatusCode.IsOk().Should().BeTrue();

            root.Rule.Children.Count.Should().Be(1);
            root.Rule.Children.OfType<TerminalSymbol>().First().Action(x =>
            {
                x.Name.Should().Be("number");
                x.Text.Should().Be("[+-]?[0-9]+");
                x.Regex.Should().BeTrue();
            });
        }
    }

    [Fact]
    public void ParseRule2()
    {
        var rule = "equal = '=' ;";
        test(rule);

        rule = "equal='=';";
        test(rule);

        static void test(string rule)
        {
            var root = MetaParser.ParseRules(rule);
            root.StatusCode.IsOk().Should().BeTrue();

            root.Rule.Children.Count.Should().Be(1);
            root.Rule.Children.OfType<TerminalSymbol>().First().Action(x =>
            {
                x.Name.Should().Be("equal");
                x.Text.Should().Be("=");
                x.Regex.Should().BeFalse();
            });
        }
    }

    [Fact]
    public void ParseRule3()
    {
        var rule = "add-sym = 'add' ;";
        test(rule);

        rule = "add-sym='add';";
        test(rule);

        static void test(string rule)
        {
            var root = MetaParser.ParseRules(rule);
            root.StatusCode.IsOk().Should().BeTrue();

            root.Rule.Children.Count.Should().Be(1);
            root.Rule.Children.OfType<TerminalSymbol>().First().Action(x =>
            {
                x.Name.Should().Be("add-sym");
                x.Text.Should().Be("add");
                x.Regex.Should().BeFalse();
            });
        }
    }
}
