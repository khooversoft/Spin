using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SpinClusterCmd.Application;
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

        if (!File.Exists(jsonFile))
        {
            context.Trace().LogError("File {file} does not exist", jsonFile);
            return;
        }

        string json = File.ReadAllText(jsonFile);
        ScenarioOption? option = json.ToObject<ScenarioOption>();
        if (option == null)
        {
            context.Trace().LogError("Cannot parse {file}", jsonFile);
            return;
        }
        var v = option.Validate();
        if (v.IsError())
        {
            context.Trace().LogError("Option is not valid, error={error}", v.Error);
            return;
        }

        context.Trace().LogInformation("Starting load");

        SetupBuilder builder = new SetupBuilder(option);
        await builder.Build(_service, context);

        context.Trace().LogInformation("Completed load");
    }
}
