using Microsoft.Extensions.DependencyInjection;
using SpinClient.sdk;
using SpinCluster.abstraction;
using SpinClusterApi.test.Basics;
using Toolbox.Tools.Should;
using Toolbox.Types;

namespace SpinClusterApi.test.Application;

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

        var subscription = await SubscriptionTests.CreateSubscription(_cluster.ServiceProvider, subscriptionId, _context);
        subscription.IsOk().Should().BeTrue();

        var tenant = await TenantTests.CreateTenant(_cluster.ServiceProvider, tenantId, subscriptionId, _context);
        tenant.IsOk().Should().BeTrue();

        var user = await UserTest.CreateUser(_cluster.ServiceProvider, principalId, _context);

        Option<UserModel> readOption = await client.Get(principalId, _context);
        readOption.IsOk().Should().BeTrue();
    }

    public async Task DeleteUser(IServiceProvider service, string subscriptionId, string tenantId, string principalId)
    {
        Option deleteOption = await UserTest.DeleteUser(service, principalId, _context);
        deleteOption.StatusCode.IsOk().Should().BeTrue();
        await VerifyKeys(_cluster.ServiceProvider, principalId, false);

        Option deleteTenantOption = await TenantTests.DeleteTenant(_cluster.ServiceProvider, tenantId, _context);
        deleteTenantOption.StatusCode.IsOk().Should().BeTrue();

        Option deleteSubscriptionOption = await SubscriptionTests.DeleteSubscription(_cluster.ServiceProvider, subscriptionId, _context);
        deleteSubscriptionOption.StatusCode.IsOk().Should().BeTrue();
    }

    public async Task VerifyKeys(IServiceProvider service, string principalId, bool mustExist)
    {
        PrincipalKeyClient publicKeyClient = service.GetRequiredService<PrincipalKeyClient>();
        var publicKeyExist = await publicKeyClient.Get(principalId, _context);
        (publicKeyExist.IsOk() == mustExist).Should().BeTrue();

        PrincipalPrivateKeyClient publicPrivateKeyClient = service.GetRequiredService<PrincipalPrivateKeyClient>();
        var privateKeyExist = await publicPrivateKeyClient.Get(principalId, _context);
        (privateKeyExist.IsOk() == mustExist).Should().BeTrue();
    }
}
