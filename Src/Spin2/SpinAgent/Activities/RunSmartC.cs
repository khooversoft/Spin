using System.Threading.Tasks.Dataflow;
using Microsoft.Extensions.Logging;
using SpinAgent.Application;
using Toolbox.Tools;
using Toolbox.Tools.Local;
using Toolbox.Types;

namespace SpinAgent.Activities;

internal class RunSmartC
{
    private readonly ILogger<RunSmartC> _logger;
    private readonly ActionBlock<string> _output;
    private readonly AgentOption _agentOption;
    private readonly AbortSignal _abortSignal;

    public RunSmartC(AgentOption agentOption, AbortSignal abortSignal, ILogger<RunSmartC> logger)
    {
        _agentOption = agentOption.NotNull();
        _abortSignal = abortSignal.NotNull();
        _logger = logger.NotNull();

        _output = new ActionBlock<string>(OutputSync);
    }

    public async Task<Option> Run(ScopeContext context)
    {
        using CancellationTokenSource tokenSource = CancellationTokenSource.CreateLinkedTokenSource(_abortSignal.GetToken());
        context = new ScopeContext(context.TraceId, _logger, tokenSource.Token);

        //context.Location().LogInformation("Starting SmartC, commandLine={commandLine}", _agentOption.CommandLine);

        try
        {
            var result = await new LocalProcessBuilder()
                //.SetCommandLine(_agentOption.CommandLine)
                .SetCaptureOutput(x => _output.Post(x))
                .Build()
                .Run(context);

            return result.ToOptionStatus();
        }
        catch (Exception ex)
        {
            context.Location().LogCritical(ex, "Local process failed");
            return StatusCode.InternalServerError;
        }
    }

    private void OutputSync(string line)
    {
        Console.WriteLine(line);
    }
}
