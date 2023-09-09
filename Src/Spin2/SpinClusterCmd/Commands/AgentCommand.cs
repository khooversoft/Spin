using System.CommandLine;
using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Actors.Agent;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinClusterCmd.Commands;

internal class AgentCommand : Command
{
    private readonly AgentClient _client;
    private readonly ILogger<AgentCommand> _logger;

    public AgentCommand(AgentClient client, ILogger<AgentCommand> logger) : base("agent", "Agent commands")
    {
        _client = client.NotNull();
        _logger = logger.NotNull();

        AddCommand(Register());
        AddCommand(Remove());
    }

    private Command Register()
    {
        var cmd = new Command("register", "Register agent");
        Argument<string> idArgument = new Argument<string>("agentId", "Agent's ID to register, ex: agent:{agentName}");

        cmd.AddArgument(idArgument);

        cmd.SetHandler(async (agentId) =>
        {
            var context = new ScopeContext(_logger);

            var model = new AgentModel
            {
                AgentId = agentId,
                Enabled = true,
            };

            Toolbox.Types.Option response = await _client.Set(model, context);
            context.Trace().LogStatus(response, "Creating/Updating agent, agentId={agentId}", agentId);

        }, idArgument);

        return cmd;
    }

    private Command Remove()
    {
        var cmd = new Command("remove", "Remove registered agent");
        Argument<string> agentId = new Argument<string>("agentId", "Agent's ID to remove, ex: agent:{agentName}");

        cmd.AddArgument(agentId);

        cmd.SetHandler(async (agentId) =>
        {
            var context = new ScopeContext(_logger);

            Toolbox.Types.Option response = await _client.Delete(agentId, context);
            context.Trace().LogStatus(response, "Deleted agent, agentId={agentId}", agentId);

        }, agentId);

        return cmd;
    }
}
