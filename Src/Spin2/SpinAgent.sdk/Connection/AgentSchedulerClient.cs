using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Actors.Scheduler;
using SpinCluster.sdk.Actors.ScheduleWork;
using Toolbox.Data;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinAgent.sdk;

public class AgentSchedulerClient
{
    private readonly AgentOption _option;
    private readonly SchedulerClient _schedulerClient;
    private readonly ILogger<AgentSchedulerClient> _logger;

    public AgentSchedulerClient(AgentOption option, SchedulerClient schedulerClient, ILogger<AgentSchedulerClient> logger)
    {
        _option = option.NotNull();
        _schedulerClient = schedulerClient.NotNull();
        _logger = logger.NotNull();
    }

    public Task<Option> CreateSchedule<T1>(string command, string smartcId, T1 data1, ScopeContext context) where T1 : class
    {
        var set = new DataObjectSet().Set(data1);
        return CreateSchedule(command, smartcId, set, context);
    }

    public Task<Option> CreateSchedule<T1, T2>(string command, string smartcId, T1 data1, T2 data2, ScopeContext context) where T1 : class where T2 : class
    {
        var set = new DataObjectSet().Set(data1).Set(data2);
        return CreateSchedule(command, smartcId, set, context);
    }

    public async Task<Option> CreateSchedule(string command, string smartcId, DataObjectSet dataSet, ScopeContext context)
    {
        command.NotEmpty();
        smartcId.NotEmpty();
        dataSet.NotNull();
        context = context.With(_logger);

        var createRequest = new ScheduleCreateModel
        {
            SmartcId = smartcId,
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
