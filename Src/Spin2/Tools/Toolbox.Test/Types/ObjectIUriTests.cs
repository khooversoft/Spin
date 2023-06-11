using System.Text.RegularExpressions;
using FluentAssertions;
using Toolbox.Application;
using Toolbox.Types;

namespace Toolbox.Test.Types;

public class ObjectIUriTests
{
    [Theory]
    [InlineData("", false)]
    [InlineData("&", false)]
    [InlineData("a", true)]
    [InlineData("domain", true)]
    [InlineData("do88main", true)]
    [InlineData("domain3", true)]
    [InlineData("domain3-", false)]
    [InlineData("doma#in", false)]
    [InlineData("do$main", false)]
    [InlineData("dom-ain", true)]
    [InlineData("domain/", true)]
    [InlineData("domain/path", true)]
    [InlineData("a/b", true)]
    [InlineData("domain/path/", true)]
    [InlineData("dom-ain/pa-th", true)]
    [InlineData("domain/path/path2", true)]
    [InlineData("domain/pa-th/pa-th2", true)]
    [InlineData("domain/path/path2/", true)]

    [InlineData(".domain", false)]
    [InlineData("domain.", true)]
    [InlineData("domain/.file", true)]
    public void TestRegex(string input, bool expected)
    {
        bool pass = ObjectUri.IsValid(input);
        (expected == pass).Should().BeTrue();
    }

    [Theory]
    [InlineData("domain", "domain", null)]
    [InlineData("domain/path", "domain", "path")]
    [InlineData("d1/path", "d1", "path")]
    [InlineData("domain/path/path2", "domain", "path/path2")]
    public void TestObjectIdParse(string input, string domain, string? path)
    {
        ObjectUri objectId = input.ToObjectUri();
        objectId.Domain.Should().Be(domain);
        objectId.Path.Should().Be(path);
    }

    [Fact]
    public void TestLevel1()
    {
        ObjectUri uri = "domain".ToObjectUri();
        uri.Domain.Should().Be("domain");
        uri.Path.Should().BeNull();
        uri.GetParent().Should().Be("domain");
        uri.GetFolder().Should().BeNull();
        uri.GetFile().Should().BeNull();
    }

    [Fact]
    public void TestLevel2()
    {
        ObjectUri uri = "domain/file".ToObjectUri();
        uri.Domain.Should().Be("domain");
        uri.Path.Should().Be("file");
        uri.GetParent().Should().Be("domain");
        uri.GetFolder().Should().BeNull();
        uri.GetFile().Should().Be("file");
    }

    [Fact]
    public void TestLevel3()
    {
        ObjectUri uri = "domain/folder/file".ToObjectUri();
        uri.Domain.Should().Be("domain");
        uri.Path.Should().Be("folder/file");
        uri.GetParent().Should().Be("domain");
        uri.GetFolder().Should().Be("folder");
        uri.GetFile().Should().Be("file");
    }

    [Fact]
    public void TestLevel4()
    {
        ObjectUri uri = "domain/folder1/folder2/file".ToObjectUri();
        uri.Domain.Should().Be("domain");
        uri.Path.Should().Be("folder1/folder2/file");
        uri.GetParent().Should().Be("domain/folder1");
        uri.GetFolder().Should().Be("folder1/folder2");
        uri.GetFile().Should().Be("file");
    }

    [Fact]
    public void SetDomain()
    {
        ObjectUri uri = "folder/file".ToObjectUri().SetDomain("domain");
        uri.ToString().Should().Be("domain/folder/file");
    }

    [Fact]
    public void ReplaceDomain()
    {
        ObjectUri uri = "domain1/folder/file".ToObjectUri().WithDomain("domain2");
        uri.ToString().Should().Be("domain2/folder/file");
    }
}
