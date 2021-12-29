using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using DirectoryApi;
using System.Threading;
using System.Net.Http;
using Toolbox.Application;
using DirectoryApi.Application;
using Microsoft.Extensions.DependencyInjection;
using Spin.Common.Client;
using System;
using Microsoft.AspNetCore.Hosting;
using Directory.sdk.Client;

namespace Directory.Test.Application
{
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
    }
}