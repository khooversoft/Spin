using System.IO.Compression;
using Microsoft.Extensions.Logging;
using SpinClient.sdk;
using SpinCluster.abstraction;
using SpinClusterCmd.Application;
using Toolbox.CommandRouter;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinClusterCmd.Activities;

internal class SmartcPackage : ICommandRoute
{
    private readonly StorageClient _storageClient;
    private readonly SmartcClient _smartcClient;
    private readonly ILogger<SmartcPackage> _logger;
    private readonly CmdOption _cmdOption;

    public SmartcPackage(CmdOption cmdOption, StorageClient storageClient, SmartcClient smartcClient, ILogger<SmartcPackage> logger)
    {
        _cmdOption = cmdOption.NotNull();
        _storageClient = storageClient.NotNull();
        _smartcClient = smartcClient.NotNull();
        _logger = logger.NotNull();
    }

    public CommandSymbol CommandSymbol() => new CommandSymbol("package", "Create or expand SmartC package")
    {
        new CommandSymbol("create", "Create SmartC package").Action(command =>
        {
            var jsonFile = command.AddArgument<string>("jsonFile", "Json file with package details");
            var verboseOption = command.AddOption<bool>("--verbose", "List all files being packaged");

            command.SetHandler(CreateAndUpload, jsonFile,verboseOption);
        }),
        new CommandSymbol("download", "Upload SmartC package").Action(command =>
        {
            var jsonFile = command.AddArgument<string>("jsonFile", "Json file with package details");
            command.SetHandler(Download, jsonFile);
        }),
    };

    public async Task CreateAndUpload(string jsonFile, bool verbose)
    {
        var context = new ScopeContext(_logger);
        context.LogInformation("Processing file {file} to create and upload package", jsonFile);

        var optionOption = LoadOption(jsonFile, context);
        if (optionOption.IsError()) return;
        PackageOption option = optionOption.Return();

        string workingPackageFile = ResourceId.Create(option.SmartcExeId).Return().Path + ".zip";
        option = option with { SmartcPackageFile = Path.Combine(_cmdOption.WorkingFolder, workingPackageFile) };

        var createOption = await Create(option, verbose, context);
        if (createOption.IsError())
        {
            context.LogError("Failed to create package");
            return;
        }

        var uploadOption = await Upload(option, context);
        if (uploadOption.IsError())
        {
            context.LogError("Failed to upload package");
            return;
        }

        var setSmartOption = await SetSmartC(option, createOption.Return(), uploadOption.Return(), context);
        if (setSmartOption.IsError())
        {
            context.LogError("Failed to set SmartC details");
            return;
        }
    }

    public async Task Download(string jsonFile)
    {
        var context = new ScopeContext(_logger);
        context.LogInformation("Processing file {file} to download package", jsonFile);

        var optionOption = LoadOption(jsonFile, context);
        if (optionOption.IsError()) return;
        PackageOption option = optionOption.Return();

        var blobOption = await _storageClient.Get(option.SmartcExeId, context);
        if (blobOption.IsError()) return;

        StorageBlob blob = blobOption.Return();

        File.WriteAllBytes(option.SmartcPackageFile, blob.Content);

        context.LogInformation("Download smartcExeId={smartcExeId} to smartcPackageFile={smartcPackageFile}",
            option.SmartcExeId, option.SmartcPackageFile);
    }

