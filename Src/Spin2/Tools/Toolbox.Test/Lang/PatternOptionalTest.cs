using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Toolbox.LangTools.Pattern;
using Toolbox.Types;

namespace Toolbox.Test.Lang;

public class PatternOptionalTest
{
    [Fact]
    public void SingleOptional()
    {
        var syntax = new PmRoot() + (new PmOption("option") + new PmValue("value"));

        var results = PatternParser.Parse(syntax, "name");
        results.IsOk().Should().BeTrue();

        PatternNodes syntaxList = results.Return();
        syntaxList.Count.Should().Be(1);
        syntaxList[0].SyntaxNode.Name.Should().Be("value");
        syntaxList[0].Value.Should().Be("name");
    }

    [Fact]
    public void TwoOptional()
    {
        //var syntax = new PmRoot() + new PmValue("v1") + new PmValue("v2");

        //var results = PatternParser.Parse(syntax, "+ *");
        //results.IsOk().Should().BeTrue();

        //PatternNodes syntaxList = results.Return();
        //syntaxList.Count.Should().Be(2);
        //syntaxList[0].SyntaxNode.Name.Should().Be("v1");
        //syntaxList[0].Value.Should().Be("+");
        //syntaxList[1].SyntaxNode.Name.Should().Be("v2");
        //syntaxList[1].Value.Should().Be("*");

        //results = PatternParser.Parse(syntax, "*");
        //results.IsError().Should().BeTrue();
    }
}
