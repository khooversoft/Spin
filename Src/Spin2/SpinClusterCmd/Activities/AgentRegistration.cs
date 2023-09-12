using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Actors.Agent;
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

    public async Task Register(string agentId)
    {
        var context = new ScopeContext(_logger);

        var model = new AgentModel
        {
            AgentId = agentId,
            Enabled = true,
        };

        Option response = await _client.Set(model, context);
        context.Trace().LogStatus(response, "Creating/Updating agent, agentId={agentId}", agentId);
    }

    public async Task Remove(string agentId)
    {
        var context = new ScopeContext(_logger);

        Option response = await _client.Delete(agentId, context);
        context.Trace().LogStatus(response, "Deleted agent, agentId={agentId}", agentId);
    }
}
