using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using SpinCluster.sdk.Actors.Domain;
using SpinClusterApi.test.Application;
using SpinTestTools.sdk.ObjectBuilder;
using Toolbox.Types;

namespace SpinClusterApi.test.Basics;

[Collection("config-Test")]
public class DomainTestWithTenantLookup : IClassFixture<ClusterApiFixture>
{

    private const string _setup = """
        {
           "Configs": [
              {
                "ConfigId": "spinconfig:validDomain",
                "Properties": {
                    "outlook.com": "true",
                    "gmail.com" : "true"
                }
              }
            ],
           "Subscriptions": [
              {
                "SubscriptionId": "subscription:domainTest",
                "Name": "Rental Management",
                "ContactName": "user1@rental.com",
                "Email": "admin@rental.com"
              }
            ],
           "Tenants": [
              {
                "TenantId": "tenant:domainTest.com",
                "Subscription": "Domain Test",
                "Domain": "domainTest.com",
                "SubscriptionId": "subscription:domainTest",
                "ContactName": "Admin",
                "Email": "admin@domainTest.com"
              }        
            ]
        }
        """;

    private readonly ClusterApiFixture _cluster;
    private readonly ScopeContext _context = new ScopeContext(NullLogger.Instance);

    public DomainTestWithTenantLookup(ClusterApiFixture fixture)
    {
        _cluster = fixture;
    }

    [Fact]
    public async Task LifecycleTest()
    {
        DomainClient domainClient = _cluster.ServiceProvider.GetRequiredService<DomainClient>();

        var builder = new TestObjectBuilder()
            .SetJson(_setup)
            .SetService(_cluster.ServiceProvider)
            .AddStandard();

        var result = await builder.Build(_context);
        result.IsOk().Should().BeTrue();

        string outlookDomain = "outlook.com";
        string tenantdomain = "domainTest.com";

        var existCheckOption = await domainClient.GetDetails(outlookDomain, _context);
        existCheckOption.IsOk().Should().BeTrue();

        Option<DomainDetail> domainOption = await domainClient.GetDetails(tenantdomain, _context);
        domainOption.IsOk().Should().BeTrue();

        DomainDetail detail = domainOption.Return();
        detail.Should().NotBeNull();
        detail.Domain.Should().Be(tenantdomain);
        detail.TenantId.Should().Be($"tenant:{tenantdomain}");
        var deleteOption = await builder.DeleteAll(_context);
        deleteOption.IsOk().Should().BeTrue();
    }
}
