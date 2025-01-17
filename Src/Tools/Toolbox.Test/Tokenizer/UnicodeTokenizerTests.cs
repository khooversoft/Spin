using Toolbox.LangTools;
using Toolbox.Tools.Should;

namespace Toolbox.Test.Tokenizer;

public class UnicodeTokenizerTests
{
    [Fact]
    public void CommonUnicoded()
    {
        IReadOnlyList<IToken> tokens = new StringTokenizer()
            .UseCollapseWhitespace()
            .UseDoubleQuote()
            .UseSingleQuote()
            .UseUnicode()
            .Parse("  abc\\u00B3def  ");

        var expectedTokens = new IToken[]
        {
                new TokenValue(" "),
                new TokenValue("abc"),
                new TokenValue("\\u00B3"),
                new TokenValue("def"),
                new TokenValue(" "),
        };

        tokens.Count.Should().Be(expectedTokens.Length);

        tokens
            .Zip(expectedTokens, (o, i) => (o, i))
            .All(x => x.o.Value == x.i.Value)
            .Should().BeTrue();

        tokens[2].TokenType.Should().Be(TokenType.Unicode);
    }

    [Fact]
    public void CommonUnicodedNotEscaped()
    {
        IReadOnlyList<IToken> tokens = new StringTokenizer()
            .UseCollapseWhitespace()
            .UseDoubleQuote()
            .UseSingleQuote()
            .UseUnicode()
            .Parse("  abc\u00B3def  ");

        var expectedTokens = new IToken[]
        {
                new TokenValue(" "),
                new TokenValue("abcdef"),
                new TokenValue(" "),
        };

        tokens.Count.Should().Be(expectedTokens.Length);

        tokens
            .Zip(expectedTokens, (o, i) => (o, i))
            .All(x => x.o.Value == x.i.Value)
            .Should().BeTrue();
    }

    [Fact]
    public void StandardUnicoded()
    {
        IReadOnlyList<IToken> tokens = new StringTokenizer()
            .UseCollapseWhitespace()
            .UseDoubleQuote()
            .UseSingleQuote()
            .UseUnicode()
            .Parse("  abcU+00B3def  ");

        var expectedTokens = new IToken[]
        {
                new TokenValue(" "),
                new TokenValue("abc"),
                new TokenValue("U+00B3"),
                new TokenValue("def"),
                new TokenValue(" "),
        };

        tokens.Count.Should().Be(expectedTokens.Length);

        tokens
            .Zip(expectedTokens, (o, i) => (o, i))
            .All(x => x.o.Value == x.i.Value)
            .Should().BeTrue();

        tokens[2].TokenType.Should().Be(TokenType.Unicode);
    }
}
