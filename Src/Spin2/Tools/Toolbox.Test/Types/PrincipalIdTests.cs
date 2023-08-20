using FluentAssertions;
using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.Test.Types;

public class PrincipalIdTests
{
    [Theory]
    [InlineData("a@a.com")]
    [InlineData("abcdefghijklmnopqrstuvwxyz-._0123456789@abcdefghijklmnopqrstuvwxyz0123-456789.com")]
    public void TestValidFormats(string ownerId)
    {
        PrincipalId.IsValid(ownerId).Should().BeTrue();
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
        PrincipalId.IsValid(ownerId).Should().BeFalse();
    }

    [Fact]
    public void TestOwnerId()
    {
        const string name = "name@domain.com";
        var ownerId = new PrincipalId(name);
        ownerId.Id.Should().Be(name);
        (ownerId == name).Should().BeTrue();

        var ownerId2 = new PrincipalId(name);
        (ownerId == ownerId2).Should().BeTrue();
    }

    [Fact]
    public void TestEqual()
    {
        var p1 = new PrincipalId("name1@domain.com");
        var p2 = new PrincipalId("name1@domain.com");
        (p1 == p2).Should().BeTrue();
    }

    [Fact]
    public void EqualToString()
    {
        var p1 = new PrincipalId("name1@domain.com");
        (p1 == "name1@domain.com").Should().BeTrue();
        (p1 != "name2@domain.com").Should().BeTrue();
        ("name1@domain.com" == p1).Should().BeTrue();
        ("name2@domain.com" != p1).Should().BeTrue();
    }

    [Fact]
    public void SerializationTest()
    {
        var p1 = new PrincipalId("name1@domain.com");

        string json = p1.ToJson();

        var p2 = json.ToObject<PrincipalId>();
        (p1 == p2).Should().BeTrue();
    }

    [Fact]
    public void ImplicitAssigment()
    {
        PrincipalId b = "name1@domain.com";
        (b.Id == "name1@domain.com").Should().BeTrue();
        (b == "name1@domain.com").Should().BeTrue();

        string sb = b;
        (sb == "name1@domain.com").Should().BeTrue();
    }
}
