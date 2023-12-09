using System.Threading.Tasks.Dataflow;
using Microsoft.Extensions.Logging;
using SpinCluster.abstraction;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinAgent.Activities;

internal class RunSmartC
{
    private readonly ILogger<RunSmartC> _logger;
    private readonly AbortSignal _abortSignal;

    public RunSmartC(AbortSignal abortSignal, ILogger<RunSmartC> logger)
    {
        _abortSignal = abortSignal.NotNull();
        _logger = logger.NotNull();
    }

    public async Task<Option> Run(string folder, WorkAssignedModel workAssignedModel, bool whatIf, ScopeContext context)
    {
        folder.NotEmpty();
        workAssignedModel.NotNull();
        context = context.With(_logger);

        using CancellationTokenSource tokenSource = CancellationTokenSource.CreateLinkedTokenSource(_abortSignal.GetToken());
        context = new ScopeContext(context.TraceId, _logger, tokenSource.Token);

        var actionBlock = new ActionBlock<string>(x => context.Trace().LogInformation("[localHost] {line}", x));

        string args = new string[]
        {
            folder,
            workAssignedModel.Command,
            $"--workId {workAssignedModel.WorkId}",
        }.Join(' ');

        context.Location().LogInformation("Starting SmartC, commandLine={commandLine}", args);

        if (whatIf)
        {
            context.Location().LogInformation("[whatIf] skipping running SmartC");
            return StatusCode.OK;
        }

        try
        {
            var result = await new LocalProcessBuilder()
                .SetCommandLine(args)
                .SetCaptureOutput(x => actionBlock.Post(x))
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
}
