using System.Collections.Generic;
using System.Linq;
using ArtifactStore.Application;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Spin.Common.Configuration;
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
                .AddSpin("ArtifactStore")
                .AddCommandLine(args)
                .AddPropertyResolver()
                .Build()
                .Bind<Option>();

            //Option option = new OptionBuilder()
            //    .SetArgs(args)
            //    .Build()
            //    .VerifyNotNull("Help is not supported");

            IHost host = CreateHostBuilder(args, option).Build();

            LogConfigurations(option, host.Services.GetRequiredService<ILogger<Program>>());

            host.Run();
        }

        private static IHostBuilder CreateHostBuilder(string[] args, Option option) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices(services =>
                {
                    services.AddSingleton(option);
                    services.AddSingleton(option.Stores);
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
            var list = new List<string>();
            list.Add(option.ApiKey);
            list.AddRange(option.Stores.Select(x => x.Store.AccountKey));

            ISecretFilter filter = new SecretFilter(list);

            logger.LogConfigurations(option, filter);
        }
    }
}