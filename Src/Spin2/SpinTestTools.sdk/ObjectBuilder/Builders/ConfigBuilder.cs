using Microsoft.Extensions.DependencyInjection;
using SpinCluster.sdk.Actors.Agent;
using SpinCluster.sdk.Actors.Configuration;
using Toolbox.Types;

namespace SpinTestTools.sdk.ObjectBuilder.Builders;

public class ConfigBuilder : IObjectBuilder
{
    public async Task<Option> Create(IServiceProvider service, ObjectBuilderOption option, ScopeContext context)
    {
        ConfigClient client = service.GetRequiredService<ConfigClient>();

        var test = new OptionTest();

        foreach (var model in option.Configs)
        {
            Option setOption = await client.Set(model, context);

            context.Trace().LogStatus(setOption, "Creating config configId={configId}", model.ConfigId);
            test.Test(() => setOption);
        }

        return test;
    }

    public async Task<Option> Delete(IServiceProvider service, ObjectBuilderOption option, ScopeContext context)
    {
        ConfigClient client = service.GetRequiredService<ConfigClient>();

        foreach (var item in option.Configs)
        {
            await client.Delete(item.ConfigId, context);
            context.Trace().LogInformation("Config deleted: {configId}", item.ConfigId);
        }

        return StatusCode.OK;
    }
}
