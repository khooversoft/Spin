using Microsoft.Extensions.DependencyInjection;
using SpinCluster.sdk.Actors.Agent;
using Toolbox.Types;

namespace SpinTestTools.sdk.ObjectBuilder.Builders;

public class AgentBuilder : IObjectBuilder
{
    public async Task<Option> Create(IServiceProvider service, ObjectBuilderOption option, ScopeContext context)
    {
        AgentClient client = service.GetRequiredService<AgentClient>();

        var test = new OptionTest();

        foreach (var item in option.Agents)
        {
            var model = item with
            {
                Enabled = true,
            };

            Option setOption = await client.Set(model, context);

            context.Trace().LogStatus(setOption, "Creating Agent agentId={agentId}", item.AgentId);
            test.Test(() => setOption);
        }

        return test;
    }

    public async Task<Option> Delete(IServiceProvider service, ObjectBuilderOption option, ScopeContext context)
    {
        AgentClient client = service.GetRequiredService<AgentClient>();

        foreach (var item in option.Agents)
        {
            await client.Delete(item.AgentId, context);
            context.Trace().LogInformation("Agent deleted: {agentId}", item.AgentId);
        }

        return StatusCode.OK;
    }
}
