using Microsoft.Extensions.DependencyInjection;

namespace Toolbox.Block.Contract;

public static class ContractStartup
{
    public static IServiceCollection AddBlockHost(this IServiceCollection services)
    {
        services.AddTransient<ContractHost>();

        return services;
    }
}
