using System.Threading.Tasks.Dataflow;
using Microsoft.Extensions.Logging;
using SpinCluster.abstraction;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinClient.sdk;

public class RunSmartC : IRunSmartc
{
    private readonly ILogger<RunSmartC> _logger;
    private readonly AbortSignal _abortSignal;
    private readonly PackageManagement _packageManagement;
    private readonly ScheduleWorkClient _scheduleWorkClient;

    public RunSmartC(PackageManagement packageManagement, ScheduleWorkClient scheduleWorkClient, AbortSignal abortSignal, ILogger<RunSmartC> logger)
    {
        _packageManagement = packageManagement.NotNull();
        _abortSignal = abortSignal.NotNull();
        _scheduleWorkClient = scheduleWorkClient.NotNull();
        _logger = logger.NotNull();
    }

    public async Task<Option> Run(ScheduleAssigned scheduleAssigned, bool whatIf, ScopeContext context)
    {
        context = context.With(_logger);
        scheduleAssigned.NotNull();

        var unpackPackageLocation = await UnpackPackage(scheduleAssigned, context);
        if (unpackPackageLocation.IsError())
        {
            await _scheduleWorkClient.CompletedWork(
                scheduleAssigned.ScheduleOption.AgentId,
                scheduleAssigned.WorkAssigned.WorkId,
                unpackPackageLocation.ToOptionStatus(),
                "Unpack package",
                context);

            return StatusCode.InternalServerError;
        }

        var runResult = await RunLocal(unpackPackageLocation.Return(), scheduleAssigned.WorkAssigned, whatIf, context);
        await _scheduleWorkClient.CompletedWork(scheduleAssigned.ScheduleOption.AgentId, scheduleAssigned.WorkAssigned.WorkId, runResult, "Run local", context);

        return StatusCode.OK;
    }

    private async Task<Option<string>> UnpackPackage(ScheduleAssigned scheduleAssigned, ScopeContext context)
    {
        context.Trace().LogInformation("Unpacking SmartC package smartcId={smartcId}", scheduleAssigned.WorkAssigned.SmartcId);

        var result = await _packageManagement.LoadPackage(scheduleAssigned.ScheduleOption.AgentId, scheduleAssigned.WorkAssigned.SmartcId, context);
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
