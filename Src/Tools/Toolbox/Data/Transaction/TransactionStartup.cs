using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Toolbox.Tools;

namespace Toolbox.Data;

public static class TransactionStartup
{
    public static IServiceCollection AddTransactionServices(this IServiceCollection services, TransactionManagerOption option)
    {
        option.NotNull();
        //services.AddKeyedSingleton<TransactionProviderRegistry>(option.Name);
        services.AddTransient<TransactionManager>(services => ActivatorUtilities.CreateInstance<TransactionManager>(services, option));
        services.TryAddSingleton<LogSequenceNumber>();

        return services;
    }
}
