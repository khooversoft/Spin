using FluentAssertions;
using Toolbox.Extensions;
using Toolbox.Types;

namespace NBlog.sdk.test;

public class CommandGrammarParserTests
{
    [Fact]
    public void Assignment()
    {
        Option<IReadOnlyList<CommandNode>> commandNodeListOption = CommandGrammarParser.Parse("spin/tools/SpinClusterCommandSyntaxSummary/summary = SpinClusterCommandSyntaxSummary.md");
        commandNodeListOption.IsOk().Should().BeTrue();

        IReadOnlyList<CommandNode> commandNodeList = commandNodeListOption.Return();
        commandNodeList.Count.Should().Be(1);

        commandNodeList[0].Action(x =>
        {
            x.Attributes.Should().NotBeNull();
            x.Attributes.Count.Should().Be(0);

            x.FileId.Should().Be("spin/tools/SpinClusterCommandSyntaxSummary/summary");
            x.LocalFilePath.Should().Be("SpinClusterCommandSyntaxSummary.md");
        });
    }

    [Fact]
    public void SingleAttributeWithAssignment()
    {
        Option<IReadOnlyList<CommandNode>> commandNodeListOption = CommandGrammarParser.Parse("[summary] spin/tools/SpinClusterCommandSyntaxSummary/summary = SpinClusterCommandSyntaxSummary.md");
        commandNodeListOption.IsOk().Should().BeTrue();

        IReadOnlyList<CommandNode> commandNodeList = commandNodeListOption.Return();
        commandNodeList.Count.Should().Be(1);

        commandNodeList[0].Action(x =>
        {
            x.Attributes.Should().NotBeNull();
            x.Attributes.Count.Should().Be(1);
            x.Attributes[0].Should().Be("summary");

            x.FileId.Should().Be("spin/tools/SpinClusterCommandSyntaxSummary/summary");
            x.LocalFilePath.Should().Be("SpinClusterCommandSyntaxSummary.md");
        });
    }

    [Fact]
    public void TwoAttributeWithAssignment()
    {
        Option<IReadOnlyList<CommandNode>> commandNodeListOption = CommandGrammarParser.Parse("[summary;main] spin/tools/SpinClusterCommandSyntaxSummary/summary = SpinClusterCommandSyntaxSummary.md");
        commandNodeListOption.IsOk().Should().BeTrue();

        IReadOnlyList<CommandNode> commandNodeList = commandNodeListOption.Return();
        commandNodeList.Count.Should().Be(1);

        commandNodeList[0].Action(x =>
        {
            x.Attributes.Should().NotBeNull();
            x.Attributes.Count.Should().Be(2);
            x.Attributes[0].Should().Be("summary");
            x.Attributes[1].Should().Be("main");

            x.FileId.Should().Be("spin/tools/SpinClusterCommandSyntaxSummary/summary");
            x.LocalFilePath.Should().Be("SpinClusterCommandSyntaxSummary.md");
        });
    }
}
