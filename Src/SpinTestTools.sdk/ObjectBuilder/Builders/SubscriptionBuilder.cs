using Microsoft.Extensions.DependencyInjection;
using SpinClient.sdk;
using Toolbox.Types;

namespace SpinTestTools.sdk.ObjectBuilder.Builders;

public class SubscriptionBuilder : IObjectBuilder
{
    public async Task<Option> Create(IServiceProvider service, ObjectBuilderOption option, ScopeContext context)
    {
        var client = new Lazy<SubscriptionClient>(() => service.GetRequiredService<SubscriptionClient>());

        var test = new OptionTest();

        foreach (var subscription in option.Subscriptions)
        {
            Option setOption = await client.Value.Set(subscription, context);

            setOption.LogStatus(context, "Creating Subscription name={name}", subscription.Name);
            test.Test(() => setOption);
        }

        return test;
    }

    public async Task<Option> Delete(IServiceProvider service, ObjectBuilderOption option, ScopeContext context)
    {
        var client = new Lazy<SubscriptionClient>(() => service.GetRequiredService<SubscriptionClient>());

        foreach (var item in option.Subscriptions)
        {
            await client.Value.Delete(item.Name, context);
            context.LogInformation("Subscription deleted: {name}", item.Name);
        }

        return StatusCode.OK;
    }
}
