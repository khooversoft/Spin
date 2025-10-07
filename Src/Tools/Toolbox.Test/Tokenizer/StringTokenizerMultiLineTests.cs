using Toolbox.Extensions;
using Toolbox.LangTools;
using Toolbox.Tools;

namespace Toolbox.Test.Tokenizer
{
    public class StringTokenizerMultiLineTests
    {
        [Fact]
        public void EmptyString()
        {
            var parser = new StringTokenizer()
                .UseCollapseWhitespace()
                .UseDoubleQuote()
                .UseSingleQuote()
                .Build();

            var tokens = parser.Parse("" + Environment.NewLine);
            tokens.Count.Be(0);

            var subject = "" + Environment.NewLine + Environment.NewLine;
            tokens = parser.Parse(subject);
            tokens.Count.Be(0);

            subject = Environment.NewLine + "" + Environment.NewLine + Environment.NewLine;
            tokens = parser.Parse(subject);
            tokens.Count.Be(0);
        }

        [Fact]
        public void PadString()
        {
            var parser = new StringTokenizer()
                .UseCollapseWhitespace()
                .UseDoubleQuote()
                .UseSingleQuote()
                .Build();

            IReadOnlyList<IToken> tokens = parser.Parse("      " + Environment.NewLine);
            tokens.Count.Be(1);
            tokens[0].Value.Be(" ");

            tokens = parser.Parse(Environment.NewLine + "      " + Environment.NewLine);
            tokens.Count.Be(1);
            tokens[0].Value.Be(" ");
        }

        [Fact]
        public void Comment()
        {
            var parser = new StringTokenizer()
                .UseCollapseWhitespace()
                .UseDoubleQuote()
                .UseSingleQuote()
                .Build();

            IReadOnlyList<IToken> tokens = parser.Parse("// this is a test");
            tokens.Count.Be(0);

            tokens = parser.Parse("// this is a test      " + Environment.NewLine);
            tokens.Count.Be(0);

            tokens = parser.Parse(Environment.NewLine + "// this is a test      " + Environment.NewLine);
            tokens.Count.Be(0);

            tokens = parser.Parse(Environment.NewLine + "      // this is a test      " + Environment.NewLine);
            tokens.Count.Be(1);
            tokens[0].Value.Be(" ");
        }

        [Fact]
        public void MultipleLines()
        {
            var parser = new StringTokenizer()
                .UseCollapseWhitespace()
                .UseDoubleQuote()
                .UseSingleQuote()
                .Build();

            var subject = new[]
            {
                "      ",
                "// this is a test",
                "   abc def   ",
                "// another comment",
                "   name  // comment  ",
                "   age state  // comment  ",
                " // this is a test",
            }.Join(Environment.NewLine);

            IReadOnlyList<IToken> tokens = parser.Parse(subject);
            tokens.Count.Be(11);
            int index = 0;
            tokens[index++].Value.Be(" ");
            tokens[index++].Value.Be("abc");
            tokens[index++].Value.Be(" ");
            tokens[index++].Value.Be("def");
            tokens[index++].Value.Be(" ");
            tokens[index++].Value.Be("name");
            tokens[index++].Value.Be(" ");
            tokens[index++].Value.Be("age");
            tokens[index++].Value.Be(" ");
            tokens[index++].Value.Be("state");
            tokens[index++].Value.Be(" ");
        }
    }
}