    private async Task<Option<IReadOnlyList<PackageFile>>> Create(PackageOption option, bool verbose, ScopeContext context)
    {
        context.LogInformation("Creating package, smartcId={smartcId}, smartcExeId={smartcExeId}", option.SmartcId, option.SmartcExeId);

        CreateFolder(option.SmartcPackageFile);

        var copy = Directory.EnumerateFiles(option.SourceFolder, "*.*", SearchOption.AllDirectories)
            .Select(x => new CopyTo { Source = x, Destination = "bin\\" + x[(option.SourceFolder.Length + 1)..] })
            .ToArray();

        Option<IReadOnlyList<FileTool.FileHash>> fileHashes = await FileTool.GetFileHashes(copy.Select(x => x.Source).ToArray(), context);
        if (fileHashes.IsError()) return fileHashes.ToOptionStatus<IReadOnlyList<PackageFile>>();
        if (verbose) hashDump(fileHashes.Return());

        IReadOnlyList<PackageFile> packageFiles = fileHashes.Return()
            .Join(copy, x => x.File, x => x.Source, (o, i) => new PackageFile { File = i.Destination, FileHash = o.Hash })
            .ToArray();

        using (var stream = new FileStream(option.SmartcPackageFile, FileMode.Create))
        {
            using var zipArchive = new ZipArchive(stream, ZipArchiveMode.Create, false);

            zipArchive.CompressFiles(copy, context.Token);
        }

        if (verbose) dump(copy);

        context.LogInformation("Created SmartC package, smartcPackageFile={smartcPackageFile}", option.SmartcPackageFile);

        return packageFiles.ToOption();

        void dump(CopyTo[] copyTo) => copyTo
            .Select(x => $"  Adding to package: {x.Source} -> {x.Destination}")
            .Join(Environment.NewLine)
            .Action(x => context.LogInformation(x));

        void hashDump(IEnumerable<FileTool.FileHash> files) => files
            .Select(x => $"  File hashes: {x.File} -> {x.Hash}")
            .Join(Environment.NewLine)
            .Action(x => context.LogInformation(x));
    }

    private async Task<Option<string>> Upload(PackageOption option, ScopeContext context)
    {
        context.LogInformation("Uploading package, smartcPackageFile={smartcPackageFile}", option.SmartcExeId);

        StorageBlob blob = new StorageBlobBuilder()
            .SetStorageId(option.SmartcExeId)
            .SetContentFromFile(option.SmartcPackageFile)
            .Build();

        Option uploadOption = await _storageClient.Set(blob, context);

        uploadOption.LogStatus(context, "Upload blob to storage, smartcExeId={smartcExeId}, smartcPackageFile={smartcPackageFile}",
            option.SmartcExeId, option.SmartcPackageFile);

        return blob.BlobHash;
    }

    private async Task<Option> SetSmartC(PackageOption option, IReadOnlyList<PackageFile> packageFiles, string blobHash, ScopeContext context)
    {
        var model = new SmartcModel
        {
            SmartcId = option.SmartcId,
            SmartcExeId = option.SmartcExeId,
            ContractId = option.ContractId,
            Executable = option.Executable,
            Enabled = option.Enabled,
            PackageFiles = packageFiles,
            BlobHash = blobHash,
        };

        var setOption = await _smartcClient.Set(model, context);

        setOption.LogStatus(context, "Set SmartC, smartcId={smartcId}, smartcExeId={smartcExeId}", option.SmartcId, option.SmartcExeId);
        return setOption;
    }

    private Option<PackageOption> LoadOption(string jsonFile, ScopeContext context)
    {
        var readResult = CmdTools.LoadJson<PackageOption>(jsonFile, PackageOption.Validator, context);
        if (readResult.IsError()) return readResult;

        PackageOption option = readResult.Return();

        if (!Directory.Exists(option.SourceFolder))
        {
            context.LogError("Folder {folder} does not exist", option.SourceFolder);
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

    private record PackageOption
    {
        public string SmartcId { get; init; } = null!;
        public string SmartcExeId { get; init; } = null!;
        public string SourceFolder { get; init; } = null!;
        public string ContractId { get; init; } = null!;
        public string Executable { get; init; } = null!;
        public bool Enabled { get; init; }
        public string SmartcPackageFile { get; init; } = null!;

        public static IValidator<PackageOption> Validator { get; } = new Validator<PackageOption>()
            .RuleFor(x => x.SmartcId).ValidResourceId(ResourceType.DomainOwned, "smartc")
            .RuleFor(x => x.SmartcExeId).ValidResourceId(ResourceType.DomainOwned, "smartc-exe")
            .RuleFor(x => x.SourceFolder).NotEmpty()
            .RuleFor(x => x.ContractId).ValidResourceId(ResourceType.DomainOwned, "contract")
            .RuleFor(x => x.Executable).NotEmpty()
            .Build();
    }
}
