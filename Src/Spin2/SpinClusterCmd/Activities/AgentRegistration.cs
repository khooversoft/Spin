using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Actors.Agent;
using SpinClusterCmd.Application;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinClusterCmd.Activities;

internal class AgentRegistration
{
    private readonly AgentClient _client;
    private readonly ILogger<AgentRegistration> _logger;

    public AgentRegistration(AgentClient client, ILogger<AgentRegistration> logger)
    {
        _client = client.NotNull();
        _logger = logger.NotNull();
    }

    public async Task Register(string jsonFile)
    {
        var context = new ScopeContext(_logger);

        var readResult = CmdTools.LoadJson<AgentModel>(jsonFile, AgentModel.Validator, context);
        if (readResult.IsError()) return;

        AgentModel model = readResult.Return();

        Option response = await _client.Set(model, context);
        context.Trace().LogStatus(response, "Creating/Updating agent, model={model}", model);
    }

    public async Task Remove(string agentId)
    {
        var context = new ScopeContext(_logger);

        Option response = await _client.Delete(agentId, context);
        context.Trace().LogStatus(response, "Deleted agent, agentId={agentId}", agentId);
    }

    public async Task Get(string agentId)
    {
        var context = new ScopeContext(_logger);

        var readOption = await _client.Get(agentId, context);
        if (readOption.IsError())
        {
            context.Trace().LogError("Cannot get details on agentId={agentId}", agentId);
            return;
        }

        string result = readOption.Return()
            .GetConfigurationValues()
            .Select(x => $" - {x.Key}={x.Value}")
            .Prepend($"Configuration...")
            .Join(Environment.NewLine) + Environment.NewLine;

        context.Trace().LogInformation(result);
    }
}
