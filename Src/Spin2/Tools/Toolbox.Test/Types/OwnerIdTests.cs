using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Toolbox.Types;

namespace Toolbox.Test.Types;

public class OwnerIdTests
{
    [Theory]
    [InlineData("a@a.com")]
    [InlineData("abcdefghijklmnopqrstuvwxyz-._0123456789@abcdefghijklmnopqrstuvwxyz0123-456789.com")]
    public void TestValidFormats(string ownerId)
    {
        OwnerId.IsValid(ownerId).StatusCode.Should().Be(StatusCode.OK);
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
        OwnerId.IsValid(ownerId).StatusCode.Should().Be(StatusCode.BadRequest);
    }

    [Fact]
    public void TestOwnerId()
    {
        const string name = "name@domain.com";
        var ownerId = new OwnerId(name);
        ownerId.Id.Should().Be(name);
        (ownerId == name).Should().BeTrue();

        var ownerId2 = new OwnerId(name);
        (ownerId == ownerId2).Should().BeTrue();
    }
}
