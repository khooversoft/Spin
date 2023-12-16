using FluentAssertions;
using Toolbox.Extensions;
using Toolbox.LangTools;
using Toolbox.Tools;
using Toolbox.Types;

namespace NBlog.sdk.test;

public class ArticleCommandGrammarTests
{
    [Fact]
    public void OnlyFileIdAssignment()
    {
        LangResult tree = CommandGrammar.Root.Parse("spin/tools/SpinClusterCommandSyntaxSummary/summary = SpinClusterCommandSyntaxSummary.md");
        tree.StatusCode.IsOk().Should().BeTrue(tree.StatusCode.ToString());

        tree.Should().NotBeNull();
        tree.IsOk().Should().BeTrue(tree.Error);

        IMatchLangResult[] toMatch = [
            new MatchLangResult<LsValue>("spin/tools/SpinClusterCommandSyntaxSummary/summary", "fileId"),
            new MatchLangResult<LsToken>("=", "equal"),
            new MatchLangResult<LsValue>("SpinClusterCommandSyntaxSummary.md", "localFilePath"),
        ];

        tree.LangNodes.NotNull().Count.Should().Be(toMatch.Length);
        tree.LangNodes.Children.Zip(toMatch).All(x => x.Second.IsMatch(x.First).IsOk()).Should().BeTrue();
    }

    [Fact]
    public void WithSingleAttribute()
    {
        LangResult tree = CommandGrammar.Root.Parse("[summary] spin/tools/SpinClusterCommandSyntaxSummary/summary = SpinClusterCommandSyntaxSummary.md");
        tree.StatusCode.IsOk().Should().BeTrue(tree.StatusCode.ToString());

        tree.Should().NotBeNull();
        tree.IsOk().Should().BeTrue(tree.Error);

        IMatchLangResult[] toMatch = [
            new MatchLangResult<LsGroup>("[", "attribute-group"),
            new MatchLangResult<LsValue>("summary", "attributeName"),
            new MatchLangResult<LsGroup>("]", "attribute-group"),
            new MatchLangResult<LsValue>("spin/tools/SpinClusterCommandSyntaxSummary/summary", "fileId"),
            new MatchLangResult<LsToken>("=", "equal"),
            new MatchLangResult<LsValue>("SpinClusterCommandSyntaxSummary.md", "localFilePath"),
        ];

        tree.LangNodes.NotNull().Count.Should().Be(toMatch.Length);
        tree.LangNodes.Children.Zip(toMatch).ForEach(x => x.Second.IsMatch(x.First).Action(y => y.IsOk().Should().BeTrue($"{x.Second} - {x.First}, error={y.Error}")));
    }

    [Fact]
    public void WithTwoAttribute()
    {
        LangResult tree = CommandGrammar.Root.Parse("[summary;Main] spin/tools/SpinClusterCommandSyntaxSummary/summary = SpinClusterCommandSyntaxSummary.md");
        tree.StatusCode.IsOk().Should().BeTrue(tree.StatusCode.ToString());

        tree.Should().NotBeNull();
        tree.IsOk().Should().BeTrue(tree.Error);

        IMatchLangResult[] toMatch = [
            new MatchLangResult<LsGroup>("[", "attribute-group"),
            new MatchLangResult<LsValue>("summary", "attributeName"),
            new MatchLangResult<LsToken>(";", "delimiter"),
            new MatchLangResult<LsValue>("Main", "attributeName"),
            new MatchLangResult<LsGroup>("]", "attribute-group"),
            new MatchLangResult<LsValue>("spin/tools/SpinClusterCommandSyntaxSummary/summary", "fileId"),
            new MatchLangResult<LsToken>("=", "equal"),
            new MatchLangResult<LsValue>("SpinClusterCommandSyntaxSummary.md", "localFilePath"),
        ];

        tree.LangNodes.NotNull().Count.Should().Be(toMatch.Length);
        tree.LangNodes.Children.Zip(toMatch).ForEach(x => x.Second.IsMatch(x.First).Action(y => y.IsOk().Should().BeTrue($"{x.Second} - {x.First}, error={y.Error}")));
    }
}
