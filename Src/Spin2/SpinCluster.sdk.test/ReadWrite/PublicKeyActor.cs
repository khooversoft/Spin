using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Orleans.TestingHost;
using SpinCluster.sdk.Actors.Key;
using SpinCluster.sdk.Actors.PrincipalKey;
using SpinCluster.sdk.Application;
using SpinCluster.sdk.test.Application;
using Toolbox.Orleans.Types;
using Toolbox.Types;

namespace SpinCluster.sdk.test.ReadWrite;

public class PublicKeyActor : IClassFixture<ClusterFixture>
{
    private readonly TestCluster _cluster;
    private readonly ScopeContext _context = new ScopeContext(NullLogger.Instance);

    public PublicKeyActor(ClusterFixture fixture)
    {
        _cluster = fixture.Cluster;
    }

    [Fact]
    public async Task TestReadWrite()
    {
        string ownerId = "signKey@test.com";
        string keyId = $"{SpinConstants.Schema.PrincipalKey}/test/TestReadWrite/{ownerId}";

        IPrincipalKeyActor actor = _cluster.GrainFactory.GetGrain<IPrincipalKeyActor>(keyId);

        await actor.Delete(_context.TraceId);

        var rsaKey = new RsaKeyPair("key");
        var request = new PrincipalKeyModel
        {
            KeyId = keyId,
            OwnerId = ownerId,
            Audience = "test.com",
            Name = "test sign key",
            PublicKey = rsaKey.PublicKey,
        };

        SpinResponse writeResult = await actor.Set(request, _context.TraceId);
        writeResult.StatusCode.IsOk().Should().BeTrue();

        SpinResponse<PrincipalKeyModel> read = await actor.Get(_context.TraceId);
        read.StatusCode.IsOk().Should().BeTrue();

        (request == read.Return()).Should().BeTrue();

        await actor.Delete(_context.TraceId);
    }
}