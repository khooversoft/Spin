using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Tools;

namespace ContractHost.sdk;

public class ContractState
{
    private readonly Dictionary<string, CheckFunction> _checks;
    private readonly Dictionary<string, RunFunction> _runs;
    private readonly ILogger _logger;

    public delegate Task<bool> CheckFunction(IContractHost runHost);

    public delegate Task RunFunction(IContractHost runHost, string eventName);


    public ContractState(IReadOnlyDictionary<string, CheckFunction> checks, IReadOnlyDictionary<string, RunFunction> runs, ILogger logger)
    {
        checks.VerifyNotNull(nameof(checks));
        runs.VerifyNotNull(nameof(runs));
        logger.VerifyNotNull(nameof(logger));

        _checks = checks.ToDictionary(x => x.Key, x => x.Value);
        _runs = runs.ToDictionary(x => x.Key, x => x.Value);
        _logger = logger;
    }

    public async Task<string?> RunChecks(IContractHost runHost)
    {
        _logger.LogTrace("Starting check run");

        string? lastEventName = null;

        foreach (var item in _checks)
        {
            bool state = await item.Value(runHost);
            if (!state) break;

            lastEventName = item.Key;
        }

        _logger.LogTrace("Completed check run, eventName={eventName}", lastEventName);
        return lastEventName;
    }

    public async Task RunState(IContractHost runHost, string eventName)
    {
        runHost.VerifyNotNull(nameof(runHost));
        eventName.VerifyNotEmpty(nameof(eventName));

        _logger.LogTrace("Starting run, eventName={eventName}", eventName);

        if (!_runs.TryGetValue(eventName, out RunFunction? runFunction))
        {
            _logger.LogError("Event name does not exist to run, {eventName}", eventName);
            throw new ArgumentException($"EventName={eventName} does not exist");
        }

        await runFunction(runHost, eventName);

        _logger.LogTrace("Completed run, eventName={eventName}", eventName);
    }
}
