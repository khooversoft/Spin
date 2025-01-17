using Toolbox.Tools.Should;
using Toolbox.Types;

namespace Toolbox.Test.Types;

public class DomainTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("`()")]
    [InlineData("name")]
    [InlineData("domain.")]
    [InlineData("domain.com.")]
    [InlineData("domain..com")]
    [InlineData("domain.com-")]
    [InlineData("-domain.com")]
    [InlineData("2domain.com")]
    public void Invalid(string? nameId)
    {
        IdPatterns.IsDomain(nameId).Should().BeFalse();
    }

    [Theory]
    [InlineData("abcdefghijklmnopqrstuvwxyzABCDEFGHIJ.KLMNOPQRSTUVWXYZ0123456789$-A")]
    [InlineData("domain.com")]
    [InlineData("dom.ain.com")]
    public void Valid(string nameId)
    {
        IdPatterns.IsDomain(nameId).Should().BeTrue();
    }
}
