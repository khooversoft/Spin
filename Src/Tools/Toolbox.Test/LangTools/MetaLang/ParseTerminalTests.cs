using Toolbox.Extensions;
using Toolbox.LangTools;
using Toolbox.Tools;
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
        root.StatusCode.IsError().BeTrue();
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
            root.StatusCode.IsOk().BeTrue();

            root.Rule.Children.Count.Be(1);
            root.Rule.Children.OfType<TerminalSymbol>().First().Action(x =>
            {
                x.Name.Be("number");
                x.Text.Be("[+-]?[0-9]+");
                x.Type.Be(TerminalType.Regex);
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
            root.StatusCode.IsOk().BeTrue();

            root.Rule.Children.Count.Be(1);
            root.Rule.Children.OfType<TerminalSymbol>().First().Action(x =>
            {
                x.Name.Be("equal");
                x.Text.Be("=");
                x.Type.Be(TerminalType.Token);
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
            root.StatusCode.IsOk().BeTrue();

            root.Rule.Children.Count.Be(1);
            root.Rule.Children.OfType<TerminalSymbol>().First().Action(x =>
            {
                x.Name.Be("add-sym");
                x.Text.Be("add");
                x.Type.Be(TerminalType.Token);
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
            root.StatusCode.IsOk().BeTrue();

            root.Rule.Children.Count.Be(1);
            root.Rule.Children.OfType<TerminalSymbol>().First().Action(x =>
            {
                x.Name.Be("add-sym");
                x.Text.Be("add");
                x.Type.Be(TerminalType.Token);
                Enumerable.SequenceEqual(x.Tags, ["tag"]).BeTrue();
                Enumerable.SequenceEqual(x.Tags, ["taxg"]).BeFalse();
            });
        }
    }

    [Fact]
    public void With2Tag()
    {
        var rule = "add-sym = 'add' #start-group #group ;";
        test(rule);

        // Tags are not token parsed, just data so you can't have a space between them
        rule = "add-sym='add' #start-group #group;";
        test(rule);

        static void test(string rule)
        {
            var root = MetaParser.ParseRules(rule);
            root.StatusCode.IsOk().BeTrue();

            root.Rule.Children.Count.Be(1);
            root.Rule.Children.OfType<TerminalSymbol>().First().Action(x =>
            {
                x.Name.Be("add-sym");
                x.Text.Be("add");
                x.Type.Be(TerminalType.Token);
                Enumerable.SequenceEqual(x.Tags, ["start-group", "group"]).BeTrue();
            });
        }
    }
}
