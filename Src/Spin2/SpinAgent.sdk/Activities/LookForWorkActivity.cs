using Microsoft.Extensions.Logging;
using Toolbox.CommandRouter;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinAgent.sdk;

public class LookForWorkActivity : ICommandRoute
{
    private readonly ILogger<LookForWorkActivity> _logger;
    private readonly IRunSmartc _runSmartC;
    private readonly AgentSession _agentSession;

    public LookForWorkActivity(
        IRunSmartc runSmartC,
        AgentSession agentSession,
        ILogger<LookForWorkActivity> logger)
    {
        _runSmartC = runSmartC.NotNull();
        _agentSession = agentSession.NotNull();
        _logger = logger.NotNull();
    }

    public CommandSymbol CommandSymbol() => new CommandSymbol("run", "Start the agent and process requests").Action(x =>
    {
        var whatIf = x.AddOption<bool>("--whatIf", "Execute but will not run SmartC");
        x.SetHandler(Run, whatIf);
    });

    public async Task<Option> Run(bool whatIf)
    {
        var context = new ScopeContext(_logger);

        var workOption = await _agentSession.LookForWork(context);
        if (workOption.IsError()) return workOption.ToOptionStatus();

        WorkSession work = workOption.Return();
        context.Location().LogInformation("Running workId={workId}, smartcId={smartcId}, command={command}", work.WorkAssigned.WorkId, work.WorkAssigned.SmartcId, work.WorkAssigned.Command);

        var runOption = await _runSmartC.Run(work, whatIf, context);
        if (runOption.IsError())
        {
            context.Location().LogError("Failed to start workId={workId}, smartcId={smartcId}, command={command}", work.WorkAssigned.WorkId, work.WorkAssigned.SmartcId, work.WorkAssigned.Command);
            return runOption;
        }

        return StatusCode.OK;
    }
}
