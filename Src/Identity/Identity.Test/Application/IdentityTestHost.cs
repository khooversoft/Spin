using Identity.Application;
using Identity.sdk.Client;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Spin.Common.Application;
using Spin.Common.Client;
using Spin.Common.Configuration;
using System;
using System.Net.Http;
using System.Threading;
using Toolbox.Configuration;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Identity.Test.Application
{
    internal class IdentityTestHost
    {
        protected IHost? _host;
        protected HttpClient? _client;
        private readonly ILogger<IdentityTestHost> _logger;

        public IdentityTestHost(ILogger<IdentityTestHost> logger) => _logger = logger;

        public HttpClient Client => _client ?? throw new ArgumentNullException(nameof(Client));

        public T Resolve<T>() where T : class => _host?.Services.GetService<T>() ?? throw new InvalidOperationException($"Cannot find service {typeof(T).Name}");

        public IIdentityClient IdentityClient => new IdentityClient(Client, Resolve<ILoggerFactory>().CreateLogger<IdentityClient>());

        public PingClient GetPingClient() => new PingClient(Client, Resolve<ILoggerFactory>().CreateLogger<PingClient>());

        public IdentityTestHost StartApiServer()
        {
            Option option = GetOption();

            var host = new HostBuilder()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .UseTestServer()
                        .UseStartup<Startup>();
                })
                .ConfigureLogging(builder =>
                {
                    builder.AddDebug();
                    builder.AddFilter(x => true);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton(option);
                    services.AddSingleton(option.ArtifactStore);
                    services.AddSingleton(option.Namespaces);
                });

            _host = host.Start();

            _client = _host.GetTestServer().CreateClient();
            _client.DefaultRequestHeaders.Add(Constants.ApiKeyName, option.ArtifactStore.ApiKey);
            return this;
        }

        public void Shutdown()
        {
            _logger.LogInformation($"{nameof(Shutdown)} - Shutting down");

            Interlocked.Exchange(ref _client, null)?.Dispose();

            var host = Interlocked.Exchange(ref _host, null);
            if (host != null)
            {
                try
                {
                    host.StopAsync(TimeSpan.FromMinutes(10)).Wait();
                }
                catch { }
                finally
                {
                    host.Dispose();
                }
            }
        }

        private Option GetOption()
        {
            string[] args = new string[]
            {
                "Environment=dev",
            };

            return new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddSpin("Identity")
                .AddCommandLine(args)
                .AddPropertyResolver()
                .Build()
                .Bind<Option>();
        }

        //private class FakeHttpFactory : IHttpClientFactory
        //{
        //    private readonly HttpClient _httpClient;

        //    public FakeHttpFactory(HttpClient httpClient) => _httpClient = httpClient;

        //    public HttpClient CreateClient(string name) => _httpClient;
        //}
    }
}