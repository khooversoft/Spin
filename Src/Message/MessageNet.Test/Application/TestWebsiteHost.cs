using MessageNet.Application;
using MessageNet.sdk.Client;
using MessageNet.sdk.Endpoint;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Spin.Common.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Tools;

namespace MessageNet.Test.Application
{
    internal class TestWebsiteHost
    {
        protected IHost? _host;
        protected HttpClient? _client;
        private readonly ILogger<TestWebsiteHost> _logger;

        public TestWebsiteHost(ILogger<TestWebsiteHost> logger) => _logger = logger;

        public HttpClient Client => _client ?? throw new ArgumentNullException(nameof(Client));

        public T Resolve<T>() where T : class => _host?.Services.GetService<T>() ?? throw new InvalidOperationException($"Cannot find service {typeof(T).Name}");

        public PingClient GetPingClient() => new PingClient(Client, Resolve<ILoggerFactory>().CreateLogger<PingClient>());

        public RegisterClient GetRegisterClient() => new RegisterClient(Client, Resolve<ILoggerFactory>().CreateLogger<RegisterClient>());
        
        public MessageClient GetMessageClient() => new MessageClient(Client, Resolve<ILoggerFactory>().CreateLogger<MessageClient>());

        public TestWebsiteHost StartApiServer()
        {
            Option option = new TestOptionBuilder()
                .Build()
                .VerifyNotNull("Build option failed");

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

                    var factory = new TestCallbackReceiver();
                    services.AddSingleton<ICallbackFactory>(factory);
                    services.AddSingleton<TestCallbackReceiver>(factory);
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
    }
}
