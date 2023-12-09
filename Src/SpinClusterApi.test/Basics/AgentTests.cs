using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using SpinClient.sdk;
using SpinCluster.abstraction;
using SpinClusterApi.test.Application;
using Toolbox.Types;

namespace SpinClusterApi.test.Basics;

public class AgentTests : IClassFixture<ClusterApiFixture>
{
    private readonly ClusterApiFixture _cluster;
    private readonly ScopeContext _context = new ScopeContext(NullLogger.Instance);

    public AgentTests(ClusterApiFixture fixture)
    {
        _cluster = fixture;
    }

    [Fact]
    public async Task LifecycleTest()
    {
        const string agentId = "agent:agent1";
        AgentClient client = _cluster.ServiceProvider.GetRequiredService<AgentClient>();

        var existOption = await client.Get(agentId, _context);
        if (existOption.IsOk()) await client.Delete(agentId, _context);

        var agentModel = new AgentModel
        {
            AgentId = agentId,
            Enabled = true,
        };

        Option setResult = await client.Set(agentModel, _context);
        setResult.IsOk().Should().BeTrue(setResult.Error);

        var readOption = await client.Get(agentId, _context);
        readOption.IsOk().Should().BeTrue();

        (agentModel == readOption.Return()).Should().BeTrue();
    }
}
