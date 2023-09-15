using System.IO.Compression;
using Microsoft.Azure.Amqp.Framing;
using Microsoft.Extensions.Logging;
using SpinAgent.Application;
using SpinCluster.sdk.Actors.Agent;
using SpinCluster.sdk.Actors.Smartc;
using SpinCluster.sdk.Actors.Storage;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Tools.Zip;
using Toolbox.Types;

namespace SpinAgent.Activities;

internal class WorkMonitor
{
    private readonly ILogger<WorkMonitor> _logger;
    private readonly RunSmartC _runSmartC;
    private readonly ScheduleClient _scheduleClient;
    private readonly AbortSignal _abortSignal;
    private readonly AgentOption _option;
    private readonly StorageClient _storageClient;
    private readonly AgentClient _agentClient;
    private readonly SmartcClient _smartcClient;

    public WorkMonitor(
        RunSmartC runSmartC,
        AgentClient agentClient,
        ScheduleClient scheduleClient,
        StorageClient storageClient,
        SmartcClient smartcClient,
        AbortSignal abortSignal,
        AgentOption option, ILogger<WorkMonitor> logger)
    {
        _runSmartC = runSmartC.NotNull();
        _scheduleClient = scheduleClient.NotNull();
        _storageClient = storageClient.NotNull();
        _agentClient = agentClient.NotNull();
        _smartcClient = smartcClient;
        _abortSignal = abortSignal.NotNull();
        _option = option.NotNull();
        _logger = logger.NotNull();
    }

    public async Task Run(ScopeContext context)
    {
        context = context.With(_logger);

        var agentModelOption = await Startup(context);
        if (agentModelOption.IsError()) return;

        AgentModel agentModel = agentModelOption.Return();

        while (!_abortSignal.GetToken().IsCancellationRequested)
        {
            var workSchedule = await LookForWork(context);
            if (workSchedule.IsError()) return;

            var unpackPackageOption = await UnpackPackage(agentModel, workSchedule.Return(), context);
            if (workSchedule.IsError()) continue;
        }



        await _runSmartC.Run(context);

        return StatusCode.OK;
    }

    private async Task<Option<AgentModel>> Startup(ScopeContext context)
    {
        var agentModelOption = await _agentClient.Get(_option.AgentId, context);
        if (agentModelOption.IsError() || agentModelOption.Return().Validate().IsError())
        {
            context.Trace().LogError("Cannot get agent details, agentId={agentId}", _option.AgentId);
            return agentModelOption;
        }

        Directory.CreateDirectory(agentModelOption.Return().WorkingFolder);
        return agentModelOption;
    }

    private async Task<Option<ScheduleWorkModel>> LookForWork(ScopeContext context)
    {
        while (!_abortSignal.GetToken().IsCancellationRequested)
        {
            var result = await _scheduleClient.AssignWork(_option.AgentId, context);
            if (result.IsOk()) return result;

            await Task.Delay(TimeSpan.FromSeconds(5));
        }

        return StatusCode.ServiceUnavailable;
    }

    private async Task<Option> UnpackPackage(AgentModel agentModel, ScheduleWorkModel workSchedule, ScopeContext context)
    {
        context.Trace().LogInformation("Unpacking SmartC package smartcId={smartcId}", workSchedule.SmartcId);

        var storageBlobOption = await WriteToCache(agentModel, workSchedule.SmartcId, context);
        if (storageBlobOption.IsError()) return storageBlobOption.ToOptionStatus();

    }

    private async Task<Option<string>> WriteToCache(AgentModel agentModel, string smartcId, ScopeContext context)
    {
        var smartcModelOption = await _smartcClient
            .Get(smartcId, context)
            .LogResult(context.Trace());
        if (smartcModelOption.IsError())
        {
            context.Trace().LogError("Cannot find smartcId={smartcId}", smartcId);
            return smartcModelOption.ToOptionStatus<string>();
        }

        SmartcModel smartcModel = smartcModelOption.Return();

        var checkCacheResult = await CheckCache(agentModel, smartcModel.SmartcExeId, context);
        if (checkCacheResult.IsOk()) return checkCacheResult;

        return await DownloadAndExpand(agentModel, smartcModel.SmartcExeId, context);
    }

    private async Task<Option<string>> CheckCache(AgentModel agentModel, string smartcExeId, ScopeContext context)
    {
        Option<StorageBlobInfo> blobInfoOption = await _storageClient
            .GetInfo(smartcExeId, context)
            .LogResult(context.Trace());
        if (blobInfoOption.IsError())
        {
            context.Trace().LogError("Cannot get blob info for smartcExeId={smartcExeId}", smartcExeId);
            return blobInfoOption.ToOptionStatus<string>();
        }

        string packageFolder = Path.Combine(agentModel.WorkingFolder, blobInfoOption.Return().BlobHash);

        return Directory.Exists(packageFolder) switch
        {
            true => packageFolder,
            false => StatusCode.NotFound,
        };
    }

    private async Task<Option<string>> DownloadAndExpand(AgentModel agentModel, string smartcExeId, ScopeContext context)
    {
        context.Trace().LogInformation("Downloading and unpacking smartcExeId={smartcExeId}", smartcExeId);

        var blobPackageOption = await _storageClient.Get(smartcExeId, context);
        if (blobPackageOption.IsError())
        {
            context.Trace().LogError("Cannot download blob package for smartcExeId={smartcExeId}", smartcExeId);
            return StatusCode.NotFound;
        }

        StorageBlob blob = blobPackageOption.Return();

        string packageFolder = Path.Combine(agentModel.WorkingFolder, blob.Content.ToSHA256HexHash());
        if (Directory.Exists(packageFolder)) throw new InvalidOperationException("Directory should not exist");

        using (var memoryBuffer = new MemoryStream(blob.Content))
        {
            using var read = new ZipArchive(memoryBuffer, ZipArchiveMode.Read, leaveOpen: true);
            read.ExtractToFolder(packageFolder, _abortSignal.GetToken(), null);
        }

        context.Trace().LogInformation("Extracted SmartC package smartcExeId={smartcExeId} to folder={folder}", packageFolder);
        return packageFolder;
    }
}
