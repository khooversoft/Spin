using SpinCluster.sdk.Actors.Tenant;
using SpinClusterApi.Connectors;

namespace SpinClusterApi.Application;

internal static class Startup
{
    public static IServiceCollection AddSpinApi(this IServiceCollection services)
    {
        services.AddSingleton<SchemaConnector>();
        services.AddSingleton<LeaseConnector>();
        services.AddSingleton<ConfigurationConnector>();
        services.AddSingleton<SearchConnector>();
        services.AddSingleton<ResourceConnect>();
        services.AddSingleton<TenantConnector>();

        return services;
    }

    public static void MapSpinApi(this IEndpointRouteBuilder app)
    {
        app.ServiceProvider.GetRequiredService<SchemaConnector>().Setup(app);
        app.ServiceProvider.GetRequiredService<LeaseConnector>().Setup(app);
        app.ServiceProvider.GetRequiredService<ConfigurationConnector>().Setup(app);
        app.ServiceProvider.GetRequiredService<SearchConnector>().Setup(app);
        app.ServiceProvider.GetRequiredService<ResourceConnect>().Setup(app);
        app.ServiceProvider.GetRequiredService<TenantConnector>().Setup(app);
    }
}
