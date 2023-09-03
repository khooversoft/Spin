using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SpinAgent.Activities;
using SpinAgent.Application;
using SpinAgent.Commands;
using System.CommandLine;
using System.Reflection;
using Toolbox.Extensions;
using Toolbox.Tools.Local;

try
{
    Console.WriteLine($"Spin Agent CLI - Version {Assembly.GetExecutingAssembly().GetName().Version}");
    Console.WriteLine();

    (string[] configArgs, string[] cmdArgs) = args
        .Select(x => (config: x.Split('=').Length > 1 ? 0 : 1, arg: x))
        .Func(x => (
            configArgs: x.Where(y => y.config == 0).Select(x => x.arg).ToArray(),
            cmdArgs: x.Where(y => y.config == 1).Select(x => x.arg).ToArray())
            );

    AgentOption option = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", true)
        .AddEnvironmentVariables("SPIN_AGENT_")
        .AddCommandLine(configArgs)
        .Build()
        .Bind<AgentOption>()
        .Verify();

    using var serviceProvider = BuildContainer(option);

    int statusCode = await Run(serviceProvider, args);
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
    var serviceCollection = new ServiceCollection();

    serviceCollection.AddLogging(config => config.AddConsole());

    serviceCollection.AddSingleton(option);
    serviceCollection.AddSingleton<AbortSignal>();
    serviceCollection.AddSingleton<RunCommand>();

    serviceCollection.AddSingleton<CommandMonitor>();
    serviceCollection.AddSingleton<RunSmartC>();

    return serviceCollection.BuildServiceProvider();
}