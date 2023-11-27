using System.Threading.Tasks.Dataflow;
using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Actors.Scheduler;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Tools.Local;
using Toolbox.Types;

namespace SpinAgent.sdk;

public class RunSmartC : IRunSmartc
{
    private readonly ILogger<RunSmartC> _logger;
    private readonly AbortSignal _abortSignal;
    private readonly PackageManagement _packageManagement;

    public RunSmartC(PackageManagement packageManagement, AbortSignal abortSignal, ILogger<RunSmartC> logger)
    {
        _packageManagement = packageManagement.NotNull();
        _abortSignal = abortSignal.NotNull();
        _logger = logger.NotNull();
    }

    public async Task<Option> Run(WorkSession agentWorkClient, bool whatIf, ScopeContext context)
    {
        context = context.With(_logger);
        agentWorkClient.NotNull();

        var unpackPackageLocation = await UnpackPackage(agentWorkClient, context);
        if (unpackPackageLocation.IsError())
        {
            await agentWorkClient.UpdateWorkStatus(unpackPackageLocation.StatusCode, unpackPackageLocation.Error, context);
            return StatusCode.InternalServerError;
        }

        var runResult = await RunLocal(unpackPackageLocation.Return(), agentWorkClient.WorkAssigned, whatIf, context);
        await agentWorkClient.UpdateWorkStatus(runResult.StatusCode, runResult.Error, context);

        return StatusCode.OK;
    }

    private async Task<Option<string>> UnpackPackage(WorkSession agentWorkClient, ScopeContext context)
    {
        context.Trace().LogInformation("Unpacking SmartC package smartcId={smartcId}", agentWorkClient.WorkAssigned.SmartcId);

        var result = await _packageManagement.LoadPackage(agentWorkClient.WorkAssigned.SmartcId, context);
        if (result.IsError())
        {
            await agentWorkClient.UpdateWorkStatus(result.StatusCode, result.Error, context);
            return result;
        }

        return result;
    }

    private async Task<Option> RunLocal(string folder, WorkAssignedModel workAssignedModel, bool whatIf, ScopeContext context)
    {
        folder.NotEmpty();
        workAssignedModel.NotNull();
        context = context.With(_logger);

        using CancellationTokenSource tokenSource = CancellationTokenSource.CreateLinkedTokenSource(_abortSignal.GetToken());
        context = new ScopeContext(context.TraceId, _logger, tokenSource.Token);

        var actionBlock = new ActionBlock<string>(x => context.Trace().LogInformation("[localHost] {line}", x));

        string args = new string[]
        {
            folder,
            workAssignedModel.Command,
            $"--workId {workAssignedModel.WorkId}",
        }.Join(' ');

        context.Location().LogInformation("Starting SmartC, commandLine={commandLine}", args);

        if (whatIf)
        {
            context.Location().LogInformation("[whatIf] skipping running SmartC");
            return StatusCode.OK;
        }

        try
        {
            var result = await new LocalProcessBuilder()
                .SetCommandLine(args)
                .SetCaptureOutput(x => actionBlock.Post(x))
                .Build()
                .Run(context);

            return result.ToOptionStatus();
        }
        catch (Exception ex)
        {
            context.Location().LogCritical(ex, "Local process failed");
            return StatusCode.InternalServerError;
        }
    }
}
