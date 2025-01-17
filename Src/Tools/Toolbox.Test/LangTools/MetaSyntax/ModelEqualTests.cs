using Toolbox.LangTools;
using Toolbox.Tools.Should;

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
        var v1 = new SyntaxTree { MetaSyntaxName = "hello" };
        var v2 = new SyntaxTree { MetaSyntaxName = "hello" };
        (v1 == v2).Should().BeTrue();
        v1.Should().Be(v2);
    }

    [Fact]
    public void SyntaxTreeNotEqualJustMetaSyntax()
    {
        var v1 = new SyntaxTree { MetaSyntaxName = "hello" };
        var v2 = new SyntaxTree { MetaSyntaxName = "hello2" };
        (v1 == v2).Should().BeFalse();
        v1.Should().NotBe(v2);
    }

    [Fact]
    public void SyntaxPairEqual()
    {
        var t1 = new TokenValue("a", 10);
        var t2 = new TokenValue("a", 10);
        (t1 == t2).Should().BeTrue();
        t1.Should().Be(t2);

        var v1 = new SyntaxPair { Token = t1, MetaSyntaxName = "hello" };
        var v2 = new SyntaxPair { Token = t2, MetaSyntaxName = "hello" };
        (v1 == v2).Should().BeTrue();
        v1.Should().Be(v2);
    }

    [Fact]
    public void SyntaxTreeWithChildren()
    {
        var t1 = new TokenValue("a", 10);
        var t2 = new TokenValue("a", 10);
        (t1 == t2).Should().BeTrue();
        t1.Should().Be(t2);

        var v1 = new SyntaxPair { Token = t1, MetaSyntaxName = "hello" };
        var v2 = new SyntaxPair { Token = t2, MetaSyntaxName = "hello" };
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
        var t1 = new TokenValue("a", 10);
        var t2 = new TokenValue("a", 10);
        (t1 == t2).Should().BeTrue();
        t1.Should().Be(t2);

        var v1 = new SyntaxPair { Token = new TokenValue("a", 10), MetaSyntaxName = "hello" };
        var v12 = new SyntaxPair { Token = new TokenValue("a", 10), MetaSyntaxName = "hello" };
        var v2 = new SyntaxPair { Token = new TokenValue("a", 10), MetaSyntaxName = "hello" };
        var v22 = new SyntaxPair { Token = new TokenValue("a", 10), MetaSyntaxName = "hello" };
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
        var t1 = new TokenValue("a", 10);
        var t2 = new TokenValue("a", 10);
        (t1 == t2).Should().BeTrue();
        t1.Should().Be(t2);

        var v1 = new SyntaxPair { Token = new TokenValue("a", 10), MetaSyntaxName = "hello" };
        var v12 = new SyntaxPair { Token = new TokenValue("a", 10), MetaSyntaxName = "hello" };
        var v2 = new SyntaxPair { Token = new TokenValue("a", 10), MetaSyntaxName = "hello" };
        var v22 = new SyntaxPair { Token = new TokenValue("a", 10), MetaSyntaxName = "hello2" };
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
