using Directory.sdk.Client;
using DirectoryApi.Application;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Spin.Common.Client;
using System.Net.Http;
using Toolbox.Application;

namespace Directory.Test.Application;

internal static class TestApplication
{
    private static HttpClient? _client;
    private static WebApplicationFactory<Program> _host = null!;
    private static object _lock = new object();

    public static HttpClient GetClient()
    {
        lock (_lock)
        {
            if (_client != null) return _client;

            _host = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder => builder.UseEnvironment("Test"));

            ApplicationOption option = _host.Services.GetRequiredService<ApplicationOption>();

            _client = _host.CreateClient();
            _client.DefaultRequestHeaders.Add(Constants.ApiKeyName, option.ApiKey);
            _client.DefaultRequestHeaders.Add(Constants.BypassCacheName, "true");

            return _client;
        }
    }

    public static PingClient GetPingClient() => new PingClient(GetClient(), _host.Services.GetRequiredService<ILoggerFactory>().CreateLogger<PingClient>());

    public static DirectoryClient GetDirectoryClient() => new DirectoryClient(GetClient(), _host.Services.GetRequiredService<ILoggerFactory>().CreateLogger<DirectoryClient>());
    
    public static IdentityClient GetIdentityClient() => new IdentityClient(GetClient(), _host.Services.GetRequiredService<ILoggerFactory>().CreateLogger<IdentityClient>());
}
