using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Toolbox.Data;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox;

public static class TransactionStartup
{
    public static IServiceCollection AddTransactionServices(this IServiceCollection services, TransactionManagerOption option)
    {
        option.NotNull();
        services.AddTransient<TransactionManager>(services => ActivatorUtilities.CreateInstance<TransactionManager>(services, option));
        services.TryAddSingleton<LogSequenceNumber>();

        return services;
    }

    public static IServiceCollection AddTransaction(this IServiceCollection services, TransactionOption option)
    {
        option.NotNull().Validate().ThrowOnError();

        services.AddSingleton<TransactionOption>(option);
        services.TryAddSingleton<LogSequenceNumber>();
        services.AddTransient<Transaction>();

        return services;
    }
}
