using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Actors.Storage;
using SpinClusterCmd.Application;
using Toolbox.Azure.Queue;
using Toolbox.Tools.Validation;
using Toolbox.Tools.Zip;
using Toolbox.Types;

namespace SpinClusterCmd.Activities;

internal class SmartcPackage
{
    private readonly StorageClient _client;
    private readonly ILogger<SmartcPackage> _logger;

    public SmartcPackage(StorageClient client, ILogger<SmartcPackage> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task Package(string jsonFile)
    {
        var context = new ScopeContext(_logger);
        context.Trace().LogInformation("Processing file {file}", jsonFile);

        var readResult = CmdTools.LoadJson<PackageOption>(jsonFile, PackageOption.Validator, context);
        if (readResult.IsError()) return;

        PackageOption option = readResult.Return();

        Package(option, context);
    }

    private void Package(PackageOption option, ScopeContext context)
    {
        if (Validate(option, context).IsError()) return;

        var copy = new[]
        {
            new CopyTo { Source = option.SourceFolder, Destination = "bin" },
        };

        string zipFile = $"smartczTemp_{Guid.NewGuid()}.zip";

        using var stream = new FileStream(zipFile, FileMode.Create);
        using var zipArchive = new ZipArchive(stream, ZipArchiveMode.Create, false);

        zipArchive.CompressFiles(copy, context.Token, monitor);


        void monitor(FileActionProgress progress) => context.Trace().LogInformation("Monitor: {monitor}", progress);
    }

    private Option Validate(PackageOption option, ScopeContext context)
    {
        if (!Directory.Exists(option.SourceFolder))
        {
            context.Trace().LogError("Folder {folder} does not exist", option.SourceFolder);
            return StatusCode.OK;
        }

        if (Directory.Exists(option.WorkingFolder))
        {
            context.Trace().LogInformation("Clearing working {workingFolder}", option.WorkingFolder);
            Directory.Delete(option.WorkingFolder);
        }

        if (!Directory.Exists(option.WorkingFolder))
        {
            context.Trace().LogInformation("Creating working folder {workingFolder}", option.WorkingFolder);
            Directory.CreateDirectory(option.WorkingFolder);
        }

        return StatusCode.OK;
    }


    private record PackageOption
    {
        public string SmartcExeId { get; init; } = null!;
        public string SourceFolder { get; init; } = null!;
        public string WorkingFolder { get; init; } = null!;

        public static IValidator<PackageOption> Validator { get; } = new Validator<PackageOption>()
            .RuleFor(x => x.SmartcExeId).ValidResourceId(ResourceType.DomainOwned, "smartc")
            .RuleFor(x => x.SourceFolder).NotEmpty()
            .RuleFor(x => x.WorkingFolder).NotEmpty()
            .Build();
    }
}
