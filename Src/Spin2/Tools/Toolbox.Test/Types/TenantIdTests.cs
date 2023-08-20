using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        TenantId.IsValid(nameId).Should().BeFalse();
    }

    [Theory]
    [InlineData("name")]
    [InlineData("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789$_-A")]
    public void Valid(string nameId)
    {
        TenantId.IsValid(nameId).Should().BeTrue();
    }

    [Fact]
    public void Equal()
    {
        var name1 = new TenantId("name1");
        var name2 = new TenantId("name1");

        (name1 == name2).Should().BeTrue();
        (name1 != name2).Should().BeFalse();

        (name1 == "name1").Should().BeTrue();
        (name1 != "name1").Should().BeFalse();
        (name1 == "name1").Should().BeTrue();
        (name1 != "name1").Should().BeFalse();
    }

    [Fact]
    public void TenantIdNotEqual()
    {
        var name1 = new TenantId("name1");
        var name2 = new TenantId("name2");

        (name1 == name2).Should().BeFalse();
        (name1 != name2).Should().BeTrue();
    }

    [Fact]
    public void ImplicitAssigment()
    {
        TenantId b = "name1";
        (b.Value == "name1").Should().BeTrue();
        (b == "name1").Should().BeTrue();

        string sb = b;
        (sb == "name1").Should().BeTrue();
    }
}
