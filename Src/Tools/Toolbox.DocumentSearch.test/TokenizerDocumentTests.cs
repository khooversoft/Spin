using FluentAssertions;
using Toolbox.Extensions;

namespace Toolbox.DocumentSearch.test;

public class TokenizerDocumentTests
{
    [Theory]
    [InlineData(null, new string[0])]
    [InlineData("", new string[0])]
    [InlineData("notReserved", new[] { "notreserved" })]
    [InlineData("this works", new[] { "works" })]
    [InlineData("notReserved   works", new[] { "notreserved", "works" })]
    [InlineData("hello+works,next", new[] { "hello", "works", "next" })]
    public void Parse(string? value, string[] expected)
    {
        var tokens = new DocumentTokenizer().Parse(value!);
        tokens.Count.Should().Be(expected.Length, tokens.Select(x => x.ToString()).Join(';'));
        Enumerable.SequenceEqual(expected, tokens.Select(x => x.Word)).Should().BeTrue(tokens.Select(x => x.Word).Join(';'));
    }
}
