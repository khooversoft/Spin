using FluentAssertions;
using Toolbox.Types;

namespace Toolbox.Test.Types;

public class NameIdTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("`()")]
    public void InvalidNameId(string nameId)
    {
        IdPatterns.IsName(nameId).Should().BeFalse();
    }

    [Theory]
    [InlineData("name")]
    [InlineData("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789$_-A")]
    public void ValidNameIds(string nameId)
    {
        IdPatterns.IsName(nameId).Should().BeTrue();
    }
}
