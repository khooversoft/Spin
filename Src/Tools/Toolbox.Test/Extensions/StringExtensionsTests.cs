using Toolbox.Extensions;
using Toolbox.Tools;

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
    public void TestRemoveTrailing(string? subject, char ch, string? expected)
    {
        string? result = subject.RemoveTrailing(ch);
        result.Be(expected);
    }

    [Theory]
    [InlineData(null, "dkd", false)]
    [InlineData("test", null, false)]
    [InlineData("", "dkd", false)]
    [InlineData("test", "test", true)]
    [InlineData("test:value", "test:value", true)]
    [InlineData("test", "Test", true)]
    [InlineData("test:value", "test:*", true)]
    [InlineData("test:value", "tEst:*", true)]
    [InlineData("test:value", "test?*", true)]
    [InlineData("test:value", "*:value", true)]
    public void TestWildcardTest(string? input, string? pattern, bool expected)
    {
        bool result = input.Like(pattern);
        result.Be(expected);
    }
}
