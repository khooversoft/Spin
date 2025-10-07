using Toolbox.LangTools;
using Toolbox.Tools;

namespace Toolbox.Test.LangTools.MetaSyntax;

public class ModelEqualTests
{
    [Fact]
    public void SyntaxTreeEqualDefault()
    {
        var v1 = new SyntaxTree();
        var v2 = new SyntaxTree();
        (v1 == v2).BeTrue();
    }

    [Fact]
    public void SyntaxTreeEqualJustMetaSyntax()
    {
        var v1 = new SyntaxTree { MetaSyntaxName = "hello" };
        var v2 = new SyntaxTree { MetaSyntaxName = "hello" };
        (v1 == v2).BeTrue();
    }

    [Fact]
    public void SyntaxTreeNotEqualJustMetaSyntax()
    {
        var v1 = new SyntaxTree { MetaSyntaxName = "hello" };
        var v2 = new SyntaxTree { MetaSyntaxName = "hello2" };
        (v1 == v2).BeFalse();
    }

    [Fact]
    public void SyntaxPairEqual()
    {
        var t1 = new TokenValue("a", 10);
        var t2 = new TokenValue("a", 10);
        (t1 == t2).BeTrue();

        var v1 = new SyntaxPair { Token = t1, Name = "hello" };
        var v2 = new SyntaxPair { Token = t2, Name = "hello" };
        (v1 == v2).BeTrue();
    }

    [Fact]
    public void SyntaxTreeWithChildren()
    {
        var t1 = new TokenValue("a", 10);
        var t2 = new TokenValue("a", 10);
        (t1 == t2).BeTrue();
        t1.Assert(x => x == t2);

        var v1 = new SyntaxPair { Token = t1, Name = "hello" };
        var v2 = new SyntaxPair { Token = t2, Name = "hello" };
        (v1 == v2).BeTrue();

        var st1 = new SyntaxTree
        {
            Children = [v1],
        };

        var st2 = new SyntaxTree
        {
            Children = [v2],
        };

        (st1 == st2).BeTrue();
    }

    [Fact]
    public void SyntaxTreeWithTwoChildren()
    {
        var t1 = new TokenValue("a", 10);
        var t2 = new TokenValue("a", 10);
        (t1 == t2).BeTrue();
        t1.Assert(x => x == t2);

        var v1 = new SyntaxPair { Token = new TokenValue("a", 10), Name = "hello" };
        var v12 = new SyntaxPair { Token = new TokenValue("a", 10), Name = "hello" };
        var v2 = new SyntaxPair { Token = new TokenValue("a", 10), Name = "hello" };
        var v22 = new SyntaxPair { Token = new TokenValue("a", 10), Name = "hello" };
        (v1 == v2).BeTrue();

        var st1 = new SyntaxTree
        {
            Children = [v1, v12],
        };

        var st2 = new SyntaxTree
        {
            Children = [v2, v22],
        };

        (st1 == st2).BeTrue();
        st1.Assert(x => x == st2);

        var ust1 = new SyntaxTree
        {
            Children = [st1],
        };

        var ust2 = new SyntaxTree
        {
            Children = [st2],
        };

        (ust1 == ust2).BeTrue();
    }


    [Fact]
    public void SyntaxTreeWithTwoNotChildren()
    {
        var t1 = new TokenValue("a", 10);
        var t2 = new TokenValue("a", 10);
        (t1 == t2).BeTrue();
        t1.Assert(x => x == t2);

        var v1 = new SyntaxPair { Token = new TokenValue("a", 10), Name = "hello" };
        var v12 = new SyntaxPair { Token = new TokenValue("a", 10), Name = "hello" };
        var v2 = new SyntaxPair { Token = new TokenValue("a", 10), Name = "hello" };
        var v22 = new SyntaxPair { Token = new TokenValue("a", 10), Name = "hello2" };
        (v1 == v2).BeTrue();

        var st1 = new SyntaxTree
        {
            Children = [v1, v12],
        };

        var st2 = new SyntaxTree
        {
            Children = [v2, v22],
        };

        (st1 == st2).BeFalse();

        var ust1 = new SyntaxTree
        {
            Children = [st1],
        };

        var ust2 = new SyntaxTree
        {
            Children = [st2],
        };

        (ust1 == ust2).BeFalse();
    }

}
