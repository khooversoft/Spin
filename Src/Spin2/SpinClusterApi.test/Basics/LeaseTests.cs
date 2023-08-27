using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using SpinCluster.sdk.Actors.Contract;
using SpinCluster.sdk.Actors.Lease;
using SpinCluster.sdk.Actors.Tenant;
using SpinCluster.sdk.Application;
using SpinClusterApi.test.Application;
using Toolbox.Extensions;
using Toolbox.Types;

namespace SpinClusterApi.test.Basics;

public class LeaseTests : IClassFixture<ClusterApiFixture>
{
    private readonly ClusterApiFixture _cluster;
    private readonly ScopeContext _context = new ScopeContext(NullLogger.Instance);

    public LeaseTests(ClusterApiFixture fixture)
    {
        _cluster = fixture;
    }

    //[Fact(Skip = "server")]
    [Fact]
    public async Task LifecycleTest()
    {
        const string leaseKey = "key1";
        LeaseClient client = _cluster.ServiceProvider.GetRequiredService<LeaseClient>();

        ResourceId leaseId = IdTool.CreateLeaseId("domain10.com", "softbank/account1");

        var leaseCreate = new LeaseCreate(leaseKey);
        Option<LeaseData> acquireResponse = await client.Acquire(leaseId, leaseCreate, _context);
        acquireResponse.IsOk().Should().BeTrue();

        LeaseData leaseData = acquireResponse.Return();
        leaseData.LeaseId.Should().NotBeNullOrWhiteSpace();
        leaseData.AccountId.Should().Be(leaseId);
        leaseData.LeaseKey.Should().Be(leaseKey);
        leaseData.Payload.Should().BeNull();

        var isValidResponse = await client.IsValid(leaseId, leaseKey, _context);
        isValidResponse.IsOk().Should().BeTrue();

        var listResponse = await client.List(leaseId, _context);
        listResponse.IsOk().Should().BeTrue();
        listResponse.Return().Count().Should().Be(1);
        (leaseData == listResponse.Return().First()).Should().BeTrue();

        Option releaseResponse = await client.Release(leaseId, leaseKey, _context);
        releaseResponse.IsOk().Should().BeTrue();
    }
}
