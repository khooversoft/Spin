using System.Collections.Concurrent;
using System.IO.Compression;
using System.Threading.Tasks.Dataflow;
using Microsoft.Extensions.Logging;
using NBlog.sdk;
using NBlogCmd.Application;
using Toolbox.CommandRouter;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace NBlogCmd.Activities;

internal class Build : ICommandRoute
{
    private readonly PackageBuild _packageBuild;
    public Build(PackageBuild packageBuild) => _packageBuild = packageBuild.NotNull();

    public CommandSymbol CommandSymbol() => new CommandSymbol("build", "Build NBlog package for uploading").Action(x =>
    {
        var basePath = x.AddArgument<string>("basePath", "Path for the base folder to update");
        var packageFile = x.AddArgument<string>("packageFile", "Package file");

        x.SetHandler(BuildPackage, basePath, packageFile);
    });

    private async Task BuildPackage(string basePath, string packageFile)
    {
        await _packageBuild.Build(basePath, packageFile);
    }
}
