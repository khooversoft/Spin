using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Actors.Configuration;
using SpinCluster.sdk.Actors.Key;
using SpinCluster.sdk.Actors.PrincipalKey;
using SpinCluster.sdk.Actors.PrincipalPrivateKey;
using SpinCluster.sdk.Actors.Search;
using SpinCluster.sdk.Actors.Signature;
using SpinCluster.sdk.Actors.Tenant;
using SpinCluster.sdk.Actors.User;
using SpinCluster.sdk.Services;
using SpinCluster.sdk.State;
using Toolbox.Azure.DataLake;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Tools.Validation;
using Toolbox.Types;
//using SoftBank.sdk.Application;

namespace SpinCluster.sdk.Application;

public static class SiloStartup
{
    public static ISiloBuilder AddSpinCluster(this ISiloBuilder builder, string appsettingFile = "appsettings.json")
    {
        builder.NotNull();

        SpinClusterOption option = SpinClusterOptionTool.Read(appsettingFile);

        builder.AddDatalakeGrainStorage(option);
        builder.AddStartupTask(async (IServiceProvider services, CancellationToken _) => await services.UseSpinCluster());

        //builder.ConfigureLogging(logging =>
        //{
        //    logging.AddConsole();
        //    logging.AddDebug();

        //    if (option.AppInsightsConnectionString.IsNotEmpty())
        //    {
        //        logging.AddApplicationInsights(
        //            configureTelemetryConfiguration: (config) => config.ConnectionString = option.AppInsightsConnectionString,
        //            configureApplicationInsightsLoggerOptions: (options) => { }
        //        );
        //    }
        //});

        builder.ConfigureServices(services =>
        {
            services.AddSpinCluster(option);
        });

        //builder.AddSoftBank();

        return builder;
    }

    public static IServiceCollection AddSpinCluster(this IServiceCollection services, SpinClusterOption option)
    {
        services.AddSingleton(option);

        services.AddSingleton<IValidator<UserModel>>(UserModelValidator.Validator);
        services.AddSingleton<IValidator<SiloConfigOption>>(SiloConfigOptionValidator.Validator);
        services.AddSingleton<IValidator<SearchQuery>>(SearchQueryValidator.Validator);
        services.AddSingleton<IValidator<TenantModel>>(TenantRegisterValidator.Validator);
        services.AddSingleton<IValidator<PrincipalKeyModel>>(PrincipalKeyValidator.Validator);
        services.AddSingleton<IValidator<PrincipalKeyRequest>>(PrincipalKeyRequestValidator.Validator);
        services.AddSingleton<IValidator<PrincipalPrivateKeyModel>>(PrincipalPrivateKeyModelValidator.Validator);
        services.AddSingleton<IValidator<SignRequest>>(SignRequestValidator.Validator);
        services.AddSingleton<IValidator<ValidateRequest>>(ValidateRequestValidator.Validator);

        services.AddSingleton<DatalakeSchemaResources>();

        services.AddSingleton<SiloConfigStore>(service =>
        {
            SpinClusterOption clusterOption = service.GetRequiredService<SpinClusterOption>();
            var context = new ScopeContext(service.GetRequiredService<ILoggerFactory>().CreateLogger<SiloConfigStore>());

            DatalakeLocation datalakeLocation = DatalakeLocation.ParseConnectionString(clusterOption.BootConnectionString, context.Location()).Return();
            var loggerFactory = service.GetRequiredService<ILoggerFactory>();

            var option = new DatalakeOption
            {
                AccountName = datalakeLocation.Account,
                ContainerName = datalakeLocation.Container,
                Credentials = clusterOption.Credentials,
            };

            var store = new DatalakeStore(option, loggerFactory.CreateLogger<DatalakeStore>());

            return new SiloConfigStore(datalakeLocation, store, loggerFactory.CreateLogger<SiloConfigStore>());
        });



        return services;
    }

    public static Task UseSpinCluster(this IHost app) => UseSpinCluster(app.Services);

    public static async Task UseSpinCluster(this IServiceProvider serviceProvider)
    {
        serviceProvider.NotNull();

        DatalakeSchemaResources datalakeResources = serviceProvider.GetRequiredService<DatalakeSchemaResources>();
        ILoggerFactory factory = serviceProvider.GetRequiredService<ILoggerFactory>();

        await datalakeResources.Startup(new ScopeContext(factory.CreateLogger(nameof(UseSpinCluster))));
    }
}
