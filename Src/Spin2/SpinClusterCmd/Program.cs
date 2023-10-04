using System.CommandLine;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SoftBank.sdk.SoftBank;
using SpinCluster.sdk.Actors.Agent;
using SpinCluster.sdk.Actors.Configuration;
using SpinCluster.sdk.Actors.Contract;
using SpinCluster.sdk.Actors.Scheduler;
using SpinCluster.sdk.Actors.Smartc;
using SpinCluster.sdk.Actors.Storage;
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

    Console.WriteLine($"Configuration: {option}");

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
            service.GetRequiredService<AgentCommand>(),
            service.GetRequiredService<DumpContractCommand>(),
            service.GetRequiredService<LoadScenarioCommand>(),
            service.GetRequiredService<ScheduleCommand>(),
            service.GetRequiredService<SmartcCommand>(),
            service.GetRequiredService<PackageCommand>(),
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

    service.AddSingleton<AgentCommand>();
    service.AddSingleton<ConfigCommand>();
    service.AddSingleton<DumpContractCommand>();
    service.AddSingleton<LeaseCommand>();
    service.AddSingleton<LoadScenarioCommand>();
    service.AddSingleton<PackageCommand>();
    service.AddSingleton<ScheduleCommand>();
    service.AddSingleton<SmartcCommand>();
    service.AddSingleton<SubscriptionCommand>();
    service.AddSingleton<TenantCommand>();

    service.AddSingleton<AgentRegistration>();
    service.AddSingleton<Configuration>();
    service.AddSingleton<DumpContract>();
    service.AddSingleton<Schedule>();
    service.AddSingleton<Lease>();
    service.AddSingleton<LoadScenario>();
    service.AddSingleton<SmartcPackage>();
    service.AddSingleton<SmartcRegistration>();
    service.AddSingleton<Subscription>();
    service.AddSingleton<Tenant>();

    service.AddHttpClient<AgentClient>(client => client.BaseAddress = new Uri(option.ClusterApiUri));
    service.AddHttpClient<ConfigClient>(client => client.BaseAddress = new Uri(option.ClusterApiUri));
    service.AddHttpClient<ContractClient>(client => client.BaseAddress = new Uri(option.ClusterApiUri));
    service.AddHttpClient<SoftBankClient>(client => client.BaseAddress = new Uri(option.ClusterApiUri));
    service.AddHttpClient<SmartcClient>(client => client.BaseAddress = new Uri(option.ClusterApiUri));
    service.AddHttpClient<SubscriptionClient>(client => client.BaseAddress = new Uri(option.ClusterApiUri));
    service.AddHttpClient<TenantClient>(client => client.BaseAddress = new Uri(option.ClusterApiUri));
    service.AddHttpClient<SchedulerClient>(client => client.BaseAddress = new Uri(option.ClusterApiUri));
    service.AddHttpClient<StorageClient>(client => client.BaseAddress = new Uri(option.ClusterApiUri));
    service.AddHttpClient<UserClient>(client => client.BaseAddress = new Uri(option.ClusterApiUri));

    service.AddLogging(config =>
    {
        config.AddConsole();
        config.AddFilter("System.Net.Http.HttpClient", LogLevel.Warning);
        config.AddFilter("SpinCluster.sdk.Actors.Agent.AgentClient", LogLevel.Warning);
        config.AddFilter("SpinCluster.sdk.Actors.Tenant.TenantClient", LogLevel.Warning);
        config.AddFilter("SpinCluster.sdk.Actors.Smartc.SmartcClient", LogLevel.Warning);
        config.AddFilter("SpinCluster.sdk.Actors.User.UserClient", LogLevel.Warning);
        config.AddFilter("SpinCluster.sdk.Actors.Subscription.SubscriptionClient", LogLevel.Warning);
        config.AddFilter("SoftBank.sdk.SoftBank.SoftBankClient", LogLevel.Warning);
        config.AddFilter("SpinCluster.sdk.Actors.Smartc.ScheduleClient", LogLevel.Warning);
        config.AddFilter("SpinCluster.sdk.Actors.Configuration.ConfigClient", LogLevel.Warning);
    });

    return service.BuildServiceProvider();
}