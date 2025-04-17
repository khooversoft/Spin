using Toolbox.LangTools;
using Toolbox.Tools.Should;

namespace Toolbox.Test.Tokenizer;

public class TokenizerModelTests
{
    [Fact]
    public void TokenValueEqual()
    {
        var v1 = new TokenValue("a");
        var v2 = new TokenValue("a");
        (v1 == v2).Should().BeTrue();

        var v3 = new TokenValue("b");
        (v1 == v3).Should().BeFalse();
    }

    [Fact]
    public void TokenValueWithIndexEqual()
    {
        var v1 = new TokenValue("a", 10);
        var v2 = new TokenValue("a", 10);
        (v1 == v2).Should().BeTrue();

        var v3 = new TokenValue("b");
        (v1 == v3).Should().BeFalse();

        var v4 = new TokenValue("a", 5);
        (v1 == v4).Should().BeTrue();
    }

    [Fact]
    public void BlockTokenEqual()
    {
        var v1 = new BlockToken("av1b", 'a', 'b', 10);
        var v2 = new BlockToken("av1b", 'a', 'b', 10);
        (v1 == v2).Should().BeTrue();

        var v3 = new BlockToken("av2b", 'a', 'b', 10);
        (v1 == v3).Should().BeFalse();

        var v4 = new BlockToken("cv1b", 'c', 'b', 10);
        (v1 == v4).Should().BeFalse();

        var v5 = new BlockToken("av1x", 'a', 'x', 10);
        (v1 == v5).Should().BeFalse();

        var v6 = new BlockToken("av1b", 'a', 'b', 15);
        (v1 == v6).Should().BeTrue();
    }
}
