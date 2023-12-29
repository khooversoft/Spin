using System.Data;
using Microsoft.Extensions.Logging;
using SpinClient.sdk;
using SpinCluster.abstraction;
using SpinCluster.sdk.Actors.Scheduler;
using SpinClusterCmd.Application;
using Toolbox.CommandRouter;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinClusterCmd.Activities;

internal class Schedule : ICommandRoute
{
    private readonly SchedulerClient _schedulerClient;
    private readonly ILogger<Schedule> _logger;
    private readonly ScheduleWorkClient _scheduleWorkClient;

    public Schedule(SchedulerClient client, ScheduleWorkClient scheduleWorkClient, ILogger<Schedule> logger)
    {
        _schedulerClient = client.NotNull();
        _scheduleWorkClient = scheduleWorkClient.NotNull();
        _logger = logger.NotNull();
    }

    public CommandSymbol CommandSymbol() => new CommandSymbol("schedule", "Create, clear, dump schedule queues")
    {
        new CommandSymbol("add", "Add a schedule").Action(x =>
        {
            var jsonFile = x.AddArgument<string>("file", "Json file command details");
            x.SetHandler(Add, jsonFile);
        }),
        new CommandSymbol("clear", "Clear schedule queues").Action(x =>
        {
            var schedulerId = x.AddArgument<string>("schedulerId", "SchedulerId, ex. scheduler:{name");
            var principalId = x.AddArgument<string>("principalId", "PrincipalId, ex. {user}@{domain}");
            x.SetHandler(Clear, schedulerId, principalId);
        }),
        new CommandSymbol("get", "Get schedules").Action(x =>
        {
            var schedulerId = x.AddArgument<string>("schedulerId", "SchedulerId, ex. scheduler:{name");
            x.SetHandler(Get, schedulerId);
        }),
    };

    public async Task Add(string jsonFile)
    {
        var context = new ScopeContext(_logger);
        context.LogInformation("Processing file {file}", jsonFile);

        var readResult = CmdTools.LoadJson<ScheduleCommandModel>(jsonFile, ScheduleCommandModel.Validator, context);
        if (readResult.IsError()) return;

        ScheduleCreateModel model = readResult.Return().ConvertTo();

        context.LogInformation("Adding schedule, model={model}", model);
        var queueResult = await _schedulerClient.CreateSchedule(model, context);
        if (queueResult.IsError())
        {
            queueResult.LogStatus(context, "Failed to add scehdule, model={model}", model);
            return;
        }

        context.LogInformation("Queued command, workId={workId}", model.WorkId);
    }

    public async Task Clear(string schedulerId, string principalId)
    {
        var context = new ScopeContext(_logger);
        context.LogInformation("Clearing schedule queue");

        var clearAllOption = await _schedulerClient.ClearAllWorkSchedules(schedulerId, _scheduleWorkClient, context);
        clearAllOption.LogStatus(context, "Clear schedule queue");

        Option deleteResponse = await _schedulerClient.Delete(schedulerId, principalId, context);
        deleteResponse.LogStatus(context, "Delete schedule queue");
    }

    public async Task Get(string schedulerId)
    {
        var context = new ScopeContext(_logger);
        context.LogInformation("Getting schedules");

        var scheduleModel = await _schedulerClient.GetSchedules(schedulerId, context);
        if (scheduleModel.IsError())
        {
            scheduleModel.LogStatus(context, "Failed to get schedule");
            return;
        }

        SchedulesResponseModel model = scheduleModel.Return();

        var lines = new string[][]
        {
            new [] { "Active schedules..." },
            dumpValues(model.ActiveItems.Values),
            new [] { "Completed wpork..." },
            dumpValues(model.CompletedItems.Values),
            new [] { string.Empty },
        };

        string[] dumpValues<T>(ICollection<T> values) => values switch
        {
            { Count: 0 } => new[] { "  No items" },
            var v => v.Select((x, i) => $"  ({i}) {fixUp(x)}").ToArray(),
        };

        string fixUp<T>(T value) => value?.ToString()?.Replace("{", string.Empty).Replace("}", string.Empty).Trim() ?? "<no data>";

        string line = lines
            .SelectMany(x => x)
            .Join(Environment.NewLine);

        context.LogInformation(line);
    }
}
