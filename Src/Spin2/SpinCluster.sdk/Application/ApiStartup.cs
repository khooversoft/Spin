using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using SpinCluster.sdk.Actors.Configuration;
using SpinCluster.sdk.Actors.Contract;
using SpinCluster.sdk.Actors.PrincipalKey;
using SpinCluster.sdk.Actors.Subscription;
using SpinCluster.sdk.Actors.Tenant;
using SpinCluster.sdk.Actors.User;
using SpinClusterApi.Connectors;

namespace SpinCluster.sdk.Application;

public static class ApiStartup
{
    public static IServiceCollection AddSpinApi(this IServiceCollection services)
    {
        services.AddSingleton<SubscriptionConnector>();
        services.AddSingleton<TenantConnector>();
        services.AddSingleton<UserConnector>();
        services.AddSingleton<ConfigurationConnector>();
        services.AddSingleton<SearchConnector>();
        services.AddSingleton<PrincipalKeyConnector>();
        services.AddSingleton<PrincipalPrivateKeyConnector>();
        services.AddSingleton<SignatureConnector>();
        services.AddSingleton<ContractConnector>();
        services.AddSingleton<LeaseConnector>();

        return services;
    }

    public static void MapSpinApi(this IEndpointRouteBuilder app)
    {
        app.ServiceProvider.GetRequiredService<SubscriptionConnector>().Setup(app);
        app.ServiceProvider.GetRequiredService<TenantConnector>().Setup(app);
        app.ServiceProvider.GetRequiredService<UserConnector>().Setup(app);
        app.ServiceProvider.GetRequiredService<ConfigurationConnector>().Setup(app);
        app.ServiceProvider.GetRequiredService<SearchConnector>().Setup(app);
        app.ServiceProvider.GetRequiredService<PrincipalKeyConnector>().Setup(app);
        app.ServiceProvider.GetRequiredService<PrincipalPrivateKeyConnector>().Setup(app);
        app.ServiceProvider.GetRequiredService<SignatureConnector>().Setup(app);
        app.ServiceProvider.GetRequiredService<ContractConnector>().Setup(app);
        app.ServiceProvider.GetRequiredService<LeaseConnector>().Setup(app);
    }
}
