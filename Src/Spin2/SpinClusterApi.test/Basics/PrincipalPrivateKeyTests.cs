using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using SpinCluster.sdk.Actors.PrincipalKey;
using SpinCluster.sdk.Actors.PrincipalPrivateKey;
using SpinCluster.sdk.Actors.Subscription;
using SpinCluster.sdk.Actors.User;
using SpinClusterApi.test.Application;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinClusterApi.test.Basics;

public class PrincipalPrivateKeyTests : IClassFixture<ClusterApiFixture>
{
    private readonly ClusterApiFixture _cluster;
    private readonly ScopeContext _context = new ScopeContext(NullLogger.Instance);

    public PrincipalPrivateKeyTests(ClusterApiFixture fixture)
    {
        _cluster = fixture;
    }

    //[Fact(Skip = "server")]
    [Fact]
    public async Task LifecycleTest()
    {
        PrincipalPrivateKeyClient client = _cluster.ServiceProvider.GetRequiredService<PrincipalPrivateKeyClient>();
        PrincipalId principalId = "user1@company4.com";

        Option<PrincipalPrivateKeyModel> result = await client.Get(principalId, _context);
        if (result.IsOk()) await client.Delete(principalId, _context);

        var rsaKey = new RsaKeyPair("key");

        var model = new PrincipalPrivateKeyModel
        {
            KeyId = PrincipalPrivateKeyModel.CreateId(principalId),
            PrincipalId = principalId,
            Name = "test",
            Audience = "audience",
            PrivateKey = rsaKey.PrivateKey,
            AccountEnabled = true,
        };

        Option setOption = await client.Set(model, _context);
        setOption.StatusCode.IsOk().Should().BeTrue();

        Option<PrincipalPrivateKeyModel> readOption = await client.Get(principalId, _context);
        readOption.IsOk().Should().BeTrue();

        (model == readOption.Return()).Should().BeTrue();

        Option deleteOption = await client.Delete(principalId, _context);
        deleteOption.StatusCode.IsOk().Should().BeTrue();
    }
}
