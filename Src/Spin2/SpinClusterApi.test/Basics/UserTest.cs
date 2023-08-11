using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using SpinCluster.sdk.Actors.Subscription;
using SpinCluster.sdk.Actors.Tenant;
using SpinCluster.sdk.Actors.User;
using SpinCluster.sdk.Application;
using SpinClusterApi.test.Application;
using Toolbox.Types;

namespace SpinClusterApi.test.Basics;

public class UserTest : IClassFixture<ClusterApiFixture>
{
    private readonly ClusterApiFixture _cluster;
    private readonly ScopeContext _context = new ScopeContext(NullLogger.Instance);

    public UserTest(ClusterApiFixture fixture)
    {
        _cluster = fixture;
    }

    //[Fact(Skip = "server")]
    [Fact]
    public async Task LifecycleTest()
    {
        UserClient client = _cluster.ServiceProvider.GetRequiredService<UserClient>();
        NameId subscriptionId = "Company3Subscription";
        TenantId tenantId = "company3.com";
        PrincipalId principalId = "user1@company3.com";

        var subscription = await SubscriptionTests.CreateSubscription(_cluster.ServiceProvider, subscriptionId, _context);
        subscription.IsOk().Should().BeTrue();

        var tenant = await TenantTests.CreateTenant(_cluster.ServiceProvider, tenantId, subscriptionId, _context);
        tenant.IsOk().Should().BeTrue();

        var user = await CreateUser(_cluster.ServiceProvider, principalId, _context);

        Option<UserModel> readOption = await client.Get(principalId, _context);
        readOption.IsOk().Should().BeTrue();

        (user.Return() == readOption.Return()).Should().BeTrue();

        Option deleteOption = await client.Delete(principalId, _context);
        deleteOption.StatusCode.IsOk().Should().BeTrue();

        Option deleteTenantOption = await TenantTests.DeleteTenant(_cluster.ServiceProvider, tenantId, _context);
        deleteTenantOption.StatusCode.IsOk().Should().BeTrue();

        Option deleteSubscriptionOption = await SubscriptionTests.DeleteSubscription(_cluster.ServiceProvider, subscriptionId, _context);
        deleteSubscriptionOption.StatusCode.IsOk().Should().BeTrue();
    }


    public static async Task<Option<UserModel>> CreateUser(IServiceProvider service, PrincipalId principalId, ScopeContext context)
    {
        UserClient client = service.GetRequiredService<UserClient>();

        Option<UserModel> result = await client.Get(principalId, context);
        if (result.IsOk()) await client.Delete(principalId, context);

        var user = new UserModel
        {
            UserId = UserModel.CreateId(principalId),
            PrincipalId = principalId,
            DisplayName = "User display name",
            FirstName = "First",
            LastName = "Last",
            AccountEnabled = true,
        };

        Option setOption = await client.Set(user, context);
        setOption.StatusCode.IsOk().Should().BeTrue();

        return user;
    }

    public static async Task<Option> DeleteTenant(IServiceProvider service, PrincipalId principalId, ScopeContext context)
    {
        UserClient client = service.GetRequiredService<UserClient>();

        Option deleteOption = await client.Delete(principalId, context);
        deleteOption.StatusCode.IsOk().Should().BeTrue();

        return StatusCode.OK;
    }
}
