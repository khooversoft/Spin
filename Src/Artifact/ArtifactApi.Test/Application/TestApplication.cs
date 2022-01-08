﻿using Microsoft.Extensions.Logging;
using Spin.Common.Client;
using System.Net.Http;
using System.Threading;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Artifact.Application;
using Toolbox.Application;
using Artifact.sdk.Client;

namespace Artifact.Test.Application
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

        public static ArtifactClient GetArtifactClient() => new ArtifactClient(GetClient(), _host.Services.GetRequiredService<ILoggerFactory>().CreateLogger<ArtifactClient>());
    }
}
