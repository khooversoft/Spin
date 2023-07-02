using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Actors.Configuration;
using SpinCluster.sdk.Actors.Key;
using SpinCluster.sdk.Actors.Search;
using SpinCluster.sdk.Actors.Tenant;
using SpinCluster.sdk.Actors.User;
using SpinCluster.sdk.Services;
using Toolbox.Azure.DataLake;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Application;

public static class Setup
{
    public static IServiceCollection AddSpinCluster(this IServiceCollection services, SpinClusterOption option)
    {
        services.AddSingleton(option);

        services.AddSingleton<Validator<UserModel>>(UserModelValidator.Validator);
        services.AddSingleton<Validator<PrincipalKey>>(PrincipalKeyValidator.Validator);
        services.AddSingleton<Validator<SiloConfigOption>>(SiloConfigOptionValidator.Validator);
        services.AddSingleton<Validator<SearchQuery>>(SearchQueryValidator.Validator);
        services.AddSingleton<Validator<TenantModel>>(TenantRegisterValidator.Validator);

        services.AddSingleton<DatalakeSchemaResources>();

        services.AddSingleton<SiloConfigStore>(service =>
        {
            SpinClusterOption clusterOption = service.GetRequiredService<SpinClusterOption>();
            DatalakeLocation datalakeLocation = DatalakeLocation.ParseConnectionString(clusterOption.BootConnectionString).Return();
            var loggerFactory = service.GetRequiredService<ILoggerFactory>();

            var option = new DatalakeOption
            {
                AccountName = datalakeLocation.Account,
                ContainerName = datalakeLocation.Container,
                Credentials = clusterOption.ClientCredentials,
            };

            var store = new DatalakeStore(option, loggerFactory.CreateLogger<DatalakeStore>());

            return new SiloConfigStore(datalakeLocation, store, loggerFactory.CreateLogger<SiloConfigStore>());
        });



        return services;
    }

    public static async Task UseSpinCluster(this IHost app)
    {
        DatalakeSchemaResources datalakeResources = app.Services.GetRequiredService<DatalakeSchemaResources>();
        ILoggerFactory factory = app.Services.GetRequiredService<ILoggerFactory>();

        await datalakeResources.Startup(new ScopeContext(factory.CreateLogger(nameof(UseSpinCluster))));
    }
}
