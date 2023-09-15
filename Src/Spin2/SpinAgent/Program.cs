using System.CommandLine;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SpinAgent.Activities;
using SpinAgent.Application;
using SpinAgent.Commands;
using SpinCluster.sdk.Actors.Smartc;
using SpinCluster.sdk.Actors.Storage;
using Toolbox.Extensions;
using Toolbox.Tools;

try
{
    Console.WriteLine($"Spin Agent CLI - Version {Assembly.GetExecutingAssembly().GetName().Version}");
    Console.WriteLine();

    (string[] ConfigArgs, string[] CommandLineArgs) = ArgumentTool.Split(args);

    AgentOption option = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", true)
        .AddEnvironmentVariables("SPIN_AGENT_")
        .AddCommandLine(ConfigArgs)
        .Build()
        .Bind<AgentOption>()
        .Verify();

    using var serviceProvider = BuildContainer(option);

    int statusCode = await Run(serviceProvider, CommandLineArgs);
    Console.WriteLine($"StatusCode: {statusCode}");

    return statusCode;
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    return 1;
}

async Task<int> Run(IServiceProvider service, string[] args)
{
    try
    {
        AbortSignal abortSignal = service.GetRequiredService<AbortSignal>();
        abortSignal.StartTracking();

        var rc = new RootCommand()
        {
            service.GetRequiredService<RunCommand>(),
        };

        await rc.InvokeAsync(args);

        return abortSignal.GetToken().IsCancellationRequested ? 1 : 0;
    }
    finally
    {
        Console.WriteLine();
        Console.WriteLine("Completed");
    }
}

ServiceProvider BuildContainer(AgentOption option)
{
    var service = new ServiceCollection();

    service.AddLogging(config => config.AddConsole());

    service.AddSingleton(option);
    service.AddSingleton<AbortSignal>();
    service.AddSingleton<RunCommand>();

    service.AddSingleton<WorkMonitor>();
    service.AddSingleton<RunSmartC>();

    service.AddHttpClient<ScheduleClient>(client => client.BaseAddress = new Uri(option.ClusterApiUri));
    service.AddHttpClient<StorageClient>(client => client.BaseAddress = new Uri(option.ClusterApiUri));
    service.AddHttpClient<SmartcClient>(client => client.BaseAddress = new Uri(option.ClusterApiUri));

    service.AddLogging(config =>
    {
        config.AddConsole();
        config.AddFilter("System.Net.Http.HttpClient", LogLevel.Warning);
        config.AddFilter("SpinCluster.sdk.Actors.Smartc.ScheduleClient", LogLevel.Warning);
        config.AddFilter("SpinCluster.sdk.Actors.Storage.StorageClient", LogLevel.Warning);
        config.AddFilter("SpinCluster.sdk.Actors.Smartc.SmartcClient", LogLevel.Warning);
    });


    return service.BuildServiceProvider();
}