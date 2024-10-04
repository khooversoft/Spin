using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Toolbox.Tools;

namespace Toolbox.TransactionLog;

public static class TransactionLogStartup
{
    public static IServiceCollection AddTransactionLogProvider(this IServiceCollection services, string? connectionString = null, int? maxCount = 1000)
    {
        services.NotNull();

        services.TryAddSingleton(new TransactionLogFileOption
        {
            ConnectionString = connectionString ?? "journal=/journal/data",
            MaxCount = maxCount ?? 1000
        });
        services.AddSingleton<ITransactionLogWriter, TransactionLogFile>();
        services.AddSingleton<ITransactionLog, TransactionLogProvider>();

        return services;
    }
}

