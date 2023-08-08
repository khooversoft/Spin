using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Toolbox.Tools;

namespace SpinClusterApi.test.Application;

public class ClusterApiFixture : IDisposable
{
    private HttpClient? _client;
    private WebApplicationFactory<Program>? _host;

    public ClusterApiFixture()
    {
        ILogger logger = LoggerFactory.Create(builder =>
        {
            builder.AddDebug();
        }).CreateLogger<Program>();

        _host = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Test");
            });
    }

    public void Dispose() => Interlocked.Exchange(ref _host, null)?.Dispose();

    public HttpClient CreateClient() => _host.NotNull().CreateClient();
}
