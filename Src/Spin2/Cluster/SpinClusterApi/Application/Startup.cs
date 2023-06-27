using Microsoft.AspNetCore.Mvc;
using SpinClusterApi.Connectors;
using Toolbox.Types;

namespace SpinClusterApi.Application;

internal static class Startup
{
    public static IServiceCollection AddSpinApi(this IServiceCollection services)
    {
        services.AddSingleton<SchemaConnector>();
        services.AddSingleton<LeaseConnector>();
        services.AddSingleton<ConfigurationConnector>();
        services.AddSingleton<SearchConnector>();

        return services;
    }

    public static void MapSpinApi(this IEndpointRouteBuilder app)
    {
        app.ServiceProvider.GetRequiredService<SchemaConnector>().Setup(app);
        app.ServiceProvider.GetRequiredService<LeaseConnector>().Setup(app);
        app.ServiceProvider.GetRequiredService<ConfigurationConnector>().Setup(app);
        app.ServiceProvider.GetRequiredService<SearchConnector>().Setup(app);
    }
}
