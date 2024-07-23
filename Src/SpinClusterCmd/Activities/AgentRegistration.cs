using Microsoft.Extensions.Logging;
using SpinClient.sdk;
using SpinCluster.abstraction;
using SpinClusterCmd.Application;
using Toolbox.CommandRouter;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinClusterCmd.Activities;

internal class AgentRegistration : ICommandRoute
{
    private readonly AgentClient _client;
    private readonly ILogger<AgentRegistration> _logger;

    public AgentRegistration(AgentClient client, ILogger<AgentRegistration> logger)
    {
        _client = client.NotNull();
        _logger = logger.NotNull();
    }

    public CommandSymbol CommandSymbol() => new CommandSymbol("agent", "Agent commands")
    {
        new CommandSymbol("register", "Register agent").Action(x =>
        {
            var jsonFile = x.AddArgument<string>("jsonFile", "Json file with agent details");
            x.SetHandler(Register, jsonFile);
        }),
        new CommandSymbol("get", "Get agent details").Action(x =>
        {
            var agentId = x.AddArgument<string>("agentId", "Agent's ID to remove, ex: agent:{agentName}");
            x.SetHandler(Get, agentId);
        }),
        new CommandSymbol("remove", "Remove a agent from the registery").Action(x =>
        {
            var agentId = x.AddArgument<string>("agentId", "Agent's ID to remove, ex: agent:{agentName}");
            x.SetHandler(Register, agentId);
        }),
    };

    public async Task Get(string agentId)
    {
        var context = new ScopeContext(_logger);

        var readOption = await _client.Get(agentId, context);
        if (readOption.IsError())
        {
            context.LogError("Cannot get details on agentId={agentId}", agentId);
            return;
        }

        string result = readOption.Return()
            .ToDictionary()
            .Select(x => $" - {x.Key}={x.Value}")
            .Prepend($"Configuration...")
            .Join(Environment.NewLine) + Environment.NewLine;

        context.LogInformation(result);
    }

    public async Task Register(string jsonFile)
    {
        var context = new ScopeContext(_logger);

        var readResult = CmdTools.LoadJson<AgentModel>(jsonFile, AgentModel.Validator, context);
        if (readResult.IsError()) return;

        AgentModel model = readResult.Return();
        context.LogInformation("Creating/Updating agent, model={model}", model);
        (await _client.Set(model, context)).LogStatus(context, "Creating/Updating agent");
    }

    public async Task Remove(string agentId)
    {
        var context = new ScopeContext(_logger);

        context.LogInformation("Deleting agentId={agentId}", agentId);
        (await _client.Delete(agentId, context)).LogStatus(context, "delete agent");
    }
}
