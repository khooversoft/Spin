using Microsoft.Extensions.DependencyInjection;
using Orleans.Runtime;
using Orleans.Storage;
using SpinCluster.sdk.Application;
using Toolbox.Extensions;

namespace SpinCluster.sdk.State;

public static class DatalakeStorageExtensions
{
    public static ISiloBuilder AddDatalakeGrainStorage(this ISiloBuilder builder, SpinClusterOption option) =>
        builder.ConfigureServices(services => services.AddDatalakeGrainStorage(option));

    public static IServiceCollection AddDatalakeGrainStorage(this IServiceCollection services, SpinClusterOption option)
    {
        services.AddSingleton<DatalakeStorageFactory>();

        option.Schemas.ForEach(x => services.AddSingletonNamedService(x.SchemaName, CreateStorage));

        //services.AddSingletonNamedService(datalakeText, (p, n) => (ILifecycleParticipant<ISiloLifecycle>)p.GetRequiredServiceByName<IGrainStorage>(n));

        return services;
    }

    private static IGrainStorage CreateStorage(IServiceProvider service, string name)
    {
        DatalakeStorageFactory factory = service.GetRequiredService<DatalakeStorageFactory>();
        return factory.CreateStorage(service, name);
    }
}
