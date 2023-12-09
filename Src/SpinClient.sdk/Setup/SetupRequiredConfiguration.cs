using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SpinCluster.abstraction;
using Toolbox.Extensions;
using Toolbox.Types;

namespace SpinClient.sdk;

public static class SetupRequiredConfiguration
{
    public static SetupBuilder AddSpinSetupProviders(this SetupBuilder builder)
    {
        builder.AddProvider(async (service, context) =>
        {
            var client = service.GetRequiredService<ConfigClient>();
            var items = GetList<ConfigModel>(service, x => x.Configs);
            var option = await ExecItems(items, async x => await client.Exist(x.ConfigId, context), async x => await client.Set(x, context));
            return option;
        });

        builder.AddProvider(async (service, context) =>
        {
            var client = service.GetRequiredService<SubscriptionClient>();
            var items = GetList<SubscriptionModel>(service, x => x.Subscriptions);
            var option = await ExecItems(items, async x => await client.Exist(x.SubscriptionId, context), async x => await client.Set(x, context));
            return option;
        });

        builder.AddProvider(async (service, context) =>
        {
            var client = service.GetRequiredService<TenantClient>();
            var items = GetList<TenantModel>(service, x => x.Tenants);
            var option = await ExecItems(items, async x => await client.Exist(x.TenantId, context), async x => await client.Set(x, context));
            return option;
        });

        builder.AddProvider(async (service, context) =>
        {
            var client = service.GetRequiredService<UserClient>();
            var items = GetList<UserCreateModel>(service, x => x.Users);
            var option = await ExecItems(items, async x => await client.Exist(x.UserId, context), async x => await client.Create(x, context));
            return option;
        });

        builder.AddProvider(async (service, context) =>
        {
            var client = service.GetRequiredService<AgentClient>();
            var items = GetList<AgentModel>(service, x => x.Agents);
            var option = await ExecItems(items, async x => await client.Exist(x.AgentId, context), async x => await client.Set(x, context));
            return option;
        });

        return builder;
    }

    private static IReadOnlyList<TModel> GetList<TModel>(IServiceProvider service, Func<SetupOption, IReadOnlyList<TModel>?> getElements)
    {
        ConfigClient agentClient = service.GetRequiredService<ConfigClient>();
        SetupOption setupOption = service.GetRequiredService<IConfiguration>().Func(x => x.Bind<SetupOption>());
        var items = getElements(setupOption)?.ToArray() ?? Array.Empty<TModel>();
        return items;
    }

    private static async Task<Option> ExecItems<TModel>(IReadOnlyList<TModel> items, Func<TModel, Task<Option>> existTest, Func<TModel, Task<Option>> update)
    {
        foreach (var item in items)
        {
            var ifExist = await existTest(item);
            if (ifExist.IsOk()) return StatusCode.OK;

            var option = await update(item);
            if (option.IsError()) return option;
        }

        return StatusCode.OK;
    }
}
