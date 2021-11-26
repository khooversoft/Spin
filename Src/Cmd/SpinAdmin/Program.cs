using System;
using System.CommandLine;
using System.Reflection;
using System.Threading.Tasks;
using Directory.sdk;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SpinAdmin.Activities;
using SpinAdmin.Commands;

namespace SpinAdmin
{
    internal class Program
    {
        private readonly string _programTitle = $"Spin Administrator CLI - Version {Assembly.GetExecutingAssembly().GetName().Version}";
         
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

            try
            {
                using (ServiceProvider container = BuildContainer())
                {
                    var rc = new RootCommand()
                    {
                        container.GetRequiredService<EnvironmentCommand>(),
                        //container.GetRequiredService<QueueCommand>(),
                        //container.GetRequiredService<StorageCommand>(),
                        //container.GetRequiredService<SecretCommand>(),
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

            service.AddDirectory();

            //service.AddSingleton<ConfigurationStore>();

            //service.AddSingleton<StoreActivity>();
            //service.AddSingleton<EnvironmentActivity>();
            //service.AddSingleton<QueueActivity>();
            //service.AddSingleton<StorageActivity>();
            //service.AddSingleton<SecretActivity>();
            service.AddSingleton<PublishActivity>();

            service.AddSingleton<EnvironmentCommand>();
            //service.AddSingleton<QueueCommand>();
            //service.AddSingleton<StorageCommand>();
            //service.AddSingleton<SecretCommand>();

            return service.BuildServiceProvider();
        }
    }
}