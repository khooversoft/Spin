using System.CommandLine;
using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Actors.Smartc;
using SpinClusterCmd.Activities;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinClusterCmd.Commands;

internal class SmartcCommand : Command
{
    private readonly SmartcRegistration _smartcRegistration;

    public SmartcCommand(SmartcRegistration smartcRegistration) : base("smartc", "SmartC package registration")
    {
        _smartcRegistration = smartcRegistration.NotNull();

        AddCommand(Register());
        AddCommand(Remove());
    }

    private Command Register()
    {
        var cmd = new Command("register", "Register SmartC");
        Argument<string> smartcIdArg = new Argument<string>("smartcId", "SmartC's ID to register, ex: smartc:{domain}/{package}");

        cmd.AddArgument(smartcIdArg);
        cmd.SetHandler(_smartcRegistration.Register, smartcIdArg);

        return cmd;
    }

    private Command Remove()
    {
        var cmd = new Command("remove", "Remove registered agent");
        Argument<string> idArgument = new Argument<string>("smartcId", "SmartC's ID to remove, ex: smartc:{domain}/{package}");

        cmd.AddArgument(idArgument);
        cmd.SetHandler(_smartcRegistration.Remove, idArgument);

        return cmd;
    }
}
