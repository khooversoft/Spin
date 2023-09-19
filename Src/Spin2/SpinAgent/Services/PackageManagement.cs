using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        var smartcModelOption = await _smartcClient.Get(smartcId, context).LogResult(context.Trace());
        if (smartcModelOption.IsError())
        {
            context.Trace().LogError("Cannot find smartcId={smartcId}", smartcId);
            return smartcModelOption.ToOptionStatus<string>();
        }

        SmartcModel smartcModel = smartcModelOption.Return();

        var checkCacheResult = await CheckCache(smartcModel.SmartcExeId, context);
        if (checkCacheResult.IsOk()) return checkCacheResult;

        return await DownloadAndExpand(smartcModel.SmartcExeId, context);
    }

    private async Task<Option<string>> CheckCache(string smartcExeId, ScopeContext context)
    {
        Option<StorageBlobInfo> blobInfoOption = await _storageClient
            .GetInfo(smartcExeId, context)
            .LogResult(context.Trace());

        if (blobInfoOption.IsError())
        {
            context.Trace().LogError("Cannot get blob info for smartcExeId={smartcExeId}", smartcExeId);
            return blobInfoOption.ToOptionStatus<string>();
        }

        AgentModel agentModel = _agentConfiguration.Get(context);

        string packageFolder = Path.Combine(agentModel.WorkingFolder, blobInfoOption.Return().BlobHash);

        Option<string> result = Directory.Exists(packageFolder) switch
        {
            true => packageFolder,
            false => StatusCode.NotFound,
        };

        if (result.IsOk())
        {
            context.Trace().LogInformation("Package is in cache, smartcExeId={smartcExeId}, packageFolder={packageFolder}",
                smartcExeId, packageFolder);
        }

        return result;
    }

    private async Task<Option<string>> DownloadAndExpand(string smartcExeId, ScopeContext context)
    {
        context.Trace().LogInformation("Downloading and unpacking smartcExeId={smartcExeId}", smartcExeId);

        var blobPackageOption = await _storageClient.Get(smartcExeId, context);
        if (blobPackageOption.IsError())
        {
            context.Trace().LogError("Cannot download blob package for smartcExeId={smartcExeId}", smartcExeId);
            return StatusCode.NotFound;
        }

        StorageBlob blob = blobPackageOption.Return();
        AgentModel agentModel = _agentConfiguration.Get(context);

        string packageFolder = Path.Combine(agentModel.WorkingFolder, blob.Content.ToSHA256HexHash());
        if (Directory.Exists(packageFolder)) throw new InvalidOperationException("Directory should not exist");
        Directory.CreateDirectory(packageFolder);

        using (var memoryBuffer = new MemoryStream(blob.Content))
        {
            using var read = new ZipArchive(memoryBuffer, ZipArchiveMode.Read, leaveOpen: true);
            read.ExtractToFolder(packageFolder, _abortSignal.GetToken(), null);
        }

        context.Trace().LogInformation("Extracted SmartC package smartcExeId={smartcExeId} to folder={folder}", packageFolder);
        return packageFolder;
    }

}
