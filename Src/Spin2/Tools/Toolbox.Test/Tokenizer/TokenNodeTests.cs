using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Toolbox.Extensions;
using Toolbox.Tokenizer;
using Toolbox.Tokenizer.Token;
using Toolbox.Tokenizer.Tree;
using Toolbox.Types;

namespace Toolbox.Test.Tokenizer;

public class TokenNodeTests
{
    [Fact]
    public void LangSyntax()
    {
        // Format: s = v

        var root = new LangRoot()
            .AddValue("lvalue")
            .AddSyntax("=", "equal")
            .AddValue("rvalue");

        var parser = new StringTokenizer()
            .UseSingleQuote()
            .UseDoubleQuote()
            .UseCollapseWhitespace()
            .SetFilter(x => x.Value.IsNotEmpty())
            .Add("=");

        var lines = new[] { "s=5", "s= 5", "s =5", "s =    5" };

        foreach (var test in lines)
        {
            IReadOnlyList<IToken> tokens = parser.Parse(test);

            Option<ILangTree> tree = LangParser.Parse(root, tokens);
            tree.Should().NotBeNull();
            tree.IsOk().Should().BeTrue();
            tree.Return().Action(x =>
            {
                x.Children.Count.Should().Be(3);
                x.Children[0].Parent.Should().BeSameAs(x);
                x.Children[0].SyntaxNode.Type.Should().Be(LangType.Value);
                x.Children[0].SyntaxNode.Name.Should().Be("lvalue");
                x.Children[0].Value.Should().Be("s");

                x.Children[1].Parent.Should().BeSameAs(x);
                x.Children[1].SyntaxNode.Type.Should().Be(LangType.SyntaxToken);
                x.Children[1].SyntaxNode.Name.Should().Be("equal");
                x.Children[1].Value.Should().Be("=");

                x.Children[2].Parent.Should().BeSameAs(x);
                x.Children[2].SyntaxNode.Type.Should().Be(LangType.Value);
                x.Children[2].SyntaxNode.Name.Should().Be("rvalue");
                x.Children[2].Value.Should().Be("5");
            });
        }
    }
}
