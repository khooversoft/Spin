using System.CommandLine;
using Loan_smartc_v1.Application;
using Microsoft.Extensions.Logging;
using Toolbox.Tools;
using Toolbox.Types;

namespace Loan_smartc_v1.Commands;

internal class RunCommand : Command
{
    private readonly AbortSignal _abortSignal;
    private readonly ILogger<RunCommand> _logger;

    public RunCommand(AbortSignal abortSignal, ILogger<RunCommand> logger)
        : base("run", "Start the agent and process requests")
    {
        _abortSignal = abortSignal.NotNull();
        _logger = logger.NotNull();

        this.SetHandler(() =>
        {
            var context = new ScopeContext(logger);

            _logger.LogInformation("Contract is running");
            Task.WaitAll(Run());

            _logger.LogInformation("Contract has stopped");
        });
    }
    private async Task Run()
    {
        using var tokenSource = CancellationTokenSource.CreateLinkedTokenSource(_abortSignal.GetToken());

        int count = 0;
        while (!tokenSource.IsCancellationRequested && count < 100)
        {
            try { await Task.Delay(TimeSpan.FromSeconds(2), tokenSource.Token); } catch { }

            _logger.LogInformation("Ping...");
            count++;
        }
    }
}
