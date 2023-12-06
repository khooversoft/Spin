using Microsoft.Extensions.Logging;
using SpinAgent.Application;
using SpinClient.sdk;
using SpinCluster.abstraction;
using Toolbox.CommandRouter;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinAgent.Activities;

internal class WorkMonitor : ICommandRoute
{
    private readonly ILogger<WorkMonitor> _logger;
    private readonly RunSmartC _runSmartC;
    private readonly SchedulerClient _scheduleClient;
    private readonly AbortSignal _abortSignal;
    private readonly AgentOption _option;
    private readonly PackageManagement _packageManagement;
    private readonly ScheduleWorkClient _scheduleWorkClient;

    public WorkMonitor(
        RunSmartC runSmartC,
        PackageManagement packageManagement,
        SchedulerClient scheduleClient,
        ScheduleWorkClient scheduleWorkClient,
        AbortSignal abortSignal,
        AgentOption option,
        ILogger<WorkMonitor> logger)
    {
        _runSmartC = runSmartC.NotNull();
        _packageManagement = packageManagement.NotNull();
        _scheduleClient = scheduleClient.NotNull();
        _scheduleWorkClient = scheduleWorkClient.NotNull();
        _abortSignal = abortSignal.NotNull();
        _option = option.NotNull();
        _logger = logger.NotNull();
    }

    public CommandSymbol CommandSymbol() => new CommandSymbol("run", "Start the agent and process requests").Action(x =>
    {
        var whatIf = x.AddOption<bool>("--whatIf", "Execute but will not run SmartC");
        x.SetHandler(Run, whatIf);
    });

    public async Task Run(bool whatIf)
    {
        var context = new ScopeContext(_logger);

        while (!_abortSignal.GetToken().IsCancellationRequested)
        {
            var workScheduleOption = await LookForWork(context);
            if (workScheduleOption.IsError()) return;

            WorkAssignedModel workSchedule = workScheduleOption.Return();

            var unpackPackageLocation = await UnpackPackage(workSchedule, context);
            if (unpackPackageLocation.IsError())
            {
                await UpdateWorkStatus(workSchedule.WorkId, unpackPackageLocation.StatusCode, unpackPackageLocation.Error, context);
                continue;
            }

            var runResult = await _runSmartC.Run(unpackPackageLocation.Return(), workSchedule, whatIf, context);

            await UpdateWorkStatus(workSchedule.WorkId, runResult.StatusCode, runResult.Error, context);
        }
    }

    private async Task<Option<WorkAssignedModel>> LookForWork(ScopeContext context)
    {
        while (!_abortSignal.GetToken().IsCancellationRequested)
        {
            var result = await _scheduleClient.AssignWork(_option.SchedulerId, _option.AgentId, context);
            if (result.IsOk()) return result;

            await Task.Delay(TimeSpan.FromSeconds(5));
        }

        return StatusCode.ServiceUnavailable;
    }

    private async Task<Option<string>> UnpackPackage(WorkAssignedModel workSchedule, ScopeContext context)
    {
        context.Trace().LogInformation("Unpacking SmartC package smartcId={smartcId}", workSchedule.SmartcId);

        var result = await _packageManagement.LoadPackage(_option.AgentId, workSchedule.SmartcId, context);
        if (result.IsError())
        {
            await UpdateWorkStatus(workSchedule.WorkId, result.StatusCode, result.Error, context);
            return result;
        }

        return result;
    }

    private async Task UpdateWorkStatus(string workId, StatusCode statusCode, string? message, ScopeContext context)
    {
        var completeStatus = new AssignedCompleted
        {
            AgentId = _option.AgentId,
            WorkId = workId,
            StatusCode = statusCode,
            Message = statusCode.IsOk() ? message ?? "Completed" : message ?? "< no message >",
        };

        var updateOption = await _scheduleWorkClient.CompletedWork(completeStatus, context);
        if (updateOption.IsError())
        {
            context.Location().LogError("Could not update complete work status on schedule, model={model}", completeStatus);
        }
    }
}
