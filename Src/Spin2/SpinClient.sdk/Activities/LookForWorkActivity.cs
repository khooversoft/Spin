using Microsoft.Extensions.Logging;
using SpinCluster.abstraction;
using Toolbox.CommandRouter;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinClient.sdk;

public class LookForWorkActivity : ICommandRoute
{
    private readonly ILogger<LookForWorkActivity> _logger;
    private readonly SchedulerClient _schedulerClient;
    private readonly ScheduleOption _option;
    private readonly ICommandRouterHost _commandHost;

    public LookForWorkActivity(ScheduleOption option, ICommandRouterHost commandHost, SchedulerClient schedulerClient, ILogger<LookForWorkActivity> logger)
    {
        _option = option.NotNull();
        _commandHost = commandHost.NotNull();
        _schedulerClient = schedulerClient.NotNull();
        _logger = logger.NotNull();
    }

    public CommandSymbol CommandSymbol() => new CommandSymbol("run", "Start the agent and process requests").Action(x =>
    {
        var waitFor = x.AddOption<int?>("--waitFor", "Wait for n seconds, default = no timeout");
        var whatIf = x.AddOption<bool>("--whatIf", "Execute but will not run SmartC");

        x.SetHandler(Run, waitFor, whatIf);
    });

    public async Task<Option> Run(int? waitFor, bool whatIf)
    {
        var context = waitFor switch
        {
            null => new ScopeContext(_logger),
            int v => new ScopeContext(_logger, new CancellationTokenSource(TimeSpan.FromSeconds((int)waitFor)).Token)
        };

        var workOption = await _schedulerClient.LookForWork(_option, context);
        if (workOption.IsError()) return workOption.ToOptionStatus();

        ScheduleAssigned work = workOption.Return();

        context.Location().LogInformation(
            "Running 'runWork', workId={workId}, smartcId={smartcId}, command={command}",
            work.WorkAssigned.WorkId, work.WorkAssigned.SmartcId, work.WorkAssigned.Command
            );

        string[] args = ["runWork", "--workId", work.WorkAssigned.WorkId];
        args = whatIf ? [.. args, "--whatIf"] : args;

        _commandHost.Enqueue(args);
        return StatusCode.OK;
    }
}
