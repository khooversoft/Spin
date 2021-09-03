using Identity.Application;
using Identity.sdk.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Spin.Common.Configuration;
using Toolbox.Application;
using Toolbox.Configuration;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Identity
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Option option = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddSpin("Identity")
                .AddCommandLine(args)
                .AddPropertyResolver()
                .Build()
                .Bind<Option>();

            //Option? option = new OptionBuilder()
            //    .SetArgs(args)
            //    .Build()
            //    .VerifyNotNull("Help is not supported");

            IHost host = CreateHostBuilder(args, option).Build();

            ILogger<Program> logger = host.Services.GetRequiredService<ILogger<Program>>();
            logger.LogConfigurations(option);

            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args, Option option) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices(services =>
                {
                    services.AddSingleton(option);
                    services.AddSingleton(option.ArtifactStore);
                    services.AddSingleton(option.Namespaces);
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
    }
}