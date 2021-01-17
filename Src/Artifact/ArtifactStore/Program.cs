using ArtifactStore.Application;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Toolbox.Application;
using Toolbox.Logging;
using Toolbox.Tools;

namespace ArtifactStore
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
                    services.AddSingleton(option.Store);
                })
                .ConfigureLogging(config =>
                {
                    config.AddConsole();
                    config.AddDebug();
                    config.AddFilter(x => true);

                    LoggerBuffer loggingBuffer = new LoggerBuffer();
                    config.Services.AddSingleton<LoggerBuffer>(loggingBuffer);

                    config.AddProvider(new TargetBlockLoggerProvider(loggingBuffer.TargetBlock));
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}