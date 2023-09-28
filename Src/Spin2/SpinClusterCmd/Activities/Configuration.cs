using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Actors.Configuration;
using SpinClusterCmd.Application;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinClusterCmd.Activities;

internal class Configuration
{
    private readonly ConfigClient _client;
    private readonly ILogger<Configuration> _logger;

    public Configuration(ConfigClient client, ILogger<Configuration> logger)
    {
        _client = client.NotNull();
        _logger = logger.NotNull();
    }

    public async Task Create(string jsonFile)
    {
        var context = new ScopeContext(_logger);

        var readResult = CmdTools.LoadJson<ConfigModel>(jsonFile, ConfigModel.Validator, context);
        if (readResult.IsError()) return;

        ConfigModel model = readResult.Return();

        Option response = await _client.Set(model, context);
        context.Trace().LogStatus(response, "Creating/Updating configuration, model={model}", model);
    }

    public async Task SetProperty(string configId, string key, string value)
    {
        var context = new ScopeContext(_logger);

        var request = new SetPropertyModel
        {
            ConfigId = configId,
            Key = key,
            Value = value,
        };

        var response = await _client.SetProperty(request, context);
        context.Trace().LogStatus(response, "Set property");
    }

    public async Task RemoveProperty(string configId, string key)
    {
        var context = new ScopeContext(_logger);

        var request = new RemovePropertyModel
        {
            ConfigId = configId,
            Key = key,
        };

        var response = await _client.RemoveProperty(request, context);
        context.Trace().LogStatus(response, "Remove property");
    }

    public async Task Get(string configId)
    {
        var context = new ScopeContext(_logger);

        var response = await _client.Get(configId, context);
        if (response.IsError())
        {
            context.Trace().LogError("Failed to get configId={confgiId}", configId);
            return;
        }

        string result = response.Return()
            .GetConfigurationValues()
            .Select(x => $" - {x.Key}={x.Value}")
            .Prepend($"Configuration...")
            .Join(Environment.NewLine) + Environment.NewLine;

        context.Trace().LogInformation(result);
    }
}
