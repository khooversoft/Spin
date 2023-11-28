using Microsoft.Extensions.DependencyInjection;
using SpinClient.sdk;
using Toolbox.Types;

namespace SpinTestTools.sdk.ObjectBuilder.Builders;

public class AgentBuilder : IObjectBuilder
{
    public async Task<Option> Create(IServiceProvider service, ObjectBuilderOption option, ScopeContext context)
    {
        var client = new Lazy<AgentClient>(() => service.GetRequiredService<AgentClient>());

        var test = new OptionTest();

        foreach (var model in option.Agents)
        {
            Option setOption = await client.Value.Set(model, context);

            context.Trace().LogStatus(setOption, "Creating Agent agentId={agentId}", model.AgentId);
            test.Test(() => setOption);
        }

        return test;
    }

    public async Task<Option> Delete(IServiceProvider service, ObjectBuilderOption option, ScopeContext context)
    {
        var client = new Lazy<AgentClient>(() => service.GetRequiredService<AgentClient>());

        foreach (var item in option.Agents)
        {
            await client.Value.Delete(item.AgentId, context);
            context.Trace().LogInformation("Agent deleted: {agentId}", item.AgentId);
        }

        return StatusCode.OK;
    }
}
