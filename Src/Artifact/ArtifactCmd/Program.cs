using ArtifactCmd.Activities;
using ArtifactCmd.Application;
using ArtifactStore.sdk.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Spin.Common.Application;
using System;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Toolbox.Extensions;

namespace ArtifactCmd
{
    class Program
    {
        private const int _ok = 0;
        private const int _error = 1;
        private readonly string _programTitle = $"Artifact Server CLI - Version {Assembly.GetExecutingAssembly().GetName().Version}";

        static async Task<int> Main(string[] args)
        {
            try
            {
                return await new Program().Run(args);
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine("*Error: " + ex.Message);
                DisplayStartDetails(args);
                DisplayHelp();
            }
            catch (TaskCanceledException) { }
            catch (Exception ex)
            {
                Console.WriteLine("Unhanded exception: " + ex.ToString());
            }

            return _error;
        }

        private static void DisplayStartDetails(string[] args) => Console.WriteLine($"Arguments: {string.Join(", ", args)}");

        private static void DisplayHelp() => OptionExtensions.GetHelp()
            .Prepend(string.Empty)
            .Append(string.Empty)
            .ForEach(x => Console.WriteLine(x));

        private async Task<int> Run(string[] args)
        {
            Console.WriteLine(_programTitle);
            Console.WriteLine();

            Option? option = new OptionBuilder()
                .SetArgs(args)
                .SetEnableHelp()
                .Build();

            if (option == null)
            {
                DisplayHelp();
                return _ok;
            }

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            using (ServiceProvider container = BuildContainer(option))
            {
                ILogger<Program> logger = container
                    .GetRequiredService<ILoggerFactory>().CreateLogger<Program>();

                option.LogConfigurations(logger);

                Console.CancelKeyPress += (object? _, ConsoleCancelEventArgs e) =>
                {
                    e.Cancel = true;
                    cancellationTokenSource.Cancel();
                    Console.WriteLine("Canceling...");
                };

                var executeQueue = new ActionBlock<Func<Task>>(async x => await x());

                if (option.Get) await executeQueue.SendAsync(() => container.GetRequiredService<GetActivity>().Get(cancellationTokenSource.Token));
                if (option.Set) await executeQueue.SendAsync(() => container.GetRequiredService<SetActivity>().Set(cancellationTokenSource.Token));
                if (option.Delete) await executeQueue.SendAsync(() => container.GetRequiredService<DeleteActivity>().Delete(cancellationTokenSource.Token));
                if (option.List) await executeQueue.SendAsync(() => container.GetRequiredService<ListActivity>().List(cancellationTokenSource.Token));

                executeQueue.Complete();
                await executeQueue.Completion;
            }

            Console.WriteLine();
            Console.WriteLine("Completed");
            return _ok;
        }

        private ServiceProvider BuildContainer(Option option)
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

            service.AddHttpClient<IArtifactClient, ArtifactClient>(http =>
            {
                http.BaseAddress = new Uri(option.ArtifactUrl);
                http.DefaultRequestHeaders.Add(Constants.ApiKeyName, option.ApiKey);
            });

            return service.BuildServiceProvider();
        }
    }
}
