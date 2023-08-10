using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Numerics;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Azure.Amqp.Framing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using SpinCluster.sdk.Actors.Subscription;
using SpinCluster.sdk.Actors.User;
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

    [Fact(Skip = "server")]
    //[Fact]
    public async Task LifecycleTest()
    {
        SubscriptionClient client = _cluster.ServiceProvider.GetRequiredService<SubscriptionClient>();
        NameId nameId = "Company1Subscription";

        var subscription = await CreateSubscription(_cluster.ServiceProvider, nameId, _context);
        subscription.IsOk().Should().BeTrue();

        Option<SubscriptionModel> readOption = await client.Get(nameId, _context);
        readOption.IsOk().Should().BeTrue();

        (subscription.Return() == readOption.Return()).Should().BeTrue();

        Option deleteOption = await DeleteSubscription(_cluster.ServiceProvider, nameId, _context);
        deleteOption.StatusCode.IsOk().Should().BeTrue();
    }

    public static async Task<Option<SubscriptionModel>> CreateSubscription(IServiceProvider service, NameId nameId, ScopeContext context)
    {
        SubscriptionClient client = service.GetRequiredService<SubscriptionClient>();

        Option<SubscriptionModel> result = await client.Get(nameId, context);
        if (result.IsOk()) await client.Delete(nameId, context);

        var subscription = new SubscriptionModel
        {
            SubscriptionId = SubscriptionModel.CreateId(nameId),
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

    public static async Task<Option> DeleteSubscription(IServiceProvider service, NameId nameId, ScopeContext context)
    {
        SubscriptionClient client = service.GetRequiredService<SubscriptionClient>();

        Option deleteOption = await client.Delete(nameId, context);
        deleteOption.StatusCode.IsOk().Should().BeTrue();

        return StatusCode.OK;
    }
}
