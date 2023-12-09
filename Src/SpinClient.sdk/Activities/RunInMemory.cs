using Microsoft.Extensions.Logging;
using SpinCluster.abstraction;
using Toolbox.CommandRouter;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinClient.sdk;

public class RunInMemory : IRunSmartc
{
    private readonly ICommandRouterHost _commandRouterHost;
    private readonly ILogger<RunInMemory> _logger;

    public RunInMemory(ICommandRouterHost commandRouterHost, ILogger<RunInMemory> logger)
    {
        _commandRouterHost = commandRouterHost.NotNull();
        _logger = logger.NotNull();
    }

    public Task<Option> Run(ScheduleAssigned scheduleAssigned, bool whatIf, ScopeContext context)
    {
        context = context.With(_logger);
        context.Location().LogInformation("Enqueue command: command={command} for workId={workId}", scheduleAssigned.WorkAssigned.Command, scheduleAssigned.WorkAssigned.WorkId);

        _commandRouterHost.Enqueue(scheduleAssigned.WorkAssigned.Command, "--workId", scheduleAssigned.WorkAssigned.WorkId);
        return new Option(StatusCode.OK).ToTaskResult();
    }
}
