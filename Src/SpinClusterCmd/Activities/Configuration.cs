using Microsoft.Extensions.Logging;
using SpinClient.sdk;
using SpinCluster.abstraction;
using SpinClusterCmd.Application;
using Toolbox.CommandRouter;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinClusterCmd.Activities;

internal class Configuration : ICommandRoute
{
    private readonly ConfigClient _client;
    private readonly ILogger<Configuration> _logger;

    public Configuration(ConfigClient client, ILogger<Configuration> logger)
    {
        _client = client.NotNull();
        _logger = logger.NotNull();
    }

    public CommandSymbol CommandSymbol() => new CommandSymbol("config", "Configuration")
    {
        new CommandSymbol("create", "Create SmartC package").Action(x =>
        {
            var jsonFile = x.AddArgument<string>("jsonFile", "Json file with package details");
            x.SetHandler(Create, jsonFile);
        }),
        new CommandSymbol("set", "Set property in configuration").Action(x =>
        {
            var configId = x.AddArgument<string>("configId", "Configuration ID, ex: config:{name}");
            var key = x.AddArgument<string>("key", "Key of property");
            var value = x.AddArgument<string>("value", "Value of property");

            x.SetHandler(SetProperty, configId, key, value);
        }),
        new CommandSymbol("remove", "Remove property in configuration").Action(x =>
        {
            var configId = x.AddArgument<string>("configId", "Configuration ID, ex: config:{name}");
            var key = x.AddArgument<string>("key", "Key of property");

            x.SetHandler(RemoveProperty, configId, key);
        }),
        new CommandSymbol("Get", "Get properties in configuration").Action(x =>
        {
            var configId = x.AddArgument<string>("configId", "Configuration ID, ex: config:{name}");
            x.SetHandler(Get, configId);
        }),
    };

    public async Task Create(string jsonFile)
    {
        var context = new ScopeContext(_logger);

        var readResult = CmdTools.LoadJson<ConfigModel>(jsonFile, ConfigModel.Validator, context);
        if (readResult.IsError()) return;

        ConfigModel model = readResult.Return();

        context.LogInformation("Creating/Updating Tenant from jsonFile={jsonFile}", jsonFile);
        Option response = await _client.Set(model, context);
        response.LogStatus(context, "Creating/Updating Tenant");
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

        context.LogInformation("Set property, configId={configId}, key={key}, value={value}", configId, key, value);
        var response = await _client.SetProperty(request, context);
        response.LogStatus(context, "Set property");
    }

    public async Task RemoveProperty(string configId, string key)
    {
        var context = new ScopeContext(_logger);

        var request = new RemovePropertyModel
        {
            ConfigId = configId,
            Key = key,
        };

        context.LogInformation("Remove property, configId={configId}, key={key}", configId, key);
        var response = await _client.RemoveProperty(request, context);
        response.LogStatus(context, "Remove property");
    }

    public async Task Get(string configId)
    {
        var context = new ScopeContext(_logger);

        var response = await _client.Get(configId, context);
        if (response.IsError())
        {
            context.LogError("Failed to get configId={confgiId}", configId);
            return;
        }

        string result = response.Return()
            .ToDictionary()
            .Select(x => $" - {x.Key}={x.Value}")
            .Prepend($"Configuration...")
            .Join(Environment.NewLine) + Environment.NewLine;

        context.LogInformation(result);
    }
}
