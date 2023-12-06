using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SoftBank.sdk.Application;
using SpinClient.sdk;
using SpinClusterCmd.Activities;
using SpinClusterCmd.Application;
using Toolbox.CommandRouter;
using Toolbox.Extensions;

Console.WriteLine($"Spin Cluster CLI - Version {Assembly.GetExecutingAssembly().GetName().Version}");
Console.WriteLine();

var state = await new CommandRouterBuilder()
    .SetArgs(args)
    .ConfigureAppConfiguration((config, service) =>
    {
        config.AddJsonFile("appsettings.json");
        config.AddEnvironmentVariables("SPIN_CLI_");
        service.AddSingleton(config.Build().Bind<CmdOption>().Verify());
    })
    .AddCommand<AgentRegistration>()
    .AddCommand<Configuration>()
    .AddCommand<Contract>()
    .AddCommand<Lease>()
    .AddCommand<LoadScenario>()
    .AddCommand<Schedule>()
    .AddCommand<ScheduleWork>()
    .AddCommand<SmartcPackage>()
    .AddCommand<SmartcRegistration>()
    .AddCommand<Subscription>()
    .AddCommand<Tenant>()
    .AddCommand<User>()
    .ConfigureService(x =>
    {
        x.AddSpinClusterClients(LogLevel.Warning);
        x.AddSpinClusterAdminClients(LogLevel.Warning);
        x.AddSoftBankClients(LogLevel.Warning);
    })
    .Build()
    .Run();

return state;
