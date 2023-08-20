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
        IdPatterns.IsBlockType(nameId).Should().BeFalse();
    }

    [Theory]
    [InlineData("name")]
    [InlineData("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ$._-0123456789")]
    public void ValidBlockType(string nameId)
    {
        IdPatterns.IsBlockType(nameId).Should().BeTrue();
    }
}
