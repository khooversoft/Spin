using Microsoft.Extensions.Logging;
using SpinClient.sdk;
using SpinCluster.abstraction;
using SpinClusterCmd.Application;
using Toolbox.CommandRouter;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinClusterCmd.Activities;

internal class Tenant : ICommandRoute
{
    private readonly TenantClient _client;
    private readonly ILogger<Tenant> _logger;

    public Tenant(TenantClient client, ILogger<Tenant> logger)
    {
        _client = client.NotNull();
        _logger = logger.NotNull();
    }

    public CommandSymbol CommandSymbol() => new CommandSymbol("tenant", "Tenant management")
    {
        new CommandSymbol("delete", "Delete a Tenant").Action(command =>
        {
            var tenantId = command.AddArgument<string>("tenantId", "Id of Tenant");
            command.SetHandler(Delete, tenantId);
        }),
        new CommandSymbol("get", "Get Tenant details").Action(command =>
        {
            var tenantId = command.AddArgument<string>("tenantId", "Id of Tenant");
            command.SetHandler(Get, tenantId);
        }),
        new CommandSymbol("set", "Create or update Tenant details").Action(command =>
        {
            var jsonFile = command.AddArgument<string>("jsonFile", "Json with Tenant details");
            command.SetHandler(Set, jsonFile);
        }),
    };

    public async Task Delete(string tenantId)
    {
        var context = new ScopeContext(_logger);
        context.Trace().LogInformation("Deleting tenant tenantId={tenantId}", tenantId);

        Option response = await _client.Delete(tenantId, context);
        context.Trace().LogStatus(response, "Deleting Tenant");
    }

    public async Task Get(string tenantId)
    {
        var context = new ScopeContext(_logger);
        context.Trace().LogInformation("Get Tenant tenantId={tenantId}", tenantId);

        var response = await _client.Get(tenantId, context);
        if (response.IsError())
        {
            context.Trace().LogError("Cannot get Tenant tenantId={tenantId}", tenantId);
            return;
        }

        var result = response.Return()
            .ToDictionary()
            .Select(x => $" - {x.Key}={x.Value}")
            .Prepend($"Tenant...")
            .Join(Environment.NewLine) + Environment.NewLine;

        context.Trace().LogInformation(result);
    }

    public async Task Set(string jsonFile)
    {
        var context = new ScopeContext(_logger);

        var readResult = CmdTools.LoadJson<TenantModel>(jsonFile, TenantModel.Validator, context);
        if (readResult.IsError()) return;

        TenantModel model = readResult.Return();

        Option response = await _client.Set(model, context);
        context.Trace().LogStatus(response, "Creating/Updating Tenant, model={model}", model);
    }
}
