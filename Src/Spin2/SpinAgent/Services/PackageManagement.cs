using System.IO.Compression;
using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Actors.Agent;
using SpinCluster.sdk.Actors.Smartc;
using SpinCluster.sdk.Actors.Storage;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Tools.Zip;
using Toolbox.Types;

namespace SpinAgent.Services;

internal class PackageManagement
{
    private readonly AgentConfiguration _agentConfiguration;
    private readonly ILogger<PackageManagement> _logger;
    private readonly SmartcClient _smartcClient;
    private readonly StorageClient _storageClient;
    private readonly AbortSignal _abortSignal;

    public PackageManagement(
        AgentConfiguration agentConfiguration,
        SmartcClient smartcClient,
        StorageClient storageClient,
        AbortSignal abortSignal,
        ILogger<PackageManagement> logger)
    {
        _agentConfiguration = agentConfiguration.NotNull();
        _smartcClient = smartcClient.NotNull();
        _storageClient = storageClient.NotNull();
        _abortSignal = abortSignal.NotNull();
        _logger = logger.NotNull();
    }

    public async Task<Option<string>> LoadPackage(string smartcId, ScopeContext context)
    {
        context = context.With(_logger);
        context.Trace().LogInformation("Unpacking SmartC package smartcId={smartcId}", smartcId);

        var workContextModel = await Setup(smartcId, context);
        if (workContextModel.IsError()) return workContextModel.ToOptionStatus<string>();

        WorkContext workContext = workContextModel.Return();

        var checkCacheResult = await CheckCache(workContext, context);
        if (checkCacheResult.IsOk()) return checkCacheResult.ToOptionStatus<string>(); ;

        var downloadOption = await DownloadAndExpand(workContext, context);
        if (downloadOption.IsError()) return downloadOption.ToOptionStatus<string>();

        return workContext.PackageFolder;
    }

    private async Task<Option<WorkContext>> Setup(string smartcId, ScopeContext context)
    {

        var smartcModelOption = await _smartcClient.Get(smartcId, context).LogResult(context.Trace());
        if (smartcModelOption.IsError())
        {
            context.Trace().LogError("Cannot find smartcId={smartcId}", smartcId);
            return smartcModelOption.ToOptionStatus<WorkContext>();
        }

        var model = smartcModelOption.Return();
        var agent = _agentConfiguration.Get(context);

        return new WorkContext
        {
            SmartcModel = model,
            AgentModel = agent,
            PackageFolder = Path.Combine(agent.WorkingFolder, model.BlobHash)
        };
    }

    private async Task<Option> CheckCache(WorkContext workContext, ScopeContext context)
    {
        if (!Directory.Exists(workContext.PackageFolder)) return StatusCode.NotFound;

        var result = await CheckCacheFiles(workContext, context);
        if (result.IsError()) return result;

        Directory.Delete(workContext.PackageFolder);
        return StatusCode.NotFound;
    }

    private async Task<Option> DownloadAndExpand(WorkContext workContext, ScopeContext context)
    {
        context.Trace().LogInformation("Downloading and unpacking smartcExeId={smartcExeId}", workContext.SmartcModel.SmartcExeId);

        var blobPackageOption = await _storageClient.Get(workContext.SmartcModel.SmartcExeId, context);
        if (blobPackageOption.IsError())
        {
            context.Trace().LogError("Cannot download blob package for smartcExeId={smartcExeId}", workContext.SmartcModel.SmartcExeId);
            return StatusCode.NotFound;
        }

        StorageBlob blob = blobPackageOption.Return();
        if (blob.BlobHash != blob.CalculateHash())
        {
            context.Trace().LogCritical("SmartC package's blobs hash is invalid, smartcExeId={smartcExeId}", workContext.SmartcModel.SmartcExeId);
            return StatusCode.Conflict;
        }

        Directory.CreateDirectory(workContext.PackageFolder);

        using (var memoryBuffer = new MemoryStream(blob.Content))
        {
            using var read = new ZipArchive(memoryBuffer, ZipArchiveMode.Read, leaveOpen: true);
            read.ExtractToFolder(workContext.PackageFolder, _abortSignal.GetToken(), null);
        }

        context.Trace().LogInformation("Extracted SmartC package smartcExeId={smartcExeId} to folder={folder}",
            workContext.SmartcModel.SmartcExeId, workContext.PackageFolder);

        return StatusCode.OK;
    }

    private async Task<Option> CheckCacheFiles(WorkContext workContext, ScopeContext context)
    {
        var localFiles = workContext.SmartcModel.PackageFiles
            .Select(x => (packageFile: x, file: Path.Combine(workContext.PackageFolder, x.File)))
            .ToArray();

        // Get file hashes
        Option<IReadOnlyList<FileTool.FileHash>> fileHashes = await FileTool.GetFileHashes(localFiles.Select(x => x.file).ToArray(), context);
        if (fileHashes.IsError())
        {
            context.Trace().LogError("Package file's hash violations detected, deleting folder for refresh, smartcExeId={smartcExeId}",
                workContext.SmartcModel.SmartcExeId);

            return fileHashes.ToOptionStatus();
        }

        // Checking hashes against recorded violations 
        var hashViolations = fileHashes.Return()
            .Zip(workContext.SmartcModel.PackageFiles)
            .Where(x => x.First.File != x.Second.File || x.First.Hash != x.Second.FileHash)
            .ToArray();

        if (hashViolations.Length > 0) return (StatusCode.Conflict, "File violations detected");

        var allFiles = Directory.EnumerateFiles(workContext.PackageFolder, "*.*", SearchOption.AllDirectories)
            .Except(localFiles.Select(x => x.file))
            .ToArray();

        if (allFiles.Length > 0)
        {
            context.Trace().LogError("There are more files then recorded for package, count={count}, files={files}, smartcExeId={smartcExeId}",
                allFiles.Length, allFiles.Join(';'), workContext.SmartcModel.SmartcExeId);

            return StatusCode.Conflict;
        }

        return StatusCode.OK;
    }

    private sealed record WorkContext
    {
        public SmartcModel SmartcModel { get; init; } = null!;
        public AgentModel AgentModel { get; init; } = null!;
        public string PackageFolder { get; init; } = null!;
    }
}
