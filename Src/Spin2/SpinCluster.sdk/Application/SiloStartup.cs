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
        option.Validate(out Option v).Assert(x => x == true, $"SpinClusterOption is invalid, errors={v.Error}");

        builder.AddDatalakeGrainStorage();
        builder.AddStartupTask(async (IServiceProvider services, CancellationToken _) => await services.RunStartup());

        builder.ConfigureServices(services =>
        {
            services.AddSingleton(option);
            services.AddSingleton<DatalakeSchemaResources>();
            services.AddSingleton<DatalakeResourceIdMap>();

            services.AddSingleton<SiloConfigStore>(service =>
            {
                SpinClusterOption clusterOption = service.GetRequiredService<SpinClusterOption>();

                DatalakeEndpoint datalakeLocation = DatalakeEndpoint.Create(clusterOption.BootConnectionString);
                datalakeLocation.Validate().ThrowOnError();

                var option = new DatalakeOption
                {
                    Account = datalakeLocation.Account,
                    Container = datalakeLocation.Container,
                    Credentials = clusterOption.Credentials,
                };

                IDatalakeStore store = ActivatorUtilities.CreateInstance<DatalakeStore>(service, option);
                return ActivatorUtilities.CreateInstance<SiloConfigStore>(service, datalakeLocation, store);
            });
        });

        return builder;
    }

    private static async Task RunStartup(this IServiceProvider serviceProvider)
    {
        serviceProvider.NotNull();

        DatalakeSchemaResources datalakeResources = serviceProvider.GetRequiredService<DatalakeSchemaResources>();
        ILoggerFactory factory = serviceProvider.GetRequiredService<ILoggerFactory>();

        await datalakeResources.Startup(new ScopeContext(factory.CreateLogger(nameof(RunStartup))));
    }
}
