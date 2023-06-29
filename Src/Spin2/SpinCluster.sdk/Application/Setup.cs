using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Actors.Configuration;
using SpinCluster.sdk.Actors.Directory;
using SpinCluster.sdk.Actors.Search;
using SpinCluster.sdk.Actors.Storage;
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

        services.AddSingleton<Validator<UserPrincipal>>(UserPrincipalValidator.Validator);
        services.AddSingleton<Validator<PrincipalKey>>(PrincipalKeyValidator.Validator);
        services.AddSingleton<Validator<SiloConfigOption>>(SiloConfigOptionValidator.Validator);
        services.AddSingleton<Validator<SearchQuery>>(SearchQueryValidator.Validator);

        services.AddSingleton<DatalakeResources>();

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
        DatalakeResources datalakeResources = app.Services.GetRequiredService<DatalakeResources>();
        ILoggerFactory factory = app.Services.GetRequiredService<ILoggerFactory>();

        await datalakeResources.Startup(new ScopeContext(factory.CreateLogger(nameof(UseSpinCluster))));
    }
}
