using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using SpinCluster.sdk.Actors.Configuration;
using SpinCluster.sdk.Actors.Domain;
using SpinClusterApi.test.Application;
using Toolbox.Types;

namespace SpinClusterApi.test.Basics;

[Collection("config-Test")]
public class DomainTests : IClassFixture<ClusterApiFixture>
{
    private readonly ClusterApiFixture _cluster;
    private readonly ScopeContext _context = new ScopeContext(NullLogger.Instance);

    public DomainTests(ClusterApiFixture fixture)
    {
        _cluster = fixture;
    }

    [Fact]
    public async Task LifecycleTest()
    {
        ConfigClient configClient = _cluster.ServiceProvider.GetRequiredService<ConfigClient>();
        DomainClient domainClient = _cluster.ServiceProvider.GetRequiredService<DomainClient>();

        string configId = "spinconfig:validDomain";
        string domain = "outlook.com";
        string addDomain = "addDomain.com";

        var existOption = await configClient.Exist(configId, _context);
        (existOption.IsOk() || existOption.IsNotFound()).Should().BeTrue();
        if (existOption.IsOk()) await configClient.Delete(configId, _context);

        var existCheckOption = await domainClient.GetDetails(domain, _context);
        existCheckOption.IsError().Should().BeTrue();

        var model = new ConfigModel
        {
            ConfigId = configId,
            Properties = new[]
            {
                new KeyValuePair<string, string>("outlook.com", "true"),
                new KeyValuePair<string, string>("gmail.com", "true"),
            }.ToDictionary(x => x.Key, x => x.Value),
        };

        var setOption = await configClient.Set(model, _context);
        setOption.IsOk().Should().BeTrue(setOption.ToString());

        Option<DomainDetail> domainOption = await domainClient.GetDetails(domain, _context);
        domainOption.IsOk().Should().BeTrue(domainOption.ToString());

        DomainDetail detail = domainOption.Return();
        detail.Should().NotBeNull();
        detail.Domain.Should().Be(domain);
        detail.TenantId.Should().BeNull();

        var addResult = await domainClient.SetExternalDomain(addDomain, _context);
        addResult.IsOk().Should().BeTrue();

        var configModelOption = await configClient.Get(configId, _context);
        configModelOption.IsOk().Should().BeTrue();

        ConfigModel configModel = configModelOption.Return();
        configModel.ConfigId.Should().Be(configId);
        configModel.Properties.Count.Should().Be(3);
        configModel.Properties.ContainsKey(addDomain).Should().BeTrue();

        var removeResult = await domainClient.RemoveExternalDomain(addDomain, _context);
        removeResult.IsOk().Should().BeTrue();

        configModelOption = await configClient.Get(configId, _context);
        configModelOption.IsOk().Should().BeTrue();

        configModel = configModelOption.Return();
        configModel.ConfigId.Should().Be(configId);
        configModel.Properties.Count.Should().Be(2);

        Option deleteOption = await configClient.Delete(configId, _context);
        deleteOption.IsOk().Should().BeTrue(deleteOption.ToString());
    }
}
