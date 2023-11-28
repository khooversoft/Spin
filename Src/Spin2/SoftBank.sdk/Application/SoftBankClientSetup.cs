using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SoftBank.sdk.SoftBank;
using SoftBank.sdk.Trx;
using SpinClient.sdk;

namespace SoftBank.sdk.Application;

public static class SoftBankClientSetup
{
    public static IServiceCollection AddSoftBankClients(this IServiceCollection services, LogLevel? logLevel = null)
    {
        services.AddClusterHttpClient<SoftBankClient>();
        services.AddClusterHttpClient<SoftBankTrxClient>();

        if (logLevel != null)
        {
            services.AddLogging(config =>
            {
                config.AddFilter(typeof(SoftBankClient).FullName, (LogLevel)logLevel);
                config.AddFilter(typeof(SoftBankTrxClient).FullName, (LogLevel)logLevel);
            });
        }

        return services;
    }
}