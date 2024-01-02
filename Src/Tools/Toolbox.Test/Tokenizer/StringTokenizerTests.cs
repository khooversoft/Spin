using FluentAssertions;
using Toolbox.Extensions;
using Toolbox.LangTools;

namespace Toolbox.Test.Tokenizer
{
    public class StringTokenizerTests
    {
        [Fact]
        public void EmptyString()
        {
            IReadOnlyList<IToken> tokens = new StringTokenizer()
                .UseCollapseWhitespace()
                .UseDoubleQuote()
                .UseSingleQuote()
                .Parse("");

            tokens.Count.Should().Be(0);
        }

        [Fact]
        public void PadString()
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
        public void TokenIsSpace()
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
        public void TokenIsSpaceAndPad()
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
        public void TokenIsSpaceAndPadWithFilter()
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
        public void KnownTokenSpecified()
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
        public void EscapeIsUsed()
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
