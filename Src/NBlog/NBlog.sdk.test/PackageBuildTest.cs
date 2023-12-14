using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NBlog.sdk.test.Application;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace NBlog.sdk.test;

public class PackageBuildTest : IClassFixture<TestFixture>
{
    private readonly TestFixture _fixture;
    public PackageBuildTest(TestFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task BuildPackage()
    {
        PackageBuild build = _fixture.ServiceProvider.GetRequiredService<PackageBuild>();
        string rootPath = GetRoot();
        string packageFile = Path.Combine(GetWorkPath(), "nblogv1");

        Option result = await build.Build(rootPath, packageFile);
        result.IsOk().Should().BeTrue();
    }

    private string GetRoot()
    {
        string[] roots = [@"d:\sources", @"c:\sources"];
        var stack = roots.Reverse().ToStack();

        while (stack.TryPop(out var rootPath))
        {
            var list = DirectoryTool.Find(rootPath, "NBlogArticles\\Src");
            if (list.Count == 0) continue;

            string matchTo = Path.Combine(rootPath, "NBlogArticles", "Src");
            list[0].Should().Be(matchTo);
            return list[0];
        }

        throw new Exception("failed");
    }

    private string GetWorkPath()
    {
        string[] roots = [@"d:\work", @"c:\work"];
        var stack = roots.Reverse().ToStack();

        while (stack.TryPop(out var rootPath))
        {
            if (Directory.Exists(rootPath)) return rootPath;
        }

        throw new Exception("failed");
    }
}
