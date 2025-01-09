using Microsoft.Extensions.DependencyInjection;
using SpinClient.sdk;
using Toolbox.Logging;
using Toolbox.Types;

namespace SpinTestTools.sdk.ObjectBuilder.Builders;

public class ConfigBuilder : IObjectBuilder
{
    public async Task<Option> Create(IServiceProvider service, ObjectBuilderOption option, ScopeContext context)
    {
        var client = new Lazy<ConfigClient>(() => service.GetRequiredService<ConfigClient>());

        var test = new OptionTest();

        foreach (var model in option.Configs)
        {
            Option setOption = await client.Value.Set(model, context);

            setOption.LogStatus(context, "Creating config configId={configId}", [model.ConfigId]);
            test.Test(() => setOption);
        }

        return test;
    }

    public async Task<Option> Delete(IServiceProvider service, ObjectBuilderOption option, ScopeContext context)
    {
        var client = new Lazy<ConfigClient>(() => service.GetRequiredService<ConfigClient>());

        foreach (var item in option.Configs)
        {
            await client.Value.Delete(item.ConfigId, context);
            context.LogInformation("Config deleted: {configId}", item.ConfigId);
        }

        return StatusCode.OK;
    }
}
