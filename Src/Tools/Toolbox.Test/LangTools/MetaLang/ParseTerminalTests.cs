using FluentAssertions;
using Toolbox.Extensions;
using Toolbox.LangTools;
using Toolbox.Types;

namespace Toolbox.Test.LangTools.Meta;

public class ParseTerminalTests
{
    [Theory]
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
        var rule = "number  = regex '[+-]?[0-9]+' ;";
        test(rule);

        rule = "number=regex'[+-]?[0-9]+';";
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
                x.Type.Should().Be(TerminalType.Regex);
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
                x.Type.Should().Be(TerminalType.Token);
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
                x.Type.Should().Be(TerminalType.Token);
            });
        }
    }

    [Fact]
    public void WithTag()
    {
        var rule = "add-sym = 'add' #tag ;";
        test(rule);

        rule = "add-sym='add'#tag;";
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
                x.Type.Should().Be(TerminalType.Token);
                Enumerable.SequenceEqual(x.Tags, ["tag"]).Should().BeTrue();
                Enumerable.SequenceEqual(x.Tags, ["taxg"]).Should().BeFalse();
            });
        }
    }
}
