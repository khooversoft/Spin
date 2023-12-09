using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;
using Toolbox.TestApi;

namespace Toolbox.Test.Rest;

public class TestApiHost : WebApplicationFactory<TestApiServer>
{
    protected override IHost CreateHost(IHostBuilder builder)
    {
        return base.CreateHost(builder);
    }
}

