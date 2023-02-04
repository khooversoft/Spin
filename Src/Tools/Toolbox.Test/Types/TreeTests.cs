using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Abstractions.Tools;
using Toolbox.Extensions;
using Toolbox.Types.Structure;
using Xunit;

namespace Toolbox.Test.Types;

public class SampleClass : TreeNode<ITreeNode>, ITreeNode { }

public class TreeTests
{
    [Fact]
    public void SimpleTreeNode_ShouldPass()
    {
        var root = new SampleClass() + new SampleClass();
        root.Count.Should().Be(1);
    }

    [Fact]
    public void CreateAddOneNode_ShouldPass()
    {
        var tree = new Tree();

        tree.Add(new SampleClass());

        tree.Count.Should().Be(1);
        tree.OfType<SampleClass>().Count().Should().Be(1);
    }

    [Fact]
    public void CreateAddNodeCompose_ShouldPass()
    {
        var tree = new Tree()
            + new SampleClass()
            + (new SampleClass() + new SampleClass() + new SampleClass())
            + new SampleClass();

        tree.Count.Should().Be(3);
        tree.OfType<SampleClass>().Count().Should().Be(3);

        tree.OfType<SampleClass>().First().Count.Should().Be(0);
        tree.OfType<SampleClass>().Skip(1).First().Count.Should().Be(2);
        tree.OfType<SampleClass>().Skip(2).First().Count.Should().Be(0);
    }

    [Fact]
    public void CreateAddDifferentTypesNodeCompose_ShouldPass()
    {
        var tree = new Tree()
            + new SampleClass()
            + (new SampleClass() + new SampleClass() + new SampleClass())
            + new SampleClass();

        tree.Count.Should().Be(3);
        tree.OfType<SampleClass>().Count().Should().Be(3);

        tree.Take(1).OfType<SampleClass>().First().Count.Should().Be(0);
        tree.Skip(1).Take(1).OfType<SampleClass>().First().Count.Should().Be(2);
        tree.Skip(2).OfType<SampleClass>().First().Count.Should().Be(0);
    }

    [Fact]
    public void CreateAddOneWithOperatorNode_ShouldPass()
    {
        Tree tree = new Tree();

        tree += new SampleClass();

        tree.Count.Should().Be(1);
        tree.OfType<SampleClass>().Count().Should().Be(1);
    }

    [Fact]
    public void CreateTree_ShouldPass()
    {
        var tree = new Tree()
        {
            new SampleClass(),
            new SampleClass(),
        };

        tree.Count.Should().Be(2);
        tree.OfType<SampleClass>().Count().Should().Be(2);
    }

    [Fact]
    public void CreateNestedTree_ShouldPass()
    {
        var tree = new Tree()
        {
            new SampleClass(),
            new SampleClass()
            {
                new SampleClass(),
            },
            new SampleClass(),
        };

        tree.Count.Should().Be(3);
        tree.Skip(1).OfType<SampleClass>().Take(1).First().Count.Should().Be(1);
    }

    [Fact]
    public void GeneralCursorOperations_ShouldPass()
    {
        var tree = new Tree();
        tree.Count.Should().Be(0);
        tree.Cursor.NotNull();
        tree.Cursor.Current.NotNull();
        (tree.Cursor.Current == tree).Should().BeTrue();

        ITreeNode n1 = new SampleClass();
        tree.Cursor.Add(n1);
        tree.Count.Should().Be(1);
        tree.Cursor.NotNull();
        tree.Cursor.Current.NotNull();
        (tree.Cursor.Current == tree).Should().BeTrue();
        tree.Cursor.Current.Count.Should().Be(1);

        ITreeNode n2 = new SampleClass();
        tree.Cursor += n2;
        tree.Count.Should().Be(2);
        tree.Cursor.NotNull();
        tree.Cursor.Current.NotNull();
        (tree.Cursor.Current == tree).Should().BeTrue();
        tree.Cursor.Current.Count.Should().Be(2);


        // Set cursor to last added
        ITreeNode n3 = new SampleClass();
        tree.Cursor.Add(n3).Cursor.SetLast();

        tree.Cursor.NotNull();
        tree.Cursor.Current.NotNull();
        (tree.Cursor.Current == n3).Should().BeTrue();
        tree.Cursor.Current.Count.Should().Be(0);

        tree.Cursor += new SampleClass();
        (tree.Cursor.Current == n3).Should().BeTrue();
        tree.Cursor.Current.Count.Should().Be(1);

        // Move back to parent
        tree.Cursor.SetParent();
        tree.Cursor.Current.Count.Should().Be(3);
    }
}
