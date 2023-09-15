using System.CommandLine;
using System.Data;
using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Actors.Scheduler;
using SpinCluster.sdk.Actors.Smartc;
using SpinClusterCmd.Application;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinClusterCmd.Activities;

internal class Schedule
{
    private readonly ScheduleClient _client;
    private readonly ILogger<Schedule> _logger;

    public Schedule(ScheduleClient client, ILogger<Schedule> logger)
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
        var queueResult = await _client.AddSchedule(model, context).LogResult(context.Location());
        if (queueResult.IsError()) return;

        context.Trace().LogInformation("Queued command, workId={workId}", model.WorkId);
    }

    public async Task Clear(string principalId)
    {
        var context = new ScopeContext(_logger);
        context.Trace().LogInformation("Clearing schedule queue");

        var clearOption = await _client
            .Clear(principalId, context)
            .LogResult(context.Location());

        if (clearOption.IsError())
        {
            context.Trace().LogError("Failed to clear schedule queue");
            return;
        }
    }

    public async Task Get()
    {
        var context = new ScopeContext(_logger);
        context.Trace().LogInformation("Clearing schedule queue");

        var scheduleModel = await _client
            .GetSchedules(context)
            .LogResult(context.Location());

        if (scheduleModel.IsError())
        {
            context.Trace().LogError("Failed to get schedule");
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
