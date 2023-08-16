using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using SpinCluster.sdk.Actors.PrincipalKey;
using SpinCluster.sdk.Actors.Subscription;
using SpinCluster.sdk.Actors.User;
using SpinCluster.sdk.Application;
using SpinClusterApi.test.Application;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinClusterApi.test.Basics;

public class PrincipalKeyTests : IClassFixture<ClusterApiFixture>
{
    private readonly ClusterApiFixture _cluster;
    private readonly ScopeContext _context = new ScopeContext(NullLogger.Instance);

    public PrincipalKeyTests(ClusterApiFixture fixture)
    {
        _cluster = fixture;
    }

    //[Fact(Skip = "server")]
    [Fact]
    public async Task LifecycleTest()
    {
        PrincipalKeyClient client = _cluster.ServiceProvider.GetRequiredService<PrincipalKeyClient>();
        PrincipalId principalId = "user1@company3.com";

        Option<PrincipalKeyModel> result = await client.Get(principalId, _context);
        if (result.IsOk()) await client.Delete(principalId, _context);

        (await IsPrivateKeyExist(principalId)).StatusCode.IsError().Should().BeTrue();

        var create = new PrincipalKeyCreateModel
        {
            KeyId = IdTool.CreatePublicKeyId(principalId),
            PrincipalId = principalId,
            Name = "user1",
            AccountEnabled = true,
        };

        Option createOption = await client.Create(create, _context);
        createOption.StatusCode.IsOk().Should().BeTrue();

        (await IsPrivateKeyExist(principalId)).StatusCode.IsOk().Should().BeTrue();

        Option<PrincipalKeyModel> readOption = await client.Get(principalId, _context);
        readOption.IsOk().Should().BeTrue();

        PrincipalKeyModel readModel = readOption.Return();
        create.KeyId.Should().Be(readModel.KeyId);
        create.PrincipalId.Should().Be(readModel.PrincipalId);
        create.Name.Should().Be(readModel.Name);
        create.AccountEnabled.Should().Be(readModel.AccountEnabled);
        readModel.Audience.Should().NotBeNullOrEmpty();
        readModel.PublicKey.Should().NotBeNull();
        readModel.PublicKey.Length.Should().BeGreaterThan(0);

        Option setOption = await client.Update(readModel, _context);
        setOption.StatusCode.IsOk().Should().BeTrue();

        Option<PrincipalKeyModel> compareOption = await client.Get(principalId, _context);
        readOption.IsOk().Should().BeTrue();

        (readModel == compareOption.Return()).Should().BeTrue();

        Option deleteOption = await client.Delete(principalId, _context);
        deleteOption.StatusCode.IsOk().Should().BeTrue();

        (await IsPrivateKeyExist(principalId)).StatusCode.IsError().Should().BeTrue();
    }

    private async Task<Option> IsPrivateKeyExist(PrincipalId principalId)
    {
        var exist = await _cluster.ServiceProvider.GetRequiredService<PrincipalPrivateKeyClient>().Get(principalId, _context);
        return exist.ToOptionStatus();
    }
}
