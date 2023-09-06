using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using SpinCluster.sdk.Actors.PrincipalKey;
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
        string subscriptionId = "Company5Subscription";
        string tenantId = "company5.com";
        string principalId = "user1@company5.com";

        var subscription = await SubscriptionTests.CreateSubscription(_cluster.ServiceProvider, subscriptionId, _context);
        subscription.IsOk().Should().BeTrue();

        var tenant = await TenantTests.CreateTenant(_cluster.ServiceProvider, tenantId, subscriptionId, _context);
        tenant.IsOk().Should().BeTrue();

        Option<UserModel> userModelOption = await client.Get(principalId, _context);
        if (userModelOption.IsOk()) await client.Delete(principalId, _context);

        await VerifyKeys(_cluster.ServiceProvider, principalId, false);
        var user = await CreateUser(_cluster.ServiceProvider, principalId, _context);
        await VerifyKeys(_cluster.ServiceProvider, principalId, true);

        Option<UserModel> readOption = await client.Get(principalId, _context);
        readOption.IsOk().Should().BeTrue();

        UserCreateModel userModel = user.Return();
        UserModel readUserModel = readOption.Return();
        userModel.UserId.Should().Be(readUserModel.UserId);
        userModel.PrincipalId.Should().Be(principalId);
        userModel.DisplayName.Should().Be(readUserModel.DisplayName);
        userModel.FirstName.Should().Be(readUserModel.FirstName);
        userModel.LastName.Should().Be(readUserModel.LastName);

        Option deleteOption = await client.Delete(principalId, _context);
        deleteOption.StatusCode.IsOk().Should().BeTrue();
        await VerifyKeys(_cluster.ServiceProvider, principalId, false);

        Option deleteTenantOption = await TenantTests.DeleteTenant(_cluster.ServiceProvider, tenantId, _context);
        deleteTenantOption.StatusCode.IsOk().Should().BeTrue();

        Option deleteSubscriptionOption = await SubscriptionTests.DeleteSubscription(_cluster.ServiceProvider, subscriptionId, _context);
        deleteSubscriptionOption.StatusCode.IsOk().Should().BeTrue();
    }

    private async Task VerifyKeys(IServiceProvider service, string principalId, bool mustExist)
    {
        const string sign = "sign";

        PrincipalKeyClient publicKeyClient = service.GetRequiredService<PrincipalKeyClient>();
        var publicKeyExist = await publicKeyClient.Get(principalId, sign, _context);
        (publicKeyExist.IsOk() == mustExist).Should().BeTrue();

        PrincipalPrivateKeyClient publicPrivateKeyClient = service.GetRequiredService<PrincipalPrivateKeyClient>();
        var privateKeyExist = await publicPrivateKeyClient.Get(principalId, sign, _context);
        (privateKeyExist.IsOk() == mustExist).Should().BeTrue();
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
        (deleteOption.IsOk() || deleteOption.IsNotFound()).Should().BeTrue();

        return StatusCode.OK;
    }
}
