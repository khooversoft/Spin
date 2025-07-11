using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Test.Extensions;

public class RemoveDuplicateSequencesTests
{
    [Theory]
    [InlineData(null, ' ', null)]
    [InlineData("", ' ', "")]
    [InlineData("a", 'b', "a")]
    [InlineData("aa", 'a', "a")]
    [InlineData("aa", 'b', "aa")]
    [InlineData("aaa", 'b', "aaa")]
    [InlineData("aaa", 'a', "a")]
    [InlineData("bba", 'b', "ba")]
    [InlineData("a//b//c", '/', "a/b/c")]
    [InlineData("a//b//c/", '/', "a/b/c/")]
    [InlineData("a//b//c//", '/', "a/b/c/")]
    public void TestRemoveDuplicate(string? source, char token, string? expected)
    {
        string? result = source.RemoveDuplicates(token);
        result.Be(expected);
    }
}
