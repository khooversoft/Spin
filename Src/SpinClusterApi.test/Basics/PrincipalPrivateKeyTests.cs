using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using SpinClient.sdk;
using SpinCluster.abstraction;
using SpinClusterApi.test.Application;
using Toolbox.Tools;
using Toolbox.Tools.Should;
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
        string principalId = "user1@company4.com";

        Option<PrincipalPrivateKeyModel> result = await client.Get(principalId, _context);
        if (result.IsOk()) await client.Delete(principalId, _context);

        var rsaKey = new RsaKeyPair("key");

        var model = new PrincipalPrivateKeyModel
        {
            PrincipalPrivateKeyId = IdTool.CreatePrivateKeyId(principalId),
            KeyId = IdTool.CreateKid(principalId),
            PrincipalId = principalId,
            Name = "test",
            Audience = "audience",
            PrivateKey = rsaKey.PrivateKey,
            Enabled = true,
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
