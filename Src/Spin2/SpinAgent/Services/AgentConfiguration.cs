using Microsoft.Extensions.Logging;
using SpinAgent.Application;
using SpinCluster.sdk.Actors.Agent;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinAgent.Services;

internal class AgentConfiguration
{
    private readonly AgentClient _agentClient;
    private readonly ILogger<AgentConfiguration> _logger;
    private readonly AgentOption _option;
    private AgentModel _agentModel = null!;

    public AgentConfiguration(AgentClient agentClient, AgentOption option, ILogger<AgentConfiguration> logger)
    {
        _agentClient = agentClient.NotNull();
        _option = option;
        _logger = logger.NotNull();
    }

    public async Task Startup()
    {
        const string msg = "Cannot get agent's configuration";
        var context = new ScopeContext(_logger);

        bool result = await InternalGet(context);
        if (!result)
        {
            context.Trace().LogError(msg);
            throw new InvalidOperationException(msg);
        }

        Directory.CreateDirectory(_agentModel.WorkingFolder);
        context.Trace().LogInformation("Agent configuration, model={model}", _agentModel);
    }

    public AgentModel Get(ScopeContext context) => _agentModel.NotNull();

    private async Task<bool> InternalGet(ScopeContext context)
    {
        context = context.With(_logger);

        var agentModelOption = await _agentClient
            .Get(_option.AgentId, context)
            .LogResult(context.Trace());

        if (agentModelOption.IsError() || agentModelOption.Return().Validate().IsError())
        {
            context.Trace().LogError("Cannot get agent details, agentId={agentId}", _option.AgentId);
            return false;
        }

        _agentModel = agentModelOption.Return();
        return true;
    }
}
