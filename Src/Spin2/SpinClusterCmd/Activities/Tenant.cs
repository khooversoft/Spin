using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Actors.Tenant;
using SpinClusterCmd.Application;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinClusterCmd.Activities;

internal class Tenant
{
    private readonly TenantClient _client;
    private readonly ILogger<Tenant> _logger;

    public Tenant(TenantClient client, ILogger<Tenant> logger)
{
        _client = client.NotNull();
        _logger = logger.NotNull();
    }


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
            .GetConfigurationValues()
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
