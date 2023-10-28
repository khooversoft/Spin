using FluentAssertions;
using Toolbox.Extensions;
using Toolbox.Tokenizer;
using Toolbox.Tokenizer.Token;

namespace Toolbox.Test.Tokenizer
{
    public class StringTokenizerTests
    {
        [Fact]
        public void BasicToken_WhenEmptyString_ShouldReturnNoTokens()
        {
            IReadOnlyList<IToken> tokens = new StringTokenizer()
                .UseCollapseWhitespace()
                .UseDoubleQuote()
                .UseSingleQuote()
                .Parse("");

            tokens.Count.Should().Be(0);
        }

        [Fact]
        public void BlockStringToken_WhenDoubleQuotedString_ShouldReturnString()
        {
            IReadOnlyList<IToken> tokens = new StringTokenizer()
                .UseCollapseWhitespace()
                .UseDoubleQuote()
                .UseSingleQuote()
                .Parse("\"quote\"");

            tokens.Count.Should().Be(1);
        }

        [Fact]
        public void BlockStringToken_WhenDoubleQuoteInString_ShouldReturnString()
        {
            var tokenizer = new StringTokenizer()
                .UseCollapseWhitespace()
                .UseDoubleQuote()
                .UseSingleQuote();

            IReadOnlyList<IToken> tokens = tokenizer.Parse("\"this \"is quote");

            IToken[] expectedTokens = new IToken[]
            {
                new TokenValue("this "),
                new TokenValue("is"),
                new TokenValue(" "),
                new TokenValue("quote"),
            };

            test();

            tokens = tokenizer.Parse("this is \"quote\"");

            expectedTokens = new IToken[]
            {
                new TokenValue("this"),
                new TokenValue(" "),
                new TokenValue("is"),
                new TokenValue(" "),
                new TokenValue("quote"),
            };

            test();

            tokens = tokenizer.Parse("this \"is text\" quote");

            expectedTokens = new IToken[]
            {
                new TokenValue("this"),
                new TokenValue(" "),
                new TokenValue("is text"),
                new TokenValue(" "),
                new TokenValue("quote"),
            };

            test();


            void test()
            {
                tokens.Count.Should().Be(expectedTokens.Length);

                tokens
                    .Zip(expectedTokens, (o, i) => (o, i))
                    .All(x => x.o.Value == x.i.Value)
                    .Should().BeTrue();
            }
        }

        [Fact]
        public void BlockStringToken_WhenSingleFullString_ShouldReturnString()
        {
            IReadOnlyList<IToken> tokens = new StringTokenizer()
                .UseCollapseWhitespace()
                .UseDoubleQuote()
                .UseSingleQuote()
                .Parse("'quote'");

            tokens.Count.Should().Be(1);
        }

        [Fact]
        public void BasicToken_WhenPadString_ShouldReturnSpaceToken()
        {
            IReadOnlyList<IToken> tokens = new StringTokenizer()
                .UseCollapseWhitespace()
                .UseDoubleQuote()
                .UseSingleQuote()
                .Parse("      ");

            tokens.Count.Should().Be(1);
            tokens[0].Value.Should().Be(" ");
        }

        [Fact]
        public void BasicToken_WhenTokenIsSpace_ShouldReturnValidTokens()
        {
            IReadOnlyList<IToken> tokens = new StringTokenizer()
                .UseCollapseWhitespace()
                .UseDoubleQuote()
                .UseSingleQuote()
                .Parse("abc def");

            var expectedTokens = new IToken[]
            {
                new TokenValue("abc"),
                new TokenValue(" "),
                new TokenValue("def"),
            };

            tokens.Count.Should().Be(expectedTokens.Length);

            tokens
                .Zip(expectedTokens, (o, i) => (o, i))
                .All(x => x.o.Value == x.i.Value)
                .Should().BeTrue();
        }

        [Fact]
        public void BasicToken_WhenTokenIsSpaceAndPad_ShouldReturnValidTokens()
        {
            IReadOnlyList<IToken> tokens = new StringTokenizer()
                .UseCollapseWhitespace()
                .UseDoubleQuote()
                .UseSingleQuote()
                .Parse("  abc   def  ");

            var expectedTokens = new IToken[]
            {
                new TokenValue(" "),
                new TokenValue("abc"),
                new TokenValue(" "),
                new TokenValue("def"),
                new TokenValue(" "),
            };

            tokens.Count.Should().Be(expectedTokens.Length);

            tokens
                .Zip(expectedTokens, (o, i) => (o, i))
                .All(x => x.o.Value == x.i.Value)
                .Should().BeTrue();
        }

        [Fact]
        public void BasicToken_WhenTokenIsSpaceAndPadWithFilter()
        {
            IReadOnlyList<IToken> tokens = new StringTokenizer()
                .UseCollapseWhitespace()
                .UseDoubleQuote()
                .UseSingleQuote()
                .SetFilter(x => x.Value.IsNotEmpty())
                .Parse("  abc   def  ");

            var expectedTokens = new IToken[]
            {
                new TokenValue("abc"),
                new TokenValue("def"),
            };

            tokens.Count.Should().Be(expectedTokens.Length);

            tokens
                .Zip(expectedTokens, (o, i) => (o, i))
                .All(x => x.o.Value == x.i.Value)
                .Should().BeTrue();
        }

        [Fact]
        public void BasicToken_WhenKnownTokenSpecified_ShouldReturnValidTokens()
        {
            IReadOnlyList<IToken> tokens = new StringTokenizer()
                .UseCollapseWhitespace()
                .UseDoubleQuote()
                .UseSingleQuote()
                .Add("[", "]")
                .Parse("  abc   [def]  ");

            var expectedTokens = new IToken[]
            {
                new TokenValue(" "),
                new TokenValue("abc"),
                new TokenValue(" "),
                new TokenValue("["),
                new TokenValue("def"),
                new TokenValue("]"),
                new TokenValue(" "),
            };

            tokens.Count.Should().Be(expectedTokens.Length);

            tokens
                .Zip(expectedTokens, (o, i) => (o, i))
                .All(x => x.o.Value == x.i.Value)
                .Should().BeTrue();
        }

        [Fact]
        public void PropertyName_WhenEscapeIsUsed_ShouldReturnValidTokens()
        {
            IReadOnlyList<IToken> tokens = new StringTokenizer()
                .Add("{", "}", "{{", "}}")
                .Parse("Escape {{firstName}} end");

            var expectedTokens = new IToken[]
            {
                new TokenValue("Escape "),
                new TokenValue("{{"),
                new TokenValue("firstName"),
                new TokenValue("}}"),
                new TokenValue(" end"),
            };

            tokens.Count.Should().Be(expectedTokens.Length);

            tokens
                .Zip(expectedTokens, (o, i) => (o, i))
                .All(x => x.o.Value == x.i.Value)
                .Should().BeTrue();
        }
    }
}
