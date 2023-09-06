using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using SpinCluster.sdk.Actors.Tenant;
using SpinCluster.sdk.Application;
using SpinClusterApi.test.Application;
using Toolbox.Types;

namespace SpinClusterApi.test.Basics;

public class TenantTests : IClassFixture<ClusterApiFixture>
{
    private readonly ClusterApiFixture _cluster;
    private readonly ScopeContext _context = new ScopeContext(NullLogger.Instance);

    public TenantTests(ClusterApiFixture fixture)
    {
        _cluster = fixture;
    }

    //[Fact(Skip = "server")]
    [Fact]
    public async Task LifecycleTest()
    {
        TenantClient client = _cluster.ServiceProvider.GetRequiredService<TenantClient>();
        string subscriptionId = "Company2Subscription";
        string tenantId = "company2.com";

        var subscription = await SubscriptionTests.CreateSubscription(_cluster.ServiceProvider, subscriptionId, _context);
        subscription.IsOk().Should().BeTrue();

        var tenant = await CreateTenant(_cluster.ServiceProvider, tenantId, subscriptionId, _context);
        tenant.IsOk().Should().BeTrue();

        Option<TenantModel> readOption = await client.Get(tenantId, _context);
        readOption.IsOk().Should().BeTrue();

        (tenant.Return() == readOption.Return()).Should().BeTrue();

        Option deleteOption = await client.Delete(tenantId, _context);
        deleteOption.IsOk().Should().BeTrue();

        Option deleteSubscriptionOption = await SubscriptionTests.DeleteSubscription(_cluster.ServiceProvider, subscriptionId, _context);
        deleteSubscriptionOption.StatusCode.IsOk().Should().BeTrue();
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
        setOption.StatusCode.IsOk().Should().BeTrue(setOption.Error);

        return tenant;
    }

    public static async Task<Option> DeleteTenant(IServiceProvider service, string tenantId, ScopeContext context)
    {
        TenantClient client = service.GetRequiredService<TenantClient>();

        Option deleteOption = await client.Delete(tenantId, context);
        (deleteOption.IsOk() || deleteOption.IsNotFound()).Should().BeTrue();

        return StatusCode.OK;
    }
}
