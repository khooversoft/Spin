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

    public static IServiceCollection AddTransactionLogProvider(this IServiceCollection services, string connectionString)
    {
        services.NotNull();
        connectionString.NotEmpty();

        services.AddSingleton(new TransactionLogFileOption { ConnectionString = connectionString });
        services.AddSingleton<ITransactionLogWriter, TransactionLogFile>();

        services.NotNull().AddSingleton<ITransactionLog, TransactionLogProvider>((service) =>
        {
            var writer = service.GetRequiredService<ITransactionLogWriter>();
            var provider = new TransactionLogProvider();
            provider.Add(writer);
            return provider;
        });

        return services;
    }
}

