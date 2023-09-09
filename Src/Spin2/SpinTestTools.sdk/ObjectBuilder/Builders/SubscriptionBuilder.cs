using Microsoft.Extensions.DependencyInjection;
using SpinCluster.sdk.Actors.Subscription;
using Toolbox.Types;

namespace SpinTestTools.sdk.ObjectBuilder.Builders;

public class SubscriptionBuilder : IObjectBuilder
{
    public async Task<Option> Create(IServiceProvider service, ObjectBuilderOption option, ScopeContext context)
    {
        var client = service.GetRequiredService<SubscriptionClient>();

        var test = new OptionTest();

        foreach (var item in option.Subscriptions)
        {
            SubscriptionModel subscription = item with
            {
                AccountEnabled = true,
                ActiveDate = DateTime.UtcNow,
            };

            Option setOption = await client.Set(subscription, context);

            context.Trace().LogStatus(setOption, "Creating Subscription name={name}", item.Name);
            test.Test(() => setOption);
        }

        return test;
    }

    public async Task<Option> Delete(IServiceProvider service, ObjectBuilderOption option, ScopeContext context)
    {
        SubscriptionClient client = service.GetRequiredService<SubscriptionClient>();

        foreach (var item in option.Subscriptions)
        {
            await client.Delete(item.Name, context);
            context.Trace().LogInformation("Subscription deleted: {name}", item.Name);
        }

        return StatusCode.OK;
    }
}
