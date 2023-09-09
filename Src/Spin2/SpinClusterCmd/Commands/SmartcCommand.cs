using System.CommandLine;
using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Actors.Smartc;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinClusterCmd.Commands;

internal class SmartcCommand : Command
{
    private readonly SmartcClient _client;
    private readonly ILogger<SmartcCommand> _logger;

    public SmartcCommand(SmartcClient client, ILogger<SmartcCommand> logger) : base("smartc", "SmartC package registration")
    {
        _client = client.NotNull();
        _logger = logger.NotNull();

        AddCommand(Register());
        AddCommand(Remove());
    }

    private Command Register()
    {
        var cmd = new Command("register", "Register SmartC");
        Argument<string> idArgument = new Argument<string>("smartcId", "SmartC's ID to register, ex: smartc:{domain}/{package}");

        cmd.AddArgument(idArgument);

        cmd.SetHandler(async (smartcId) =>
        {
            var context = new ScopeContext(_logger);

            var model = new SmartcModel
            {
                SmartcId = smartcId,
                Enabled = true,
            };

            Toolbox.Types.Option response = await _client.Set(model, context);
            context.Trace().LogStatus(response, "Creating/Updating SmartC, smartcId={smartcId}", smartcId);

        }, idArgument);

        return cmd;
    }

    private Command Remove()
    {
        var cmd = new Command("remove", "Remove registered agent");
        Argument<string> idArgument = new Argument<string>("smartcId", "SmartC's ID to remove, ex: smartc:{domain}/{package}");

        cmd.AddArgument(idArgument);

        cmd.SetHandler(async (smartcId) =>
        {
            var context = new ScopeContext(_logger);

            Toolbox.Types.Option response = await _client.Delete(smartcId, context);
            context.Trace().LogStatus(response, "Deleted SmartC, smartcId={smartcId}", smartcId);

        }, idArgument);

        return cmd;
    }
}
