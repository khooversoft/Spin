using Microsoft.Extensions.DependencyInjection;

namespace Toolbox.CommandRouter;

public static class CommandRouterSetup
{
    public static IServiceCollection AddCommandRoute<T>(this IServiceCollection services) where T : class, ICommandRoute
    {
        services.AddSingleton<ICommandRoute, T>();
        return services;
    }
}
