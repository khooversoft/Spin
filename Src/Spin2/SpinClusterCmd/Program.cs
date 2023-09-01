using System.CommandLine;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SpinClusterCmd;
using SpinClusterCmd.Application;
using Toolbox.Extensions;

try
{
    Console.WriteLine($"Spin Cluster CLI - Version {Assembly.GetExecutingAssembly().GetName().Version}");
    Console.WriteLine();

    CmdOption option = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .Build()
        .Bind<CmdOption>()
        .Verify();

    using var serviceProvider = BuildContainer(option);

    return await Run(serviceProvider, args);
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
        var rc = new RootCommand()
        {
            //service.GetRequiredService<DirectoryCommand>(),
        };

        return await rc.InvokeAsync(args);
    }
    finally
    {
        Console.WriteLine();
        Console.WriteLine("Completed");
    }
}

ServiceProvider BuildContainer(CmdOption option)
{
    var serviceCollection = new ServiceCollection()
        .AddApplication(option)
        .AddLogging(config => config.AddConsole());

    return serviceCollection.BuildServiceProvider();
}