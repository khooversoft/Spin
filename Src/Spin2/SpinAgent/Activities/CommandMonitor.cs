using Microsoft.Extensions.Logging;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinAgent.Activities;

internal class CommandMonitor
{
    private readonly ILogger<CommandMonitor> _logger;
    private readonly RunSmartC _runSmartC;

    public CommandMonitor(RunSmartC runSmartC, ILogger<CommandMonitor> logger)
    {
        _logger = logger.NotNull();
        _runSmartC = runSmartC.NotNull();
    }

    public async Task<Option> Run(ScopeContext context)
    {
        context = context.With(_logger);

        await _runSmartC.Run(context);

        return StatusCode.OK;
    }
}
