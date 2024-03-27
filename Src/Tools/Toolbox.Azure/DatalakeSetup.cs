using Microsoft.Extensions.DependencyInjection;
using Toolbox.Tools;

namespace Toolbox.Azure;

public static class DatalakeSetup
{
    public static IServiceCollection AddDatalakeManager(this IServiceCollection services, Action<IDatalakeManagerConfigure> manager)
    {
        services.NotNull();
        manager.NotNull();

        services.AddSingleton<IDatalakeManager>(services =>
        {
            var datalakeManager = new DatalakeManager(services);
            manager(datalakeManager);
            return datalakeManager;
        });

        return services;
    }
}
