using System.Text.RegularExpressions;
using FluentAssertions;
using Toolbox.Application;

namespace Toolbox.Abstractions.Test.Type;

public class DocumentIdPathRegexTests
{
    [Theory]
    [InlineData("", false)]
    [InlineData("resource:/", false)]
    [InlineData("resource:", false)]
    [InlineData("resource:path", true)]
    [InlineData("resource:path/path2", true)]
    [InlineData("resource:path/path2/", false)]
    [InlineData("path", true)]
    [InlineData("path/path2", true)]
    [InlineData("path/path2/", false)]
    public void TestRegex(string input, bool expected)
    {
        Match m = Regex.Match(input, ToolboxConstants.DocumentIdRegexPattern, RegexOptions.IgnoreCase);
        (expected == m.Success).Should().BeTrue();
    } 
}
