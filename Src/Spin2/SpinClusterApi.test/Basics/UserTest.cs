using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using SpinClient.sdk;
using SpinCluster.abstraction;
using SpinCluster.sdk.Application;
using SpinClusterApi.test.Application;
using SpinTestTools.sdk.ObjectBuilder;
using Toolbox.Types;

namespace SpinClusterApi.test.Basics;

public class UserTest : IClassFixture<ClusterApiFixture>
{
    private readonly ClusterApiFixture _cluster;
    private readonly ScopeContext _context = new ScopeContext(NullLogger.Instance);
    private const string _setup = """
        {
            "Subscriptions": [
              {
                "SubscriptionId": "subscription:Company5Subscription",
                "Name": "Company5Subscription",
                "ContactName": "Company 55 contact name",
                "Email": "admin@company55.com"
              }
            ],
            "Tenants": [
              {
                "TenantId": "tenant:company5.com",
                "Subscription": "Tenant 5",
                "Domain": "company5.com",
                "SubscriptionId": "subscription:Company5Subscription",
                "ContactName": "Admin",
                "Email": "admin@company5.com",
                "Enabled": true
              }
            ]
        }
        """;

    public UserTest(ClusterApiFixture fixture)
    {
        _cluster = fixture;
    }

    //[Fact(Skip = "server")]
    [Fact]
    public async Task LifecycleTest()
    {
        UserClient client = _cluster.ServiceProvider.GetRequiredService<UserClient>();
        string principalId = "user1@company5.com";

        ObjectBuilderOption option = ObjectBuilderOptionTool.ReadFromJson(_setup);
        option.Validate().IsOk().Should().BeTrue();

        var objectBuild = new TestObjectBuilder()
            .SetService(_cluster.ServiceProvider)
            .SetOption(option)
            .AddStandard();

        var buildResult = await objectBuild.Build(_context);
        buildResult.IsOk().Should().BeTrue();

        string userId = "user:" + principalId;
        Option<UserModel> userModelOption = await client.Get(userId, _context);
        if (userModelOption.IsOk()) await client.Delete(userId, _context);

        await VerifyKeys(_cluster.ServiceProvider, principalId, false);
        var user = await CreateUser(_cluster.ServiceProvider, principalId, _context);
        await VerifyKeys(_cluster.ServiceProvider, principalId, true);

        Option<UserModel> readOption = await client.Get(userId, _context);
        readOption.IsOk().Should().BeTrue();

        UserCreateModel userModel = user.Return();
        UserModel readUserModel = readOption.Return();
        userModel.UserId.Should().Be(readUserModel.UserId);
        userModel.PrincipalId.Should().Be(principalId);
        userModel.DisplayName.Should().Be(readUserModel.DisplayName);
        userModel.FirstName.Should().Be(readUserModel.FirstName);
        userModel.LastName.Should().Be(readUserModel.LastName);

        Option deleteOption = await client.Delete(userId, _context);
        deleteOption.StatusCode.IsOk().Should().BeTrue();
        await VerifyKeys(_cluster.ServiceProvider, principalId, false);

        await objectBuild.DeleteAll(_context);
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

        Option deleteOption = await client.Delete("user:" + principalId, context);
        (deleteOption.IsOk() || deleteOption.IsNotFound()).Should().BeTrue();

        return StatusCode.OK;
    }
}
