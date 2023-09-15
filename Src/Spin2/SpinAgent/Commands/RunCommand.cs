using System.CommandLine;
using Microsoft.Extensions.Logging;
using SpinAgent.Activities;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinAgent.Commands;

internal class RunCommand : Command
{
    private readonly WorkMonitor _monitor;
    private readonly ILogger<RunCommand> _logger;

    public RunCommand(WorkMonitor monitor, ILogger<RunCommand> logger)
        : base("run", "Start the agent and process requests")
    {
        _monitor = monitor.NotNull();
        _logger = logger.NotNull();

        this.SetHandler(() =>
        {
            var context = new ScopeContext(logger);

            _logger.LogInformation("Agent is running");
            Task.WaitAll(monitor.Run(context));

            _logger.LogInformation("Agent has stopped");
        });
    }
}
