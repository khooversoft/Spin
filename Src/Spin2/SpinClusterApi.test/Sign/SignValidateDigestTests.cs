using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using SpinCluster.sdk.Actors.PrincipalKey;
using SpinCluster.sdk.Actors.Signature;
using SpinCluster.sdk.Actors.User;
using SpinClusterApi.test.Application;
using SpinClusterApi.test.Basics;
using Toolbox.Extensions;
using Toolbox.Types;

namespace SpinClusterApi.test.Sign;

public class SignValidateDigestTests : IClassFixture<ClusterApiFixture>
{
    private readonly ClusterApiFixture _cluster;
    private readonly ScopeContext _context = new ScopeContext(NullLogger.Instance);

    public SignValidateDigestTests(ClusterApiFixture fixture) => _cluster = fixture;

    //[Fact(Skip = "server")]
    [Fact]
    public async Task LifecycleTest()
    {
        NameId subscriptionId = "Company6Subscription";
        TenantId tenantId = "company6.com";
        PrincipalId principalId = "user1@company6.com";

        await Delete(_cluster.ServiceProvider, subscriptionId, tenantId, principalId);
        await Create(_cluster.ServiceProvider, subscriptionId, tenantId, principalId);

        string msg = "this is a message";
        string messageDigest = msg.ToBytes().ToSHA256Hash();

        SignatureClient signatureClient = _cluster.ServiceProvider.GetRequiredService<SignatureClient>();

        var request = new SignRequest
        {
            PrincipalId = principalId,
            MessageDigest = messageDigest
        };

        var jwtOption = await signatureClient.Sign(request, _context);
        jwtOption.IsOk().Should().BeTrue();

        SignResponse response = jwtOption.Return();

        request.PrincipalId.Should().Be(response.Kid);
        request.MessageDigest.Should().Be(response.MessageDigest);
        response.JwtSignature.Should().NotBeNullOrEmpty();

        var validationRequest = new ValidateRequest
        {
            JwtSignature = response.JwtSignature,
            MessageDigest = messageDigest
        };

        var validation = await signatureClient.ValidateDigest(validationRequest, _context);
        validation.IsOk().Should().BeTrue();
    }

    private async Task Create(IServiceProvider service, NameId subscriptionId, TenantId tenantId, PrincipalId principalId)
    {
        UserClient client = service.GetRequiredService<UserClient>();

        var subscription = await SubscriptionTests.CreateSubscription(_cluster.ServiceProvider, subscriptionId, _context);
        subscription.IsOk().Should().BeTrue();

        var tenant = await TenantTests.CreateTenant(_cluster.ServiceProvider, tenantId, subscriptionId, _context);
        tenant.IsOk().Should().BeTrue();

        await VerifyKeys(_cluster.ServiceProvider, principalId, false);
        var user = await UserTest.CreateUser(_cluster.ServiceProvider, principalId, _context);
        await VerifyKeys(_cluster.ServiceProvider, principalId, true);

        Option<UserModel> readOption = await client.Get(principalId, _context);
        readOption.IsOk().Should().BeTrue();

        //(user.Return() == readOption.Return()).Should().BeTrue();
    }

    private async Task Delete(IServiceProvider service, NameId subscriptionId, TenantId tenantId, PrincipalId principalId)
    {
        Option deleteOption = await UserTest.DeleteUser(service, principalId, _context);
        deleteOption.StatusCode.IsOk().Should().BeTrue();
        await VerifyKeys(_cluster.ServiceProvider, principalId, false);

        Option deleteTenantOption = await TenantTests.DeleteTenant(_cluster.ServiceProvider, tenantId, _context);
        deleteTenantOption.StatusCode.IsOk().Should().BeTrue();

        Option deleteSubscriptionOption = await SubscriptionTests.DeleteSubscription(_cluster.ServiceProvider, subscriptionId, _context);
        deleteSubscriptionOption.StatusCode.IsOk().Should().BeTrue();
    }

    private async Task VerifyKeys(IServiceProvider service, PrincipalId principalId, bool mustExist)
    {
        PrincipalKeyClient publicKeyClient = service.GetRequiredService<PrincipalKeyClient>();
        var publicKeyExist = await publicKeyClient.Get(principalId, _context);
        (publicKeyExist.IsOk() == mustExist).Should().BeTrue();

        PrincipalPrivateKeyClient publicPrivateKeyClient = service.GetRequiredService<PrincipalPrivateKeyClient>();
        var privateKeyExist = await publicPrivateKeyClient.Get(principalId, _context);
        (privateKeyExist.IsOk() == mustExist).Should().BeTrue();
    }
}
