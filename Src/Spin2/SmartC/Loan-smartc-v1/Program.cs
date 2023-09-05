using System.CommandLine;
using System.Reflection;
using Loan_smartc_v1.Application;
using Loan_smartc_v1.Commands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Tools;

try
{
    Console.WriteLine($"Loan-smartc-v1 CLI - Version {Assembly.GetExecutingAssembly().GetName().Version}");
    Console.WriteLine();

    (string[] ConfigArgs, string[] CommandLineArgs) = ArgumentTool.Split(args);

    AppOption option = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", true)
        .AddEnvironmentVariables("SPIN_SMARTC_")
        .AddCommandLine(ConfigArgs)
        .Build()
        .Bind<AppOption>()
        .Verify();

    using var serviceProvider = BuildContainer();

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

ServiceProvider BuildContainer()
{
    var serviceCollection = new ServiceCollection();

    serviceCollection.AddLogging(config => config.AddConsole());
    serviceCollection.AddSingleton<RunCommand>();
    serviceCollection.AddSingleton<AbortSignal>();

    return serviceCollection.BuildServiceProvider();
}

