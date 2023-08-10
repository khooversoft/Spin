using Microsoft.Extensions.DependencyInjection;
using Orleans.Runtime;
using Orleans.Storage;
using SpinCluster.sdk.Application;

namespace SpinCluster.sdk.State;

public static class DatalakeStateExtensions
{
    public static ISiloBuilder AddDatalakeGrainStorage(this ISiloBuilder builder) =>
        builder.ConfigureServices(services =>
        {
            services.AddSingleton<DatalakeStateConnector>();
            services.AddSingletonNamedService(SpinConstants.SpinStateStore, CreateStorage);
        });

    private static IGrainStorage CreateStorage(IServiceProvider service, string name)
    {
        return name switch
        {
            SpinConstants.SpinStateStore => service.GetRequiredService<DatalakeStateConnector>(),

            _ => throw new InvalidOperationException($"Invalid storage name={name}"),
        };
    }
}
