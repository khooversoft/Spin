using System.Text.RegularExpressions;
using FluentAssertions;
using Toolbox.Application;
using Toolbox.Types;

namespace Toolbox.Test.Types;

public class ObjectIdTests
{
    [Theory]
    [InlineData("", false)]
    [InlineData("domain:/", false)]
    [InlineData("domain:", false)]
    [InlineData("domain:path", true)]
    [InlineData("domain:path/path2", true)]
    [InlineData("domain:path/path2/", false)]
    [InlineData("path", false)]
    [InlineData("path/path2", false)]
    [InlineData("path/path2/", false)]
    [InlineData("d:a/b/c/d", true)]
    public void TestRegex(string input, bool expected)
    {
        bool pass = ObjectId.IsValid(input);
        (expected == pass).Should().BeTrue();
    }

    [Theory]
    [InlineData("domain:path", "domain", "path")]
    [InlineData("d1:path", "d1", "path")]
    [InlineData("domain:path/path2", "domain", "path/path2")]
    [InlineData("d:a", "d", "a")]
    [InlineData("d:a/b/c/d", "d", "a/b/c/d")]
    public void TestObjectIdParse(string input, string domain, string path)
    {
        ObjectId objectId = input.ToObjectId();
        objectId.Domain.Should().Be(domain);
        objectId.Path.Should().Be(path);
    }
}
