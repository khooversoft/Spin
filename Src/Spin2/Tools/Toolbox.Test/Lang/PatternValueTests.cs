using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Toolbox.Types;
using Toolbox.LangTools.Pattern;

namespace Toolbox.Test.Lang;

public class PatternValueTests
{
    [Fact]
    public void SingleValue()
    {
        var syntax = new PmRoot() + new PmValue("value");

        var results = PatternParser.Parse(syntax, "name");
        results.IsOk().Should().BeTrue();

        PatternNodes syntaxList = results.Return();
        syntaxList.Count.Should().Be(1);
        syntaxList[0].SyntaxNode.Name.Should().Be("value");
        syntaxList[0].Value.Should().Be("name");
    }

    [Fact]
    public void TwoValue()
    {
        var syntax = new PmRoot() + new PmValue("v1") + new PmValue("v2");

        var results = PatternParser.Parse(syntax, "+ *");
        results.IsOk().Should().BeTrue();

        PatternNodes syntaxList = results.Return();
        syntaxList.Count.Should().Be(2);
        syntaxList[0].SyntaxNode.Name.Should().Be("v1");
        syntaxList[0].Value.Should().Be("+");
        syntaxList[1].SyntaxNode.Name.Should().Be("v2");
        syntaxList[1].Value.Should().Be("*");

        results = PatternParser.Parse(syntax, "*");
        results.IsError().Should().BeTrue();
    }

    [Fact]
    public void ValueAssignment()
    {
        var syntax = new PmRoot() + new PmValue("key") + new PmToken("=", "equal") + new PmValue("value");

        var results = PatternParser.Parse(syntax, "color=red");
        results.IsOk().Should().BeTrue();

        PatternNodes syntaxList = results.Return();
        syntaxList.Count.Should().Be(3);
        syntaxList[0].SyntaxNode.Name.Should().Be("key");
        syntaxList[0].Value.Should().Be("color");
        syntaxList[1].SyntaxNode.Name.Should().Be("equal");
        syntaxList[1].Value.Should().Be("=");
        syntaxList[2].SyntaxNode.Name.Should().Be("value");
        syntaxList[2].Value.Should().Be("red");

        results = PatternParser.Parse(syntax, "color");
        results.IsError().Should().BeTrue();

        results = PatternParser.Parse(syntax, "color=");
        results.IsError().Should().BeTrue();
    }
}
