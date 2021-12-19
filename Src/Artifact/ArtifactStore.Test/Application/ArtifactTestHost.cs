using ArtifactStore.Application;
using ArtifactStore.sdk.Client;
using Directory.sdk;
using MessageNet.sdk;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Spin.Common.Client;
using System;
using System.Net.Http;
using System.Threading;
using Toolbox.Application;

namespace ArtifactStore.Test.Application
{
    internal class ArtifactTestHost
    {
        private const string _configStore = "d:\\SpinDisk";
        private const string _serviceId = "client";
        protected IHost? _host;
        protected HttpClient? _client;
        private readonly ILogger<ArtifactTestHost> _logger;
        private IServiceProvider? _serviceProvider = null;
        private ArtifactMessageClient? _artifactMessageClient;

        public ArtifactTestHost(ILogger<ArtifactTestHost> logger) => _logger = logger;

        public HttpClient Client => _client ?? throw new ArgumentNullException(nameof(Client));

        public T Resolve<T>() where T : class => _host?.Services.GetService<T>() ?? throw new InvalidOperationException($"Cannot find service {typeof(T).Name}");

        public PingClient GetPingClient() => new PingClient(Client, Resolve<ILoggerFactory>().CreateLogger<PingClient>());

        public ArtifactMessageClient ArtifactClient => _artifactMessageClient ??= GetServiceProvider().GetRequiredService<ArtifactMessageClient>();

        public IDirectoryNameService Dns => GetServiceProvider().GetRequiredService<IDirectoryNameService>();

        private IServiceProvider GetServiceProvider() => _serviceProvider ?? BuildService(_configStore, RunEnvironment.Dev.ToString());

        public ArtifactTestHost StartApiServer()
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
                });

            _host = host.Start();

            _client = _host.GetTestServer().CreateClient();
            _client.DefaultRequestHeaders.Add(Constants.ApiKeyName, option.ApiKey);
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

        private Option GetOption() => new Option
        {
            Environment = RunEnvironment.Dev,
            ApiKey = Guid.NewGuid().ToString(),
            HostUrl = null,
            ConfigStore = _configStore,
            HostServiceId = "artifact",
        };

        private IServiceProvider BuildService(string configStore, string environment)
        {
            IServiceProvider services = new ServiceCollection()
                .AddDirectory()
                .AddMessageHost()
                .AddSingleton<ArtifactMessageClient>()
                .AddLogging()
                .BuildServiceProvider();

            services.ConfigureDirectory(configStore, environment);
            services.StartMessageHost(_serviceId);


            return services;
        }
    }
}