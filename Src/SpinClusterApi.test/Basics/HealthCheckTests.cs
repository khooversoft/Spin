using System.Net;
using Microsoft.Extensions.Logging.Abstractions;
using SpinClusterApi.test.Application;
using Toolbox.Rest;
using Toolbox.Tools.Should;
using Toolbox.Types;

namespace SpinClusterApi.test.Basics;

public class HealthCheckTests : IClassFixture<ClusterApiFixture>
{
    private readonly ClusterApiFixture _cluster;
    private readonly ScopeContext _context = new ScopeContext(NullLogger.Instance);

    public HealthCheckTests(ClusterApiFixture fixture)
    {
        _cluster = fixture;
    }

    //[Fact(Skip = "server")]
    [Fact]
    public async Task TestHealthCheckApi()
    {
        RestResponse result = await new RestClient(_cluster.GetClient())
            .SetPath("_health")
            .GetAsync(_context);

        result.Should().NotBeNull();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Content.Should().Be("Healthy");
    }
}
