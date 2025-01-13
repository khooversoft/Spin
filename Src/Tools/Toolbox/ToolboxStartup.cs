using Microsoft.Extensions.DependencyInjection;
using Toolbox.Journal;
using Toolbox.Store;
using Toolbox.Tools;

namespace Toolbox;

public static class ToolboxStartup
{
    public static IServiceCollection AddInMemoryFileStore(this IServiceCollection services)
    {
        services.NotNull().AddSingleton<IFileStore, InMemoryFileStore>();
        return services;
    }

    //public static IServiceCollection AddTransactionLogProvider(this IServiceCollection services, string? connectionString = null, int? maxCount = 1000)
    //{
    //    services.NotNull();

    //    services.TryAddSingleton(new TransactionLogFileOption
    //    {
    //        ConnectionString = connectionString ?? "journal=/journal/data",
    //        MaxCount = maxCount ?? 1000
    //    });

    //    services.AddSingleton<ITransactionLogWriter, TransactionLogFile>();
    //    services.AddSingleton<ITransactionLog, TransactionLogProvider>();

    //    return services;
    //}

    public static IServiceCollection AddJournalLog(this IServiceCollection services, string key, string connectionString, bool useBackgroundWriter = false)
    {
        services.NotNull();
        key.NotEmpty();
        connectionString.NotEmpty();

        services.AddKeyedSingleton<IJournalFile>(key, (iServices, _) =>
        {
            var option = new JournalFileOption
            {
                ConnectionString = connectionString,
                UseBackgroundWriter = useBackgroundWriter,
            };

            return ActivatorUtilities.CreateInstance<JournalFile>(iServices, option);
        });

        return services;
    }
}
