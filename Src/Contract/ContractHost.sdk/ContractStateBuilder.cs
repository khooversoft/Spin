using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Tools;
using ContractHost.sdk;
using Microsoft.Extensions.Logging;

namespace ContractHost.sdk;

public class ContractStateBuilder
{
    private readonly Dictionary<string, ContractState.CheckFunction> _checks = new Dictionary<string, ContractState.CheckFunction>();
    private readonly Dictionary<string, ContractState.RunFunction> _runs = new Dictionary<string, ContractState.RunFunction>();

    public ContractStateBuilder()
    {
    }

    public ContractStateBuilder AddCheck(string eventName, ContractState.CheckFunction func)
    {
        eventName.VerifyNotEmpty(nameof(eventName));
        func.VerifyNotNull(nameof(func));

        _checks.TryAdd(eventName, func).VerifyAssert(x => x == true, $"{eventName} already exist");
        return this;
    }

    public ContractStateBuilder AddRun(string eventName, ContractState.RunFunction func)
    {
        eventName.VerifyNotEmpty(nameof(eventName));
        func.VerifyNotNull(nameof(func));

        _runs.TryAdd(eventName, func).VerifyAssert(x => x == true, $"{eventName} already exist");
        return this;
    }

    public ContractState Build(ILogger logger) => new ContractState(_checks, _runs, logger);
}
