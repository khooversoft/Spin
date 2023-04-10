using ArtifactCmd.Activities;
using ArtifactCmd.Application;
using Artifact.sdk.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.CommandLine;
using System.Reflection;
using System.Threading.Tasks;
using Toolbox.Application;

namespace ArtifactCmd
{
    class Program
    {
        private readonly string _programTitle = $"Artifact Server CLI - Version {Assembly.GetExecutingAssembly().GetName().Version}";

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

            ConfigOption option = new OptionBuilder().Build(args);

            try
            {
                using (ServiceProvider container = BuildContainer(option))
                {
                    var rc = new RootCommand()
                    {
                        new Option<RunEnvironment>(new[] { "--environment", "-e" }, "Specify environment to use"),

                        container.GetRequiredService<ListCommand>(),
                        container.GetRequiredService<GetCommand>(),
                        container.GetRequiredService<DeleteCommand>(),
                        container.GetRequiredService<SetCommand>(),
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

        private ServiceProvider BuildContainer(ConfigOption option)
        {
            var service = new ServiceCollection();

            service.AddLogging(x =>
            {
                x.AddConsole();
                x.AddDebug();
            });

            service.AddSingleton(option);

            service.AddSingleton<ListActivity>();
            service.AddSingleton<GetActivity>();
            service.AddSingleton<DeleteActivity>();
            service.AddSingleton<SetActivity>();

            service.AddSingleton<DeleteCommand>();
            service.AddSingleton<ListCommand>();
            service.AddSingleton<GetCommand>();
            service.AddSingleton<SetCommand>();

            //service.AddHttpClient<IArtifactClient, ArtifactClient>(http =>
            //{
            //    http.BaseAddress = new Uri(option.ArtifactUrl);
            //    http.DefaultRequestHeaders.Add(Constants.ApiKeyName, option.ApiKey);
            //});

            return service.BuildServiceProvider();
        }
    }
}
