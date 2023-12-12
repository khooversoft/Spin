using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                "Tags": "Spin Tools",
                "Category": "Spin Cluster"
            }
            """;

        ArticleManifest articleManifest = rawData.ToObject<ArticleManifest>().NotNull();
        articleManifest.Validate().IsOk().Should().BeTrue();

        IReadOnlyList<CommandNode> commands = articleManifest.GetCommands();
        commands.Count.Should().Be(3);

        CommandNode[] matchTo = [
            new CommandNode { Attributes = (string[])["summary"], FileId = "spin/tools/SpinClusterCommandSyntaxSummary/summary", LocalFilePath = "SpinClusterCommandSyntaxSummary.md" },
            new CommandNode { Attributes = (string[])["main"], FileId = "spin/tools/SpinClusterCommandSyntaxSummary/doc", LocalFilePath = "SpinClusterCommandSyntax.md" },
            new CommandNode { FileId = "spin/tools/SpinClusterCommandSyntaxSummary/support", LocalFilePath = "SpinClusterCommandSyntaxSupport.md" },
        ];

        commands.Count.Should().Be(matchTo.Length);
        commands.Zip(matchTo).ForEach(x => x.First.Should().BeEquivalentTo(x.Second));
    }
}
