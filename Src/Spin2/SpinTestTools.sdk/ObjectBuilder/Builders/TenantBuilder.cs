using Microsoft.Extensions.DependencyInjection;
using SpinCluster.sdk.Actors.Tenant;
using Toolbox.Types;

namespace SpinTestTools.sdk.ObjectBuilder.Builders;

public class TenantBuilder : IObjectBuilder
{
    public async Task<Option> Create(IServiceProvider service, ObjectBuilderOption option, ScopeContext context)
    {
        var client = new Lazy<TenantClient>(() => service.GetRequiredService<TenantClient>());

        var test = new OptionTest();

        foreach (var tenant in option.Tenants)
        {
            Option setOption = await client.Value.Set((TenantModel)tenant, context);

            context.Trace().LogStatus(setOption, "Creating Tenant domain={domain}", tenant.Domain);
            test.Test(() => setOption);
        }

        return test;
    }

    public async Task<Option> Delete(IServiceProvider service, ObjectBuilderOption option, ScopeContext context)
    {
        var client = new Lazy<TenantClient>(() => service.GetRequiredService<TenantClient>());

        foreach (var item in option.Tenants)
        {
            await client.Value.Delete(item.Domain, context);
            context.Trace().LogInformation("Tenant deleted: {domains}", item.Domain);
        }

        return StatusCode.OK;
    }
}
