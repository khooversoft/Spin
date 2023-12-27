using NBlog.sdk;
using Toolbox.CommandRouter;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace NBlogCmd.Activities;

internal class Upload : ICommandRoute
{
    private readonly PackageUpload _packageUpload;
    private readonly StorageOption _cmdOption;
    private readonly PackageBuild _packageBuild;

    public Upload(PackageUpload packageUpload, PackageBuild packageBuild, StorageOption cmdOption)
    {
        _packageUpload = packageUpload.NotNull();
        _packageBuild = packageBuild.NotNull();
        _cmdOption = cmdOption.NotNull();
    }

    public CommandSymbol CommandSymbol() => new CommandSymbol("upload", "Update articles and support files to datalake").Action(x =>
    {
        var packageFile = x.AddArgument<string>("packageFile", "Package file to upload to Spin Cluster");
        var build = x.AddOption<string?>("--build", "Build package, specify the basePath");

        x.SetHandler(UploadPackage, packageFile, build);
    });

    private async Task UploadPackage(string packageFile, string? basePath)
    {
        if (basePath.IsNotEmpty())
        {
            await _packageBuild.Build(basePath, packageFile);
        }

        await _packageUpload.Upload(packageFile, _cmdOption.Storage);
    }
}
