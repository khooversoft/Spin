using System.CommandLine;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SoftBank.sdk.SoftBank;
using SpinCluster.sdk.Actors.Contract;
using SpinCluster.sdk.Actors.Subscription;
using SpinCluster.sdk.Actors.Tenant;
using SpinCluster.sdk.Actors.User;
using SpinClusterCmd.Activities;
using SpinClusterCmd.Application;
using SpinClusterCmd.Commands;
using Toolbox.Extensions;
using Toolbox.Tools;

try
{
    Console.WriteLine($"Spin Cluster CLI - Version {Assembly.GetExecutingAssembly().GetName().Version}");
    Console.WriteLine();

    (string[] ConfigArgs, string[] CommandLineArgs) = ArgumentTool.Split(args);

    CmdOption option = new ConfigurationBuilder()
        .AddCommandLine(ConfigArgs)
        .AddEnvironmentVariables("SPIN_CMD_")
        .AddJsonFile("appsettings.json")
        .Build()
        .Bind<CmdOption>()
        .Verify();

    using var serviceProvider = BuildContainer(option);

    return await Run(serviceProvider, CommandLineArgs);
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
            service.GetRequiredService<LoadScenarioCommand>(),
            service.GetRequiredService<DumpContractCommand>(),
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
    var service = new ServiceCollection();

    service.AddSingleton(option);
    service.AddSingleton<LoadScenarioCommand>();
    service.AddSingleton<DumpContractCommand>();

    service.AddSingleton<LoadScenario>();
    service.AddSingleton<DumpContract>();

    service.AddHttpClient<SoftBankClient>(client => client.BaseAddress = new Uri(option.ClusterApiUri));
    service.AddHttpClient<UserClient>(client => client.BaseAddress = new Uri(option.ClusterApiUri));
    service.AddHttpClient<TenantClient>(client => client.BaseAddress = new Uri(option.ClusterApiUri));
    service.AddHttpClient<SubscriptionClient>(client => client.BaseAddress = new Uri(option.ClusterApiUri));
    service.AddHttpClient<ContractClient>(client => client.BaseAddress = new Uri(option.ClusterApiUri));

    service.AddLogging(config => config.AddConsole());

    return service.BuildServiceProvider();
}