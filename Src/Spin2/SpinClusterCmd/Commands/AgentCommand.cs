using System.CommandLine;
using SpinClusterCmd.Activities;
using Toolbox.Tools;

namespace SpinClusterCmd.Commands;

internal class AgentCommand : Command
{
    private readonly AgentRegistration _agentRegistration;

    public AgentCommand(AgentRegistration agentRegistration) : base("agent", "Agent commands")
    {
        _agentRegistration = agentRegistration.NotNull();

        AddCommand(Register());
        AddCommand(Remove());
        AddCommand(Get());
    }

    private Command Register()
    {
        var cmd = new Command("register", "Register agent");
        Argument<string> agentIdArg = new Argument<string>("jsonFile", "Json file with agent details");

        cmd.AddArgument(agentIdArg);
        cmd.SetHandler(_agentRegistration.Register, agentIdArg);

        return cmd;
    }

    private Command Remove()
    {
        var cmd = new Command("remove", "Remove registered agent");
        Argument<string> agentId = new Argument<string>("agentId", "Agent's ID to remove, ex: agent:{agentName}");

        cmd.AddArgument(agentId);
        cmd.SetHandler(_agentRegistration.Remove, agentId);

        return cmd;
    }

    private Command Get()
    {
        var cmd = new Command("get", "Get agent's registration");
        Argument<string> agentId = new Argument<string>("agentId", "Agent's ID to get, ex: agent:{agentName}");

        cmd.AddArgument(agentId);
        cmd.SetHandler(_agentRegistration.Get, agentId);

        return cmd;
    }
}
