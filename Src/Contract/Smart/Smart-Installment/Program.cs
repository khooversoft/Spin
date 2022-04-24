
using ContractHost.sdk;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;

await (await new ContractHostBuilder()
    .AddCommand(args)
    .AddSingleton<CheckPoint>()
    .Build()).Run();


internal class CheckPoint : IStateService
{
    private readonly ILogger<CheckPoint> _logger;
    private readonly ContractState _contractState;

    public CheckPoint(ILogger<CheckPoint> logger)
    {
        _logger = logger;

        _contractState = new ContractStateBuilder()
            .AddCheck("calculate-interest-charge", CalculateInterestCharge)
            .AddRun("run", Run)
            .Build(logger);
    }

    public async Task Run(IContractHost runHost)
    {
        string? runEventName = await _contractState.RunChecks(runHost);
        _logger.LogTrace("RunCheck returned {runEventName}", runEventName);
        if (runEventName.IsEmpty()) return;

        await _contractState.RunState(runHost, runEventName);
    }

    private Task<bool> CalculateInterestCharge(IContractHost contractRunHost) => Task.FromResult(true);

    private Task Run(IContractHost runHost, string runEventName) => Task.CompletedTask;
}
