//using FluentAssertions;
//using Microsoft.Extensions.Logging.Abstractions;
//using Orleans.TestingHost;
//using SpinCluster.sdk.Actors.Signature;
//using SpinCluster.sdk.Application;
//using SpinCluster.sdk.test.Application;
//using Toolbox.Orleans.Types;
//using Toolbox.Security.Jwt;
//using Toolbox.Types;

//namespace SpinCluster.sdk.test.Signing;

//public class SignAndVerify : IClassFixture<ClusterFixture>
//{
//    private readonly TestCluster _cluster;
//    private readonly ScopeContext _context = new ScopeContext(NullLogger.Instance);

//    public SignAndVerify(ClusterFixture fixture)
//    {
//        _cluster = fixture.Cluster;
//    }

//    [Fact(Skip = "Go to API")]
//    public async Task SignDigest()
//    {
//        string ownerId = "signKey@test.com";
//        string keyId = $"{SpinConstants.Schema.PrincipalKey}/test/SignAndVerify/{ownerId}";
//        string privateKeyId = $"{SpinConstants.Schema.PrincipalPrivateKey}/test/SignAndVerify/{ownerId}";

//        var request = new PrincipalKeyRequest
//        {
//            KeyId = keyId,
//            OwnerId = ownerId,
//            Audience = "test.com",
//            Name = "test sign key",
//        };

//        ISignatureActor signatureActor = _cluster.GrainFactory.GetGrain<ISignatureActor>(keyId);
//        await signatureActor.Delete(_context.TraceId);

//        Option result = await signatureActor.Create(request, _context.TraceId);
//        result.StatusCode.IsOk().Should().BeTrue();

//        string digest = "this is a digest";

//        Option<string> signResponse = await signatureActor.Sign(digest, _context.TraceId);
//        signResponse.StatusCode.IsOk().Should().BeTrue();
//        signResponse.Value.Should().NotBeNull();

//        string jwtSignature = signResponse.Return();
//        string? kid = JwtTokenParser.GetKidFromJwtToken(jwtSignature);
//        kid.Should().Be(request.KeyId);

//        Option validationResponse = await signatureActor.ValidateJwtSignature(jwtSignature, digest, _context.TraceId);
//        validationResponse.StatusCode.IsOk().Should().BeTrue();

//        await signatureActor.Delete(_context.TraceId);
//    }
//}
