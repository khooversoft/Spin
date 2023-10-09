using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using SoftBank.sdk.SoftBank;
using SoftBank.sdk.Trx;
using Toolbox.Tools;

namespace SoftBank.sdk.Application;

public static class SoftBankApiStartup
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
        services.AddSingleton<SoftBankTrxConnector>();

        return services;
    }

    public static void MapSoftBank(this IEndpointRouteBuilder app)
    {
        app.ServiceProvider.GetRequiredService<SoftBankConnector>().Setup(app);
        app.ServiceProvider.GetRequiredService<SoftBankTrxConnector>().Setup(app);
    }
}
