using Microsoft.Extensions.Logging;
using SpinCluster.abstraction;
using Toolbox.CommandRouter;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinClient.sdk;

public class RunWorkActivity : ICommandRoute
{
    private readonly ScheduleWorkClient _scheduleWorkClient;
    private readonly ILogger<RunWorkActivity> _logger;
    private readonly IRunSmartc _runSmartc;
    private readonly ScheduleOption _scheduleOption;

    public RunWorkActivity(ScheduleWorkClient scheduleWorkClient, ScheduleOption scheduleOption, IRunSmartc runSmartc, ILogger<RunWorkActivity> logger)
    {
        _scheduleWorkClient = scheduleWorkClient.NotNull();
        _scheduleOption = scheduleOption.NotNull();
        _runSmartc = runSmartc.NotNull();
        _logger = logger.NotNull();
    }

    public CommandSymbol CommandSymbol() => new CommandSymbol("runWork", "Execute the details of the work").Action(x =>
    {
        var workId = x.AddOption<string>("--workId", "Work Id of work", isRequired: true);
        var whatIf = x.AddOption<bool>("--whatIf", "No execution");

        x.SetHandler(RunWork, workId, whatIf);
    });

    private async Task RunWork(string workId, bool whatIf)
    {
        workId.NotEmpty();
        var context = new ScopeContext(_logger);

        var scheduleWorkModelOption = await _scheduleWorkClient.Get(workId, context);
        if (scheduleWorkModelOption.IsError())
        {
            context.Location().LogStatus(scheduleWorkModelOption.ToOptionStatus(), "failed to get workId={workId}", workId);
            return;
        }

        var assigned = new ScheduleAssigned
        {
            ScheduleOption = _scheduleOption,
            WorkAssigned = scheduleWorkModelOption.Return().ConvertTo(),
        };

        Option result = await _runSmartc.Run(assigned, whatIf, context);
        context.Location().LogStatus(result, "runSmartc");
    }
}
