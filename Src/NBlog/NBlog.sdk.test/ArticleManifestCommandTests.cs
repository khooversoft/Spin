using FluentAssertions;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace NBlog.sdk.test;

public class ArticleManifestCommandTests
{
    [Fact]
    public void GetCommandFromArtificate()
    {
        string rawData = """
            {
                "ArticleId": "spin/tools/SpinClusterCommandSyntaxSummary",
                "Title": "Spin Cluster Command CLI",
                "Author": "khoover",
                "Commands": [
                    "[summary] spin/tools/SpinClusterCommandSyntaxSummary/summary = SpinClusterCommandSyntaxSummary.md",
                    "[main] spin/tools/SpinClusterCommandSyntaxSummary/doc = SpinClusterCommandSyntax.md",
                    "spin/tools/SpinClusterCommandSyntaxSummary/support = SpinClusterCommandSyntaxSupport.md"
                ],
                "Tags": "Spin Tools;db=dbName;area=testArea",
                "Category": "Spin Cluster"
            }
            """;

        ArticleManifest articleManifest = rawData.ToObject<ArticleManifest>().NotNull();
        articleManifest.Validate().IsOk().Should().BeTrue(articleManifest.ToString());

        IReadOnlyList<CommandNode> commands = articleManifest.GetCommands();
        commands.Count.Should().Be(3);

        CommandNode[] matchTo = [
            new CommandNode { Attributes = (string[])["summary"], FileId = "spin/tools/SpinClusterCommandSyntaxSummary/summary", FileIdValue = "SpinClusterCommandSyntaxSummary.md" },
            new CommandNode { Attributes = (string[])["main"], FileId = "spin/tools/SpinClusterCommandSyntaxSummary/doc", FileIdValue = "SpinClusterCommandSyntax.md" },
            new CommandNode { FileId = "spin/tools/SpinClusterCommandSyntaxSummary/support", FileIdValue = "SpinClusterCommandSyntaxSupport.md" },
        ];

        commands.Count.Should().Be(matchTo.Length);
        commands.Zip(matchTo).ForEach(x => x.First.Should().BeEquivalentTo(x.Second));
    }
}
