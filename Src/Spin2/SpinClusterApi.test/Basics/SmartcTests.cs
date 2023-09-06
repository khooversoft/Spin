using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using SpinCluster.sdk.Actors.Agent;
using SpinCluster.sdk.Actors.Smartc;
using SpinCluster.sdk.Actors.Subscription;
using SpinCluster.sdk.Application;
using SpinClusterApi.test.Application;
using Toolbox.Types;

namespace SpinClusterApi.test.Basics;

public class SmartcTests : IClassFixture<ClusterApiFixture>
{
    private readonly ClusterApiFixture _cluster;
    private readonly ScopeContext _context = new ScopeContext(NullLogger.Instance);

    public SmartcTests(ClusterApiFixture fixture)
    {
        _cluster = fixture;
    }

    [Fact]
    public async Task LifecycleTest()
    {
        const string smartcId = "smartc:domain.com/package";
        SmartcClient client = _cluster.ServiceProvider.GetRequiredService<SmartcClient>();

        var existOption = await client.Get(smartcId, _context);
        if (existOption.IsOk()) await client.Delete(smartcId, _context);

        var model = new SmartcModel
        {
            SmartcId = smartcId,
            Enabled = true,
        };

        Option setResult = await client.Set(model, _context);
        setResult.IsOk().Should().BeTrue();

        var readOption = await client.Get(smartcId, _context);
        readOption.IsOk().Should().BeTrue();

        (model == readOption.Return()).Should().BeTrue();
    }
}
