using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Actors.ScheduleWork;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinClusterCmd.Activities;

internal class ScheduleWork
{
    private readonly ScheduleWorkClient _client;
    private readonly ILogger<ScheduleWork> _logger;

    public ScheduleWork(ScheduleWorkClient client, ILogger<ScheduleWork> logger)
    {
        _client = client.NotNull();
        _logger = logger.NotNull();
    }

    public async Task Delete(string workId)
    {
        var context = new ScopeContext(_logger);
        context.Trace().LogInformation("Deleting workId={workId}", workId);

        var clearOption = await _client.Delete(workId, context);
        if (clearOption.IsError())
        {
            context.Trace().LogStatus(clearOption, "Failed to delete workId={workId}", workId);
            return;
        }
    }

    public async Task Get(string workId)
    {
        var context = new ScopeContext(_logger);
        context.Trace().LogInformation("Getting workId={workId}", workId);

        var scheduleWorkModel = await _client.Get(workId, context);
        if (scheduleWorkModel.IsError())
        {
            context.Trace().LogStatus(scheduleWorkModel.ToOptionStatus(), "Failed to get workId={workId}", workId);
            return;
        }

        string result = scheduleWorkModel.Return()
            .GetConfigurationValues()
            .Select(x => $" - {x.Key}={x.Value}".Replace("{", "{{").Replace("}", "}}"))
            .Prepend($"Schedule work...")
            .Join(Environment.NewLine) + Environment.NewLine;

        context.Trace().LogInformation(result);
    }

    public async Task ReleaseAssign(string workId)
    {
        var context = new ScopeContext(_logger);
        context.Trace().LogInformation("Releasing assignment workId={workId}", workId);

        var releaseOption = await _client.ReleaseAssign(workId, context);
        if (releaseOption.IsError())
        {
            context.Trace().LogStatus(releaseOption, "Failed to delete workId={workId}", workId);
            return;
        }

        await Get(workId);
    }
}
