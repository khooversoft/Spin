using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Orleans.TestingHost;
using SpinCluster.sdk.Actors.Key;
using SpinCluster.sdk.Actors.Key.Private;
using SpinCluster.sdk.Application;
using SpinCluster.sdk.test.Application;
using SpinCluster.sdk.Types;
using Toolbox.Security.Jwt;
using Toolbox.Types;
using Xunit;

namespace SpinCluster.sdk.test.Signing;

public class SignAndVerify : IClassFixture<ClusterFixture>
{
    private readonly TestCluster _cluster;
    private readonly ScopeContext _context = new ScopeContext(NullLogger.Instance);

    public SignAndVerify(ClusterFixture fixture)
    {
        _cluster = fixture.Cluster;
    }

    [Fact]
    public async Task SignDigest()
    {
        string ownerId = "signKey@test.com";
        string keyId = $"{SpinConstants.Schema.PrincipalKey}/test/SignAndVerify/{ownerId}";
        string privateKeyId = $"{SpinConstants.Schema.PrincipalPrivateKey}/test/SignAndVerify/{ownerId}";

        var request = new PrincipalKeyRequest
        {
            KeyId = keyId,
            OwnerId = ownerId,
            Audience = "test.com",
            Name = "test sign key",
        };

        IPrincipalKeyActor principalKeyActor = _cluster.GrainFactory.GetGrain<IPrincipalKeyActor>(request.KeyId);
        await principalKeyActor.Delete(_context.TraceId);

        SpinResponse result = await principalKeyActor.Create(request, _context.TraceId);
        result.StatusCode.IsOk().Should().BeTrue();

        string digest = "this is a digest";

        IPrincipalPrivateKeyActor privateKey = _cluster.GrainFactory.GetGrain<IPrincipalPrivateKeyActor>(privateKeyId);
        SpinResponse<string> signResponse = await privateKey.Sign(digest, _context.TraceId);
        signResponse.StatusCode.IsOk().Should().BeTrue();
        signResponse.Value.Should().NotBeNull();

        string jwtSignature = signResponse.Return();
        string? kid = JwtTokenParser.GetKidFromJwtToken(jwtSignature);
        kid.Should().Be(request.KeyId);

        SpinResponse validationResponse = await principalKeyActor.ValidateJwtSignature(jwtSignature, digest, _context.TraceId);
        validationResponse.StatusCode.IsOk().Should().BeTrue();

        await principalKeyActor.Delete(_context.TraceId);
    }
}
