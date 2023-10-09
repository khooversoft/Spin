using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SoftBank.sdk.Application;
using SpinCluster.sdk.Application;
using SpinClusterCmd.Activities;
using SpinClusterCmd.Application;
using Toolbox.CommandRouter;
using Toolbox.Extensions;

Console.WriteLine($"Spin Cluster CLI - Version {Assembly.GetExecutingAssembly().GetName().Version}");
Console.WriteLine();

var state = await new CommandRouterBuilder()
    .SetArgs(args)
    .ConfigureAppConfiguration(config => config.AddEnvironmentVariables("SPIN_CLI_"))
    .ConfigureAppConfiguration((config, service) => service.AddSingleton(config.Build().Bind<CmdOption>().Verify()))
    .AddCommand<AgentRegistration>()
    .AddCommand<Configuration>()
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
        x.AddSpinClusterClients();
        x.AddSpinClusterAdminClients();
        x.AddSoftBankClients();
    })
    .Build()
    .Run();

return state;
