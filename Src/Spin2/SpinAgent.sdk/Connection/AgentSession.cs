using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SpinClient.sdk;
using SpinCluster.abstraction;
using Toolbox.Data;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinAgent.sdk;

public class AgentSession
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AgentSession> _logger;
    private readonly AgentOption _option;
    private readonly SchedulerClient _schedulerClient;

    public AgentSession(IServiceProvider serviceProvider, AgentOption option, ILogger<AgentSession> logger)
    {
        _serviceProvider = serviceProvider.NotNull();
        _option = option.NotNull();
        _logger = logger.NotNull();

        _schedulerClient = _serviceProvider.GetRequiredService<SchedulerClient>();
        Client = serviceProvider.GetRequiredService<AgentSchedulerClient>();
    }

    public AgentSchedulerClient Client { get; }

    public async Task<Option<WorkSession>> LookForWork(ScopeContext context)
    {
        context = context.With(_logger);

        while (!context.IsCancellationRequested)
        {
            context.Location().LogInformation("Looking for work, scehdulerId={schedulerId}, agentId={agentId}", _option.SchedulerId, _option.AgentId);

            var result = await _schedulerClient.AssignWork(_option.SchedulerId, _option.AgentId, context);
            if (result.IsOk())
            {
                context.Location().LogInformation("Found work, scehdulerId={schedulerId}, agentId={agentId}, model={model}", _option.SchedulerId, _option.AgentId, result.Return());

                WorkAssignedModel assignment = result.Return();
                WorkSession agentWorkClient = ActivatorUtilities.CreateInstance<WorkSession>(_serviceProvider, assignment);
                return agentWorkClient;
            }

            await Task.Delay(TimeSpan.FromSeconds(5));
        }

        return StatusCode.NotFound;
    }

    public Task<Option> CreateSchedule<T1>(string command, string smartcId, T1 data1, ScopeContext context) where T1 : class
    {
        var set = new DataObjectSet().Set(data1);
        return CreateScheduleInternal(command, smartcId, set, context);
    }

    public Task<Option> CreateSchedule<T1, T2>(string command, string smartcId, T1 data1, T2 data2, ScopeContext context) where T1 : class where T2 : class
    {
        var set = new DataObjectSet().Set(data1).Set(data2);
        return CreateScheduleInternal(command, smartcId, set, context);
    }

    private async Task<Option> CreateScheduleInternal(string command, string smartcId, DataObjectSet dataSet, ScopeContext context)
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
