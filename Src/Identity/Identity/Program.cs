using Identity.Application;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Toolbox.Application;
using Toolbox.Tools;

namespace Identity
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Option? option = new OptionBuilder()
                .SetArgs(args)
                .Build()
                .VerifyNotNull("Help is not supported");

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
                })
                .ConfigureLogging(config =>
                {
                    config.AddConsole();
                    config.AddDebug();
                    config.AddFilter(x => true);
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
