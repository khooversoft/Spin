using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using SpinClient.sdk;
using SpinCluster.abstraction;
using SpinClusterApi.test.Application;
using Toolbox.Types;

namespace SpinClusterApi.test.Basics;

[Collection("config-Test")]
public class ConfigTests : IClassFixture<ClusterApiFixture>
{
    private readonly ClusterApiFixture _cluster;
    private readonly ScopeContext _context = new ScopeContext(NullLogger.Instance);

    public ConfigTests(ClusterApiFixture fixture)
    {
        _cluster = fixture;
    }

    [Fact]
    public async Task LifecycleTest()
    {
        ConfigClient client = _cluster.ServiceProvider.GetRequiredService<ConfigClient>();

        string configId = "spinconfig:test-config-1";

        var existOption = await client.Exist(configId, _context);
        (existOption.IsOk() || existOption.IsNotFound()).Should().BeTrue();
        if (existOption.IsOk()) await client.Delete(configId, _context);

        var model = new ConfigModel
        {
            ConfigId = configId,
            Properties = new[]
            {
                new KeyValuePair<string, string>("property1", "value1"),
                new KeyValuePair<string, string>("property2", "value2"),
            }.ToDictionary(x => x.Key, x => x.Value),
        };

        var setOption = await client.Set(model, _context);
        setOption.IsOk().Should().BeTrue(setOption.ToString());

        var readOption = await client.Get(configId, _context);
        readOption.IsOk().Should().BeTrue(readOption.ToString());

        (model == readOption.Return()).Should().BeTrue();

        Option deleteOption = await client.Delete(configId, _context);
        deleteOption.IsOk().Should().BeTrue(deleteOption.ToString());
    }
}
