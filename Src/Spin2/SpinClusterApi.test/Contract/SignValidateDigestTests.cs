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
    private readonly SetupTools _setupTools;

    public SignValidateDigestTests(ClusterApiFixture fixture)
    {
        _cluster = fixture;
        _setupTools = new SetupTools(_cluster, _context);
    }

    //[Fact(Skip = "server")]
    [Fact]
    public async Task LifecycleTest()
    {
        string subscriptionId = "Company6Subscription";
        string tenantId = "company6.com";
        string principalId = "user1@company6.com";

        await _setupTools.DeleteUser(_cluster.ServiceProvider, subscriptionId, tenantId, principalId);
        await _setupTools.CreateUser(_cluster.ServiceProvider, subscriptionId, tenantId, principalId);

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
}
