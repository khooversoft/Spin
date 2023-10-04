using System.Data;
using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Actors.Scheduler;
using SpinCluster.sdk.Actors.ScheduleWork;
using SpinClusterCmd.Application;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinClusterCmd.Activities;

internal class Schedule
{
    private readonly SchedulerClient _client;
    private readonly ILogger<Schedule> _logger;

    public Schedule(SchedulerClient client, ILogger<Schedule> logger)
    {
        _client = client.NotNull();
        _logger = logger.NotNull();
    }

    public async Task Add(string jsonFile)
    {
        var context = new ScopeContext(_logger);
        context.Trace().LogInformation("Processing file {file}", jsonFile);

        var readResult = CmdTools.LoadJson<ScheduleCreateModel>(jsonFile, ScheduleCreateModel.Validator, context);
        if (readResult.IsError()) return;

        ScheduleCreateModel model = readResult.Return();

        context.Trace().LogInformation("Adding schedule, model={model}", model);
        var queueResult = await _client.CreateSchedule(model, context);
        if (queueResult.IsError())
        {
            context.Trace().LogStatus(queueResult, "Failed to add scehdule, model={model}", model);
            return;
        }

        context.Trace().LogInformation("Queued command, workId={workId}", model.WorkId);
    }

    public async Task Clear(string principalId)
    {
        var context = new ScopeContext(_logger);
        context.Trace().LogInformation("Clearing schedule queue");

        var clearOption = await _client.Clear(principalId, context);
        if (clearOption.IsError())
        {
            context.Trace().LogStatus(clearOption, "Failed to clear schedule queue");
            return;
        }
    }

    public async Task Get()
    {
        var context = new ScopeContext(_logger);
        context.Trace().LogInformation("Getting schedules");

        var scheduleModel = await _client.GetSchedules(context);
        if (scheduleModel.IsError())
        {
            context.Trace().LogStatus(scheduleModel.ToOptionStatus(), "Failed to get schedule");
            return;
        }

        string result = scheduleModel.Return()
            .GetConfigurationValues()
            .Select(x => $" - {x.Key}={x.Value}")
            .Prepend($"Schedules...")
            .Join(Environment.NewLine) + Environment.NewLine;

        context.Trace().LogInformation(result);
    }
}
