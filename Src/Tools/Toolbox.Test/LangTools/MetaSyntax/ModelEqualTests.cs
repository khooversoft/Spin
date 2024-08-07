using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Toolbox.LangTools;

namespace Toolbox.Test.LangTools.MetaSyntax;

public class ModelEqualTests
{
    [Fact]
    public void SyntaxTreeEqualDefault()
    {
        var v1 = new SyntaxTree();
        var v2 = new SyntaxTree();
        (v1 == v2).Should().BeTrue();
        v1.Should().Be(v2);
    }

    [Fact]
    public void SyntaxTreeEqualJustMetaSyntax()
    {
        var s1 = new TerminalSymbol { Name = "hello", Type = TerminalType.String, Text = "world" };
        var s2 = new TerminalSymbol { Name = "hello", Type = TerminalType.String, Text = "world" };
        (s1 == s2).Should().BeTrue();
        s1.Should().Be(s2);

        var v1 = new SyntaxTree { MetaSyntax = s1 };
        var v2 = new SyntaxTree { MetaSyntax = s2 };
        (v1 == v2).Should().BeTrue();
        v1.Should().Be(v2);
    }

    [Fact]
    public void SyntaxTreeNotEqualJustMetaSyntax()
    {
        var s1 = new TerminalSymbol { Name = "hello", Type = TerminalType.String, Text = "world" };
        var s2 = new TerminalSymbol { Name = "hello2", Type = TerminalType.String, Text = "world" };
        (s1 == s2).Should().BeFalse();
        s1.Should().NotBe(s2);

        var v1 = new SyntaxTree { MetaSyntax = s1 };
        var v2 = new SyntaxTree { MetaSyntax = s2 };
        (v1 == v2).Should().BeFalse();
        v1.Should().NotBe(v2);
    }

    [Fact]
    public void SyntaxPairEqual()
    {
        var s1 = new TerminalSymbol { Name = "hello", Type = TerminalType.String, Text = "world" };
        var s2 = new TerminalSymbol { Name = "hello", Type = TerminalType.String, Text = "world" };
        (s1 == s2).Should().BeTrue();
        s1.Should().Be(s2);

        var t1 = new TokenValue("a", 10);
        var t2 = new TokenValue("a", 10);
        (t1 == t2).Should().BeTrue();
        t1.Should().Be(t2);

        var v1 = new SyntaxPair { Token = t1, MetaSyntax = s1 };
        var v2 = new SyntaxPair { Token = t2, MetaSyntax = s2 };
        (v1 == v2).Should().BeTrue();
        v1.Should().Be(v2);
    }

    [Fact]
    public void SyntaxTreeWithChildren()
    {
        var s1 = new TerminalSymbol { Name = "hello", Type = TerminalType.String, Text = "world" };
        var s2 = new TerminalSymbol { Name = "hello", Type = TerminalType.String, Text = "world" };
        (s1 == s2).Should().BeTrue();
        s1.Should().Be(s2);

        var t1 = new TokenValue("a", 10);
        var t2 = new TokenValue("a", 10);
        (t1 == t2).Should().BeTrue();
        t1.Should().Be(t2);

        var v1 = new SyntaxPair { Token = t1, MetaSyntax = s1 };
        var v2 = new SyntaxPair { Token = t2, MetaSyntax = s2 };
        (v1 == v2).Should().BeTrue();
        v1.Should().Be(v2);

        var st1 = new SyntaxTree
        {
            Children = [v1],
        };

        var st2 = new SyntaxTree
        {
            Children = [v2],
        };

        (st1 == st2).Should().BeTrue();
        st1.Should().Be(st1);
    }

    [Fact]
    public void SyntaxTreeWithTwoChildren()
    {
        var s1 = new TerminalSymbol { Name = "hello", Type = TerminalType.String, Text = "world" };
        var s2 = new TerminalSymbol { Name = "hello", Type = TerminalType.String, Text = "world" };
        (s1 == s2).Should().BeTrue();
        s1.Should().Be(s2);

        var t1 = new TokenValue("a", 10);
        var t2 = new TokenValue("a", 10);
        (t1 == t2).Should().BeTrue();
        t1.Should().Be(t2);

        var v1 = new SyntaxPair { Token = new TokenValue("a", 10), MetaSyntax = new TerminalSymbol { Name = "hello", Type = TerminalType.String, Text = "world" } };
        var v12 = new SyntaxPair { Token = new TokenValue("a", 10), MetaSyntax = new TerminalSymbol { Name = "hello", Type = TerminalType.String, Text = "world" } };
        var v2 = new SyntaxPair { Token = new TokenValue("a", 10), MetaSyntax = new TerminalSymbol { Name = "hello", Type = TerminalType.String, Text = "world" } };
        var v22 = new SyntaxPair { Token = new TokenValue("a", 10), MetaSyntax = new TerminalSymbol { Name = "hello", Type = TerminalType.String, Text = "world" } };
        (v1 == v2).Should().BeTrue();
        v1.Should().Be(v2);

        var st1 = new SyntaxTree
        {
            Children = [v1, v12],
        };

        var st2 = new SyntaxTree
        {
            Children = [v2, v22],
        };

        (st1 == st2).Should().BeTrue();
        st1.Should().Be(st2);

        var ust1 = new SyntaxTree
        {
            Children = [st1],
        };

        var ust2 = new SyntaxTree
        {
            Children = [st2],
        };

        (ust1 == ust2).Should().BeTrue();
        ust1.Should().Be(ust2);
    }


    [Fact]
    public void SyntaxTreeWithTwoNotChildren()
    {
        var s1 = new TerminalSymbol { Name = "hello", Type = TerminalType.String, Text = "world" };
        var s2 = new TerminalSymbol { Name = "hello", Type = TerminalType.String, Text = "world" };
        (s1 == s2).Should().BeTrue();
        s1.Should().Be(s2);

        var t1 = new TokenValue("a", 10);
        var t2 = new TokenValue("a", 10);
        (t1 == t2).Should().BeTrue();
        t1.Should().Be(t2);

        var v1 = new SyntaxPair { Token = new TokenValue("a", 10), MetaSyntax = new TerminalSymbol { Name = "hello", Type = TerminalType.String, Text = "world" } };
        var v12 = new SyntaxPair { Token = new TokenValue("a", 10), MetaSyntax = new TerminalSymbol { Name = "hello", Type = TerminalType.String, Text = "world" } };
        var v2 = new SyntaxPair { Token = new TokenValue("a", 10), MetaSyntax = new TerminalSymbol { Name = "hello", Type = TerminalType.String, Text = "world" } };
        var v22 = new SyntaxPair { Token = new TokenValue("a", 10), MetaSyntax = new TerminalSymbol { Name = "hello2", Type = TerminalType.String, Text = "world" } };
        (v1 == v2).Should().BeTrue();
        v1.Should().Be(v2);

        var st1 = new SyntaxTree
        {
            Children = [v1, v12],
        };

        var st2 = new SyntaxTree
        {
            Children = [v2, v22],
        };

        (st1 == st2).Should().BeFalse();
        st1.Should().NotBe(st2);

        var ust1 = new SyntaxTree
        {
            Children = [st1],
        };

        var ust2 = new SyntaxTree
        {
            Children = [st2],
        };

        (ust1 == ust2).Should().BeFalse();
        ust1.Should().NotBe(ust2);
    }

}
