using FluentAssertions;
using Toolbox.Extensions;

namespace Toolbox.Test.Extensions;

public class StringExtensionsTests
{
    [Theory]
    [InlineData(null, '/', null)]
    [InlineData("a", '/', "a")]
    [InlineData("abc", '/', "abc")]
    [InlineData("/", '/', "")]
    [InlineData("/b", '/', "/b")]
    [InlineData("/bc", '/', "/bc")]
    [InlineData("//", '/', "/")]
    [InlineData("/ab/", '/', "/ab")]
    public void TestRemoveTrailing(string? subject, char ch, string expected)
    {
        string? result = subject.RemoveTrailing(ch);
        result.Should().Be(expected);
    }
}
