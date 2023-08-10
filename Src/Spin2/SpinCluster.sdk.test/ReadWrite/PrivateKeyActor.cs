using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Orleans.TestingHost;
using SpinCluster.sdk.Actors.Key;
using SpinCluster.sdk.Actors.PrincipalPrivateKey;
using SpinCluster.sdk.Application;
using SpinCluster.sdk.test.Application;
using Toolbox.Orleans.Types;
using Toolbox.Types;

namespace SpinCluster.sdk.test.ReadWrite;

public class PrivateKeyActor : IClassFixture<ClusterFixture>
{
    private readonly TestCluster _cluster;
    private readonly ScopeContext _context = new ScopeContext(NullLogger.Instance);

    public PrivateKeyActor(ClusterFixture fixture)
    {
        _cluster = fixture.Cluster;
    }

    [Fact(Skip = "Go to API")]
    public async Task TestReadWrite()
    {
        string ownerId = "signKey@test.com";
        string keyId = $"{SpinConstants.Schema.PrincipalPrivateKey}/test/TestReadWrite/{ownerId}";

        IPrincipalPrivateKeyActor actor = _cluster.GrainFactory.GetGrain<IPrincipalPrivateKeyActor>(keyId);

        await actor.Delete(_context.TraceId);

        var rsaKey = new RsaKeyPair("key");
        var request = new PrincipalPrivateKeyModel
        {
            KeyId = keyId,
            OwnerId = ownerId,
            Audience = "test.com",
            Name = "test sign key",
            PrivateKey = rsaKey.PrivateKey,
        };

        Option writeResult = await actor.Set(request, _context.TraceId);
        writeResult.StatusCode.IsOk().Should().BeTrue();

        Option<PrincipalPrivateKeyModel> read = await actor.Get(_context.TraceId);
        read.StatusCode.IsOk().Should().BeTrue();

        (request == read.Return()).Should().BeTrue();

        await actor.Delete(_context.TraceId);
    }
}