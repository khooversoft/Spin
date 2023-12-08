using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using SoftBank.sdk.SoftBank;
using SoftBank.sdk.Trx;
using SpinCluster.abstraction;
using SpinCluster.sdk.Actors.User;
using SpinCluster.sdk.Application;
using Toolbox.Tools;
using Toolbox.Types;

namespace SoftBank.sdk.Application;

public static class SoftBankApiStartup
{
    private static Plan _siloPlan { get; } = new Plan(PlanMode.First)
        .AddAsync(CheckIfSoftbankPrincipalExist)
        .AddAsync(AddSoftbankPrincipal);

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
        services.AddSingleton<IPlan>(_siloPlan);

        return services;
    }

    public static void MapSoftBank(this IEndpointRouteBuilder app)
    {
        app.ServiceProvider.GetRequiredService<SoftBankConnector>().Setup(app);
        app.ServiceProvider.GetRequiredService<SoftBankTrxConnector>().Setup(app);
    }

    private static async Task<Option> CheckIfSoftbankPrincipalExist(PlanContext planContext, ScopeContext context)
    {
        planContext.NotNull();

        string userId = $"user:{SoftBankConstants.SoftBankPrincipalId}";

        var option = await planContext.Service
            .GetRequiredService<IClusterClient>()
            .GetResourceGrain<IUserActor>(userId)
            .Exist(context.TraceId);

        return option;
    }

    private static async Task<Option> AddSoftbankPrincipal(PlanContext planContext, ScopeContext context)
    {
        planContext.NotNull();

        var createUser = new UserCreateModel
        {
            UserId = $"user:{SoftBankConstants.SoftBankPrincipalId}",
            PrincipalId = SoftBankConstants.SoftBankPrincipalId,
            DisplayName = "Softbank System Principal ID",
            FirstName = "Softbank",
            LastName = "Softbank"
        };

        var option = await planContext.Service
            .GetRequiredService<IClusterClient>()
            .GetResourceGrain<IUserActor>(createUser.UserId)
            .Create(createUser, context.TraceId);

        return option;
    }
}
