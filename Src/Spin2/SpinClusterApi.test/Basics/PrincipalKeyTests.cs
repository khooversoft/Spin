﻿using System;
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

        var rsaKey = new RsaKeyPair("key");

        var model = new PrincipalKeyModel
        {
            KeyId = PrincipalKeyModel.CreateId(principalId),
            PrincipalId = principalId,
            Name = "test",
            Audience = "audience",
            PublicKey = rsaKey.PublicKey,
            AccountEnabled = true,
        };

        Option setOption = await client.Set(model, _context);
        setOption.StatusCode.IsOk().Should().BeTrue();

        Option<PrincipalKeyModel> readOption = await client.Get(principalId, _context);
        readOption.IsOk().Should().BeTrue();

        (model == readOption.Return()).Should().BeTrue();

        Option deleteOption = await client.Delete(principalId, _context);
        deleteOption.StatusCode.IsOk().Should().BeTrue();
    }
}
