using System.Collections.Generic;
using ArtifactStore.Application;
using Directory.sdk;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Toolbox.Application;
using Toolbox.Configuration;
using Toolbox.Extensions;
using Toolbox.Services;

namespace ArtifactStore
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Option option = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddDirectoryServices()
                .AddCommandLine(args)
                .AddPropertyResolver()
                .Build()
                .Bind<Option>()
                .Verify();

            IHost host = CreateHostBuilder(args, option).Build();

            LogConfigurations(option, host.Services.GetRequiredService<ILogger<Program>>());

            host.Run();
        }

        private static IHostBuilder CreateHostBuilder(string[] args, Option option) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices(services =>
                {
                    services.AddSingleton(option);
                })
                .ConfigureLogging(config =>
                {
                    config.AddConsole();
                    config.AddDebug();
                    config.AddFilter(x => true);
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    if (!option.HostUrl.IsEmpty()) webBuilder.UseUrls(option.HostUrl);

                    webBuilder.UseStartup<Startup>();
                });

        private static void LogConfigurations(Option option, ILogger logger)
        {
            ISecretFilter filter = new SecretFilter(option.ApiKey.ToEnumerable());
            logger.LogConfigurations(option, secretFilter: filter);
        }
    }
}