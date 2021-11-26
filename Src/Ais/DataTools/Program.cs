using DataTools.Activities;
using DataTools.Application;
using DataTools.Commands;
using DataTools.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.CommandLine;
using System.Reflection;
using System.Runtime.CompilerServices;
using Toolbox.Extensions;

[assembly: InternalsVisibleTo("DataTools.test")]

namespace DataTools
{
    internal class Program
    {
        private readonly string _programTitle = $"Data Tools - Version {Assembly.GetExecutingAssembly().GetName().Version}";

        private static async Task<int> Main(string[] args)
        {
            try
            {
                return await new Program().Run(args);
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return 1;
            }
        }

        private async Task<int> Run(string[] args)
        {
            Console.WriteLine(_programTitle);
            Console.WriteLine();

            AppOption appOption = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build()
                .Bind<AppOption>();

            try
            {
                using (ServiceProvider container = BuildContainer(appOption))
                {
                    var rc = new RootCommand()
                    {
                        container.GetRequiredService<ParseCommand>(),
                    };

                    return await rc.InvokeAsync(args);
                }
            }
            finally
            {
                Console.WriteLine();
                Console.WriteLine("Completed");
            }
        }

        private ServiceProvider BuildContainer(AppOption appOption)
        {
            var service = new ServiceCollection();

            service.AddLogging(x =>
            {
                x.AddConsole();
                x.AddDebug();
            });

            service.AddSingleton(appOption);

            service.AddSingleton<ParseCommand>();
            service.AddSingleton<ParseActivity>();
            service.AddSingleton<FileReader>();
            service.AddSingleton<NmeaParser>();
            service.AddSingleton<FileWriter>();
            service.AddSingleton<AisStore>();
            service.AddSingleton<Counters>();
            service.AddSingleton<Tracking>();
            service.AddSingleton<ParserErrorLog>();


            return service.BuildServiceProvider();
        }
    }
}
