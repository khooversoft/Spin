using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using SpinClient.sdk;
using SpinCluster.abstraction;
using SpinClusterApi.test.Application;
using Toolbox.Extensions;
using Toolbox.Tools.Should;
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

        await client.Release(leaseKey, _context);

        var leaseData = new LeaseData
        {
            LeaseKey = leaseKey,
        };

        Option acquireResponse = await client.Acquire(leaseData, _context);
        acquireResponse.IsOk().Should().BeTrue();

        var leaseDataOption = await client.Get(leaseKey, _context);
        leaseDataOption.IsOk().Should().BeTrue();
        (leaseData == leaseDataOption.Return()).Should().BeTrue();

        var listResponse = await client.List(QueryParameter.Parse("**/*"), _context);
        listResponse.IsOk().Should().BeTrue();
        listResponse.Return().Count().Should().Be(1);
        (leaseData == listResponse.Return().First()).Should().BeTrue();

        Option releaseResponse = await client.Release(leaseKey, _context);
        releaseResponse.IsOk().Should().BeTrue();

        releaseResponse = await client.Release(leaseKey, _context);
        releaseResponse.IsError().Should().BeTrue();
    }

    [Fact]
    public async Task LeasesWithReference()
    {
        const string leaseKey1 = "key-1R";
        const string leaseKey2 = "key-2R";
        const string leaseKey3 = "key-3R";
        const string leaseKey4 = "key-4R";
        const string reference1 = "ref1";
        const string reference2 = "ref2";
        LeaseClient client = _cluster.ServiceProvider.GetRequiredService<LeaseClient>();

        await client.Release(leaseKey1, _context);
        await client.Release(leaseKey2, _context);
        await client.Release(leaseKey3, _context);
        await client.Release(leaseKey4, _context);

        var leaseDataItems = new[]
        {
            new LeaseData { LeaseKey = leaseKey1 },
            new LeaseData { LeaseKey = leaseKey2, Reference = reference1 },
            new LeaseData { LeaseKey = leaseKey3, Reference = reference1 },
            new LeaseData { LeaseKey = leaseKey4, Reference = reference2 },
        };

        foreach (var leaseData in leaseDataItems)
        {
            Option acquireResponse = await client.Acquire(leaseData, _context);
            acquireResponse.IsOk().Should().BeTrue();

            var leaseDataOption = await client.Get(leaseData.LeaseKey, _context);
            leaseDataOption.IsOk().Should().BeTrue();
            (leaseData == leaseDataOption.Return()).Should().BeTrue();
        }

        var listResponse = await client.List(QueryParameter.Parse("**/*"), _context);
        listResponse.IsOk().Should().BeTrue();
        listResponse.Return().Count().Should().Be(4);

        IReadOnlyList<LeaseData> list = listResponse.Return().OrderBy(x => x.LeaseKey).ToArray();
        (leaseDataItems[0] == list[0]).Should().BeTrue();
        (leaseDataItems[1] == list[1]).Should().BeTrue();
        (leaseDataItems[2] == list[2]).Should().BeTrue();
        (leaseDataItems[3] == list[3]).Should().BeTrue();

        var query = new QueryParameter { Filter = reference1 };
        listResponse = await client.List(query, _context);
        listResponse.IsOk().Should().BeTrue();
        listResponse.Return().Count().Should().Be(2);

        list = listResponse.Return().OrderBy(x => x.LeaseKey).ToArray();
        (leaseDataItems[1] == list[0]).Should().BeTrue();
        (leaseDataItems[2] == list[1]).Should().BeTrue();

        query = new QueryParameter { Filter = reference2 };
        listResponse = await client.List(query, _context);
        listResponse.IsOk().Should().BeTrue();
        listResponse.Return().Count().Should().Be(1);

        list = listResponse.Return().OrderBy(x => x.LeaseKey).ToArray();
        (leaseDataItems[3] == list[0]).Should().BeTrue();

        foreach (var leaseData in leaseDataItems)
        {
            Option releaseResponse = await client.Release(leaseData.LeaseKey, _context);
            releaseResponse.IsOk().Should().BeTrue();

            releaseResponse = await client.Release(leaseData.LeaseKey, _context);
            releaseResponse.IsError().Should().BeTrue();
        }
    }
}
