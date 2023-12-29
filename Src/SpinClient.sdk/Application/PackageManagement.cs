using System.IO.Compression;
using Microsoft.Extensions.Logging;
using SpinCluster.abstraction;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinClient.sdk;

public class PackageManagement
{
    private readonly AgentClient _agentClient;
    private readonly ILogger<PackageManagement> _logger;
    private readonly SmartcClient _smartcClient;
    private readonly StorageClient _storageClient;
    private readonly AbortSignal _abortSignal;

    public PackageManagement(
        AgentClient agentClient,
        SmartcClient smartcClient,
        StorageClient storageClient,
        AbortSignal abortSignal,
        ILogger<PackageManagement> logger)
    {
        _agentClient = agentClient.NotNull();
        _smartcClient = smartcClient.NotNull();
        _storageClient = storageClient.NotNull();
        _abortSignal = abortSignal.NotNull();
        _logger = logger.NotNull();
    }

    public async Task<Option<string>> LoadPackage(string agentId, string smartcId, ScopeContext context)
    {
        agentId.NotEmpty();
        smartcId.NotEmpty();

        context = context.With(_logger);
        context.LogInformation("Unpacking SmartC package smartcId={smartcId}", smartcId);

        var workContextModel = await Setup(agentId, smartcId, context);
        if (workContextModel.IsError()) return workContextModel.ToOptionStatus<string>();

        WorkContext workContext = workContextModel.Return();

        var checkCacheResult = await CheckCache(workContext, context);
        if (checkCacheResult.IsOk()) return workContext.Executable;

        var downloadOption = await DownloadAndExpand(workContext, context);
        if (downloadOption.IsError()) return downloadOption.ToOptionStatus<string>();

        var result = await CheckCacheFiles(workContext, context);
        if (result.IsError()) return result.ToOptionStatus<string>();

        return workContext.Executable;
    }

    private async Task<Option<WorkContext>> Setup(string agentId, string smartcId, ScopeContext context)
    {
        var smartcModelOption = await _smartcClient.Get(smartcId, context);
        if (smartcModelOption.IsError())
        {
            context.LogError("Cannot find smartcId={smartcId}", smartcId);
            return smartcModelOption.ToOptionStatus<WorkContext>();
        }
        var model = smartcModelOption.Return();

        var agent = await _agentClient.Get(agentId, context);
        if (agent.IsError())
        {
            context.LogError("Cannot get agent details for agentId={agentId}", agentId);
            return smartcModelOption.ToOptionStatus<WorkContext>();
        }

        var folder = Path.Combine(agent.Return().WorkingFolder, model.BlobHash);
        Directory.CreateDirectory(folder);

        return new WorkContext
        {
            SmartcModel = model,
            PackageFolder = folder,
            Executable = Path.Combine(folder, model.Executable),
        };
    }

    private async Task<Option> CheckCache(WorkContext workContext, ScopeContext context)
    {
        if (!Directory.Exists(workContext.PackageFolder)) return StatusCode.NotFound;

        var result = await CheckCacheFiles(workContext, context);
        if (result.IsOk()) return result;

        Directory.Delete(workContext.PackageFolder);
        return StatusCode.NotFound;
    }

    private async Task<Option> DownloadAndExpand(WorkContext workContext, ScopeContext context)
    {
        context.LogInformation("Downloading and unpacking smartcExeId={smartcExeId}", workContext.SmartcModel.SmartcExeId);

        var blobPackageOption = await _storageClient.Get(workContext.SmartcModel.SmartcExeId, context);
        if (blobPackageOption.IsError())
        {
            context.LogError("Cannot download blob package for smartcExeId={smartcExeId}", workContext.SmartcModel.SmartcExeId);
            return StatusCode.NotFound;
        }

        StorageBlob blob = blobPackageOption.Return();
        if (blob.BlobHash != blob.CalculateHash())
        {
            context.LogCritical("SmartC package's blobs hash is invalid, smartcExeId={smartcExeId}", workContext.SmartcModel.SmartcExeId);
            return StatusCode.Conflict;
        }

        Directory.CreateDirectory(workContext.PackageFolder);

        using (var memoryBuffer = new MemoryStream(blob.Content))
        {
            using var read = new ZipArchive(memoryBuffer, ZipArchiveMode.Read, leaveOpen: true);
            read.ExtractToFolder(workContext.PackageFolder, _abortSignal.GetToken(), null);
        }

        context.LogInformation("Extracted SmartC package smartcExeId={smartcExeId} to folder={folder}",
            workContext.SmartcModel.SmartcExeId, workContext.PackageFolder);

        return StatusCode.OK;
    }

    private async Task<Option> CheckCacheFiles(WorkContext workContext, ScopeContext context)
    {
        var localFiles = workContext.SmartcModel.PackageFiles
            .Select(x => (packageFile: x, file: Path.Combine(workContext.PackageFolder, x.File)))
            .ToArray();

        if (localFiles.Length != workContext.SmartcModel.PackageFiles.Count)
        {
            context.LogError("Count mistmatch, filesCount={filesCount}, packageFileCount={packageFileCount}",
                localFiles.Length, workContext.SmartcModel.PackageFiles.Count);

            return StatusCode.Conflict;
        }

        // Get file hashes
        Option<IReadOnlyList<FileTool.FileHash>> fileHashesOption = await FileTool.GetFileHashes(localFiles.Select(x => x.file).ToArray(), context);
        if (fileHashesOption.IsError())
        {
            context.LogError("Package file's hash violations detected, deleting folder for refresh, smartcExeId={smartcExeId}",
                workContext.SmartcModel.SmartcExeId);

            return fileHashesOption.ToOptionStatus();
        }

        IReadOnlyList<FileTool.FileHash> fileHashes = fileHashesOption.Return()
            .Select(x => new FileTool.FileHash { File = x.File[(workContext.PackageFolder.Length + 1)..], Hash = x.Hash })
            .ToArray();

        // Checking hashes against recorded violations
        var hashViolations = fileHashes.OrderBy(x => x.File)
            .Zip(workContext.SmartcModel.PackageFiles.OrderBy(x => x.File))
            .Where(x => x.First.File != x.Second.File || x.First.Hash != x.Second.FileHash)
            .ToArray();

        if (hashViolations.Length > 0) return (StatusCode.Conflict, "File violations detected");

        var allFiles = Directory.EnumerateFiles(workContext.PackageFolder, "*.*", SearchOption.AllDirectories)
            .Except(localFiles.Select(x => x.file))
            .ToArray();

        if (allFiles.Length > 0)
        {
            context.LogError("There are more files then recorded for package, count={count}, files={files}, smartcExeId={smartcExeId}",
                allFiles.Length, allFiles.Join(';'), workContext.SmartcModel.SmartcExeId);

            return StatusCode.Conflict;
        }

        return StatusCode.OK;
    }

    private sealed record WorkContext
    {
        public SmartcModel SmartcModel { get; init; } = null!;
        public string PackageFolder { get; init; } = null!;
        public string Executable { get; init; } = null!;
    }
}
