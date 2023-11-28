using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Application;
using Toolbox.CommandRouter;
using Toolbox.Extensions;
using SpinClient.sdk;

namespace SpinAgent.sdk;

public class SpinAgentHost
{
    public CommandRouterBuilder Create(params string[] args) => new CommandRouterBuilder()
        .SetArgs(args)
        .ConfigureAppConfiguration(config => config.AddEnvironmentVariables("SPIN_AGENT_"))
        .ConfigureAppConfiguration((config, service) => service.AddSingleton(config.Build().Bind<AgentOption>().Verify()))
        .AddStartup(x => x.GetRequiredService<AgentConfiguration>().Startup())
        .AddCommand<LookForWorkActivity>()
        .ConfigureService(x =>
        {
            x.AddSingleton<IRunSmartc, RunSmartC>();
            x.AddSingleton<AgentConfiguration>();
            x.AddSingleton<PackageManagement>();
            x.AddSpinClusterClients(LogLevel.Warning);
            x.AddSpinClusterAdminClients(LogLevel.Warning);
        });

    public CommandRouterBuilder CreateTest<T>(AgentOption agentOption, params string[] args) where T : class, IRunSmartc => new CommandRouterBuilder()
        .SetArgs(args)
        .ConfigureAppConfiguration(config => config.AddEnvironmentVariables("SPIN_AGENT_"))
        .ConfigureAppConfiguration((config, service) => service.AddSingleton(agentOption.Verify()))
        .AddCommand<LookForWorkActivity>()
        .ConfigureService(x =>
        {
            x.AddSingleton<IRunSmartc, T>();
            x.AddSpinClusterClients(LogLevel.Warning);
            x.AddSpinClusterAdminClients(LogLevel.Warning);
        });
}
