using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using SpinCluster.sdk.Actors.PrincipalKey;
using SpinCluster.sdk.Actors.Subscription;
using SpinCluster.sdk.Actors.Tenant;
using SpinCluster.sdk.Actors.User;
using SpinCluster.sdk.Application;
using Toolbox.Types;

namespace SoftBank.sdk.test.Application;

public class SetupTools
{
    private readonly ClusterApiFixture _cluster;
    private readonly ScopeContext _context;

    public SetupTools(ClusterApiFixture cluster, ScopeContext context)
    {
        _cluster = cluster;
        _context = context;
    }

    public async Task CreateUser(IServiceProvider service, string subscriptionId, string tenantId, string principalId)
    {
        UserClient client = service.GetRequiredService<UserClient>();

        var subscription = await CreateSubscription(_cluster.ServiceProvider, subscriptionId, _context);
        subscription.IsOk().Should().BeTrue();

        var tenant = await CreateTenant(_cluster.ServiceProvider, tenantId, subscriptionId, _context);
        tenant.IsOk().Should().BeTrue();

        var user = await CreateUser(_cluster.ServiceProvider, principalId, _context);

        Option<UserModel> readOption = await client.Get(principalId, _context);
        readOption.IsOk().Should().BeTrue();
    }

    public async Task DeleteUser(IServiceProvider service, string subscriptionId, string tenantId, string principalId)
    {
        Option deleteOption = await DeleteUser(service, principalId, _context);
        deleteOption.StatusCode.IsOk().Should().BeTrue();

        Option deleteTenantOption = await DeleteTenant(_cluster.ServiceProvider, tenantId, _context);
        deleteTenantOption.StatusCode.IsOk().Should().BeTrue();

        Option deleteSubscriptionOption = await DeleteSubscription(_cluster.ServiceProvider, subscriptionId, _context);
        deleteSubscriptionOption.StatusCode.IsOk().Should().BeTrue();
    }

    public static async Task<Option<SubscriptionModel>> CreateSubscription(IServiceProvider service, string nameId, ScopeContext context)
    {
        SubscriptionClient client = service.GetRequiredService<SubscriptionClient>();

        Option<SubscriptionModel> result = await client.Get(nameId, context);
        if (result.IsOk()) await client.Delete(nameId, context);

        var subscription = new SubscriptionModel
        {
            SubscriptionId = IdTool.CreateSubscriptionId(nameId),
            Name = nameId,
            ContactName = nameId + "contact",
            Email = "user1@company1.com",
            AccountEnabled = true,
            ActiveDate = DateTime.UtcNow,
        };

        Option setOption = await client.Set(subscription, context);
        setOption.StatusCode.IsOk().Should().BeTrue();

        return subscription;
    }

    public static async Task<Option> DeleteSubscription(IServiceProvider service, string nameId, ScopeContext context)
    {
        SubscriptionClient client = service.GetRequiredService<SubscriptionClient>();

        Option deleteOption = await client.Delete(nameId, context);
        deleteOption.StatusCode.IsOk().Should().BeTrue();

        return StatusCode.OK;
    }

    public static async Task<Option<TenantModel>> CreateTenant(IServiceProvider service, string nameId, string subscriptionId, ScopeContext context)
    {
        TenantClient client = service.GetRequiredService<TenantClient>();

        Option<TenantModel> result = await client.Get(nameId, context);
        if (result.IsOk()) await client.Delete(nameId, context);

        var tenant = new TenantModel
        {
            TenantId = IdTool.CreateTenantId(nameId),
            Name = nameId,
            SubscriptionId = IdTool.CreateSubscriptionId(subscriptionId),
            ContactName = nameId + "contact",
            Email = "user1@company2.com",

            AccountEnabled = true,
            ActiveDate = DateTime.UtcNow,
        };

        Option setOption = await client.Set(tenant, context);
        setOption.StatusCode.IsOk().Should().BeTrue();

        return tenant;
    }

    public static async Task<Option> DeleteTenant(IServiceProvider service, string tenantId, ScopeContext context)
    {
        TenantClient client = service.GetRequiredService<TenantClient>();

        Option deleteOption = await client.Delete(tenantId, context);
        deleteOption.IsOk().Should().BeTrue();

        return StatusCode.OK;
    }

    public static async Task<Option<UserCreateModel>> CreateUser(IServiceProvider service, string principalId, ScopeContext context)
    {
        UserClient client = service.GetRequiredService<UserClient>();

        Option<UserModel> result = await client.Get(principalId, context);
        if (result.IsOk()) await client.Delete(principalId, context);

        var user = new UserCreateModel
        {
            UserId = IdTool.CreateUserId(principalId),
            PrincipalId = principalId,
            DisplayName = "User display name",
            FirstName = "First",
            LastName = "Last"
        };

        Option setOption = await client.Create(user, context);
        setOption.IsOk().Should().BeTrue();

        return user;
    }

    public static async Task<Option> DeleteUser(IServiceProvider service, string principalId, ScopeContext context)
    {
        UserClient client = service.GetRequiredService<UserClient>();

        Option deleteOption = await client.Delete(principalId, context);
        deleteOption.IsOk().Should().BeTrue();

        return StatusCode.OK;
    }
}
