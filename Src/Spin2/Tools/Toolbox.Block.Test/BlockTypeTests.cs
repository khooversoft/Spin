using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Toolbox.Types;

namespace Toolbox.Block.Test;

public class BlockTypeTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("`()")]
    public void InvalidBlockType(string nameId)
    {
        BlockType.IsValid(nameId).Should().BeFalse();
    }

    [Theory]
    [InlineData("name")]
    [InlineData("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ$._-0123456789")]
    public void ValidBlockType(string nameId)
    {
        BlockType.IsValid(nameId).Should().BeTrue();
    }

    [Fact]
    public void BlockTypeEqual()
    {
        var name1 = new BlockType("name1");
        var name2 = new BlockType("name1");

        (name1 == name2).Should().BeTrue();
        (name1 != name2).Should().BeFalse();

        ("name1" == name1).Should().BeTrue();
        ("name1" != name1).Should().BeFalse();
        (name1 == "name1").Should().BeTrue();
        (name1 != "name1").Should().BeFalse();
    }

    [Fact]
    public void BlockTypeNotEqual()
    {
        var name1 = new BlockType("name1");
        var name2 = new BlockType("name2");

        (name1 == name2).Should().BeFalse();
        (name1 != name2).Should().BeTrue();
    }

    [Fact]
    public void ImplicitBlockTypeAssigment()
    {
        BlockType b = "blockType";
        (b.Value == "blockType").Should().BeTrue();
        (b == "blockType").Should().BeTrue();

        string sb = b;
        (sb == "blockType").Should().BeTrue();
    }
}
