﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Actors.Agent;
using SpinCluster.sdk.Actors.Configuration;
using SpinCluster.sdk.Actors.Contract;
using SpinCluster.sdk.Actors.Directory;
using SpinCluster.sdk.Actors.Domain;
using SpinCluster.sdk.Actors.Lease;
using SpinCluster.sdk.Actors.PrincipalKey;
using SpinCluster.sdk.Actors.PrincipalPrivateKey;
using SpinCluster.sdk.Actors.Scheduler;
using SpinCluster.sdk.Actors.ScheduleWork;
using SpinCluster.sdk.Actors.Signature;
using SpinCluster.sdk.Actors.Smartc;
using SpinCluster.sdk.Actors.Storage;
using SpinCluster.sdk.Actors.Subscription;
using SpinCluster.sdk.Actors.Tenant;
using SpinCluster.sdk.Actors.User;
using Toolbox.Tools;

namespace SpinCluster.sdk.Application;

public static class ClientSetup
{
    public static IServiceCollection AddClusterHttpClient<T>(this IServiceCollection services) where T : class
    {
        services.AddHttpClient<T>((service, HttpClient) =>
        {
            string uri = service
                .GetRequiredService<IConfiguration>()["ClusterApiUri"]
                .NotNull("No 'ClusterApiUri' in configuration");

            HttpClient.BaseAddress = new Uri(uri);
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