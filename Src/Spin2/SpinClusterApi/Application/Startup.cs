using SpinClusterApi.Connectors;

namespace SpinClusterApi.Application;

internal static class Startup
{
    public static IServiceCollection AddSpinApiInternal(this IServiceCollection services)
    {
        services.AddSingleton<SchemaConnector>();

        return services;
    }

    public static void MapSpinApiInternal(this IEndpointRouteBuilder app)
    {
        app.ServiceProvider.GetRequiredService<SchemaConnector>().Setup(app);
    }
}
