using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Toolbox.LangTools.Pattern;
using Toolbox.Types;

namespace Toolbox.Test.Lang;

public class PatternTokenTests
{
    [Fact]
    public void SingleToken()
    {
        var syntax = new PmRoot() + new PmToken("+", name: "plus");

        var results = PatternParser.Parse(syntax, "+");
        results.IsOk().Should().BeTrue();

        PatternNodes syntaxList = results.Return();
        syntaxList.Count.Should().Be(1);
        syntaxList[0].SyntaxNode.Name.Should().Be("plus");
        syntaxList[0].Value.Should().Be("+");

        results = PatternParser.Parse(syntax, "*");
        results.IsError().Should().BeTrue();
    }

    [Fact]
    public void TwoToken()
    {
        var syntax = new PmRoot() + new PmToken("+", name: "plus") + new PmToken("*", name: "star");

        var results = PatternParser.Parse(syntax, "+ *");
        results.IsOk().Should().BeTrue();

        PatternNodes syntaxList = results.Return();
        syntaxList.Count.Should().Be(2);
        syntaxList[0].SyntaxNode.Name.Should().Be("plus");
        syntaxList[0].Value.Should().Be("+");
        syntaxList[1].SyntaxNode.Name.Should().Be("star");
        syntaxList[1].Value.Should().Be("*");

        results = PatternParser.Parse(syntax, "*");
        results.IsError().Should().BeTrue();
    }
}
