using FluentAssertions;
using Toolbox.Extensions;

namespace Toolbox.Test.Extensions;

public class StringParsingTest
{
    [Theory]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("a", 'a')]
    [InlineData("ab", 'a')]
    [InlineData("1ab", '1')]
    public void TestFirstCharacter(string? input, char? expected)
    {
        char? chr = input.GetFirstChar();
        chr.Should().Be(expected);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("a", null)]
    [InlineData("ab", 'b')]
    [InlineData("1ab", 'b')]
    public void TestLastCharacter(string? input, char? expected)
    {
        char? chr = input.GetLastChar();
        chr.Should().Be(expected);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("a", null)]
    [InlineData("ab", null)]
    [InlineData("1ab", "a")]
    [InlineData("1acb", "ac")]
    public void TestMiddleCharacters(string? input, string? expected)
    {
        string? str = input.GetMiddleChars();
        str.Should().Be(expected);
    }
}
