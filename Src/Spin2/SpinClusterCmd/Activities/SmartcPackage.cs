using System.IO.Compression;
using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Actors.Storage;
using SpinClusterCmd.Application;
using Toolbox.Extensions;
using Toolbox.Tools;
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

    public Task Create(string jsonFile, bool verbose)
    {
        var context = new ScopeContext(_logger);
        context.Trace().LogInformation("Processing file {file} to create package", jsonFile);

        var optionOption = LoadOption(jsonFile, context);
        if (optionOption.IsError()) return Task.CompletedTask;
        PackageOption option = optionOption.Return();

        CreateFolder(option.SmartcPackageFile);

        var copy = Directory.EnumerateFiles(option.SourceFolder, "*.*", SearchOption.AllDirectories)
            .Select(x => new CopyTo { Source = x, Destination = "bin\\" + x[(option.SourceFolder.Length + 1)..] })
            .ToArray();

        using (var stream = new FileStream(option.SmartcPackageFile, FileMode.Create))
        {
            using var zipArchive = new ZipArchive(stream, ZipArchiveMode.Create, false);

            zipArchive.CompressFiles(copy, context.Token);
        }

        if (verbose) dump(copy);

        context.Trace().LogInformation("Created SmartC package, smartcPackageFile={smartcPackageFile}", option.SmartcPackageFile);
        return Task.CompletedTask;

        void dump(CopyTo[] copyTo) => copyTo
            .Select(x => $"  Adding to package: {x.Source} -> {x.Destination}")
            .Join(Environment.NewLine)
            .Action(x => context.Trace().LogInformation(x));
    }

    public async Task Upload(string jsonFile)
    {
        var context = new ScopeContext(_logger);
        context.Trace().LogInformation("Processing file {file} to upload package", jsonFile);

        var optionOption = LoadOption(jsonFile, context);
        if (optionOption.IsError()) return;
        PackageOption option = optionOption.Return();

        StorageBlob blob = new StorageBlobBuilder()
            .SetStorageId(option.SmartcExeId)
            .SetContentFromFile(option.SmartcPackageFile)
            .Build();

        Option uploadOption = await _client.Set(blob, context);
        context.Trace().LogStatus(uploadOption, "Upload blob to storage, smartcExeId={smartcExeId}, smartcPackageFile={smartcPackageFile}",
            option.SmartcExeId, option.SmartcPackageFile);
    }

    public async Task Download(string jsonFile)
    {
        var context = new ScopeContext(_logger);
        context.Trace().LogInformation("Processing file {file} to download package", jsonFile);

        var optionOption = LoadOption(jsonFile, context);
        if (optionOption.IsError()) return;
        PackageOption option = optionOption.Return();

        var blobOption = await _client.Get(option.SmartcExeId, context).LogResult(context.Trace());
        if (blobOption.IsError()) return;

        StorageBlob blob = blobOption.Return();

        BackupFiles(option.SmartcPackageFile, context);
        File.WriteAllBytes(option.SmartcPackageFile, blob.Content);

        context.Trace().LogInformation("Download smartcExeId={smartcExeId} to smartcPackageFile={smartcPackageFile}",
            option.SmartcExeId, option.SmartcPackageFile);
    }


    private Option<PackageOption> LoadOption(string jsonFile, ScopeContext context)
    {
        var readResult = CmdTools.LoadJson<PackageOption>(jsonFile, PackageOption.Validator, context);
        if (readResult.IsError()) return readResult;

        PackageOption option = readResult.Return();

        if (!Directory.Exists(option.SourceFolder))
        {
            context.Trace().LogError("Folder {folder} does not exist", option.SourceFolder);
            return StatusCode.NotFound;
        }

        return option;
    }

    private void CreateFolder(string smartcPackageFile)
    {
        string? folder = Path.GetDirectoryName(smartcPackageFile);
        if (folder == null) return;

        Directory.CreateDirectory(folder);
    }

    private void BackupFiles(string outputFile, ScopeContext context)
    {
        const int maxBackup = 9;

        string folder = Path.GetDirectoryName(outputFile) ?? "\\";
        string searchPath = Path.GetFileName(outputFile) + ".bak.*";

        (string file, DateTime modDate)[] files = Directory.GetFiles(folder, searchPath, SearchOption.TopDirectoryOnly)
            .Select(x => (file: x, modDate: File.GetLastWriteTime(x)))
            .ToArray();

        string[] deleteList = files
            .OrderByDescending(x => x.modDate)
            .Take(files.Length - maxBackup)
            .Select(x => x.file)
            .ToArray();

        if (deleteList.Length > 0)
        {
            context.Trace().LogInformation("Deleting old backups... file={file}", deleteList.Join(';'));
            foreach (var file in deleteList)
            {
                File.Delete(file);
            }
        }

        if (File.Exists(outputFile))
        {
            string toFile = outputFile + ".bak." + Guid.NewGuid().ToString();
            File.Move(outputFile, toFile);
            context.Trace().LogInformation("Moving file={file} to backupFile={backupFile}", outputFile, toFile);
        }
    }

    private record PackageOption
    {
        public string SmartcExeId { get; init; } = null!;
        public string SmartcPackageFile { get; init; } = null!;
        public string SourceFolder { get; init; } = null!;

        public static IValidator<PackageOption> Validator { get; } = new Validator<PackageOption>()
            .RuleFor(x => x.SmartcExeId).ValidResourceId(ResourceType.DomainOwned, "smartc-exe")
            .RuleFor(x => x.SmartcPackageFile).NotEmpty()
            .RuleFor(x => x.SourceFolder).NotEmpty()
            .Build();
    }
}
