using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using SpinCluster.sdk.Actors.PrincipalKey;
using SpinCluster.sdk.Actors.Signature;
using SpinCluster.sdk.Actors.User;
using SpinCluster.sdk.Application;
using SpinClusterApi.test.Application;
using SpinClusterApi.test.Basics;
using Toolbox.Extensions;
using Toolbox.Types;

namespace SpinClusterApi.test.Contract;

public class SignValidateDigestTests : IClassFixture<ClusterApiFixture>
{
    private readonly ClusterApiFixture _cluster;
    private readonly ScopeContext _context = new ScopeContext(NullLogger.Instance);

    public SignValidateDigestTests(ClusterApiFixture fixture) => _cluster = fixture;

    //[Fact(Skip = "server")]
    [Fact]
    public async Task LifecycleTest()
    {
        string subscriptionId = "Company6Subscription";
        string tenantId = "company6.com";
        string principalId = "user1@company6.com";

        await Delete(_cluster.ServiceProvider, subscriptionId, tenantId, principalId);
        await Create(_cluster.ServiceProvider, subscriptionId, tenantId, principalId);

        string msg = "this is a message";
        string messageDigest = msg.ToBytes().ToSHA256Hash();

        UserClient userClient = _cluster.ServiceProvider.GetRequiredService<UserClient>();

        var signRequest = new SignRequest
        {
            PrincipalId = principalId,
            MessageDigest = messageDigest,
        };

        Option<SignResponse> jwtOption = await userClient.Sign(signRequest, _context);
        jwtOption.IsOk().Should().BeTrue();
        jwtOption.Return().Should().NotBeNull();

        SignResponse response = jwtOption.Return();
        response.Kid.Should().Be(IdTool.CreateKid(principalId, "sign"));
        response.MessageDigest.Should().Be(messageDigest);
        response.JwtSignature.Should().NotBeNullOrEmpty();

        SignatureClient signatureClient = _cluster.ServiceProvider.GetRequiredService<SignatureClient>();

        var validationRequest = new ValidateRequest
        {
            JwtSignature = response.JwtSignature,
            MessageDigest = messageDigest
        };

        var validation = await signatureClient.ValidateDigest(validationRequest, _context);
        validation.IsOk().Should().BeTrue();

        var badValidationRequest = new ValidateRequest
        {
            JwtSignature = response.JwtSignature,
            MessageDigest = messageDigest + ".",
        };

        var badValidation = await signatureClient.ValidateDigest(badValidationRequest, _context);
        badValidation.IsError().Should().BeTrue();
    }

    private async Task Create(IServiceProvider service, string subscriptionId, string tenantId, string principalId)
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

    private async Task Delete(IServiceProvider service, string subscriptionId, string tenantId, string principalId)
    {
        Option deleteOption = await UserTest.DeleteUser(service, principalId, _context);
        deleteOption.StatusCode.IsOk().Should().BeTrue();
        await VerifyKeys(_cluster.ServiceProvider, principalId, false);

        Option deleteTenantOption = await TenantTests.DeleteTenant(_cluster.ServiceProvider, tenantId, _context);
        deleteTenantOption.StatusCode.IsOk().Should().BeTrue();

        Option deleteSubscriptionOption = await SubscriptionTests.DeleteSubscription(_cluster.ServiceProvider, subscriptionId, _context);
        deleteSubscriptionOption.StatusCode.IsOk().Should().BeTrue();
    }

    private async Task VerifyKeys(IServiceProvider service, string principalId, bool mustExist)
    {
        PrincipalKeyClient publicKeyClient = service.GetRequiredService<PrincipalKeyClient>();
        var publicKeyExist = await publicKeyClient.Get(principalId, _context);
        (publicKeyExist.IsOk() == mustExist).Should().BeTrue();

        PrincipalPrivateKeyClient publicPrivateKeyClient = service.GetRequiredService<PrincipalPrivateKeyClient>();
        var privateKeyExist = await publicPrivateKeyClient.Get(principalId, _context);
        (privateKeyExist.IsOk() == mustExist).Should().BeTrue();
    }
}
