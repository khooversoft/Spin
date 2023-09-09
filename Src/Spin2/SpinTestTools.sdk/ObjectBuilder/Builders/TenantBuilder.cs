using Microsoft.Extensions.DependencyInjection;
using SpinCluster.sdk.Actors.Tenant;
using Toolbox.Types;

namespace SpinTestTools.sdk.ObjectBuilder.Builders;

public class TenantBuilder : IObjectBuilder
{
    public async Task<Option> Create(IServiceProvider service, ObjectBuilderOption option, ScopeContext context)
    {
        TenantClient client = service.GetRequiredService<TenantClient>();

        var test = new OptionTest();

        foreach (var item in option.Tenants)
        {
            var tenant = item with
            {
                AccountEnabled = true,
                ActiveDate = DateTime.UtcNow,
            };

            Option setOption = await client.Set(tenant, context);

            context.Trace().LogStatus(setOption, "Creating Tenant domain={domain}", item.Domain);
            test.Test(() => setOption);
        }

        return test;
    }

    public async Task<Option> Delete(IServiceProvider service, ObjectBuilderOption option, ScopeContext context)
    {
        TenantClient client = service.GetRequiredService<TenantClient>();

        foreach (var item in option.Tenants)
        {
            await client.Delete(item.Domain, context);
            context.Trace().LogInformation("Tenant deleted: {domains}", item.Domain);
        }

        return StatusCode.OK;
    }
}
