using Microsoft.Extensions.Logging;
using SpinClient.sdk;
using Toolbox.CommandRouter;
using Toolbox.Extensions;
using Toolbox.Logging;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinClusterCmd.Activities;

internal class ScheduleWork : ICommandRoute
{
    private readonly ScheduleWorkClient _client;
    private readonly ILogger<ScheduleWork> _logger;

    public ScheduleWork(ScheduleWorkClient client, ILogger<ScheduleWork> logger)
    {
        _client = client.NotNull();
        _logger = logger.NotNull();
    }

    public CommandSymbol CommandSymbol() => new CommandSymbol("work", "Schedule work items")
    {
        new CommandSymbol("delete", "Delete schedule work").Action(command =>
        {
            var workId = command.AddArgument<string>("workId", "WorkId ex. 'schedulework:WKID-9a8bf61d-e0b4-4c99-98da-ddca139a988f'}");
            command.SetHandler(Delete, workId);
        }),
        new CommandSymbol("get", "Get schedules").Action(command =>
        {
            var workId = command.AddArgument<string>("workId", "WorkId ex. 'schedulework:WKID-9a8bf61d-e0b4-4c99-98da-ddca139a988f'}");
            command.SetHandler(Get, workId);
        }),
        new CommandSymbol("release", "Release assignment of work").Action(command =>
        {
            var workId = command.AddArgument<string>("workId", "WorkId ex. 'schedulework:WKID-9a8bf61d-e0b4-4c99-98da-ddca139a988f'}");
            var force = command.AddOption<bool>("--force", "Force release, bypass completed check");
            command.SetHandler(ReleaseAssign, workId, force);
        }),
    };

    public async Task Delete(string workId)
    {
        var context = new ScopeContext(_logger);
        context.LogInformation("Deleting workId={workId}", workId);

        var clearOption = await _client.Delete(workId, context);
        if (clearOption.IsError())
        {
            clearOption.LogStatus(context, "Failed to delete workId={workId}", [workId]);
            return;
        }
    }

    public async Task Get(string workId)
    {
        var context = new ScopeContext(_logger);
        context.LogInformation("Getting workId={workId}", workId);

        var scheduleWorkModel = await _client.Get(workId, context);
        if (scheduleWorkModel.IsError())
        {
            scheduleWorkModel.LogStatus(context, "Failed to get workId={workId}", [workId]);
            return;
        }

        string result = scheduleWorkModel.Return()
            .ToDictionary()
            .Select(x => $" - {x.Key}={x.Value}".Replace("{", "{{").Replace("}", "}}"))
            .Prepend($"Schedule work...")
            .Join(Environment.NewLine) + Environment.NewLine;

        context.LogInformation(result);
    }

    public async Task ReleaseAssign(string workId, bool force)
    {
        var context = new ScopeContext(_logger);
        context.LogInformation("Releasing assignment workId={workId}", workId);

        var releaseOption = await _client.ReleaseAssign(workId, force, context);
        if (releaseOption.IsError())
        {
            releaseOption.LogStatus(context, "Failed to delete workId={workId}", [workId]);
            return;
        }

        await Get(workId);
    }
}
