using Microsoft.Extensions.Logging;
using SpinClusterCmd.Application;
using SpinTestTools.sdk.ObjectBuilder;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinClusterCmd.Activities;

internal class LoadScenario
{
    private readonly IServiceProvider _service;
    private readonly ILogger<LoadScenario> _logger;

    public LoadScenario(IServiceProvider service, ILogger<LoadScenario> logger)
    {
        _service = service.NotNull();
        _logger = logger.NotNull();
    }

    public async Task Load(string jsonFile)
    {
        var context = new ScopeContext(_logger);
        context.Trace().LogInformation("Processing file {file}", jsonFile);

        var readResult = CmdTools.LoadJson<ObjectBuilderOption>(jsonFile, ObjectBuilderOption.Validator, context);
        if (readResult.IsError()) return;

        context.Trace().LogInformation("Starting load");

        Option result = await new TestObjectBuilder()
            .SetOption(readResult.Return())
            .SetService(_service)
            .AddStandard()
            .Build(context);

        if (result.IsError())
        {
            context.Trace().LogError("Failed to load data, error={error}", result.Error);
            return;
        }

        context.Trace().LogInformation("Completed load");
    }
}
