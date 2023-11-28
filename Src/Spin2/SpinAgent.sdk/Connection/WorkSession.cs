using Microsoft.Extensions.Logging;
using SpinClient.sdk;
using SpinCluster.abstraction;
using Toolbox.Data;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinAgent.sdk;

public class WorkSession
{
    private readonly WorkAssignedModel _workAssignedModel;
    private readonly ILogger<WorkSession> _logger;
    private readonly AgentOption _option;
    private readonly SchedulerClient _schedulerClient;
    private readonly ScheduleWorkClient _workClient;

    public WorkSession(WorkAssignedModel workAssignedModel, AgentOption option, SchedulerClient schedulerClient, ScheduleWorkClient workClient, ILogger<WorkSession> logger)
    {
        _workAssignedModel = workAssignedModel.NotNull();
        _option = option.NotNull();
        _schedulerClient = schedulerClient.NotNull();
        _workClient = workClient.NotNull();
        _logger = logger.NotNull();
    }

    public WorkAssignedModel WorkAssigned => _workAssignedModel;

    public async Task UpdateWorkStatus(StatusCode statusCode, string? message, ScopeContext context)
    {
        var completeStatus = new AssignedCompleted
        {
            AgentId = _option.AgentId,
            WorkId = WorkAssigned.WorkId,
            StatusCode = statusCode,
            Message = statusCode.IsOk() ? message ?? "Completed" : message ?? "< no message >",
        };

        var updateOption = await _workClient.CompletedWork(completeStatus, context);
        if (updateOption.IsError())
        {
            context.Location().LogError("Could not update complete work status on schedule, model={model}", completeStatus);
        }
    }

    public Task<Option> CreateSchedule<T1>(string command, T1 data1, ScopeContext context) where T1 : class
    {
        var set = new DataObjectSet().Set(data1);
        return CreateScheduleInternal(command, set, context);
    }

    public Task<Option> CreateSchedule<T1, T2>(string command, T1 data1, T2 data2, ScopeContext context) where T1 : class where T2 : class
    {
        var set = new DataObjectSet().Set(data1).Set(data2);
        return CreateScheduleInternal(command, set, context);
    }

    private async Task<Option> CreateScheduleInternal(string command, DataObjectSet dataSet, ScopeContext context)
    {
        command.NotEmpty();
        dataSet.NotNull();
        context = context.With(_logger);

        var createRequest = new ScheduleCreateModel
        {
            SmartcId = _workAssignedModel.SmartcId,
            SchedulerId = _option.SchedulerId,
            PrincipalId = _option.PrincipalId,
            SourceId = _option.SourceId,
            Command = command,
            Payloads = dataSet,
        };

        context.Location().LogInformation("Creating schedule, createRequest={createRequest}", createRequest);
        var queueResult = await _schedulerClient.CreateSchedule(createRequest, context);
        return queueResult;
    }
}
