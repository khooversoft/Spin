using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NBlog.sdk.test.Application;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace NBlog.sdk.test;

public class PackageBuildUploadTest : IClassFixture<TestFixture>
{
    private const string _packageName = "nblogv1";
    private readonly TestFixture _fixture;
    public PackageBuildUploadTest(TestFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task BuildAndUploadPackage()
    {
        await BuildPackage();
        await UploadPackage();
    }

    private async Task BuildPackage()
    {
        PackageBuild build = _fixture.ServiceProvider.GetRequiredService<PackageBuild>();
        string rootPath = GetRoot();
        string packageFile = Path.Combine(GetWorkPath(), _packageName);

        Option result = await build.Build(rootPath, packageFile);
        result.IsOk().Should().BeTrue();
    }

    private async Task UploadPackage()
    {
        PackageUpload upload = _fixture.ServiceProvider.GetRequiredService<PackageUpload>();
        string packageFile = Path.Combine(GetWorkPath(), _packageName + ".nblogPackage");
        File.Exists(packageFile).Should().BeTrue($"File {packageFile} not exist");

        var datalakeOption = _fixture.Option.Storage with { BasePath = "NblogTest" };
        var result = await upload.Upload(packageFile, datalakeOption);
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
