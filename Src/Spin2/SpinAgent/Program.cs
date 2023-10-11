using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SpinAgent.Activities;
using SpinAgent.Application;
using SpinAgent.Services;
using SpinCluster.sdk.Application;
using Toolbox.CommandRouter;
using Toolbox.Extensions;

Console.WriteLine($"Spin Agent CLI - Version {Assembly.GetExecutingAssembly().GetName().Version}");
Console.WriteLine();

var state = await new CommandRouterBuilder()
    .SetArgs(args)
    .ConfigureAppConfiguration(config => config.AddEnvironmentVariables("SPIN_AGENT_"))
    .ConfigureAppConfiguration((config, service) => service.AddSingleton(config.Build().Bind<AgentOption>().Verify()))
    .AddStartup(x => x.GetRequiredService<AgentConfiguration>().Startup())
    .AddCommand<WorkMonitor>()
    .ConfigureService(x =>
    {
        x.AddSingleton<RunSmartC>();
        x.AddSingleton<AgentConfiguration>();
        x.AddSingleton<PackageManagement>();
        x.AddSpinClusterClients(LogLevel.Warning);
        x.AddSpinClusterAdminClients(LogLevel.Warning);
    })
    .Build()
    .Run();

return state;
