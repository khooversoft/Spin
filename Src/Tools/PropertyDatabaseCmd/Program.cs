using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PropertyDatabaseCmd.Activities;
using PropertyDatabaseCmd.Application;
using System;
using System.CommandLine;
using System.Reflection;
using System.Threading.Tasks;

namespace PropertyDatabaseCmd
{
    class Program
    {
        private readonly string _programTitle = $"Property Database CLI - Version {Assembly.GetExecutingAssembly().GetName().Version}";

        static async Task<int> Main(string[] args)
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

            try
            {
                using (ServiceProvider container = BuildContainer())
                {
                    var rc = new RootCommand()
                {
                    container.GetRequiredService<ListCommand>(),
                    container.GetRequiredService<GetCommand>(),
                    container.GetRequiredService<DeleteCommand>(),
                    container.GetRequiredService<SetCommand>(),
                    container.GetRequiredService<PublishCommand>(),
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

        private ServiceProvider BuildContainer()
        {
            var service = new ServiceCollection();

            service.AddLogging(x =>
            {
                x.AddConsole();
                x.AddDebug();
            });

            service.AddSingleton<ListActivity>();
            service.AddSingleton<GetActivity>();
            service.AddSingleton<DeleteActivity>();
            service.AddSingleton<SetActivity>();
            service.AddSingleton<PublishActivity>();

            service.AddSingleton<DeleteCommand>();
            service.AddSingleton<ListCommand>();
            service.AddSingleton<GetCommand>();
            service.AddSingleton<SetCommand>();
            service.AddSingleton<PublishCommand>();

            return service.BuildServiceProvider();
        }
    }
}
