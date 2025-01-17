using Toolbox.LangTools;
using Toolbox.Tools.Should;

namespace Toolbox.Test.Tokenizer;

public class BlockTokenizerTests
{
    [Fact]
    public void DoubleQuotedString()
    {
        IReadOnlyList<IToken> tokens = new StringTokenizer()
            .UseCollapseWhitespace()
            .UseDoubleQuote()
            .UseSingleQuote()
            .Parse("\"quote\"");

        tokens.Count.Should().Be(1);
    }


    [Fact]
    public void SingleFullString()
    {
        IReadOnlyList<IToken> tokens = new StringTokenizer()
            .UseCollapseWhitespace()
            .UseDoubleQuote()
            .UseSingleQuote()
            .Parse("'quote'");

        tokens.Count.Should().Be(1);
    }

    [Fact]
    public void QuotesBlockPerservesSpace()
    {
        var tokenizer = new StringTokenizer()
            .UseCollapseWhitespace()
            .UseDoubleQuote()
            .UseSingleQuote();

        IReadOnlyList<IToken> tokens = tokenizer.Parse("\"this  \"is quote");

        IToken[] expectedTokens = new IToken[]
        {
            new TokenValue("this  "),
            new TokenValue("is"),
            new TokenValue(" "),
            new TokenValue("quote"),
        };

        test(tokens, expectedTokens);
    }

    [Fact]
    public void QuoteFieldAtBeginning()
    {
        var tokenizer = new StringTokenizer()
            .UseCollapseWhitespace()
            .UseDoubleQuote()
            .UseSingleQuote();

        var tokens = tokenizer.Parse("\"this\" is quote");

        var expectedTokens = new IToken[]
        {
            new TokenValue("this"),
            new TokenValue(" "),
            new TokenValue("is"),
            new TokenValue(" "),
            new TokenValue("quote"),
        };

        test(tokens, expectedTokens);
    }

    [Fact]
    public void QuoteFieldAtEnd()
    {
        var tokenizer = new StringTokenizer()
            .UseCollapseWhitespace()
            .UseDoubleQuote()
            .UseSingleQuote();

        var tokens = tokenizer.Parse("this is \"quote\"");

        var expectedTokens = new IToken[]
        {
            new TokenValue("this"),
            new TokenValue(" "),
            new TokenValue("is"),
            new TokenValue(" "),
            new TokenValue("quote"),
        };

        test(tokens, expectedTokens);
    }

    [Fact]
    public void QuotedFieldInMiddle()
    {
        var tokenizer = new StringTokenizer()
            .UseCollapseWhitespace()
            .UseDoubleQuote()
            .UseSingleQuote();

        var tokens = tokenizer.Parse("this \"is text\" quote");

        var expectedTokens = new IToken[]
        {
            new TokenValue("this"),
            new TokenValue(" "),
            new TokenValue("is text"),
            new TokenValue(" "),
            new TokenValue("quote"),
        };

        test(tokens, expectedTokens);
    }

    [Fact]
    public void CustomBlockSyntax()
    {
        var tokens = new StringTokenizer()
            .UseCollapseWhitespace()
            .UseDoubleQuote()
            .UseSingleQuote()
            .AddBlock('<', '>')
            .Parse("this <is text> quote");

        var expectedTokens = new IToken[]
        {
            new TokenValue("this"),
            new TokenValue(" "),
            new TokenValue("is text"),
            new TokenValue(" "),
            new TokenValue("quote"),
        };

        test(tokens, expectedTokens);
    }

    private static void test(IReadOnlyList<IToken> tokens, IToken[] expectedTokens)
    {
        tokens.Count.Should().Be(expectedTokens.Length);

        tokens
            .Zip(expectedTokens, (o, i) => (o, i))
            .All(x => x.o.Value == x.i.Value)
            .Should().BeTrue();
    }
}
