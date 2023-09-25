using Microsoft.Extensions.DependencyInjection;
using SpinCluster.sdk.Actors.Smartc;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinTestTools.sdk.ObjectBuilder.Builders;

public class SmartcBuilder : IObjectBuilder
{
    public async Task<Option> Create(IServiceProvider service, ObjectBuilderOption option, ScopeContext context)
    {
        var client = new Lazy<SmartcClient>(() => service.GetRequiredService<SmartcClient>());

        var test = new OptionTest();

        foreach (var model in option.SmartcItems)
        {
            Option setOption = await client.Value.Set(model, context);
            setOption.Assert(x => x.IsOk(), x => $"Set failed for {x}");

            context.Trace().LogStatus(setOption, "Creating Tenant smartcId={smartcId}", model.SmartcId);
            test.Test(() => setOption);
        }

        return test;
    }

    public async Task<Option> Delete(IServiceProvider service, ObjectBuilderOption option, ScopeContext context)
    {
        var client = new Lazy<SmartcClient>(() => service.GetRequiredService<SmartcClient>());

        foreach (var item in option.SmartcItems)
        {
            await client.Value.Delete(item.SmartcId, context);
            context.Trace().LogInformation("SmartC deleted: {smartcId}", item.SmartcId);
        }

        return StatusCode.OK;
    }
}
