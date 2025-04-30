using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Test.Types;

public class IdPatternsTests
{
    [Theory]
    [InlineData("schema")]
    [InlineData("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789$-A")]
    public void IsNameOk(string subject)
    {
        IdPatterns.IsName(subject).BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("0abc")]
    [InlineData("-abc")]
    public void IsNameError(string? subject)
    {
        IdPatterns.IsName(subject).BeFalse();
    }

    [Theory]
    [InlineData("path")]
    [InlineData("abcdefghijklmnopqrstuvwxyz@ABC/DEFGHIJKLMNOPQRSTUVWXYZ-.$0123456789")]
    public void IsPathOk(string subject)
    {
        IdPatterns.IsPath(subject).BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("0abc")]
    [InlineData("-abc")]
    public void IsPathError(string? subject)
    {
        IdPatterns.IsPath(subject).BeFalse();
    }


    [Theory]
    [InlineData("abcdefghijklmnopqrstuvwxyzABCDEFGHIJ.KLMNOPQRSTUVWXYZ0123456789$-A")]
    [InlineData("domain.com")]
    [InlineData("dom.ain.com")]
    public void IsDomainOk(string nameId)
    {
        IdPatterns.IsDomain(nameId).BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("0abc")]
    [InlineData("-abc")]
    [InlineData("`()")]
    [InlineData("name")]
    [InlineData("domain.")]
    [InlineData("domain.com.")]
    [InlineData("domain..com")]
    [InlineData("domain.com-")]
    [InlineData("-domain.com")]
    [InlineData("2domain.com")]
    public void IsDomainError(string? subject)
    {
        IdPatterns.IsDomain(subject).BeFalse();
    }

    [Theory]
    [InlineData("kid:user1@domain.com")]
    [InlineData("kid:user1@domain.com/path")]
    [InlineData("kid:user1@domain.com/path/path")]
    [InlineData("kid:abcdefghijklmnopqrstuvwxyzA@BCDEFGHIJKLMNOPQRSTUVWXYZ$-.A0123456789")]
    public void IsKeyIdOk(string subject)
    {
        IdPatterns.IsKeyId(subject).BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("0abc")]
    [InlineData("-abc")]
    public void IsKeyIdError(string? subject)
    {
        IdPatterns.IsKeyId(subject).BeFalse();
    }

    [Theory]
    [InlineData("user1@domain.com")]
    [InlineData("abcdefghijklmnopqrstuvwxyzA@BCDEFGHIJKLMNOPQRSTUVWXYZ$-.A0123456789")]
    [InlineData("user:abcdefghijklmnopqrstuvwxyzA@BCDEFGHIJKLMNOPQRSTUVWXYZ$-.A0123456789")]
    public void IsPrincipalIdOk(string subject)
    {
        IdPatterns.IsPrincipalId(subject).BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("0abc")]
    [InlineData("-abc")]
    [InlineData("user1@domain.com/path")]
    [InlineData("  ")]
    [InlineData("@a")]
    [InlineData("n..ame@domain.com")]
    [InlineData("name@domain..com")]
    [InlineData("-name@domain.com")]
    [InlineData("name.@domain.com")]
    [InlineData("<>()*#.domain.com")]
    public void IsPrincipalError(string? subject)
    {
        IdPatterns.IsPrincipalId(subject).BeFalse();
    }
}
