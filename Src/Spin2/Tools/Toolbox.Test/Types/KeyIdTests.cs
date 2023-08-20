//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using FluentAssertions;
//using Toolbox.Extensions;
//using Toolbox.Types;

//namespace Toolbox.Test.Types;

//public class KeyIdTests
//{
//    [Theory]
//    [InlineData("a@a.com")]
//    [InlineData("abcdefghijklmnopqrstuvwxyz-._0123456789@abcdefghijklmnopqrstuvwxyz0123-456789.com")]
//    [InlineData("abcdefghijklmnopqrstuvwxyz-._0123456789@abcdefghijklmnopqrstuvwxyz0123-456789.com/path")]
//    [InlineData("abcdefghijklmnopqrstuvwxyz-._0123456789@abcdefghijklmnopqrstuvwxyz0123-456789.com/path/sub")]
//    public void TestValidFormats(string subject)
//    {
//        KeyId.IsValid(subject).Should().BeTrue();
//    }

//    [Theory]
//    [InlineData(null)]
//    [InlineData("")]
//    [InlineData("  ")]
//    [InlineData("@a")]
//    [InlineData("n..ame@domain.com")]
//    [InlineData("name@domain..com")]
//    [InlineData("-name@domain.com")]
//    [InlineData("name.@domain.com")]
//    [InlineData("<>()*#.domain.com")]
//    [InlineData("path")]
//    [InlineData("path/path")]
//    public void TestInvalidFormats(string subject)
//    {
//        KeyId.IsValid(subject).Should().BeFalse();
//    }

//    [Fact]
//    public void TestKeyId()
//    {
//        const string name = "name@domain.com";
//        var ownerId = new KeyId(name);
//        ownerId.Value.Should().Be(name);
//        (ownerId == name).Should().BeTrue();

//        var ownerId2 = new KeyId(name);
//        (ownerId == ownerId2).Should().BeTrue();
//    }

//    [Fact]
//    public void TestKeyIdWithPath()
//    {
//        const string name = "name@domain.com/Path";
//        var ownerId = new KeyId(name);
//        ownerId.Value.Should().Be(name);
//        (ownerId == name).Should().BeTrue();

//        var ownerId2 = new KeyId(name);
//        (ownerId == ownerId2).Should().BeTrue();
//    }

//    [Fact]
//    public void TestEqual()
//    {
//        var p1 = new KeyId("name1@domain.com");
//        var p2 = new KeyId("name1@domain.com");
//        (p1 == p2).Should().BeTrue();
//    }

//    [Fact]
//    public void EqualToString()
//    {
//        var p1 = new KeyId("name1@domain.com");
//        (p1 == "name1@domain.com").Should().BeTrue();
//        (p1 != "name2@domain.com").Should().BeTrue();
//        ("name1@domain.com" == p1).Should().BeTrue();
//        ("name2@domain.com" != p1).Should().BeTrue();

//        (PrincipalId principalId, string? path) = p1.GetDetails();
//        principalId.Id.Should().Be("name1@domain.com");
//        path.Should().BeNull();
//    }

//    [Fact]
//    public void EqualToStringWithPath()
//    {
//        var p1 = new KeyId("name1@domain.com/path");
//        (p1 == "name1@domain.com/path").Should().BeTrue();
//        (p1 != "name2@domain.com/path").Should().BeTrue();
//        ("name1@domain.com/path" == p1).Should().BeTrue();
//        ("name2@domain.com/path" != p1).Should().BeTrue();

//        (PrincipalId principalId, string? path) = p1.GetDetails();
//        principalId.Id.Should().Be("name1@domain.com");
//        path.Should().Be("path");
//    }

//    [Fact]
//    public void EqualToStringWithPaths()
//    {
//        var p1 = new KeyId("name1@domain.com/path/subType");
//        (p1 == "name1@domain.com/path/subType").Should().BeTrue();
//        (p1 != "name2@domain.com/path/subType").Should().BeTrue();
//        ("name1@domain.com/path/subType" == p1).Should().BeTrue();
//        ("name2@domain.com/path/subType" != p1).Should().BeTrue();
//    }

//    [Fact]
//    public void SerializationTest()
//    {
//        var p1 = new KeyId("name1@domain.com");

//        string json = p1.ToJson();

//        var p2 = json.ToObject<KeyId>();
//        (p1 == p2).Should().BeTrue();
//    }

//    [Fact]
//    public void SerializationTestWithPath()
//    {
//        var p1 = new KeyId("name1@domain.com/ownerKey");

//        string json = p1.ToJson();

//        var p2 = json.ToObject<KeyId>();
//        (p1 == p2).Should().BeTrue();
//    }

//    [Fact]
//    public void ImplicitAssigment()
//    {
//        KeyId b = "name1@domain.com";
//        (b.Value == "name1@domain.com").Should().BeTrue();
//        (b == "name1@domain.com").Should().BeTrue();

//        string sb = b;
//        (sb == "name1@domain.com").Should().BeTrue();
//    }
//}
