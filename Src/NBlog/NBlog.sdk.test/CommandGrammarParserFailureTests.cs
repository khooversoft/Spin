using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Toolbox.Types;

namespace NBlog.sdk.test;

public class CommandGrammarParserFailureTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("abc")]
    [InlineData("abc=")]
    [InlineData("[] abc=asdf")]
    [InlineData("[abc]")]
    [InlineData("[dkdkd,] abc=asdf")]
    public void ShouldFail(string? rawData)
    {
        Option<IReadOnlyList<CommandNode>> commandNodeListOption = CommandGrammarParser.Parse(rawData!);
        commandNodeListOption.IsError().Should().BeTrue();
    }
}
