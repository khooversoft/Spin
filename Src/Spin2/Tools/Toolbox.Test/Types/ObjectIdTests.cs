using System.Text.RegularExpressions;
using FluentAssertions;
using Toolbox.Application;
using Toolbox.Types;

namespace Toolbox.Test.Types;

public class ObjectIdTests
{
    [Theory]
    [InlineData("domain:path")]
    [InlineData("domain:path/path2")]
    [InlineData("domain:path/path2/")]
    [InlineData("path:/path2/")]
    [InlineData("path:/.path2/")]
    [InlineData("d:a/b/c/d")]
    [InlineData("-path:/path2")]
    [InlineData("path:/.path2./")]
    public void TestPositivePatterns(string input)
    {
        ObjectId.IsValid(input).Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("domain:/")]
    [InlineData("domain:")]
    [InlineData("path")]
    [InlineData("path:/data#")]
    [InlineData(".path/path2")]
    [InlineData("path:/da&ta")]
    [InlineData("5path/path2/")]
    public void TestNegativePatterns(string input)
    {
        ObjectId.IsValid(input).Should().BeFalse();
    }

    [Theory]
    [InlineData("domain:path", "domain", "path")]
    [InlineData("d1:path", "d1", "path")]
    [InlineData("domain:path/path2", "domain", "path/path2")]
    [InlineData("d:a", "d", "a")]
    [InlineData("d:a/b/c/d", "d", "a/b/c/d")]
    public void TestObjectIdParse(string input, string domain, string? path)
    {
        ObjectId objectId = input.ToObjectId();
        objectId.Domain.Should().Be(domain);
        objectId.Path.Should().Be(path);
    }
}
