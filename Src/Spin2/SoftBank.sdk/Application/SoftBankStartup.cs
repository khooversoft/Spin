using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using SpinCluster.sdk.Actors.SoftBank;
using Toolbox.Tools;

namespace SoftBank.sdk.Application;

public static class SoftBankStartup
{
    public static ISiloBuilder AddSoftBank(this ISiloBuilder builder)
    {
        builder.NotNull();

        builder.ConfigureServices(services => services.AddSoftBank());
        return builder;
    }

    public static IServiceCollection AddSoftBank(this IServiceCollection services)
    {
        services.NotNull();

        services.AddSingleton<SoftBankConnector>();

        return services;
    }

    public static void MapSoftBank(this IEndpointRouteBuilder app)
    {
        app.ServiceProvider.GetRequiredService<SoftBankConnector>().Setup(app);
    }
}
