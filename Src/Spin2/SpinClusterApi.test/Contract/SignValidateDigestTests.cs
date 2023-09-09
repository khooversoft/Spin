using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using SpinCluster.sdk.Actors.PrincipalKey;
using SpinCluster.sdk.Actors.User;
using SpinCluster.sdk.Application;
using SpinClusterApi.test.Application;
using SpinTestTools.sdk.ObjectBuilder;
using Toolbox.Extensions;
using Toolbox.Security.Sign;
using Toolbox.Types;

namespace SpinClusterApi.test.Contract;

public class SignValidateDigestTests : IClassFixture<ClusterApiFixture>
{
    private readonly ClusterApiFixture _cluster;
    private readonly ScopeContext _context = new ScopeContext(NullLogger.Instance);
    private const string _setup = """
        {
            "Subscriptions": [
              {
                "SubscriptionId": "subscription:Company6Subscription",
                "Name": "Company6Subscription",
                "ContactName": "Company 6 contact name",
                "Email": "admin@company6.com"
              }
            ],
            "Tenants": [
              {
                "TenantId": "tenant:company6.com",
                "Subscription": "Tenant 6",
                "Domain": "company6.com",
                "SubscriptionId": "subscription:Company6Subscription",
                "ContactName": "Admin",
                "Email": "admin@company6.com"
              }
            ],
            "Users": [
              {
                "UserId": "user:user1@company6.com",
                "PrincipalId": "user1@company6.com",
                "DisplayName": "User 6",
                "FirstName": "user1first",
                "LastName": "user1last"
              }
            ]
        }
        """;

    public SignValidateDigestTests(ClusterApiFixture fixture)
    {
        _cluster = fixture;
    }

    //[Fact(Skip = "server")]
    [Fact]
    public async Task LifecycleTest()
    {
        string principalId = "user1@company6.com";

        ObjectBuilderOption option = ObjectBuilderOptionTool.ReadFromJson(_setup);
        option.Validate().IsOk().Should().BeTrue();

        var objectBuild = new TestObjectBuilder()
            .SetService(_cluster.ServiceProvider)
            .SetOption(option)
            .AddStandard();

        var buildResult = await objectBuild.Build(_context);
        buildResult.IsOk().Should().BeTrue();

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

        var validationRequest = new SignValidateRequest
        {
            JwtSignature = response.JwtSignature,
            MessageDigest = messageDigest
        };

        var validation = await signatureClient.ValidateDigest(validationRequest, _context);
        validation.IsOk().Should().BeTrue(validation.StatusCode.ToString());

        var badValidationRequest = new SignValidateRequest
        {
            JwtSignature = response.JwtSignature,
            MessageDigest = messageDigest + ".",
        };

        var badValidation = await signatureClient.ValidateDigest(badValidationRequest, _context);
        badValidation.IsError().Should().BeTrue();

        await objectBuild.DeleteAll(_context);
    }
}
