using Microsoft.Extensions.DependencyInjection;
using SpinCluster.sdk.Actors.Smartc;
using Toolbox.Types;

namespace SpinTestTools.sdk.ObjectBuilder.Builders;

public class SmartcBuilder : IObjectBuilder
{
    public async Task<Option> Create(IServiceProvider service, ObjectBuilderOption option, ScopeContext context)
    {
        SmartcClient client = service.GetRequiredService<SmartcClient>();

        var test = new OptionTest();

        foreach (var item in option.SmartcItems)
        {
            var model = item with
            {
                Enabled = true
            };

            Option setOption = await client.Set(model, context);

            context.Trace().LogStatus(setOption, "Creating Tenant smartcId={smartcId}", item.SmartcId);
            test.Test(() => setOption);
        }

        return test;
    }

    public async Task<Option> Delete(IServiceProvider service, ObjectBuilderOption option, ScopeContext context)
    {
        SmartcClient client = service.GetRequiredService<SmartcClient>();

        foreach (var item in option.SmartcItems)
        {
            await client.Delete(item.SmartcId, context);
            context.Trace().LogInformation("SmartC deleted: {smartcId}", item.SmartcId);
        }

        return StatusCode.OK;
    }
}
