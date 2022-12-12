using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Parser.Syntax;
using Toolbox.Tools;

namespace Toolbox.Parser.Test;

public class SyntaxTreeTests
{
    [Fact]
    public void SimpleSyntaxNode_ShouldPass()
    {
        var root = new SyntaxNode<string>("#1");
        root += new SyntaxNode<string>("#2");

        root.Count.Should().Be(1);
    }

    [Fact]
    public void CreateAddOneNode_ShouldPass()
    {
        var tree = new SyntaxTree();

        tree.Add(new SyntaxNode<string>("#1"));

        tree.Count.Should().Be(1);
        tree.OfType<SyntaxNode<string>>().Count().Should().Be(1);

        tree.OfType<SyntaxNode<string>>().Select(x => x.Value)
            .Zip(new string[] { "#1" })
            .All(x => x.First == x.Second).Should().BeTrue();
    }

    [Fact]
    public void CreateAddOneNodeCompose_ShouldPass()
    {
        var tree = new SyntaxTree()
            + new SyntaxNode<string>("#1")
            + (new SyntaxNode<int>(2) + new SyntaxNode<string>("#2.1") + new SyntaxNode<string>("#2.1"))
            + new SyntaxNode<string>("#3");

        tree.Count.Should().Be(3);
        tree.OfType<SyntaxNode<string>>().Count().Should().Be(3);

        tree.OfType<SyntaxNode<string>>().First().Count.Should().Be(0);
        tree.OfType<SyntaxNode<string>>().Skip(1).First().Count.Should().Be(2);
        tree.OfType<SyntaxNode<string>>().Skip(2).First().Count.Should().Be(0);
    }

    [Fact]
    public void CreateAddOneWithOperatorNode_ShouldPass()
    {
        SyntaxTree tree = new SyntaxTree();

        tree += new SyntaxNode<string>("#1");

        tree.Count.Should().Be(1);
        tree.OfType<SyntaxNode<string>>().Count().Should().Be(1);

        tree.OfType<SyntaxNode<string>>().Select(x => x.Value)
            .Zip(new string[] { "#1" })
            .All(x => x.First == x.Second).Should().BeTrue();
    }

    [Fact]
    public void CreateSyntaxTree_ShouldPass()
    {
        var tree = new SyntaxTree()
        {
            new SyntaxNode<string>("#1"),
            new SyntaxNode<string>("#2"),
            SyntaxNode.Create("#3"),
        };

        tree.Count.Should().Be(3);
        tree.OfType<SyntaxNode<string>>().Count().Should().Be(3);

        tree.OfType<SyntaxNode<string>>().Select(x => x.Value)
            .Zip(new string[] { "#1", "#2", "#3" })
            .All(x => x.First == x.Second).Should().BeTrue();
    }

    [Fact]
    public void CreateNestedSyntaxTree_ShouldPass()
    {
        var tree = new SyntaxTree()
        {
            new SyntaxNode<string>("#1"),
            new SyntaxNode<string>("#2")
            {
                new SyntaxNode<string>("#2-1"),
            },
            new SyntaxNode<string>("#3"),
        };

        tree.Count.Should().Be(3);
        tree.OfType<SyntaxNode<string>>().Select(x => x.Value)
            .Zip(new string[] { "#1", "#2", "#3" })
            .All(x => x.First == x.Second).Should().BeTrue();

        tree.OfType<SyntaxNode<string>>().First(x => x.Value == "#2").Count.Should().Be(1);
        tree.OfType<SyntaxNode<string>>().First(x => x.Value == "#2").OfType<SyntaxNode<string>>().First().Value.Should().Be("#2-1");
    }

    [Fact]
    public void GeneralCursorOperations_ShouldPass()
    {
        var tree = new SyntaxTree();
        tree.Cursor.NotNull();
        tree.Cursor.Current.NotNull();
        (tree.Cursor.Current == tree).Should().BeTrue();

        ISyntaxNode n1 = new SyntaxNode<string>("#1");
        tree.Cursor.Add(n1);
        tree.Cursor.NotNull();
        tree.Cursor.Current.NotNull();
        (tree.Cursor.Current == tree).Should().BeTrue();
        tree.Cursor.Current.Count.Should().Be(1);

        ISyntaxNode n2 = new SyntaxNode<string>("#2");
        tree.Cursor += n2;
        tree.Cursor.NotNull();
        tree.Cursor.Current.NotNull();
        (tree.Cursor.Current == tree).Should().BeTrue();
        tree.Cursor.Current.Count.Should().Be(2);

        tree.OfType<SyntaxNode<string>>().Select(x => x.Value)
            .Zip(new string[] { "#1", "#2" })
            .All(x => x.First == x.Second).Should().BeTrue();


        // Set cursor to last added
        ISyntaxNode n3 = new SyntaxNode<string>("#3");
        tree.Cursor.Add(n3).Cursor.SetLast();

        tree.Cursor.NotNull();
        tree.Cursor.Current.NotNull();
        (tree.Cursor.Current == n3).Should().BeTrue();
        tree.Cursor.Current.Count.Should().Be(0);

        tree.Cursor += new SyntaxNode<string>("#4");
        (tree.Cursor.Current == n3).Should().BeTrue();
        tree.Cursor.Current.Count.Should().Be(1);

        tree.Cursor.Current.OfType<SyntaxNode<string>>().Select(x => x.Value)
            .Zip(new string[] { "#4" })
            .All(x => x.First == x.Second).Should().BeTrue();


        // Move back to parent
        tree.Cursor.SetParent();
        tree.Cursor.Current.Count.Should().Be(3);

        tree.OfType<SyntaxNode<string>>().Select(x => x.Value)
            .Zip(new string[] { "#1", "#2", "#3" })
            .All(x => x.First == x.Second).Should().BeTrue();
    }
}
