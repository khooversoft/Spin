using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Services;
using SpinCluster.sdk.State;
using Toolbox.Azure;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.sdk.Application;

public static class SiloStartup
{
    private static Plan _startupPlan = new Plan().AddAsync(SetupDatalakeSchemaResources);

    public static ISiloBuilder AddSpinCluster(this ISiloBuilder builder, HostBuilderContext hostContext)
    {
        builder.NotNull();

        SpinClusterOption option = hostContext.Configuration.Bind<SpinClusterOption>();
        option.Validate(out Option v).Assert(x => x == true, $"SpinClusterOption is invalid, errors={v.Error}");

        builder.AddDatalakeGrainStorage();
        builder.AddStartupTask(async (IServiceProvider services, CancellationToken _) => await services.RunStartup(), stage: ServiceLifecycleStage.ApplicationServices);

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

    private static async Task RunStartup(this IServiceProvider service)
    {
        service.NotNull();

        ILogger logger = service.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(RunStartup));
        var context = new ScopeContext(logger);

        var plans = service.GetServices<IPlan>().ToArray();

        var option = await new Plan()
            .Add(_startupPlan)
            .AddRange(plans)
            .Run(service, context);

        if (option.IsError())
        {
            context.Location().LogCritical("Failed to startup cluster");
            throw new InvalidOperationException("Startup failed");
        }

        context.Location().LogInformation("Startup has completed");
    }

    private static async Task<Option> SetupDatalakeSchemaResources(PlanContext planContext, ScopeContext context)
    {
        DatalakeSchemaResources datalakeResources = planContext.Service.GetRequiredService<DatalakeSchemaResources>();
        var option = await datalakeResources.Startup(context);
        return option;
    }
}
