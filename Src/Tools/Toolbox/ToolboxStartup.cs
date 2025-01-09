using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Toolbox.Journal;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.TransactionLog;

namespace Toolbox;

public static class ToolboxStartup
{
    public static IServiceCollection AddInMemoryFileStore(this IServiceCollection services)
    {
        services.NotNull().AddSingleton<IFileStore, InMemoryFileStore>();
        return services;
    }

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

    public static IServiceCollection AddJournalLog(this IServiceCollection services, string key, string connectionString)
    {
        services.NotNull();
        connectionString.NotEmpty();

        services.AddKeyedSingleton<IJournalWriter>(key, (iServices, _) =>
        {
            var option = new JournalFileOption
            {
                ConnectionString = connectionString,
            };

            return ActivatorUtilities.CreateInstance<JournalFile>(iServices, option);
        });

        return services;
    }
}
