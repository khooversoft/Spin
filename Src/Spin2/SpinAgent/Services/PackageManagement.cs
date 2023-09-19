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

        var checkCacheResult = await CheckCache(smartcModel, context);
        if (checkCacheResult.IsOk()) return checkCacheResult;

        return await DownloadAndExpand(smartcModel.SmartcExeId, context);
    }

    private async Task<Option<string>> CheckCache(SmartcModel smartcModel, ScopeContext context)
    {
        AgentModel agentModel = _agentConfiguration.Get(context);

        string packageFolder = Path.Combine(agentModel.WorkingFolder, smartcModel.BlobHash);

        if (!Directory.Exists(packageFolder)) return StatusCode.NotFound;

        var tasks = smartcModel.PackageFiles.Select(x => checkHash(x));
        var results = await Task.WhenAll(tasks);

        if (results.All(x => x.IsOk())) return packageFolder;

        context.Trace().LogError("Package file's hash did not verify, deleting folder for refresh");

        Directory.Delete(packageFolder);
        return StatusCode.NotFound;


        async Task<Option> checkHash(PackageFile packageFile)
        {
            var path = Path.Combine(packageFolder, packageFile.File);
            if (!File.Exists(path)) return StatusCode.NotFound;

            byte[] bytes = await File.ReadAllBytesAsync(path);
            string fileHash = bytes.ToSHA256HexHash();

            var option = packageFile.FileHash == fileHash ? StatusCode.OK : StatusCode.Conflict;
            if (option.IsError())
            {
                context.Trace().LogError("File {file} hash does not match recorded package details, smartcId={smartcId}", path, smartcModel.SmartcId);
            }

            return option;
        }
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
        Directory.CreateDirectory(packageFolder);

        using (var memoryBuffer = new MemoryStream(blob.Content))
        {
            using var read = new ZipArchive(memoryBuffer, ZipArchiveMode.Read, leaveOpen: true);
            read.ExtractToFolder(packageFolder, _abortSignal.GetToken(), null);
        }

        context.Trace().LogInformation("Extracted SmartC package smartcExeId={smartcExeId} to folder={folder}", smartcExeId, packageFolder);
        return packageFolder;
    }

}
