using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Toolbox.Tools;

namespace Toolbox.Data;

public static class TransactionStartup
{
    public static IServiceCollection AddTransactionServices(this IServiceCollection services)
    {
        services.AddTransient<TransactionManager>();
        services.TryAddSingleton<LogSequenceNumber>();

        return services;
    }
}
