using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using SpinCluster.sdk.Actors.Subscription;
using SpinCluster.sdk.Application;
using SpinClusterApi.test.Application;
using Toolbox.Types;

namespace SpinClusterApi.test.Basics;

public class SubscriptionTests : IClassFixture<ClusterApiFixture>
{
    private readonly ClusterApiFixture _cluster;
    private readonly ScopeContext _context = new ScopeContext(NullLogger.Instance);

    public SubscriptionTests(ClusterApiFixture fixture)
    {
        _cluster = fixture;
    }

    //[Fact(Skip = "server")]
    [Fact]
    public async Task LifecycleTest()
    {
        SubscriptionClient client = _cluster.ServiceProvider.GetRequiredService<SubscriptionClient>();
        string nameId = "Company1Subscription";

        var subscription = await CreateSubscription(_cluster.ServiceProvider, nameId, _context);
        subscription.IsOk().Should().BeTrue();

        Option<SubscriptionModel> readOption = await client.Get(nameId, _context);
        readOption.IsOk().Should().BeTrue();

        (subscription.Return() == readOption.Return()).Should().BeTrue();

        Option deleteOption = await DeleteSubscription(_cluster.ServiceProvider, nameId, _context);
        deleteOption.StatusCode.IsOk().Should().BeTrue();
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
        };

        Option setOption = await client.Set(subscription, context);
        setOption.StatusCode.IsOk().Should().BeTrue();

        return subscription;
    }

    public static async Task<Option> DeleteSubscription(IServiceProvider service, string nameId, ScopeContext context)
    {
        SubscriptionClient client = service.GetRequiredService<SubscriptionClient>();

        Option deleteOption = await client.Delete(nameId, context);
        (deleteOption.IsOk() || deleteOption.IsNotFound()).Should().BeTrue(deleteOption.Error);

        return StatusCode.OK;
    }
}
