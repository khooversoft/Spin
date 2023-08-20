using FluentAssertions;
using Toolbox.Types;

namespace Toolbox.Test.Types;

public class IdPatternsTests
{
    [Theory]
    [InlineData("schema")]
    [InlineData("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ-_.$0123456789")]
    public void IsNameOk(string subject)
    {
        IdPatterns.IsName(subject).Should().BeTrue();
        IdPatterns.IsTenant(subject).Should().BeTrue();
        IdPatterns.IsSchema(subject).Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("0abc")]
    [InlineData("-abc")]
    public void IsNameError(string subject)
    {
        IdPatterns.IsName(subject).Should().BeFalse();
        IdPatterns.IsSchema(subject).Should().BeFalse();
        IdPatterns.IsTenant(subject).Should().BeFalse();
    }

    [Theory]
    [InlineData("path")]
    [InlineData("abcdefghijklmnopqrstuvwxyz@ABC/DEFGHIJKLMNOPQRSTUVWXYZ-_.$0123456789")]
    public void IsPathOk(string subject)
    {
        IdPatterns.IsPath(subject).Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("0abc")]
    [InlineData("-abc")]
    public void IsPathError(string subject)
    {
        IdPatterns.IsPath(subject).Should().BeFalse();
    }

    [Theory]
    [InlineData("domain.com")]
    [InlineData("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ$-_.A0123456789")]
    public void IsDomainOk(string subject)
    {
        IdPatterns.IsDomain(subject).Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("0abc")]
    [InlineData("-abc")]
    public void IsDomainError(string subject)
    {
        IdPatterns.IsDomain(subject).Should().BeFalse();
    }

    [Theory]
    [InlineData("kid:user1@domain.com")]
    [InlineData("kid:user1@domain.com/path")]
    [InlineData("kid:user1@domain.com/path/path")]
    [InlineData("kid:abcdefghijklmnopqrstuvwxyzA@BCDEFGHIJKLMNOPQRSTUVWXYZ$-_.A0123456789")]
    public void IsKeyIdOk(string subject)
    {
        IdPatterns.IsKeyId(subject).Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("0abc")]
    [InlineData("-abc")]
    public void IsKeyIdError(string subject)
    {
        IdPatterns.IsKeyId(subject).Should().BeFalse();
    }

    [Theory]
    [InlineData("user1@domain.com")]
    [InlineData("abcdefghijklmnopqrstuvwxyzA@BCDEFGHIJKLMNOPQRSTUVWXYZ$-_.A0123456789")]
    [InlineData("user:abcdefghijklmnopqrstuvwxyzA@BCDEFGHIJKLMNOPQRSTUVWXYZ$-_.A0123456789")]
    public void IsPrincipalIdOk(string subject)
    {
        IdPatterns.IsPrincipalId(subject).Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("0abc")]
    [InlineData("-abc")]
    [InlineData("user1@domain.com/path")]
    public void IsPrincipalError(string subject)
    {
        IdPatterns.IsPrincipalId(subject).Should().BeFalse();
    }
}
