using FluentAssertions;
using Toolbox.Types;

namespace Toolbox.Test.Types;

public class TenantIdTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("`()")]
    public void Invalid(string nameId)
    {
        IdPatterns.IsTenant(nameId).Should().BeFalse();
    }

    [Theory]
    [InlineData("name")]
    [InlineData("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789$_-A")]
    public void Valid(string nameId)
    {
        IdPatterns.IsTenant(nameId).Should().BeTrue();
    }
}
