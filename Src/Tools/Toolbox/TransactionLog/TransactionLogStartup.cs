using Microsoft.Extensions.DependencyInjection;
using Toolbox.Tools;

namespace Toolbox.TransactionLog;

public static class TransactionLogStartup
{
    public static IServiceCollection AddTransactionLogProvider(this IServiceCollection services, Action<IServiceProvider, TransactionLogProvider> config)
    {
        config.NotNull();

        services.NotNull().AddSingleton<ITransactionLog, TransactionLogProvider>((service) =>
        {
            var provider = new TransactionLogProvider();
            config.Invoke(service, provider);
            return provider;
        });

        return services;
    }
}

