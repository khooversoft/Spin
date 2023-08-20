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
        NameId.IsValid(nameId).Should().BeFalse();
    }

    [Theory]
    [InlineData("name")]
    [InlineData("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789$_-A")]
    public void ValidNameIds(string nameId)
    {
        NameId.IsValid(nameId).Should().BeTrue();
    }

    [Fact]
    public void NameIdEqual()
    {
        var name1 = new NameId("name1");
        var name2 = new NameId("name1");

        (name1 == name2).Should().BeTrue();
        (name1 != name2).Should().BeFalse();

        (name1 == "name1").Should().BeTrue();
        (name1 != "name1").Should().BeFalse();
        (name1 == "name1").Should().BeTrue();
        (name1 != "name1").Should().BeFalse();
    }

    [Fact]
    public void NameIdNotEqual()
    {
        var name1 = new NameId("name1");
        var name2 = new NameId("name2");

        (name1 == name2).Should().BeFalse();
        (name1 != name2).Should().BeTrue();
    }

    [Fact]
    public void ImplicitAssigment()
    {
        NameId b = "name1";
        (b.Value == "name1").Should().BeTrue();
        (b == "name1").Should().BeTrue();

        string sb = b;
        (sb == "name1").Should().BeTrue();
    }
}
