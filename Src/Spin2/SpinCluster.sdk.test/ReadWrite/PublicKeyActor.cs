using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Orleans.TestingHost;
using SpinCluster.sdk.Actors.Key;
using SpinCluster.sdk.Application;
using SpinCluster.sdk.test.Application;
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

        var request = new PrincipalKeyRequest
        {
            KeyId = keyId,
            OwnerId = ownerId,
            Audience = "test.com",
            Name = "test sign key",
        };

        var writeResult = await actor.Set




    }
}