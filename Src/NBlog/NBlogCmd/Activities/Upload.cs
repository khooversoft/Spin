﻿using Microsoft.Extensions.Logging;
using NBlog.sdk;
using NBlog.sdk.Application;
using Toolbox.Azure.DataLake;
using Toolbox.CommandRouter;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace NBlogCmd.Activities;

internal class Upload : ICommandRoute
{
    private readonly PackageUpload _packageUpload;
    private readonly CmdOption _cmdOption;

    public Upload(PackageUpload packageUpload, CmdOption cmdOption)
    {
        _packageUpload = packageUpload.NotNull();
        _cmdOption = cmdOption.NotNull();
    }

    public CommandSymbol CommandSymbol() => new CommandSymbol("upload", "Update articles and support files to datalake").Action(x =>
    {
        var packageFile = x.AddArgument<string>("packageFile", "Package file to upload to Spin Cluster");

        x.SetHandler(UploadPackage, packageFile);
    });

    private async Task UploadPackage(string packageFile)
    {
        await _packageUpload.Upload(packageFile, _cmdOption.Storage);
    }
}
