using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Types;
using Toolbox.Tools;
using FluentAssertions;

namespace NBlog.sdk.test;

public class ArticleManifestTests
{
    [Fact]
    public void MinimalData()
    {
        var man = new ArticleManifest
        {
            ArticleId = "sub1/sub2/article.md",
            Title = "Title",
            Author = "autor",
            Commands = new[]
            {
                "[summary] spin/tools/SpinClusterCommandSyntaxSummary/summary = SpinClusterCommandSyntaxSummary.md",
                "[main] spin/tools/SpinClusterCommandSyntaxSummary/doc = SpinClusterCommandSyntax.md",
                "spin/tools/SpinClusterCommandSyntaxSummary/support = SpinClusterCommandSyntaxSupport.md"
            },
            Tags = "db=article;Area=Strategy;Design=Functional",
        };

        var v = man.Validate();
        v.IsOk().Should().BeTrue(v.ToString());
    }

    [Fact]
    public void MissingAttribute()
    {
        var man = new ArticleManifest
        {
            ArticleId = "sub1/sub2/article.md",
            Title = "Title",
            Author = "autor",
            Commands = new[]
            {
                "[main] spin/tools/SpinClusterCommandSyntaxSummary/doc = SpinClusterCommandSyntax.md",
                "spin/tools/SpinClusterCommandSyntaxSummary/support = SpinClusterCommandSyntaxSupport.md"
            },
            Tags = "db=article;Area=Strategy;Design=Functional",
        };

        var v = man.Validate();
        v.IsError().Should().BeTrue(v.ToString());
    }

    [Fact]
    public void MissingTag()
    {
        var man = new ArticleManifest
        {
            ArticleId = "sub1/sub2/article.md",
            Title = "Title",
            Author = "autor",
            Commands = new[]
            {
                "[summary] spin/tools/SpinClusterCommandSyntaxSummary/summary = SpinClusterCommandSyntaxSummary.md",
                "[main] spin/tools/SpinClusterCommandSyntaxSummary/doc = SpinClusterCommandSyntax.md",
                "spin/tools/SpinClusterCommandSyntaxSummary/support = SpinClusterCommandSyntaxSupport.md"
            },
            Tags = "Area=Strategy;Design=Functional",
        };

        var v = man.Validate();
        v.IsError().Should().BeTrue(v.ToString());
    }

}
