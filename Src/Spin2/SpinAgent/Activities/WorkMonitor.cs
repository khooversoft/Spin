﻿using Microsoft.Extensions.Logging;
using SpinAgent.Application;
using SpinAgent.Services;
using SpinCluster.sdk.Actors.Scheduler;
using SpinCluster.sdk.Actors.Smartc;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinAgent.Activities;

internal class WorkMonitor
{
    private readonly ILogger<WorkMonitor> _logger;
    private readonly RunSmartC _runSmartC;
    private readonly ScheduleClient _scheduleClient;
    private readonly AbortSignal _abortSignal;
    private readonly AgentOption _option;
    private readonly PackageManagement _packageManagement;

    public WorkMonitor(
        RunSmartC runSmartC,
        PackageManagement packageManagement,
        ScheduleClient scheduleClient,
        AbortSignal abortSignal,
        AgentOption option,
        ILogger<WorkMonitor> logger)
    {
        _runSmartC = runSmartC.NotNull();
        _scheduleClient = scheduleClient.NotNull();
        _packageManagement = packageManagement.NotNull();
        _abortSignal = abortSignal.NotNull();
        _option = option.NotNull();
        _logger = logger.NotNull();
    }

    public async Task Run(ScopeContext context)
    {
        context = context.With(_logger);

        while (!_abortSignal.GetToken().IsCancellationRequested)
        {
            var workScheduleOption = await LookForWork(context);
            if (workScheduleOption.IsError()) return;

            ScheduleWorkModel workSchedule = workScheduleOption.Return();

            var unpackPackageLocation = await UnpackPackage(workSchedule, context);
            if (unpackPackageLocation.IsError())
            {
                await UpdateWorkStatus(workSchedule.WorkId, unpackPackageLocation.StatusCode, unpackPackageLocation.Error, context);
                continue;
            }

            var runResult = await _runSmartC.Run(unpackPackageLocation.Return(), workSchedule, context);

            await UpdateWorkStatus(workSchedule.WorkId, runResult.StatusCode, runResult.Error, context);
        }
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

    private async Task<Option<string>> UnpackPackage(ScheduleWorkModel workSchedule, ScopeContext context)
    {
        context.Trace().LogInformation("Unpacking SmartC package smartcId={smartcId}", workSchedule.SmartcId);

        var result = await _packageManagement.LoadPackage(workSchedule.SmartcId, context);
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

        var updateOption = await _scheduleClient.CompletedWork(completeStatus, context);
        if (updateOption.IsError())
        {
            context.Location().LogError("Could not update complete work status on schedule, model={model}", completeStatus);
        }
    }
}
