using FluentAssertions;
using Toolbox.Types;

namespace Toolbox.Test.Types;

public class PrincipalIdTests
{
    [Theory]
    [InlineData("a@a.com")]
    [InlineData("abcdefghijklmnopqrstuvwxyz-._0123456789@abcdefghijklmnopqrstuvwxyz0123-456789.com")]
    public void TestValidFormats(string ownerId)
    {
        IdPatterns.IsPrincipalId(ownerId).Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("@a")]
    [InlineData("n..ame@domain.com")]
    [InlineData("name@domain..com")]
    [InlineData("-name@domain.com")]
    [InlineData("name.@domain.com")]
    [InlineData("<>()*#.domain.com")]
    public void TestInvalidFormats(string ownerId)
    {
        IdPatterns.IsPrincipalId(ownerId).Should().BeFalse();
    }
}
