using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using SpinCluster.abstraction;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinClient.sdk;

public static class ClientSetup
{
    public static IServiceCollection AddClientOption(this IServiceCollection services)
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

    public static IServiceCollection AddClusterHttpClient<T>(this IServiceCollection services) where T : class
    {
        services.AddHttpClient<T>((service, HttpClient) =>
        {
            var clientOption = service.GetRequiredService<ClientOption>();

            HttpClient.BaseAddress = new Uri(clientOption.ClusterApiUri);
        });

        return services;
    }

    public static IServiceCollection AddSpinClusterClients(this IServiceCollection services, LogLevel? logLevel = null)
    {
        services.AddClusterHttpClient<ContractClient>();
        services.AddClusterHttpClient<SchedulerClient>();
        services.AddClusterHttpClient<ScheduleWorkClient>();
        services.AddClusterHttpClient<SignatureClient>();

        if (logLevel != null)
        {
            services.AddLogging(config =>
            {
                config.AddFilter(typeof(ContractClient).FullName, (LogLevel)logLevel);
                config.AddFilter(typeof(SchedulerClient).FullName, (LogLevel)logLevel);
                config.AddFilter(typeof(ScheduleWorkClient).FullName, (LogLevel)logLevel);
                config.AddFilter(typeof(SignatureClient).FullName, (LogLevel)logLevel);
            });
        }

        return services;
    }

    public static IServiceCollection AddSpinClusterAdminClients(this IServiceCollection services, LogLevel? logLevel = null)
    {
        services.AddClusterHttpClient<AgentClient>();
        services.AddClusterHttpClient<ConfigClient>();
        services.AddClusterHttpClient<DomainClient>();
        services.AddClusterHttpClient<LeaseClient>();
        services.AddClusterHttpClient<PrincipalKeyClient>();
        services.AddClusterHttpClient<PrincipalPrivateKeyClient>();
        services.AddClusterHttpClient<SmartcClient>();
        services.AddClusterHttpClient<StorageClient>();
        services.AddClusterHttpClient<SubscriptionClient>();
        services.AddClusterHttpClient<TenantClient>();
        services.AddClusterHttpClient<UserClient>();
        services.AddClusterHttpClient<DirectoryClient>();

        if (logLevel != null)
        {
            services.AddLogging(config =>
            {
                config.AddFilter(typeof(AgentClient).FullName, (LogLevel)logLevel);
                config.AddFilter(typeof(ConfigClient).FullName, (LogLevel)logLevel);
                config.AddFilter(typeof(DomainClient).FullName, (LogLevel)logLevel);
                config.AddFilter(typeof(LeaseClient).FullName, (LogLevel)logLevel);
                config.AddFilter(typeof(PrincipalKeyClient).FullName, (LogLevel)logLevel);
                config.AddFilter(typeof(PrincipalPrivateKeyClient).FullName, (LogLevel)logLevel);
                config.AddFilter(typeof(SmartcClient).FullName, (LogLevel)logLevel);
                config.AddFilter(typeof(StorageClient).FullName, (LogLevel)logLevel);
                config.AddFilter(typeof(SubscriptionClient).FullName, (LogLevel)logLevel);
                config.AddFilter(typeof(TenantClient).FullName, (LogLevel)logLevel);
                config.AddFilter(typeof(UserClient).FullName, (LogLevel)logLevel);
                config.AddFilter(typeof(DirectoryClient).FullName, (LogLevel)logLevel);
            });
        }

        return services;
    }
}
