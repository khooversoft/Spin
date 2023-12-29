using Microsoft.Extensions.Logging;
using SpinClusterCmd.Application;
using SpinTestTools.sdk.ObjectBuilder;
using Toolbox.CommandRouter;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinClusterCmd.Activities;

internal class LoadScenario : ICommandRoute
{
    private readonly IServiceProvider _service;
    private readonly ILogger<LoadScenario> _logger;

    public LoadScenario(IServiceProvider service, ILogger<LoadScenario> logger)
    {
        _service = service.NotNull();
        _logger = logger.NotNull();
    }

    public CommandSymbol CommandSymbol() => new CommandSymbol("load", "Load scenario").Action(x =>
    {
        var jsonFile = x.AddArgument<string>("file", "Json file with scenario details");

        x.SetHandler(Load, jsonFile);
    });

    public async Task Load(string jsonFile)
    {
        var context = new ScopeContext(_logger);
        context.LogInformation("Processing file {file}", jsonFile);

        var readResult = CmdTools.LoadJson<ObjectBuilderOption>(jsonFile, ObjectBuilderOption.Validator, context);
        if (readResult.IsError()) return;

        context.LogInformation("Starting load");

        Option result = await new TestObjectBuilder()
            .SetOption(readResult.Return())
            .SetService(_service)
            .AddStandard()
            .Build(context);

        if (result.IsError())
        {
            context.LogError("Failed to load data, error={error}", result.Error);
            return;
        }

        context.LogInformation("Completed load");
    }
}
