using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Services;
using SpinCluster.sdk.State;
using Toolbox.Azure.DataLake;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.sdk.Application;

public static class SiloStartup
{
    public static ISiloBuilder AddSpinCluster(this ISiloBuilder builder, HostBuilderContext hostContext)
    {
        builder.NotNull();

        SpinClusterOption option = hostContext.Configuration.Bind<SpinClusterOption>();

        builder.AddDatalakeGrainStorage();
        builder.AddStartupTask(async (IServiceProvider services, CancellationToken _) => await services.UseSpinCluster());

        builder.ConfigureServices(services =>
        {
            services.AddSpinCluster(option);
        });

        return builder;
    }

    public static IServiceCollection AddSpinCluster(this IServiceCollection services, SpinClusterOption option)
    {
        services.AddSingleton(option);

        services.AddSingleton<DatalakeSchemaResources>();
        services.AddSingleton<DatalakeResourceIdMap>();

        services.AddSingleton<SiloConfigStore>(service =>
        {
            var context = new ScopeContext(service.GetRequiredService<ILoggerFactory>().CreateLogger<SiloConfigStore>());

            SpinClusterOption clusterOption = service.GetRequiredService<SpinClusterOption>();
            if (!clusterOption.Validate(out Option v))
            {
                context.Location().LogError("SpinClusterOption is invalid, errors={errors}", v.Error);
                throw new ArgumentException($"SpinClusterOption is invalid, errors={v.Error}");
            }

            DatalakeLocation datalakeLocation = DatalakeLocation.ParseConnectionString(clusterOption.BootConnectionString)
                .ThrowOnError()
                .Return();

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
