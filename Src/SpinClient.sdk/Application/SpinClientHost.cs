using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using SpinCluster.abstraction;
using Toolbox.CommandRouter;
using Toolbox.Extensions;
using Toolbox.Types;

namespace SpinClient.sdk;

public static class SpinClientHost
{
    public static IServiceCollection AddScheduleOption(this IServiceCollection services)
    {
        services.TryAddSingleton<ScheduleOption>(service =>
        {
            IConfiguration config = service.GetRequiredService<IConfiguration>();

            var option = config.Bind<ScheduleOption>();
            option.Validate().ThrowOnError();
            return option;
        });

        return services;
    }

    public static CommandRouterBuilder CreateLocalAgent() => new CommandRouterBuilder()
        .ConfigureAppConfiguration(config => config.AddEnvironmentVariables("SPIN_AGENT_"))
        .AddCommand<LookForWorkActivity>()
        .AddCommand<RunWorkActivity>()
        .ConfigureService(x =>
        {
            x.AddScheduleOption();
            x.AddSingleton<IRunSmartc, RunSmartC>();
            x.AddSingleton<PackageManagement>();
            x.AddSpinClusterClients(LogLevel.Warning);
            x.AddSpinClusterAdminClients(LogLevel.Warning);
        });

    /// <summary>
    /// Specialized builder for testing workflow in test cases
    /// </summary>
    /// <param name="option"></param>
    /// <returns></returns>
    public static CommandRouterBuilder CreateTestAgent(ScheduleOption option) => new CommandRouterBuilder()
        .AddCommand<LookForWorkActivity>()
        .AddCommand<RunWorkActivity>()
        .ConfigureService(x =>
        {
            x.AddSingleton(option);
            x.AddClientOption();
            x.AddSingleton<IRunSmartc, RunInMemory>();
            x.AddSingleton<PackageManagement>();
            x.AddSpinClusterClients(LogLevel.Warning);
            x.AddSpinClusterAdminClients(LogLevel.Warning);
        });
}
